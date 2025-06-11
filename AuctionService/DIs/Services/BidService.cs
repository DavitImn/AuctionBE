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
using AuctionService.SignalRFolder;
using Microsoft.AspNetCore.SignalR;

public class BidService : IBidService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IHubContext<AuctionHub> _hubContext;

    public BidService(DataContext context, IMapper mapper, IHubContext<AuctionHub> hubContext)
    {
        _context = context;
        _mapper = mapper;
        _hubContext = hubContext;
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

        var auction = await _context.Auctions
            .Include(a => a.Item)
            .Include(a => a.Bids.OrderByDescending(b => b.BidTime)) // Order bids by time
            .FirstOrDefaultAsync(a => a.Id == dto.AuctionId);

        if (auction == null || auction.Status != AuctionStatus.Active)
            throw new InvalidOperationException("Auction not found or not active.");

        if (auction.Item.SellerId == userId)
            throw new InvalidOperationException("You cannot bid on your own item.");

        var minBidAmount = auction.CurrentPrice + auction.MinimumBidIncrement;
        if (dto.Amount < minBidAmount)
            throw new InvalidOperationException($"Bid amount must be at least {minBidAmount}.");

        // 🔴 NEW: Prevent user from bidding again if they were the last bidder
        var lastBid = auction.Bids.FirstOrDefault();
        if (lastBid != null && lastBid.BidderId == userId)
            throw new InvalidOperationException("You are already the highest bidder.");

        // ✅ Create the new bid
        var bid = new Bid
        {
            AuctionId = auction.Id,
            BidderId = userId,
            Amount = dto.Amount,
            BidTime = DateTime.UtcNow,
            IsWinning = true
        };

        auction.CurrentPrice = bid.Amount;

        foreach (var existingBid in auction.Bids.Where(b => b.IsWinning))
        {
            existingBid.IsWinning = false;
        }

        _context.Bids.Add(bid);

        await _context.SaveChangesAsync();

        // ✅ Notify all clients in that auction group
        Console.WriteLine($"[DEBUG] Broadcasting ReceiveNewBid for auction-{auction.Id} → amount={bid.Amount}, bidder={bid.BidderId}");
        await _hubContext.Clients
            .Group($"auction-{auction.Id}")
            .SendAsync("ReceiveNewBid", new
            {
                AuctionId = auction.Id,
                Amount = bid.Amount,
                BidderId = bid.BidderId,
                BidTime = bid.BidTime
            });

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
