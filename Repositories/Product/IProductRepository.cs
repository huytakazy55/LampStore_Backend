using LampStoreProjects.Models;
using LampStoreProjects.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<ProductModel>> GetAllProductAsync();
        Task<ProductModel> GetProductByIdAsync(Guid id);
        Task<ProductVariantModel> GetProductVariantByIdAsync(Guid id);
        Task<List<ProductImageModel>?> GetProductImageByIdAsync(Guid id);
        Task<List<VariantTypeModel>> GetVariantTypeByIdAsync(Guid id);
        Task<List<string>> GetVariantValueByIdAsync(Guid id);
        Task<ProductModel> CreateProductAsync(ProductCreateDto productDto);
        Task<ProductModel> UpdateProductAsync(Guid productId, ProductUpdateDto productDto);
        Task DeleteImageProductAsync(Guid imageId);
        Task DeleteProductAsync(Guid id);
        Task DeleteProductVariantAsync(Guid id);
        Task BulkDeleteAsync(List<Guid> ids);
    }
}