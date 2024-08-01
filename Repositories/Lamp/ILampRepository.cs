using LampStoreProjects.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public interface ILampRepository
    {
        Task<IEnumerable<LampModel>> GetAllLampsAsync();
        Task<LampModel> GetLampByIdAsync(int id);
        Task<LampModel> AddLampAsync(LampModel lampModel);
        Task<LampModel> UpdateLampAsync(LampModel lampModel);
        Task DeleteLampAsync(int id);
    }
}