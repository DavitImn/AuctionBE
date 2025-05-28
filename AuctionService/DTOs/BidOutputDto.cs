namespace AuctionService.DTOs
{
    public class BidOutputDto
    {
        public int BidId { get; set; }
        public int AuctionId { get; set; }
        public int BidderId { get; set; }
        public string BidderName { get; set; }
        public decimal Amount { get; set; }
        public DateTime BidTime { get; set; }
        public bool IsWinning { get; set; }
    }
}
