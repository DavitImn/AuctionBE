using AuctionService.DataContextDb;
using AuctionService.DIs.Interfaces;
using AuctionService.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.DIs.Services
{
    public class AuctionLifecycleService : BackgroundService
    {

        private readonly IServiceProvider _serviceProvider;

        public AuctionLifecycleService(IServiceProvider serviceProvider)
        {

            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                        var notificationService = scope.ServiceProvider.GetRequiredService<IAuctionNotificationService>();

                        // Notify auctions starting in 10 min
                        var startingSoon = await dbContext.Auctions
                            .Include(a => a.Item).ThenInclude(i => i.Seller)
                            .Where(a => a.Status == AuctionStatus.Pending &&
                                        a.StartTime <= DateTime.Now.AddMinutes(10) &&
                                        a.StartTime > DateTime.Now &&
                                        !a.IsStartSoonNotificationSent)
                            .ToListAsync();

                        foreach (var auction in startingSoon)
                        {
                            await notificationService.NotifyAuctionStartingSoonAsync(auction);
                            auction.IsStartSoonNotificationSent = true;
                        }
                       

                        // Notidy auctions ending in 10 min
                        var endingSoon = await dbContext.Auctions
                           .Include(a => a.Item).ThenInclude(i => i.Seller)
                           .Where(a => a.Status == AuctionStatus.Active &&
                                       a.EndTime <= DateTime.Now.AddMinutes(10) &&
                                       a.EndTime > DateTime.Now &&
                                       !a.IsEndSoonNotificationSent)
                           .ToListAsync();

                        foreach (var auction in endingSoon)
                        {
                            await notificationService.NotifyAuctionEndingSoonAsync(auction);
                            auction.IsEndSoonNotificationSent = true;
                        }


                        if (true)
                        {


                        }
                        // Notify Auction Ended 
                        var endedAuction = await dbContext.Auctions
                           .Include(a => a.Item).ThenInclude(i => i.Seller)
                           .Where(a => a.Status == AuctionStatus.Closed &&
                                       a.EndTime <= DateTime.Now &&
                                       a.EndTime > DateTime.Now.AddMinutes(-1) &&
                                       !a.IsEndedNotificationSent)
                           .ToListAsync();

                        foreach (var auction in endedAuction)
                        {
                            await notificationService.NotifyAuctionEndedAsync(auction);
                            auction.IsEndedNotificationSent = true;
                        }

                        // Start auctions
                        var pendingAuctions = await dbContext.Auctions
                            .Where(a => a.Status == AuctionStatus.Pending && a.StartTime <= DateTime.Now)
                            .ToListAsync();
                        foreach (var auction in pendingAuctions)
                        {
                            auction.Status = AuctionStatus.Active;
                            auction.CurrentPrice = auction.StartingPrice;
                        }

                        // SoftDelete Auctions
                        var deleteThreshold = DateTime.Now.AddDays(-7);
                        var softDeleteAuctions = await dbContext.Auctions
                            .Where(a => (a.Status == AuctionStatus.Closed || a.Status == AuctionStatus.Cancelled)
                                        && a.EndTime <= deleteThreshold
                                        && !a.IsDeleted).ToListAsync();
                        foreach (var auction in softDeleteAuctions)
                        {
                            auction.IsDeleted = true;
                        }

                        // Close auctions
                        var activeAuctions = await dbContext.Auctions
                            .Include(a => a.Bids)
                            .Where(a => a.Status == AuctionStatus.Active && a.EndTime <= DateTime.Now)
                            .ToListAsync();
                        foreach (var auction in activeAuctions)
                        {
                            auction.Status = AuctionStatus.Closed;
                            var winningBid = auction.Bids.OrderByDescending(b => b.Amount).FirstOrDefault();
                            if (winningBid != null)
                            {
                                auction.WinnerId = winningBid.BidderId;
                            }
                        }


                        await dbContext.SaveChangesAsync();

                    }
                    // Run every minute
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in AuctionLifecycleService: {ex.Message}");
                }
            }
        }
    }
}
