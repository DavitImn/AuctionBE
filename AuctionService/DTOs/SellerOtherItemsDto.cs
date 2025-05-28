namespace AuctionService.DTOs
{
    public class SellerOtherItemsDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ThumbnailUrl { get; set; }
        public int SellerId { get; set; } // ✅ Add this
    }
}
