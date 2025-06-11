using AuctionService.Entities;
using AuctionService.Models.Requests;
using AuctionService.Models.Responses;

namespace AuctionService.DIs.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterRequestModel request);
        Task VerifyAsync(string email, int verifyCode);
        Task<JwtTokenModel> LoginAsync(LoginRequestModel request, string ipAddress);
        Task<JwtTokenModel> RefreshToken(RefreshTokenRequest refreshToken, string ipAddress);
        Task<string> SaveUserImageAsync(IFormFile image);
    }
}
