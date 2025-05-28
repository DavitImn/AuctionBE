using AuctionService.Models.Requests;

namespace AuctionService.DIs.Interfaces
{
    public interface IPasswordService
    {
        Task<string> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<string> ResetPasswordAsync(ResetPasswordRequest request);
    }
}
