using AuctionService.Entities;

namespace AuctionService.DIs.Interfaces
{
    public interface IAuctionNotificationService
    {
        Task NotifyAuctionStartingSoonAsync(Auction auction);
        Task NotifyAuctionEndingSoonAsync(Auction auction);
        Task NotifyAuctionEndedAsync(Auction auction);
    }
}
