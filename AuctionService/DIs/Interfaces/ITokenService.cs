using AuctionService.Entities;
using AuctionService.Models.Responses;
using System.Security.Claims;

namespace AuctionService.DIs.Interfaces
{
    public interface ITokenService
    {
        JwtTokenModel GenerateToken(User user, string ipAddress);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

        JwtRefreshToken GenerateRefreshToken(string ipAddress);
        Task<JwtRefreshToken?> GetValidRefreshTokenAsync(string token);
        Task<bool> RevokeRefreshTokenAsync(string token, string ipAddress);
    }
}
