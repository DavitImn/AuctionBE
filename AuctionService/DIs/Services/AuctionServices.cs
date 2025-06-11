using System.ComponentModel.DataAnnotations;
using AuctionService.DataContextDb;
using AuctionService.DIs.Interfaces;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.Enums;
using AuctionService.Helpers;
using AuctionService.Validators;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.DIs.Services
{
    public class AuctionServices : IAuctionServices
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _mapper;

        public AuctionServices(DataContext context, IWebHostEnvironment env, IMapper mapper)
        {
            _context = context;
            _env = env;
            _mapper = mapper;
        }
        public async Task<bool> CreateAuctionAsync(int userId, CreateAuctionDto dto)
        {
            // ✅ Validate the DTO using FluentValidation
            var validator = new CreateAuctionDtoValidator();
            var validationResult = validator.Validate(dto);

            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Validation failed: {errors}");
            }

            // ✅ Check if the item exists and is owned by the user
            var item = await _context.Items
                .Include(i => i.Auction)
                .FirstOrDefaultAsync(i => i.Id == dto.ItemId && i.SellerId == userId);

            if (item == null)
                throw new UnauthorizedAccessException("You are not the owner of this item.");

            if (item.Auction != null)
                throw new InvalidOperationException("This item is already on auction.");

            // ✅ Create the auction
            var auction = new Auction
            {
                ItemId = item.Id,
                StartingPrice = dto.StartingPrice,
                CurrentPrice = dto.StartingPrice,
                MinimumBidIncrement = dto.MinimumBidIncrement,
                StartTime = dto.StartTime <= DateTime.Now ? DateTime.Now : dto.StartTime,
                EndTime = dto.EndTime,
                Status = dto.StartTime <= DateTime.Now ? AuctionStatus.Active : AuctionStatus.Pending
            };

            _context.Auctions.Add(auction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AuctionOutputDto>> GetActiveAuctionsAsync()
        {
            var auctions = await _context.Auctions
                .Include(a => a.Item)
                    .ThenInclude(i => i.Category)
                .Include(a => a.Item.Seller)
                .Where(a => a.Status == AuctionStatus.Active)
                .ToListAsync();

            return _mapper.Map<List<AuctionOutputDto>>(auctions);
        }
        public async Task<List<AuctionOutputDto>> GetAllAuctionAsync()
        {
            var auctions = await _context.Auctions
                .Include(a => a.Item)
                    .ThenInclude(i => i.Category)
                .Include(a => a.Item.Seller)
                .Where(a => a.IsDeleted != true)
                .ToListAsync();

            return _mapper.Map<List<AuctionOutputDto>>(auctions);
        }
        public async Task<List<AuctionOutputDto>> GetUserAuctionsAsync(int userId)
        {
            var auctions = await _context.Auctions
                .Include(a => a.Item)
                    .ThenInclude(i => i.Category)
                .Include(a => a.Item.Seller)
                .Where(a => a.Item.SellerId == userId)
                .ToListAsync();

            return _mapper.Map<List<AuctionOutputDto>>(auctions);
        }

        public async Task<AuctionOutputDto> GetAuctionByIdAsync(int auctionId)
        {
            var auction = await _context.Auctions
           .Include(a => a.Item)
           .ThenInclude(i => i.Category)
           .Include(a => a.Item.Seller)
           .FirstOrDefaultAsync(a => a.Id == auctionId);

            return _mapper.Map<AuctionOutputDto>(auction);
        }

        public async Task<List<AuctionOutputDto>> FilterAuctionsAsync(string? category, decimal? minPrice, decimal? maxPrice, string? sortOrder)
        {
            var query = _context.Auctions
                .Include(a => a.Item)
                    .ThenInclude(i => i.Category)
                .Include(a => a.Item.Seller)
                .Where(a => a.Status == AuctionStatus.Active)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(a => a.Item.Category.Name.ToLower() == category.ToLower());
            }

            if (minPrice.HasValue)
            {
                query = query.Where(a => a.CurrentPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(a => a.CurrentPrice <= maxPrice.Value);
            }

            if (sortOrder?.ToLower() == "asc")
            {
                query = query.OrderBy(a => a.CurrentPrice);
            }
            else if (sortOrder?.ToLower() == "desc")
            {
                query = query.OrderByDescending(a => a.CurrentPrice);
            }

            var result = await query.ToListAsync();
            return _mapper.Map<List<AuctionOutputDto>>(result);
        }

    }


}
