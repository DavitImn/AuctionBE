namespace AuctionService.DTOs
{
    public class BuyNowOutputDto
    {
        public int Id { get; set; }
        public int AuctionId { get; set; }
        public string ItemTitle { get; set; }
        public decimal Price { get; set; }
        public DateTime PurchaseTime { get; set; }
        public int BuyNowId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
    }
}
