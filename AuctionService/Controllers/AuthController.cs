using System.Security.Claims;
using AuctionService.DIs.Interfaces;
using AuctionService.DIs.Services;
using AuctionService.DTOs;
using AuctionService.Models.Requests;
using AuctionService.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Register([FromForm] RegisterRequestModel request)
        {
            await _authService.RegisterAsync(request); // ✅ logic inside service
            return Ok("Registered");
        }




        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromQuery] string email, int verifyCode)
        {
            try
            {
                await _authService.VerifyAsync(email, verifyCode);
                return Ok(new { message = "Email verified successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var token = await _authService.LoginAsync(request, ipAddress);
                return Ok(token);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest model)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var newToken = await _authService.RefreshToken(model, ipAddress);
                return Ok(newToken);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }
    }
}
