using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using AuctionService.DIs.Interfaces;
using AuctionService.DTOs;
using System.ComponentModel.DataAnnotations;

[Route("api/[controller]")]
[ApiController]
public class BidsController : ControllerBase
{
    private readonly IBidService _bidService;

    public BidsController(IBidService bidService)
    {
        _bidService = bidService;
    }

    [Authorize]
    [HttpPost("bid")]
    public async Task<IActionResult> PlaceBid([FromBody] BidCreateDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var bid = await _bidService.PlaceBidAsync(userId, dto);
            return Ok(bid);
        }
        catch (ValidationException vex)
        {
            return BadRequest(new { error = vex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("auction/{auctionId}")]
    public async Task<IActionResult> GetAuctionBids(int auctionId)
    {
        var bids = await _bidService.GetAuctionBidsAsync(auctionId);
        return Ok(bids);
    }

    [Authorize]
    [HttpGet("user")]
    public async Task<IActionResult> GetUserBids()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var bids = await _bidService.GetUserBidsAsync(userId);
        return Ok(bids);
    }
}
