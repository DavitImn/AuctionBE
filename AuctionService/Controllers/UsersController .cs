using System.Security.Claims;
using AuctionService.DIs.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public UsersController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [Authorize]
        [HttpGet("user-profile")]
        public async Task<IActionResult> GetProfile()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim)) return Unauthorized();


            int loginedUser = int.Parse(idClaim);
            var auctions = await _accountService.GetUserProfileAsync(loginedUser);
            return Ok(auctions);
        }

    }
}
