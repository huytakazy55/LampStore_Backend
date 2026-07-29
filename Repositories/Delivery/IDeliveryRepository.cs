using LampStoreProjects.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public interface IDeliveryRepository
    {
        Task<IEnumerable<DeliveryModel>> GetAllAsync(int page = 1, int pageSize = 20, string? search = null);
        Task<int> CountAsync(string? search = null);
        Task<DeliveryModel> GetByIdAsync(Guid id);
        Task AddAsync(DeliveryModel DeliveryModel);
        Task UpdateAsync(DeliveryModel DeliveryModel);
        Task DeleteAsync(Guid id);
    }
}
