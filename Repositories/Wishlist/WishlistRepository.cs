using LampStoreProjects.Data;
using LampStoreProjects.Helpers;
using LampStoreProjects.Models;
using Microsoft.EntityFrameworkCore;

namespace LampStoreProjects.Repositories
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly ApplicationDbContext _context;

        public WishlistRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WishlistItemModel>> GetByUserIdAsync(string userId)
        {
            return await _context.WishlistItems!
                .Where(w => w.UserId == userId)
                .Include(w => w.Product)
                    .ThenInclude(p => p!.Images)
                .Include(w => w.Product)
                    .ThenInclude(p => p!.ProductVariant)
                .Include(w => w.Product)
                    .ThenInclude(p => p!.Category)
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => new WishlistItemModel
                {
                    Id = w.Id,
                    ProductId = w.ProductId,
                    ProductName = w.Product!.Name,
                    ProductImage = w.Product.Images != null && w.Product.Images.Any()
                        ? w.Product.Images.First().ImagePath
                        : null,
                    Price = w.Product.ProductVariant != null ? w.Product.ProductVariant.Price : 0,
                    DiscountPrice = w.Product.ProductVariant != null ? w.Product.ProductVariant.DiscountPrice : null,
                    CategoryName = w.Product.Category != null ? w.Product.Category.Name : null,
                    SellCount = w.Product.SellCount,
                    ProductStatus = w.Product.Status,
                    CreatedAt = w.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<Guid>> GetWishlistProductIdsAsync(string userId)
        {
            return await _context.WishlistItems!
                .Where(w => w.UserId == userId)
                .Select(w => w.ProductId)
                .ToListAsync();
        }

        public async Task<bool> AddAsync(string userId, Guid productId)
        {
            // Check if already exists
            var exists = await _context.WishlistItems!
                .AnyAsync(w => w.UserId == userId && w.ProductId == productId);

            if (exists) return false;

            // Check if product exists
            var productExists = await _context.Products!.AnyAsync(p => p.Id == productId);
            if (!productExists) return false;

            var wishlistItem = new WishlistItem
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTimeHelper.VietnamNow
            };

            _context.WishlistItems!.Add(wishlistItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveAsync(string userId, Guid productId)
        {
            var item = await _context.WishlistItems!
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (item == null) return false;

            _context.WishlistItems!.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsInWishlistAsync(string userId, Guid productId)
        {
            return await _context.WishlistItems!
                .AnyAsync(w => w.UserId == userId && w.ProductId == productId);
        }
    }
}
