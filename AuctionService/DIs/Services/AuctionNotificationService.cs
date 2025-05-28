using AuctionService.DIs.Interfaces;
using AuctionService.Entities;

namespace AuctionService.DIs.Services
{
    public class AuctionNotificationService : IAuctionNotificationService
    {
        private readonly IEmailSenderService _emailService;
        public AuctionNotificationService(IEmailSenderService emailService)
        {
            _emailService = emailService;
        }

        public async Task NotifyAuctionStartingSoonAsync(Auction auction)
        {
            var subject = $"Dear {auction.Item.Seller.FirstName} Your auction will start soon!";
            var body = $"Hi, your auction for item \"{auction.Item.Title}\" will start in 10 minutes.";
            _emailService.SendEmailToVerify(auction.Item.Seller.Email, subject, body);
        }

        public async Task NotifyAuctionEndingSoonAsync(Auction auction)
        {
            var subject = $"Dear {auction.Item.Seller.FirstName} Your auction will start soon!";
            var body = $"Hi, your auction for item \"{auction.Item.Title}\" will end in 10 minutes.";
            _emailService.SendEmailToVerify(auction.Item.Seller.Email, subject, body);
        }

        public async Task NotifyAuctionEndedAsync(Auction auction)
        {
            var subject = $"Dear {auction.Item.Seller.FirstName} Your auction has ended!";
            var body = $"Your item was sold to \"{auction.Winner.FirstName}\" for {auction.CurrentPrice:C}.";
            _emailService.SendEmailToVerify(auction.Item.Seller.Email, subject, body);
        }

    }
}
