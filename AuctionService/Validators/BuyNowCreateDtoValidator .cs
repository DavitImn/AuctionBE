using AuctionService.DTOs;
using FluentValidation;

namespace AuctionService.Validators
{
    public class BuyNowCreateDtoValidator : AbstractValidator<BuyNowCreateDto>
    {
        public BuyNowCreateDtoValidator()
        {
            RuleFor(x => x.AuctionId).GreaterThan(0).WithMessage("AuctionId is required.");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than 0.");
        }
    }
}
