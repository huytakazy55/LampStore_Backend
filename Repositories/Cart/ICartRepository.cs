using LampStoreProjects.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public interface ICartRepository
    {
        Task<IEnumerable<CartModel>> GetAllAsync();
        Task<CartModel> GetByIdAsync(Guid id);
        Task AddAsync(CartModel CartModel);
        Task UpdateAsync(CartModel CartModel);
        Task DeleteAsync(Guid id);
    }
}