namespace AuctionService.Models.Requests
{
    public class AddItemRequest
    {
       public string Title { get; set; }
       public string Description { get; set; }
       public int CategoryId { get; set; }
       public string Condition { get; set; }
       public List<IFormFile> Images { get; set; }
    }
}
