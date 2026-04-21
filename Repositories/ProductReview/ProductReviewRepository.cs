using LampStoreProjects.Data;
using LampStoreProjects.Helpers;
using LampStoreProjects.Models;
using Microsoft.EntityFrameworkCore;

namespace LampStoreProjects.Repositories
{
    public class ProductReviewRepository : IProductReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductReviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductReviewModel>> GetByProductIdAsync(Guid productId)
        {
            return await _context.ProductReviews!
                .Where(r => r.ProductId == productId)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreateAt)
                .Select(r => new ProductReviewModel
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    UserId = r.UserId,
                    UserName = r.User != null ? r.User.UserName ?? "Ẩn danh" : "Ẩn danh",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreateAt = r.CreateAt
                })
                .ToListAsync();
        }

        public async Task<ProductReviewModel?> AddAsync(string userId, ProductReviewModel model)
        {
            // Check product exists
            var productExists = await _context.Products!.AnyAsync(p => p.Id == model.ProductId);
            if (!productExists) return null;

            var review = new ProductReview
            {
                Id = Guid.NewGuid(),
                ProductId = model.ProductId,
                UserId = userId,
                Rating = model.Rating,
                Comment = model.Comment,
                CreateAt = DateTimeHelper.VietnamNow
            };

            _context.ProductReviews!.Add(review);
            await _context.SaveChangesAsync();

            // Get user name for response
            var user = await _context.Users.FindAsync(userId);

            return new ProductReviewModel
            {
                Id = review.Id,
                ProductId = review.ProductId,
                UserId = review.UserId,
                UserName = user?.UserName ?? "Ẩn danh",
                Rating = review.Rating,
                Comment = review.Comment,
                CreateAt = review.CreateAt
            };
        }

        public async Task<bool> HasPurchasedProductAsync(string userId, Guid productId)
        {
            return await _context.Orders!
                .Where(o => o.UserId == userId)
                .AnyAsync(o => o.OrderItems!.Any(oi => oi.ProductId == productId));
        }

        public async Task<bool> HasReviewedAsync(string userId, Guid productId)
        {
            return await _context.ProductReviews!
                .AnyAsync(r => r.UserId == userId && r.ProductId == productId);
        }
    }
}
