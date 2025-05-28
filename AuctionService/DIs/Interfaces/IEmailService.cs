namespace AuctionService.DIs.Interfaces
{
    public interface IEmailService
    {
        public void SendEmailToVerify(string toEmail, string subject, string body);
    }
}
