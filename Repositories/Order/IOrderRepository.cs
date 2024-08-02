using LampStoreProjects.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderModel>> GetAllAsync();
        Task<OrderModel> GetByIdAsync(int id);
        Task AddAsync(OrderModel OrderModel);
        Task UpdateAsync(OrderModel OrderModel);
        Task DeleteAsync(int id);
    }
}