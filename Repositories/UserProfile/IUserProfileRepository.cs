using LampStoreProjects.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public interface IUserProfileRepository
    {
        Task<IEnumerable<UserProfileModel>> GetAllAsync();
        Task<UserProfileModel> GetByIdAsync(Guid id);
        Task AddAsync(UserProfileModel UserProfileModel);
        Task UpdateAsync(UserProfileModel UserProfileModel);
        Task DeleteAsync(Guid id);
    }
}