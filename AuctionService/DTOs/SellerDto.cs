namespace AuctionService.DTOs
{
    public class SellerDto
    {
        public int Id { get; set; } // for joins, matching seller
        public string FirstName { get; set; }
    }
}
