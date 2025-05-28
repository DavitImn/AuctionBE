namespace AuctionService.DIs.Interfaces
{
    public interface IEmailSenderService
    {
        public void SendEmailToVerify(string toEmail, string subject, string body);
    }
}
