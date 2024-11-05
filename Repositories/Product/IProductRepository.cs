using LampStoreProjects.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<ProductModel>> GetAllProductAsync();
        Task<ProductModel> GetProductByIdAsync(int id);
        Task<List<ProductImageModel>?> GetProductImageByIdAsync(int id);
        Task<List<ProductVariantCreateModel>?> GetProductVariantByIdAsync(int id);
        Task AddProductVariantAsync(int productId, List<ProductVariantCreateModel> productVariants);
        Task<ProductModel> AddProductAsync(ProductModel ProductModel);
        Task<ProductModel> UpdateProductAsync(ProductModel ProductModel);
        Task DeleteImageProductAsync(int imageId);
        Task DeleteProductAsync(int id);
        Task DeleteProductVariantAsync(int id);
    }
}