using AuctionService.DataContextDb;
using AuctionService.DIs.Interfaces;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.DIs.Services
{
    public class BuyNowService : IBuyNowService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public BuyNowService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<BuyNowOutputDto> CreateBuyNowAsync(int buyerId, BuyNowCreateDto dto)
        {
            var auction = await _context.Auctions
                .Include(a => a.Item)
                    .ThenInclude(i => i.Seller) // 💡 Ensure Seller is included for validation
                .Include(a => a.Bids)
                .FirstOrDefaultAsync(a => a.Id == dto.AuctionId);

            Console.WriteLine("123");

            if (auction == null)
                throw new Exception("Auction not found.");

            if (auction.Status != AuctionStatus.Active)
                throw new Exception("Auction is not active.");

            // 🛑 You must validate BEFORE checking existingBuyNow
            if (auction.Item?.Seller?.Id == buyerId)
                throw new InvalidOperationException("You cannot buy your own item.");

            var existingBuyNow = await _context.BuyNows
                .FirstOrDefaultAsync(bn => bn.AuctionId == dto.AuctionId);

            if (existingBuyNow != null)
                throw new Exception("This item has already been bought.");

            // ✅ Calculate BuyNow price
            var highestBid = auction.Bids.OrderByDescending(b => b.Amount).FirstOrDefault()?.Amount ?? auction.StartingPrice;
            var calculatedBuyNowPrice = highestBid + (highestBid * 0.5m);

            if (dto.Price < calculatedBuyNowPrice)
                throw new Exception($"Price too low. Buy Now price is {calculatedBuyNowPrice}.");

            var buyNow = new BuyNow
            {
                AuctionId = dto.AuctionId,
                BuyerId = buyerId,
                Price = calculatedBuyNowPrice,
                PurchaseTime = DateTime.UtcNow,
            };

            // Update auction status and winner
            auction.Status = AuctionStatus.Closed;
            auction.WinnerId = buyerId;

            auction.CurrentPrice = buyNow.Price;

            Console.WriteLine("rame");

            _context.BuyNows.Add(buyNow);
            _context.Auctions.Update(auction);

            var pricecheck = auction.CurrentPrice;

            Console.WriteLine("asd");
            await _context.SaveChangesAsync();
            var fullBuyNow = await _context.BuyNows
    .Include(b => b.Buyer)
    .FirstOrDefaultAsync(b => b.Id == buyNow.Id);

            return _mapper.Map<BuyNowOutputDto>(fullBuyNow);
        }

        public async Task<List<BuyNowOutputDto>> GetUserBuyNowPurchasesAsync(int userId)
        {
            var purchases = await _context.BuyNows
                .Include(bn => bn.Auction)
                    .ThenInclude(a => a.Item)
                .Where(bn => bn.BuyerId == userId)
                .ToListAsync();

            return purchases.Select(p => _mapper.Map<BuyNowOutputDto>(p)).ToList();
        }
    }
}
