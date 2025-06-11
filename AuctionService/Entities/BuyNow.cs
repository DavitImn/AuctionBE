namespace AuctionService.Entities
{
    public class BuyNow
    {
        public int Id { get; set; }

        public int AuctionId { get; set; }
        public Auction Auction { get; set; }

        public int BuyerId { get; set; }
        public User Buyer { get; set; }

        public decimal Price { get; set; }
        public DateTime PurchaseTime { get; set; } = DateTime.UtcNow;


    }
}
