using AuctionService.Entities;
using AuctionService.Models.Requests;
using FluentValidation;

namespace AuctionService.Validators
{
    public class UserValidator : AbstractValidator<RegisterRequestModel>
    {
        public UserValidator()
        {
            RuleFor(u => u.UserName)
                .NotEmpty().WithMessage("Username is required.")
                .MinimumLength(6).WithMessage("Username must be at least 6 characters.");

            RuleFor(u => u.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

            RuleFor(u => u.FirstName)
                .NotEmpty().WithMessage("First name is required.");

            RuleFor(u => u.LastName)
                .NotEmpty().WithMessage("Last name is required.");

            RuleFor(u => u.Email)
               .NotEmpty().WithMessage("Email is required.")
               .EmailAddress().WithMessage("Invalid email address format.");
        }
    }
}
