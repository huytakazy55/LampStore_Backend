using LampStoreProjects.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public interface ICartItemRepository
    {
        Task<IEnumerable<CartItemModel>> GetAllAsync();
        Task<CartItemModel> GetByIdAsync(Guid id);
        Task<IEnumerable<CartItemModel>> GetByCartIdAsync(Guid cartId);
        Task AddAsync(CartItemModel CartItemModel);
        Task UpdateAsync(CartItemModel CartItemModel);
        Task DeleteAsync(Guid id);
        Task DeleteByCartIdAsync(Guid cartId);
    }
}