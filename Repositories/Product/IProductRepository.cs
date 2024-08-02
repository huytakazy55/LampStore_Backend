using LampStoreProjects.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<ProductModel>> GetAllProductAsync();
        Task<ProductModel> GetProductByIdAsync(int id);
        Task<ProductModel> AddProductAsync(ProductModel ProductModel);
        Task<ProductModel> UpdateProductAsync(ProductModel ProductModel);
        Task DeleteProductAsync(int id);
    }
}