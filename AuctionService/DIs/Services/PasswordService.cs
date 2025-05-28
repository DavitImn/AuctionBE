using System.ComponentModel.DataAnnotations;
using AuctionService.DataContextDb;
using AuctionService.DIs.Interfaces;
using AuctionService.Models.Requests;
using AuctionService.Validators;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.DIs.Services
{
    public class PasswordService : IPasswordService
    {

        private readonly DataContext _context;
        private readonly IEmailSenderService _emailSender;

        public PasswordService(DataContext context, IEmailSenderService emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        public async Task<string> ForgotPasswordAsync(ForgotPasswordRequest forghotpassword)
        {
            int random = new Random().Next(1000, 9999);

            var user = _context.Users.FirstOrDefault(x => x.Email == forghotpassword.Email);
            if (user == null) throw new Exception("User Was Not Found");
            user.VerifyCode = random;

            _context.Users.Update(user);
            _context.SaveChanges();

            var verifyLink = random;
            var body = $@"
            <div style='font-family: Arial, sans-serif; font-size: 14px; color: #333;'>
                <p>Hi <strong>{user.FirstName}</strong>,</p>
                <p> Please use the following code to Reset your Password:</p>
                <h2 style='color: #2c3e50;'>{random}</h2>
                <p>If you did not trying to reset you password for , auction.ge
                , please ignore this message.</p>
                <br/>
                <p>Best regards,<br/>YourApp Team</p>
            </div>";
            _emailSender.SendEmailToVerify(user.Email, "Email Verification", body);


            return "Reset Code Was Sent Succesffuly ";

        }


        public async Task<string> ResetPasswordAsync(ResetPasswordRequest resetPassword)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == resetPassword.Email);

            var validator = new PasswordResetValidator();
            var validationResult = validator.Validate(resetPassword);

            if (user == null || user.VerifyCode != resetPassword.VerifyCode)
            {
                throw new ValidationException("User Not Found Or Code Did't Matched");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetPassword.NewPassword);


            _context.Users.Update(user);
            _context.SaveChanges();

            return null;
        }
    }
}
