using AuctionService.DataContextDb;
using AuctionService.DIs.Interfaces;
using AuctionService.Entities;
using AuctionService.Models.Requests;
using AuctionService.Models.Responses;
using Microsoft.EntityFrameworkCore;
using AuctionService.Validators;
using FluentValidation;
using System;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;

namespace AuctionService.DIs.Services
{
    public class AuthService : IAuthService
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        private readonly IEmailSenderService _emailSender;
        private readonly IWebHostEnvironment _env;

        public AuthService(DataContext context, ITokenService tokenService, IEmailSenderService emailSender, IWebHostEnvironment env)
        {
            _context = context;
            _tokenService = tokenService;
            _emailSender = emailSender;
            _env = env;
        }

        public async Task<string> SaveUserImageAsync(IFormFile image)
        {
            var uploadPath = Path.Combine(_env.WebRootPath, "user-images");

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var imageUrl = $"/user-images/{uniqueFileName}"; // relative URL to be served via wwwroot
            return imageUrl;
        }

        public async Task<string> RegisterAsync(RegisterRequestModel request)
        {

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                throw new Exception("Email already registered");

            var validator = new UserValidator();
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Validation failed: {errors}");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            int random = new Random().Next(1000, 9999);

            var imageUrl = await SaveUserImageAsync(request.UserImage);

            var user = new User
            {
                Email = request.Email,
                UserName = request.UserName,
                PasswordHash = hashedPassword,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = Enums.Roles.Customer,
                IsEmailConfirmed = false,
                VerifyCode = random,
                UserImageUrl = imageUrl
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var verifyLink = random;
            var body = $@"
            <div style='font-family: Arial, sans-serif; font-size: 14px; color: #333;'>
                <p>Hi <strong>{user.FirstName}</strong>,</p>
                <p>Thank you for registering. Please use the following code to verify your email address:</p>
                <h2 style='color: #2c3e50;'>{random}</h2>
                <p>If you did not register for this account, please ignore this message.</p>
                <br/>
                <p>Best regards,<br/>YourApp Team</p>
            </div>";
            _emailSender.SendEmailToVerify(user.Email, "Email Verification", body);

            return "Registration successful. Please verify your email.";
        }
        public async Task VerifyAsync(string email, int verifyCode)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) throw new Exception("User not found");
            if (user.VerifyCode != verifyCode) throw new Exception("Verify Code Do Not Match");

            user.IsEmailConfirmed = true;
            user.VerifyCode = 0000;
            await _context.SaveChangesAsync();
        }

        public async Task<JwtTokenModel> LoginAsync(LoginRequestModel request, string ipAddress)
        {
            var user = _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefault(u => u.UserName == request.UserName);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new Exception("Invalid credentials");

            if (!user.IsEmailConfirmed)
                throw new Exception("Email not verified");

            var refreshToken = _tokenService.GenerateRefreshToken(ipAddress);
            user.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            var jwt = _tokenService.GenerateToken(user, ipAddress);

            return new JwtTokenModel
            {
                AccessToken = jwt.AccessToken,
                RefreshToken = refreshToken.Token,
                Expiration = jwt.Expiration
            };
        }

        [Authorize]
        public async Task<JwtTokenModel> RefreshToken(RefreshTokenRequest token, string ipAddress)
        {

            var blabla = _context.jwtRefreshTokens
    .Include(rt => rt.User).AsEnumerable();
            var refreshToken = blabla
                .FirstOrDefault(rt => rt.Token == token.RefreshToken && rt.IsActive);

            if (refreshToken == null)
                throw new Exception("Invalid or expired refresh token");

            var user = refreshToken.User;

            // ✅ If IP address matches and token is still valid, reuse the same refresh token
            if (refreshToken.CreatedByIp == ipAddress && refreshToken.Expires > DateTime.UtcNow)
            {
                var jwt = _tokenService.GenerateToken(user, ipAddress);

                return new JwtTokenModel
                {
                    AccessToken = jwt.AccessToken,
                    RefreshToken = refreshToken.Token, // Reuse
                    Expiration = jwt.Expiration
                };
            }

            // ✅ Otherwise: revoke old token and issue a new one (different IP or token near expiry)
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;

            var newRefreshToken = _tokenService.GenerateRefreshToken(ipAddress);
            user.RefreshTokens.Add(newRefreshToken);

            var jwtNew = _tokenService.GenerateToken(user, ipAddress);
            await _context.SaveChangesAsync();

            return new JwtTokenModel
            {
                AccessToken = jwtNew.AccessToken,
                RefreshToken = newRefreshToken.Token,
                Expiration = jwtNew.Expiration
            };
        }

        
    }
}
