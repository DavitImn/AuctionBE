using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AuctionService.DataContextDb;
using AutoMapper;
using AuctionService.DIs.Interfaces;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.Enums;
using AuctionService.Validators;
using System.ComponentModel.DataAnnotations;

public class BidService : IBidService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public BidService(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<BidOutputDto> PlaceBidAsync(int userId, BidCreateDto dto)
    {
        var validator = new BidCreateDtoValidator();
        var validationResult = validator.Validate(dto);

        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ValidationException($"Validation failed: {errors}");
        }

        // ✅ Get the auction and validate its state
        var auction = await _context.Auctions
            .Include(a => a.Item)
            .Include(a => a.Bids)
            .FirstOrDefaultAsync(a => a.Id == dto.AuctionId);

        if (auction == null || auction.Status != AuctionStatus.Active)
            throw new InvalidOperationException("Auction not found or not active.");

        // ✅ Prevent self-bidding
        if (auction.Item.SellerId == userId)
            throw new InvalidOperationException("You cannot bid on your own item.");

        // ✅ Check if the bid amount is valid
        var minBidAmount = auction.CurrentPrice + auction.MinimumBidIncrement;
        if (dto.Amount < minBidAmount)
            throw new InvalidOperationException($"Bid amount must be at least {minBidAmount}.");

        // ✅ Create the new bid
        var bid = new Bid
        {
            AuctionId = auction.Id,
            BidderId = userId,
            Amount = dto.Amount,
            BidTime = DateTime.UtcNow,
            IsWinning = true
        };

        // ✅ Update current price
        auction.CurrentPrice = bid.Amount;

        // ✅ Mark previous winning bids as not winning
        foreach (var existingBid in auction.Bids.Where(b => b.IsWinning))
        {
            existingBid.IsWinning = false;
        }

        _context.Bids.Add(bid);
        await _context.SaveChangesAsync();

        // ✅ Return the new bid as a DTO
        return _mapper.Map<BidOutputDto>(bid);
    }

    public async Task<List<BidOutputDto>> GetAuctionBidsAsync(int auctionId)
    {
        var bids = await _context.Bids
            .Include(b => b.Bidder)
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.BidTime)
            .ToListAsync();

        return _mapper.Map<List<BidOutputDto>>(bids);
    }

    public async Task<List<BidOutputDto>> GetUserBidsAsync(int userId)
    {
        var bids = await _context.Bids
            .Include(b => b.Auction)
            .Where(b => b.BidderId == userId)
            .OrderByDescending(b => b.BidTime)
            .ToListAsync();

        return _mapper.Map<List<BidOutputDto>>(bids);
    }
}
