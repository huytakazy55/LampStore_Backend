using LampStoreProjects.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderModel>> GetAllAsync();
        Task<OrderModel> GetByIdAsync(Guid id);
        Task AddAsync(OrderModel OrderModel);
        Task UpdateAsync(OrderModel OrderModel);
        Task DeleteAsync(Guid id);
    }
}