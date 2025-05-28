namespace AuctionService.DTOs
{
    public class ItemOutputDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> ImageUrls { get; set; }
        public string Category { get; set; }
        public string Condition { get; set; }
        public DateTime CreatedAt { get; set; }
        public SellerOutputDto Seller { get; set; }
        public List<SellerOtherItemsDto> SellerOtherItems { get; set; } = new();
    }
}
