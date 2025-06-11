namespace AuctionService.DTOs
{
    public class BuyNowCreateDto
    {
        public int AuctionId { get; set; }
        public decimal Price { get; set; } // Price should match auction's buy now price
    }
}
