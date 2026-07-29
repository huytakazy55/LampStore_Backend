using LampStoreProjects.Models;

namespace LampStoreProjects.Repositories
{
    public interface IBannerRepository
    {
        Task<IEnumerable<BannerModel>> GetAllAsync(int page = 1, int pageSize = 50, string? search = null);
        Task<int> CountAsync(string? search = null);
        Task<IEnumerable<BannerModel>> GetActiveBannersAsync();
        Task<BannerModel?> GetByIdAsync(int id);
        Task<BannerModel> CreateAsync(BannerModel banner);
        Task<BannerModel> UpdateAsync(BannerModel banner);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
