using AuctionService.DataContextDb;
using AuctionService.DIs.Interfaces;
using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.DIs.Services
{
    public class AccountService : IAccountService
    {

        private readonly DataContext _context;
        public AccountService(DataContext context)
        {
                _context = context;
        }
        public async Task<User> GetUserProfileAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("User not found");

            return new User
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                RegistrationDate = user.RegistrationDate
            };
        }
    }
}
