using LampStoreProjects.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public interface IOrderItemRepository
    {
        Task<IEnumerable<OrderItemModel>> GetAllAsync();
        Task<OrderItemModel> GetByIdAsync(Guid id);
        Task AddAsync(OrderItemModel OrderItemModel);
        Task UpdateAsync(OrderItemModel OrderItemModel);
        Task DeleteAsync(Guid id);
    }
}