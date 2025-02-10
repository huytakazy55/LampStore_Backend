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
        Task<List<ProductImageModel>?> GetProductImageByIdAsync(Guid id);
        Task<VariantTypeModel> GetVariantTypeByIdAsync(Guid id);
        Task<VariantValueModel> GetVariantValueByIdAsync(Guid id);
        Task<ProductModel> CreateProductAsync(ProductCreateDto productDto);
        Task<ProductModel> UpdateProductAsync(ProductModel ProductModel);
        Task DeleteImageProductAsync(Guid imageId);
        Task DeleteProductAsync(Guid id);
        Task DeleteProductVariantAsync(Guid id);
    }
}