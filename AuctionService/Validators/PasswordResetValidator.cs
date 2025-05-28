using AuctionService.Models.Requests;
using FluentValidation;

namespace AuctionService.Validators
{
    public class PasswordResetValidator : AbstractValidator<ResetPasswordRequest>
    {
        public PasswordResetValidator()
        {
            RuleFor(u => u.NewPassword)
        .NotEmpty().WithMessage("Password is required.")
        .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
        .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
        .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
        .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

            RuleFor(u => u.ConfirmPassword)
                .Equal(u => u.NewPassword).WithMessage("Passwords do not match.");
        }
    }
}
