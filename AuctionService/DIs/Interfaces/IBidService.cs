using AuctionService.DTOs;

namespace AuctionService.DIs.Interfaces
{
    public interface IBidService
    {
        Task<BidOutputDto> PlaceBidAsync(int userId, BidCreateDto dto);
        Task<List<BidOutputDto>> GetAuctionBidsAsync(int auctionId);
        Task<List<BidOutputDto>> GetUserBidsAsync(int userId);
    }
}
