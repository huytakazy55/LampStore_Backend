using LampStoreProjects.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public interface ICheckInRepository
    {
        Task<IEnumerable<CheckInModel>> GetAllAsync();
        Task<CheckInModel> GetByIdAsync(int id);
        Task AddAsync(CheckInModel CheckInModel);
        Task UpdateAsync(CheckInModel CheckInModel);
        Task DeleteAsync(int id);
    }
}