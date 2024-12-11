using AutoMapper;
using LampStoreProjects.Models;
using LampStoreProjects.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ProductRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductModel>> GetAllProductAsync()
        {
            var products = await _context.Products!.Include(l => l.Images).ToListAsync();
            return _mapper.Map<IEnumerable<ProductModel>>(products);
        }

        public async Task<ProductModel> GetProductByIdAsync(int id)
        {
            var product = await _context.Products!.Include(l => l.Images).FirstOrDefaultAsync(l => l.Id == id);
            return _mapper.Map<ProductModel>(product);
        }

        public async Task<List<ProductImageModel>?> GetProductImageByIdAsync(int id)
        {
            var images = await _context.ProductImages!.Where(x => x.ProductId == id).ToListAsync();
            if (images.Count == 0)
            {
                return null;
            }

            return _mapper.Map<List<ProductImageModel>>(images);
        }

        public async Task<List<ProductVariantCreateModel>?> GetProductVariantByIdAsync(int id)
        {
            var variants = await _context.ProductVariants!.Where(x => x.ProductId == id).ToListAsync();
            if (variants.Count == 0)
            {
                return null;
            }

            return _mapper.Map<List<ProductVariantCreateModel>>(variants);
        }
        
        public async Task AddProductVariantAsync(int productId, List<ProductVariantCreateModel> productVariants)
        {
            if (productVariants == null || !productVariants.Any())
            {
                throw new ArgumentException("Danh sách biến thể sản phẩm không được để trống!");
            }

            var productExists = await _context.Products!.AnyAsync(p => p.Id == productId);
            if (!productExists)
            {
                throw new ArgumentException("Sản phẩm không tồn tại!");
            }

            // Ánh xạ từ ProductVariantCreateModel sang ProductVariant
            var productVariantsMapper = _mapper.Map<List<ProductVariant>>(productVariants);

            // Thiết lập ProductId cho từng biến thể
            productVariantsMapper.ForEach(variant => variant.ProductId = productId);

            // Thêm vào database và lưu thay đổi
            await _context.ProductVariants!.AddRangeAsync(productVariantsMapper);
            await _context.SaveChangesAsync();
        }

        public async Task<ProductModel> AddProductAsync(ProductModel ProductModel)
        {
            var product = _mapper.Map<Product>(ProductModel);
            _context.Products!.Add(product);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductModel>(product);
        }

        public async Task<ProductModel> UpdateProductAsync(ProductModel ProductModel)
        {
            var product = _mapper.Map<Product>(ProductModel);
            _context.Products!.Update(product);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductModel>(product);
        }

        public async Task DeleteImageProductAsync(int imageId)
        {
            var Image = await _context.ProductImages!.FindAsync(imageId);
            if (Image != null)
            {
                _context.ProductImages.Remove(Image);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteProductVariantAsync(int variantId)
        {
            var variant = await _context.ProductVariants!.FindAsync(variantId);
            if (variant != null)
            {
                _context.ProductVariants.Remove(variant);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products!.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

    }
}