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
        Task<VariantTypeModel> GetVariantTypeByIdAsync(int id);
        Task<VariantValueModel> GetVariantValueByIdAsync(int id);
        Task<ProductModel> AddProductAsync(ProductModel ProductModel);
        Task<VariantTypeModel> AddVariantTypeAsync(VariantTypeModel VariantType);
        Task<VariantValueModel> AddVariantValueAsync(VariantValueModel VariantValue);
        Task<ProductModel> UpdateProductAsync(ProductModel ProductModel);
        Task DeleteImageProductAsync(int imageId);
        Task DeleteProductAsync(int id);
        Task DeleteProductVariantAsync(int id);
    }
}