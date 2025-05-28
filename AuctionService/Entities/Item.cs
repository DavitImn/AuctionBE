namespace AuctionService.Entities
{
    public class Item
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public int CategoryId { get; set; } // FK
        public Category Category { get; set; }
        public string Condition { get; set; }
        public int SellerId { get; set; }
        public User Seller { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Auction? Auction { get; set; }
    }
}
