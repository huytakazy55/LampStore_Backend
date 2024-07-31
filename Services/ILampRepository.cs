using LampStoreProjects.Models;

namespace LampStoreProjects.Repositories
{
    public interface ILampRepository
    {
        Task<IEnumerable<LampModel>> GetAllLampsAsync();
        Task<LampModel> GetLampByIdAsync(int id);
        Task<LampModel> AddLampAsync(LampModel lamp);
        Task<LampModel> UpdateLampAsync(LampModel lamp);
        Task DeleteLampAsync(int id);
    }
}