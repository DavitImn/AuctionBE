using AuctionService.DTOs;
using FluentValidation;

namespace AuctionService.Validators
{
    public class CreateAuctionDtoValidator : AbstractValidator<CreateAuctionDto>
    {
        public CreateAuctionDtoValidator()
        {
            RuleFor(a => a.ItemId).GreaterThan(0).WithMessage("ItemId must be a positive number.");
            RuleFor(a => a.StartingPrice).GreaterThan(0).WithMessage("StartingPrice must be greater than zero.");
            RuleFor(a => a.MinimumBidIncrement).GreaterThan(0).WithMessage("MinimumBidIncrement must be greater than zero.");
            RuleFor(a => a.StartTime).LessThan(a => a.EndTime).WithMessage("StartTime must be before EndTime.");
            RuleFor(a => a.EndTime).GreaterThan(DateTime.UtcNow).WithMessage("EndTime must be in the future.");
        }
    }
}
