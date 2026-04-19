using LampStoreProjects.Models;

namespace LampStoreProjects.Repositories
{
    public interface IProductReviewRepository
    {
        Task<IEnumerable<ProductReviewModel>> GetByProductIdAsync(Guid productId);
        Task<ProductReviewModel?> AddAsync(string userId, ProductReviewModel model);
        Task<bool> HasPurchasedProductAsync(string userId, Guid productId);
        Task<bool> HasReviewedAsync(string userId, Guid productId);
    }
}
