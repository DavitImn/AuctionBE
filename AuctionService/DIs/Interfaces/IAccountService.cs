using AuctionService.Entities;

namespace AuctionService.DIs.Interfaces
{
    public interface IAccountService
    {
        Task<User> GetUserProfileAsync(int userId);
    }
}
