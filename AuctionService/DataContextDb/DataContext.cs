using AuctionService.Entities;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.DataContextDb
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> opt) : base(opt) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<JwtRefreshToken> jwtRefreshTokens { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ✅ Global query filter to exclude soft-deleted users
            modelBuilder.Entity<User>()
                .HasQueryFilter(u => !u.IsDeleted);

            // JSON Conversion
            modelBuilder.Entity<Item>()
                .Property(i => i.ImageUrls)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null));

            // Item → User (Seller)
            modelBuilder.Entity<Item>()
                .HasOne(i => i.Seller)
                .WithMany(u => u.ItemsForSale)
                .HasForeignKey(i => i.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Item → Auction (1:1)
            modelBuilder.Entity<Auction>()
                .HasOne(a => a.Item)
                .WithOne(i => i.Auction)
                .HasForeignKey<Auction>(a => a.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Item → Category
            modelBuilder.Entity<Item>()
                .HasOne(i => i.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Auction → Bids
            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Auction)
                .WithMany(a => a.Bids)
                .HasForeignKey(b => b.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Bid → User (Bidder)
            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Bidder)
                .WithMany(u => u.Bids)
                .HasForeignKey(b => b.BidderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Auction → User (Winner)
            modelBuilder.Entity<Auction>()
                .HasOne(a => a.Winner)
                .WithMany(u => u.WonAuctions)
                .HasForeignKey(a => a.WinnerId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
