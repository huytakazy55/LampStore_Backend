using LampStoreProjects.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public interface IDeliveryRepository
    {
        Task<IEnumerable<DeliveryModel>> GetAllAsync();
        Task<DeliveryModel> GetByIdAsync(Guid id);
        Task AddAsync(DeliveryModel DeliveryModel);
        Task UpdateAsync(DeliveryModel DeliveryModel);
        Task DeleteAsync(Guid id);
    }
}