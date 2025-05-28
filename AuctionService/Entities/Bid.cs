namespace AuctionService.Entities
{
    public class Bid
    {
        public int Id { get; set; }
        public int AuctionId { get; set; }
        public Auction Auction { get; set; }
        public int BidderId { get; set; }
        public User Bidder { get; set; }
        public decimal Amount { get; set; }
        public DateTime BidTime { get; set; } = DateTime.UtcNow;
        public bool IsWinning { get; set; }
    }
}
