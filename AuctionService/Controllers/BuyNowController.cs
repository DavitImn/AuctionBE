using AuctionService.DIs.Interfaces;
using System.Security.Claims;
using AuctionService.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AuctionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuyNowController : ControllerBase
    {
        private readonly IBuyNowService _buyNowService;

        public BuyNowController(IBuyNowService buyNowService)
        {
            _buyNowService = buyNowService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> BuyNow([FromBody] BuyNowCreateDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _buyNowService.CreateBuyNowAsync(userId, dto);
                return Ok(result);
            }
            catch (ValidationException vex)
            {
                return BadRequest(new { error = vex.Message });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("my-purchases")]
        public async Task<IActionResult> GetMyBuyNowPurchases()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var purchases = await _buyNowService.GetUserBuyNowPurchasesAsync(userId);
            return Ok(purchases);
        }
    }
}
