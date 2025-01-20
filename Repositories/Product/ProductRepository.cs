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
            var products = await _context.Products!
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Select(p => new ProductModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    DateAdded = p.DateAdded,
                    IsAvailable = p.IsAvailable,
                    Images = p.Images.Select(i => new ProductImageModel { ImagePath = i.ImagePath }).ToList(),
                    MinPrice = p.Variants.Min(v => v.SalePrice),
                    MaxPrice = p.Variants.Max(v => v.SalePrice),
                    Quantity = p.Variants.Sum(v => v.Quantity)

                })
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProductModel>>(products);
        }

        public async Task<ProductModel> GetProductByIdAsync(int id)
        {
            var product = await _context.Products!.Include(l => l.Images).FirstOrDefaultAsync(l => l.Id == id);
            return _mapper.Map<ProductModel>(product);
        }

        public async Task<VariantTypeModel> GetVariantTypeByIdAsync(int id)
        {
            var varianttype = await _context.VariantTypes!.FirstOrDefaultAsync(l => l.Id == id);
            return _mapper.Map<VariantTypeModel>(varianttype);
        }

        public async Task<VariantValueModel> GetVariantValueByIdAsync(int id)
        {
            var variantvalue = await _context.VariantValues!.FirstOrDefaultAsync(l => l.Id == id);
            return _mapper.Map<VariantValueModel>(variantvalue);
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

        public async Task<ProductModel> AddProductAsync(ProductModel ProductModel)
        {
            var product = _mapper.Map<Product>(ProductModel);
            _context.Products!.Add(product);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductModel>(product);
        }

        public async Task<VariantTypeModel> AddVariantTypeAsync(VariantTypeModel VariantType)
        {
            var variantType = _mapper.Map<VariantType>(VariantType);
            _context.VariantTypes!.Add(variantType);
            await _context.SaveChangesAsync();
            return _mapper.Map<VariantTypeModel>(VariantType);
        }

        public async Task<VariantValueModel> AddVariantValueAsync(VariantValueModel VariantValue)
        {
            var variantvalue = _mapper.Map<VariantValue>(VariantValue);
            _context.VariantValues!.Add(variantvalue);
            await _context.SaveChangesAsync();
            return _mapper.Map<VariantValueModel>(VariantValue);
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