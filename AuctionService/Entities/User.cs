using System.Security.Cryptography;
using AuctionService.Enums;

namespace AuctionService.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsEmailConfirmed { get; set; } = false;
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public Roles Role { get; set; }
        public bool IsDeleted { get; set; }
        public int VerifyCode { get; set; }
        public ICollection<JwtRefreshToken> RefreshTokens { get; set; } = new List<JwtRefreshToken>();
        public List<Item> ItemsForSale { get; set; } = new();
        public List<Bid> Bids { get; set; } = new();
        public List<Auction> WonAuctions { get; set; } = new();
    }
}
