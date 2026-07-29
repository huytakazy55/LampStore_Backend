using LampStoreProjects.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<CategoryModel>> GetAllAsync(int page = 1, int pageSize = 50, string? search = null);
        Task<int> CountAsync(string? search = null);
        Task<CategoryModel> GetByIdAsync(Guid id);
        Task<CategoryModel> GetBySlugAsync(string slug);
        Task AddAsync(CategoryModel categoryModel);
        Task UpdateAsync(CategoryModel categoryModel);
        Task DeleteAsync(Guid id);
        Task BulkDeleteAsync(List<Guid> ids);
    }
}
