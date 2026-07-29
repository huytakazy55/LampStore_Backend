using LampStoreProjects.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderModel>> GetAllAsync(int page = 1, int pageSize = 50);
        Task<int> CountAsync();
        Task<OrderStatsModel> GetStatsAsync();
        Task<IEnumerable<OrderModel>> GetByUserIdAsync(string userId);
        Task<OrderModel?> GetByIdAsync(Guid id);
        Task<OrderCreationResult> CreateOrderAsync(OrderModel orderModel);
        Task UpdateStatusAsync(Guid id, string status);
        Task UpdatePaymentStatusAsync(Guid id, string paymentStatus);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<OrderModel>> GetByGuestTokenAsync(string guestToken);
        Task<int> ClaimGuestOrdersAsync(string guestToken, string userId);
    }
}
