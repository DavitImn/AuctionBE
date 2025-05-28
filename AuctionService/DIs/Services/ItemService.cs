using AuctionService.DataContextDb;
using AuctionService.DIs.Interfaces;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.Enums;
using AuctionService.Models.Requests;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.DIs.Services
{
    public class ItemService : IItemService
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _mapper;

        public ItemService(DataContext context, IWebHostEnvironment env, IMapper mapper)
        {
            _context = context;
            _env = env;
            _mapper = mapper;
        }
        private async Task<List<string>> SaveImagesAsync(List<IFormFile> images)
        {
            var savedImageUrls = new List<string>();
            var uploadPath = Path.Combine(_env.WebRootPath, "uploads");

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            foreach (var image in images)
            {
                if (image.Length > 0)
                {
                    var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                    var filePath = Path.Combine(uploadPath, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    var baseUrl = $"{_env.WebRootFileProvider.GetFileInfo("").PhysicalPath}";
                    var imageUrl = $"{baseUrl}/uploads/{uniqueFileName}";

                    savedImageUrls.Add(imageUrl);
                }
            }
            return savedImageUrls;
        }

        public async Task<ItemDto> AddItemAsync(AddItemRequest request, int sellerId)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId);
            if (category == null)
                throw new Exception("Category not found.");

            var imageUrls = await SaveImagesAsync(request.Images);

            var item = new Item
            {
                Title = request.Title,
                Description = request.Description,
                ImageUrls = imageUrls,
                CategoryId = request.CategoryId,
                Condition = request.Condition,
                SellerId = sellerId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            var itemDto = _mapper.Map<ItemDto>(item);
            itemDto.Category = category.Name;

            return itemDto;
        }
        public async Task<List<ItemOutputDto>> GetItemsForUserAsync(int userId, bool isAdmin)
        {
            var query = _context.Items
                .Include(i => i.Category)
                .Include(i => i.Seller)
                .AsQueryable();

            if (!isAdmin)
                query = query.Where(i => i.SellerId == userId);

            var items = await query.ToListAsync();
            var outputDtos = _mapper.Map<List<ItemOutputDto>>(items);

            var sellerIds = items.Select(i => i.SellerId).Distinct().ToList();
            var otherItems = await _context.Items
                .Where(x => sellerIds.Contains(x.SellerId))
                .Where(x => !items.Select(i => i.Id).Contains(x.Id))
                .ToListAsync();

            var otherItemDtos = _mapper.Map<List<SellerOtherItemsDto>>(otherItems);

            foreach (var outputDto in outputDtos)
            {
                var sellerId = items.First(i => i.Title == outputDto.Title)?.SellerId ?? 0;
                outputDto.SellerOtherItems = otherItemDtos
                    .Where(o => o.SellerId == sellerId)
                    .ToList();
            }

            return outputDtos;
        }

    }
}
