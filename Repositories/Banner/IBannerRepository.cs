using LampStoreProjects.Models;

namespace LampStoreProjects.Repositories
{
    public interface IBannerRepository
    {
        Task<IEnumerable<BannerModel>> GetAllAsync();
        Task<IEnumerable<BannerModel>> GetActiveBannersAsync();
        Task<BannerModel?> GetByIdAsync(int id);
        Task<BannerModel> CreateAsync(BannerModel banner);
        Task<BannerModel> UpdateAsync(BannerModel banner);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
} 