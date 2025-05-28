using System.Net;
using System.Net.Mail;
using AuctionService.DIs.Interfaces;

namespace AuctionService.DIs.Services
{
    public class EmailSenderService : IEmailSenderService
    {
        private readonly string _smtpSenderMail;
        private readonly string _smtpServer;
        private readonly int _port;
        private readonly string _smtpKey;

        public EmailSenderService(string smtpSenderMail, string smtpServer, int port, string smtpKey)
        {
            _smtpSenderMail = smtpSenderMail;
            _smtpServer = smtpServer;
            _port = port;
            _smtpKey = smtpKey;
        }
        public void SendEmailToVerify(string toEmail, string subject, string body)
        {
            using (var mail = new MailMessage())
            {
                mail.From = new MailAddress(_smtpSenderMail, "YourApp Support");
                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                using (var smtpClient = new SmtpClient(_smtpServer, _port))
                {
                    smtpClient.Credentials = new NetworkCredential(_smtpSenderMail, _smtpKey);
                    smtpClient.EnableSsl = true;

                    try
                    {
                        smtpClient.Send(mail);
                    }
                    catch (SmtpException ex)
                    {
                        // Ideally log this
                        throw new Exception("Failed to send verification email. Please try again later.", ex);
                    }
                }
            }
        }
    }
}
