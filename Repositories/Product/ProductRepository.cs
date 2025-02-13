using AutoMapper;
using LampStoreProjects.Models;
using LampStoreProjects.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using LampStoreProjects.DTOs;

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
                    Tags = p.Tags,
                    ViewCount = p.ViewCount,
                    Favorites = p.Favorites,
                    SellCount = p.SellCount,
                    Status = p.Status,                    
                    DateAdded = p.DateAdded,
                    Images = p.Images.Select(i => new ProductImageModel { ImagePath = i.ImagePath }).ToList(),
                    MinPrice = p.Variants.Min(v => v.DiscountPrice),
                    MaxPrice = p.Variants.Max(v => v.Price)

                })
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProductModel>>(products);
        }

        public async Task<ProductModel> GetProductByIdAsync(Guid id)
        {
            var product = await _context.Products!.Include(l => l.Images).FirstOrDefaultAsync(l => l.Id == id);
            return _mapper.Map<ProductModel>(product);
        }

        public async Task<VariantTypeModel> GetVariantTypeByIdAsync(Guid id)
        {
            var varianttype = await _context.VariantTypes!.FirstOrDefaultAsync(l => l.Id == id);
            return _mapper.Map<VariantTypeModel>(varianttype);
        }

        public async Task<VariantValueModel> GetVariantValueByIdAsync(Guid id)
        {
            var variantvalue = await _context.VariantValues!.FirstOrDefaultAsync(l => l.Id == id);
            return _mapper.Map<VariantValueModel>(variantvalue);
        }

        public async Task<List<ProductImageModel>?> GetProductImageByIdAsync(Guid id)
        {
            var images = await _context.ProductImages!.Where(x => x.ProductId == id).ToListAsync();
            if (images.Count == 0)
            {
                return null;
            }

            return _mapper.Map<List<ProductImageModel>>(images);
        }

        public async Task<ProductModel> CreateProductAsync(ProductCreateDto productDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Thêm sản phẩm
                var product = _mapper.Map<Product>(productDto);
                product.DateAdded = DateTime.UtcNow;

                await _context.Products!.AddAsync(product);
                await _context.SaveChangesAsync(); // Lưu để có ID cho các liên kết sau

                // Thêm Variant Types và Variant Values
                var allVariantValues = new List<VariantValue>();

                // Thêm Variant Types
                var variantTypes = new List<VariantType>();
                foreach (var variantDto in productDto.VariantTypes)
                {
                    var variantType = new VariantType
                    {
                        Name = variantDto.Name,
                        ProductId = product.Id
                    };
                    await _context.VariantTypes!.AddAsync(variantType);
                    await _context.SaveChangesAsync(); // Lưu để có ID cho VariantValues

                    var variantValues = variantDto.Values.Select(value => new VariantValue
                    {
                        TypeId = variantType.Id,
                        Value = value
                    }).ToList();

                    await _context.VariantValues!.AddRangeAsync(variantValues);
                    await _context.SaveChangesAsync();

                    allVariantValues.AddRange(variantValues);
                }

                // Thêm Product Variants
                var productVariants = productDto.ProductVariants.Select(variantDto => new ProductVariant
                {
                    ProductId = product.Id,
                    Price = variantDto.Price,
                    DiscountPrice = variantDto.DiscountPrice,
                    Stock = variantDto.Stock,
                    Weight = variantDto.Weight,
                    Materials = variantDto.Materials,
                    SKU = variantDto.SKU
                }).ToList();

                await _context.ProductVariants!.AddRangeAsync(productVariants);
                await _context.SaveChangesAsync();

                // Lấy danh sách ProductVariants vừa được lưu
                var savedProductVariants = await _context.ProductVariants
                    .Where(pv => pv.ProductId == product.Id)
                    .ToListAsync();

                // Lấy danh sách VariantValues đã lưu
                var savedVariantValues = allVariantValues;

                // Tạo danh sách ProductVariantValues với tất cả ProductVariant và VariantValue phù hợp
                var productVariantValues = new List<ProductVariantValue>();

                foreach (var productVariant in savedProductVariants)
                {
                    foreach (var variantValue in savedVariantValues)
                    {
                        productVariantValues.Add(new ProductVariantValue
                        {
                            ProductVariantId = productVariant.Id,
                            VariantValueId = variantValue.Id
                        });
                    }
                }

                await _context.ProductVariantValues!.AddRangeAsync(productVariantValues);
                await _context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                return _mapper.Map<ProductModel>(product);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        
        public async Task<ProductModel> UpdateProductAsync(ProductModel ProductModel)
        {
            var product = _mapper.Map<Product>(ProductModel);
            _context.Products!.Update(product);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductModel>(product);
        }

        public async Task DeleteImageProductAsync(Guid imageId)
        {
            var Image = await _context.ProductImages!.FindAsync(imageId);
            if (Image != null)
            {
                _context.ProductImages.Remove(Image);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteProductVariantAsync(Guid variantId)
        {
            var variant = await _context.ProductVariants!.FindAsync(variantId);
            if (variant != null)
            {
                _context.ProductVariants.Remove(variant);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteProductAsync(Guid id)
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