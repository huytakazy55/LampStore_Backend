using LampStoreProjects.Data;
using LampStoreProjects.Helpers;
using LampStoreProjects.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace LampStoreProjects.Repositories
{
    public class FlashSaleRepository : IFlashSaleRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public FlashSaleRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FlashSaleModel>> GetAllAsync()
        {
            var flashSales = await _context.FlashSales!
                .Include(f => f.Items)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
            
            var models = new List<FlashSaleModel>();
            foreach (var fs in flashSales)
            {
                var model = _mapper.Map<FlashSaleModel>(fs);
                model.Items = await MapItemsWithProductData(fs.Items);
                models.Add(model);
            }
            return models;
        }

        public async Task<FlashSaleModel?> GetActiveAsync()
        {
            var now = DateTimeHelper.VietnamNow;
            var flashSale = await _context.FlashSales!
                .Include(f => f.Items)
                .Where(f => f.IsActive && f.EndTime > now)
                .OrderBy(f => f.StartTime) // Get the earliest one (either currently running or next upcoming)
                .FirstOrDefaultAsync();
            
            if (flashSale == null) return null;
            
            var model = _mapper.Map<FlashSaleModel>(flashSale);
            model.Items = await MapItemsWithProductData(flashSale.Items);
            return model;
        }

        public async Task<FlashSaleModel?> GetByIdAsync(int id)
        {
            var flashSale = await _context.FlashSales!
                .Include(f => f.Items)
                .FirstOrDefaultAsync(f => f.Id == id);
            
            if (flashSale == null) return null;
            
            var model = _mapper.Map<FlashSaleModel>(flashSale);
            model.Items = await MapItemsWithProductData(flashSale.Items);
            return model;
        }

        public async Task<FlashSaleModel> CreateAsync(FlashSaleModel flashSale)
        {
            var entity = _mapper.Map<FlashSale>(flashSale);
            entity.CreatedAt = DateTimeHelper.VietnamNow;
            entity.Items = new List<FlashSaleItem>(); // Don't create items from model
            _context.FlashSales!.Add(entity);
            await _context.SaveChangesAsync();
            return _mapper.Map<FlashSaleModel>(entity);
        }

        public async Task<FlashSaleModel> UpdateAsync(FlashSaleModel flashSale)
        {
            var entity = await _context.FlashSales!.FindAsync(flashSale.Id);
            if (entity == null) throw new Exception("Flash Sale not found");
            
            entity.Title = flashSale.Title;
            entity.Description = flashSale.Description;
            entity.StartTime = flashSale.StartTime;
            entity.EndTime = flashSale.EndTime;
            entity.IsActive = flashSale.IsActive;
            entity.UpdatedAt = DateTimeHelper.VietnamNow;
            
            await _context.SaveChangesAsync();
            return _mapper.Map<FlashSaleModel>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.FlashSales!
                .Include(f => f.Items)
                .FirstOrDefaultAsync(f => f.Id == id);
            if (entity == null) return false;
            
            _context.FlashSaleItems!.RemoveRange(entity.Items);
            _context.FlashSales.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<FlashSaleItemModel> AddItemAsync(int flashSaleId, FlashSaleItemModel item)
        {
            var entity = new FlashSaleItem
            {
                FlashSaleId = flashSaleId,
                ProductId = item.ProductId,
                DiscountPercent = item.DiscountPercent,
                FlashSalePrice = item.FlashSalePrice,
                Stock = item.Stock,
                SoldCount = 0,
                Order = item.Order
            };
            
            _context.FlashSaleItems!.Add(entity);
            await _context.SaveChangesAsync();
            
            return _mapper.Map<FlashSaleItemModel>(entity);
        }

        public async Task<FlashSaleItemModel?> UpdateItemAsync(int flashSaleId, int itemId, FlashSaleItemModel item)
        {
            var entity = await _context.FlashSaleItems!
                .FirstOrDefaultAsync(i => i.Id == itemId && i.FlashSaleId == flashSaleId);
            if (entity == null) return null;
            
            entity.DiscountPercent = item.DiscountPercent;
            entity.FlashSalePrice = item.FlashSalePrice;
            entity.Stock = item.Stock;
            entity.Order = item.Order;
            
            await _context.SaveChangesAsync();
            return _mapper.Map<FlashSaleItemModel>(entity);
        }

        public async Task<bool> RemoveItemAsync(int flashSaleId, int itemId)
        {
            var item = await _context.FlashSaleItems!
                .FirstOrDefaultAsync(i => i.Id == itemId && i.FlashSaleId == flashSaleId);
            if (item == null) return false;
            
            _context.FlashSaleItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Map FlashSaleItem entities to models with Product data (name, slug, price, image)
        /// </summary>
        private async Task<ICollection<FlashSaleItemModel>> MapItemsWithProductData(ICollection<FlashSaleItem> items)
        {
            var result = new List<FlashSaleItemModel>();
            foreach (var item in items.OrderBy(i => i.Order))
            {
                var model = _mapper.Map<FlashSaleItemModel>(item);
                
                var product = await _context.Products!
                    .Include(p => p.Images)
                    .Include(p => p.ProductVariant)
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);
                
                if (product != null)
                {
                    model.ProductName = product.Name;
                    model.ProductSlug = product.Slug;
                    model.ProductOriginalPrice = product.ProductVariant?.Price;
                    model.ProductImageUrl = product.Images.FirstOrDefault()?.ImagePath;
                }
                
                result.Add(model);
            }
            return result;
        }
    }
}
