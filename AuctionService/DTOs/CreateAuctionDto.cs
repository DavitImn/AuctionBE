namespace AuctionService.DTOs
{
    public class CreateAuctionDto
    {
        public int ItemId { get; set; }
        public decimal StartingPrice { get; set; }
        public decimal MinimumBidIncrement { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime StartTime { get; set; }
    }
}
