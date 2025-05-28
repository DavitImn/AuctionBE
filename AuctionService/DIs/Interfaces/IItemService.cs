using AuctionService.DTOs;
using AuctionService.Models.Requests;

namespace AuctionService.DIs.Interfaces
{
    public interface IItemService
    {
        Task<ItemDto> AddItemAsync(AddItemRequest request, int sellerId);
        Task<List<ItemOutputDto>> GetItemsForUserAsync(int userId, bool isAdmin);
    }
}
