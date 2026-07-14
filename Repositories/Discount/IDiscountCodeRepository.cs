using LampStoreProjects.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public interface IDiscountCodeRepository
    {
        Task<IEnumerable<DiscountCode>> GetUserDiscountCodesAsync(string userId);
        Task<DiscountCode?> ValidateDiscountCodeAsync(string code, string userId, decimal orderTotalAmount);
        Task<bool> MarkDiscountCodeAsUsedAsync(string code);
        Task<bool> RestoreDiscountCodeAsync(string code);

        // Admin methods
        Task<IEnumerable<DiscountCode>> GetAllDiscountCodesAsync();
        Task<DiscountCode?> GetDiscountCodeByIdAsync(Guid id);
        Task<DiscountCode> CreateDiscountCodeAsync(DiscountCode discountCode);
        Task<DiscountCode?> UpdateDiscountCodeAsync(DiscountCode discountCode);
        Task DeleteDiscountCodeAsync(Guid id);
    }
}
