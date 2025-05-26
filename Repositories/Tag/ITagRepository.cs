using LampStoreProjects.Models;

namespace LampStoreProjects.Repositories
{
    public interface ITagRepository
    {
        Task<IEnumerable<TagModel>> GetAllAsync();
        Task<TagModel> GetByIdAsync(Guid id);
        Task AddAsync(TagModel tagModel);
        Task UpdateAsync(TagModel tagModel);
        Task DeleteAsync(Guid id);
        Task BulkDeleteAsync(List<Guid> ids);
    }
}
