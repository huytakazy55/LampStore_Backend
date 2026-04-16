using LampStoreProjects.Models;

namespace LampStoreProjects.Repositories
{
    public interface IWishlistRepository
    {
        Task<IEnumerable<WishlistItemModel>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Guid>> GetWishlistProductIdsAsync(string userId);
        Task<bool> AddAsync(string userId, Guid productId);
        Task<bool> RemoveAsync(string userId, Guid productId);
        Task<bool> IsInWishlistAsync(string userId, Guid productId);
    }
}
