namespace AuctionService.Models.Requests
{
    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public int VerifyCode { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
