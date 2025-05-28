using AuctionService.DTOs;
using FluentValidation;

namespace AuctionService.Validators
{
    public class BidCreateDtoValidator : AbstractValidator<BidCreateDto>
    {
        public BidCreateDtoValidator()
        {
            RuleFor(b => b.AuctionId).GreaterThan(0).WithMessage("AuctionId must be a positive number.");
            RuleFor(b => b.Amount).GreaterThan(0).WithMessage("Bid amount must be greater than zero.");
        }
    }
}
