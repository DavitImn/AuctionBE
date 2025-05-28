using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;

namespace AuctionService.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            CreateMap<Item, ItemDto>();
            CreateMap<Item, ItemOutputDto>() // public version
           .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.Name))
           .ForMember(dest => dest.Seller, opt => opt.MapFrom(src => src.Seller));

            CreateMap<User, SellerDto>();
            CreateMap<User, SellerOutputDto>()
           .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName));

            CreateMap<Item, SellerOtherItemsDto>()
           .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => src.ImageUrls.FirstOrDefault()));

            CreateMap<Auction, AuctionOutputDto>()
           .ForMember(dest => dest.AuctionId, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
           .ForMember(dest => dest.Item, opt => opt.MapFrom(src => src.Item));

            CreateMap<Bid, BidOutputDto>()
           .ForMember(dest => dest.BidId, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.BidderName, opt => opt.MapFrom(src => src.Bidder.FirstName)); // Assuming you have a navigation property

        }

    }
}
