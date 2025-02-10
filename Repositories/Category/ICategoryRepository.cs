using LampStoreProjects.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<CategoryModel>> GetAllAsync();
        Task<CategoryModel> GetByIdAsync(Guid id);
        Task AddAsync(CategoryModel categoryModel);
        Task UpdateAsync(CategoryModel categoryModel);
        Task DeleteAsync(Guid id);
    }
}