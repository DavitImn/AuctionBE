using AuctionService.Enums;

namespace AuctionService.Entities
{
    public class Auction
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public Item Item { get; set; }
        public decimal StartingPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public AuctionStatus Status { get; set; } = AuctionStatus.Pending;// "Pending", "Active", "Closed", "Cancelled"
        public int? WinnerId { get; set; }
        public User Winner { get; set; }
        public decimal MinimumBidIncrement { get; set; }
        public List<Bid> Bids { get; set; } = new();
        public bool IsDeleted { get; set; } = false; // True After One Week From End of Auction 

        // Email CHecker
        public bool IsStartSoonNotificationSent { get; set; } = false;
        public bool IsEndSoonNotificationSent { get; set; } = false;
        public bool IsEndedNotificationSent { get; set; } = false;
    }
}
