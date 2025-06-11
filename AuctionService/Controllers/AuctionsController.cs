using AuctionService.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AuctionService.DIs.Interfaces;

namespace AuctionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionsController : ControllerBase
    {

        private readonly IAuctionServices _Auction;

        public AuctionsController(IAuctionServices auctionService)
        {
            _Auction = auctionService;
        }


        [Authorize]
        [HttpPost("auction")]
        public async Task<IActionResult> CreateAuction([FromBody] CreateAuctionDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid user ID.");

            var success = await _Auction.CreateAuctionAsync(userId, dto);
            if (!success)
                return BadRequest("Item not found, doesn't belong to you, or is already on auction.");

            return Ok("Auction created successfully.");
        }

        [AllowAnonymous]
        [HttpGet("auctions")]
        public async Task<IActionResult> GetActiveAuctions()
        {
            var auctions = await _Auction.GetActiveAuctionsAsync();
            return Ok(auctions);
        }

        [AllowAnonymous]
        [HttpGet("all-auctions")]
        public async Task<IActionResult> GetAllAuctionAsync()
        {
            var auctions = await _Auction.GetAllAuctionAsync();
            return Ok(auctions);
        }

        [Authorize]
        [HttpGet("my-auctions")]
        public async Task<IActionResult> GetMyAuctions()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim)) return Unauthorized();

            int userId = int.Parse(idClaim);
            var auctions = await _Auction.GetUserAuctionsAsync(userId);
            return Ok(auctions);
        }


        [HttpGet("get-auction")]
        public async Task<IActionResult> GetAuctionByIdAsync(int auctionid)
        {
            var auction = await _Auction.GetAuctionByIdAsync(auctionid);

            if (auction == null)
            {
                return BadRequest("a  c  d");
            }

            return Ok(auction);
        }

        [AllowAnonymous]
        [HttpGet("filter")]
        public async Task<IActionResult> FilterAuctions(
        [FromQuery] string? category,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? sortOrder)
        {
            var auctions = await _Auction.FilterAuctionsAsync(category, minPrice, maxPrice, sortOrder);
            return Ok(auctions);
        }


    }
}
