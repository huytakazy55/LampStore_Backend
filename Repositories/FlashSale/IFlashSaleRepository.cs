using LampStoreProjects.Models;

namespace LampStoreProjects.Repositories
{
    public interface IFlashSaleRepository
    {
        Task<IEnumerable<FlashSaleModel>> GetAllAsync();
        Task<FlashSaleModel?> GetActiveAsync();
        Task<FlashSaleModel?> GetByIdAsync(int id);
        Task<FlashSaleModel> CreateAsync(FlashSaleModel flashSale);
        Task<FlashSaleModel> UpdateAsync(FlashSaleModel flashSale);
        Task<bool> DeleteAsync(int id);
        Task<FlashSaleItemModel> AddItemAsync(int flashSaleId, FlashSaleItemModel item);
        Task<FlashSaleItemModel?> UpdateItemAsync(int flashSaleId, int itemId, FlashSaleItemModel item);
        Task<bool> RemoveItemAsync(int flashSaleId, int itemId);
    }
}
