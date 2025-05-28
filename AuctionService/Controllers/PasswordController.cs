using System.ComponentModel.DataAnnotations;
using AuctionService.DIs.Interfaces;
using AuctionService.Models.Requests;
using AuctionService.Validators;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        private readonly IPasswordService _passwordService;
        public PasswordController(IPasswordService passwordService)
        {
            _passwordService = passwordService;
        }
        // ✅ New: Forgot Password Endpoint
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                var message = await _passwordService.ForgotPasswordAsync(request);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ✅ New: Reset Password Endpoint
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var validator = new PasswordResetValidator();
                var validationResult = validator.Validate(request);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
                }

                await _passwordService.ResetPasswordAsync(request);
                return Ok(new { message = "Password has been successfully reset." });
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
    }
}
