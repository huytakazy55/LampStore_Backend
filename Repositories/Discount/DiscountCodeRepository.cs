using LampStoreProjects.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public class DiscountCodeRepository : IDiscountCodeRepository
    {
        private readonly ApplicationDbContext _context;

        public DiscountCodeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DiscountCode>> GetUserDiscountCodesAsync(string userId)
        {
            return await _context.DiscountCodes!
                .Where(dc => (dc.UserId == userId || dc.UserId == null) && dc.Quantity > 0 && dc.Status == "Active" && dc.ExpiryDate > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<DiscountCode?> ValidateDiscountCodeAsync(string code, string userId, decimal orderTotalAmount)
        {
            var discountCode = await _context.DiscountCodes!
                .FirstOrDefaultAsync(dc => dc.Code == code && (dc.UserId == userId || dc.UserId == null));

            if (discountCode == null) return null; // Invalid code
            if (discountCode.Status != "Active") return null; // Inactive
            if (discountCode.Quantity <= 0) return null; // Out of uses
            if (discountCode.ExpiryDate <= DateTime.UtcNow) return null; // Expired
            if (orderTotalAmount < discountCode.MinOrderAmount) return null; // Not reached minimum order amount

            return discountCode;
        }

        public async Task<bool> MarkDiscountCodeAsUsedAsync(string code)
        {
            var discountCode = await _context.DiscountCodes!.FirstOrDefaultAsync(dc => dc.Code == code);
            if (discountCode != null && discountCode.Quantity > 0)
            {
                discountCode.Quantity--;
                if (discountCode.Quantity == 0) discountCode.IsUsed = true;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> RestoreDiscountCodeAsync(string code)
        {
            var discountCode = await _context.DiscountCodes!.FirstOrDefaultAsync(dc => dc.Code == code);
            if (discountCode != null)
            {
                discountCode.Quantity++;
                discountCode.IsUsed = false;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        // Admin methods
        public async Task<IEnumerable<DiscountCode>> GetAllDiscountCodesAsync()
        {
            return await _context.DiscountCodes!.OrderByDescending(dc => dc.CreatedAt).ToListAsync();
        }

        public async Task<DiscountCode?> GetDiscountCodeByIdAsync(Guid id)
        {
            return await _context.DiscountCodes!.FindAsync(id);
        }

        public async Task<DiscountCode> CreateDiscountCodeAsync(DiscountCode discountCode)
        {
            _context.DiscountCodes!.Add(discountCode);
            await _context.SaveChangesAsync();
            return discountCode;
        }

        public async Task<DiscountCode?> UpdateDiscountCodeAsync(DiscountCode discountCode)
        {
            var existing = await _context.DiscountCodes!.FindAsync(discountCode.Id);
            if (existing != null)
            {
                existing.Code = discountCode.Code;
                existing.DiscountType = discountCode.DiscountType;
                existing.DiscountPercentage = discountCode.DiscountPercentage;
                existing.DiscountAmount = discountCode.DiscountAmount;
                existing.MaxDiscountAmount = discountCode.MaxDiscountAmount;
                existing.MinOrderAmount = discountCode.MinOrderAmount;
                existing.Quantity = discountCode.Quantity;
                existing.Status = discountCode.Status;
                existing.ExpiryDate = discountCode.ExpiryDate;
                existing.IsUsed = discountCode.Quantity == 0;
                existing.UserId = discountCode.UserId;
                existing.UpdatedAt = Helpers.DateTimeHelper.VietnamNow;
                await _context.SaveChangesAsync();
                return existing;
            }
            return null;
        }

        public async Task DeleteDiscountCodeAsync(Guid id)
        {
            var existing = await _context.DiscountCodes!.FindAsync(id);
            if (existing != null)
            {
                _context.DiscountCodes.Remove(existing);
                await _context.SaveChangesAsync();
            }
        }
    }
}
