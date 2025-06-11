namespace AuctionService.DTOs
{
    public class AuctionOutputDto
    {
        public int AuctionId { get; set; }
        public decimal StartingPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public ItemOutputDto Item { get; set; }
        public int? WinnerId { get; set; } // ✅ Include this to show the winner (if auction is closed)
        public int MinimumBidIncrement { get; set; } // ✅ Include this to show bidding rules
        public bool IsDeleted { get; set; } = false; // True After One Week From End of Auction 

    }
}
