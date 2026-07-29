using LampStoreProjects.Models;

namespace LampStoreProjects.Repositories
{
    public interface ITagRepository
    {
        Task<IEnumerable<TagModel>> GetAllAsync(int page = 1, int pageSize = 50, string? search = null);
        Task<int> CountAsync(string? search = null);
        Task<TagModel> GetByIdAsync(Guid id);
        Task AddAsync(TagModel tagModel);
        Task UpdateAsync(TagModel tagModel);
        Task DeleteAsync(Guid id);
        Task BulkDeleteAsync(List<Guid> ids);
    }
}
