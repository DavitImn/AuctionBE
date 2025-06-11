using AuctionService.DTOs;

namespace AuctionService.DIs.Interfaces
{
    public interface IBuyNowService
    {
        Task<BuyNowOutputDto> CreateBuyNowAsync(int buyerId, BuyNowCreateDto dto);
        Task<List<BuyNowOutputDto>> GetUserBuyNowPurchasesAsync(int userId);

    }
}
