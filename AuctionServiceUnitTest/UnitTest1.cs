using AuctionService.DataContextDb;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.Enums;
using AuctionService.Helpers;
using AuctionService.SignalRFolder;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.AspNetCore.SignalR;

namespace AuctionServiceUnitTest
{
    public class UnitTest1
    {
        private readonly IMapper _mapper;
        public UnitTest1()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = config.CreateMapper();

            var mockHubContext = new Mock<IHubContext<AuctionHub>>();

            // (Optional) If your BidService calls SendAsync on Clients.Group(...)
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();

            mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
            mockClientProxy.Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                           .Returns(Task.CompletedTask);
            mockHubContext.Setup(c => c.Clients).Returns(mockClients.Object);
        }



        [Fact]
        public async Task PlaceBidAsync_ShouldReturnMappedBidOutputDto()
        {

           
            var mapper = new Mock<IMapper>();

            var mockHubContext = new Mock<IHubContext<AuctionHub>>();
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();

            mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
            mockClientProxy.Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                           .Returns(Task.CompletedTask);
            mockHubContext.Setup(c => c.Clients).Returns(mockClients.Object);

            // Arrange
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new DataContext(options);

            var seller = new User
            {
                Id = 1,
                FirstName = "Seller",
                LastName = "Givi",
                UserName = "sellerUser",
                Email = "seller@example.com",
                PasswordHash = "hash",
                Role = Roles.Customer,
                IsEmailConfirmed = true
            };

            var bidder = new User
            {
                Id = 2,
                FirstName = "Bidder",
                LastName = "Givi",
                UserName = "bidderUser",
                Email = "bidder@example.com",
                PasswordHash = "hash",
                Role = Roles.Customer,
                IsEmailConfirmed = true
            };

            var category = new Category
            {
                Id = 1,
                Name = "Electronics"
            };

            var item = new Item
            {
                Id = 1,
                Title = "Test Item",
                Description = "A description for testing",
                Condition = "New",
                ImageUrls = new List<string> { "http://example.com/image1.jpg" },
                CategoryId = category.Id,
                Category = category,
                SellerId = seller.Id,
                Seller = seller,
                CreatedAt = DateTime.UtcNow
            };
            category.Items.Add(item);

            var auction = new Auction
            {
                Id = 1,
                Status = AuctionStatus.Active,
                CurrentPrice = 100,
                MinimumBidIncrement = 10,
                Item = item,
                Bids = new List<Bid>()
            };

            // Add a previous winning bid to simulate existing bids
            var previousWinningBid = new Bid
            {
                Id = 1,
                AuctionId = auction.Id,
                BidderId = seller.Id, // Could be seller or any user
                Amount = 100,
                BidTime = DateTime.UtcNow.AddMinutes(-10),
                IsWinning = true,
                Auction = auction
            };

            auction.Bids.Add(previousWinningBid);

            item.Auction = auction;

            context.Users.AddRange(seller, bidder);
            context.Categories.Add(category);
            context.Items.Add(item);
            context.Auctions.Add(auction);
            await context.SaveChangesAsync();

            var service = new BidService(context, _mapper, mockHubContext.Object);

            var dto = new BidCreateDto
            {
                AuctionId = auction.Id,
                Amount = 120
            };

            // Act
            var result = await service.PlaceBidAsync(bidder.Id, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(120, result.Amount);
            Assert.Equal("Bidder", result.BidderName);
        }


    }
}