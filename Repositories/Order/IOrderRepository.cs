using LampStoreProjects.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderModel>> GetAllAsync();
        Task<IEnumerable<OrderModel>> GetByUserIdAsync(string userId);
        Task<OrderModel?> GetByIdAsync(Guid id);
        Task<OrderModel> CreateOrderAsync(OrderModel orderModel);
        Task UpdateStatusAsync(Guid id, string status);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<OrderModel>> GetByGuestTokenAsync(string guestToken);
        Task<int> ClaimGuestOrdersAsync(string guestToken, string userId);
    }
}