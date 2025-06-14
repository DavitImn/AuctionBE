using AuctionService.DTOs;

namespace AuctionService.DIs.Interfaces
{
    public interface IAuctionServices
    {
        Task<bool> CreateAuctionAsync(int userId, CreateAuctionDto dto);
        Task<List<AuctionOutputDto>> GetActiveAuctionsAsync();
        Task<List<AuctionOutputDto>> GetUserAuctionsAsync(int userId);
        Task<AuctionOutputDto> GetAuctionByIdAsync(int auctionId);
        Task<List<AuctionOutputDto>> FilterAuctionsAsync(string? category, decimal? minPrice, decimal? maxPrice, string? sortOrder);
        Task<List<AuctionOutputDto>> GetAllAuctionAsync();
        Task<List<AuctionOutputDto>> SearchAuctionsAsync(string search);
    }
}
