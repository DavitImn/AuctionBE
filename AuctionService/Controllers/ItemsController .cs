using AuctionService.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AuctionService.DIs.Interfaces;

namespace AuctionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {

        private readonly IItemService _itemService;

        public ItemsController(IItemService itemService)
        {
            _itemService = itemService;
        }



        [Authorize]
        [HttpPost("add-items")]
        public async Task<IActionResult> AddItem([FromForm] AddItemRequest request)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim))
                return Unauthorized("User ID claim not found.");

            var sellerId = int.Parse(idClaim);

            var itemDto = await _itemService.AddItemAsync(request, sellerId);
            return Ok(itemDto);
        }


        [Authorize]
        [HttpGet("get-items")]
        public async Task<IActionResult> GetItemsForUserAsync()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim))
                return Unauthorized("User ID claim not found.");

            var userId = int.Parse(idClaim);
            var isAdmin = User.IsInRole("Admin");

            var items = await _itemService.GetItemsForUserAsync(userId, isAdmin);
            return Ok(items);
        }
    }
}
