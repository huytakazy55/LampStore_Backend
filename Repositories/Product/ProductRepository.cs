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
                    MaxPrice = p.Variants.Max(v => v.Price),
                    Stock = p.Variants.Sum(s => s.Stock)
                })
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProductModel>>(products);
        }

        public async Task<ProductModel> GetProductByIdAsync(Guid id)
        {
            var product = await _context.Products!.Include(l => l.Images).FirstOrDefaultAsync(l => l.Id == id);
            return _mapper.Map<ProductModel>(product);
        }

        public async Task<ProductVariantModel> GetProductVariantByIdAsync(Guid id)
        {
            var variant = await _context.ProductVariants!.FirstOrDefaultAsync(l => l.ProductId == id);
            return _mapper.Map<ProductVariantModel>(variant);
        }

        public async Task<List<VariantTypeModel>> GetVariantTypeByIdAsync(Guid productId)
        {
            var varianttype = await _context.VariantTypes!
            .Where(l => l.ProductId == productId)
            .ToListAsync();
            return _mapper.Map<List<VariantTypeModel>>(varianttype);
        }

        public async Task<List<string>> GetVariantValueByIdAsync(Guid typeId)
        {
            var variantValues = await _context.VariantValues!
                .Where(l => l.TypeId == typeId)
                .Select(v => v.Value)
                .ToListAsync();
            return variantValues;
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


        public async Task<ProductModel> UpdateProductAsync(int productId, ProductCreateDto productDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var product = await _context.Products
                    .Include(p => p.VariantTypes)
                    .ThenInclude(vt => vt.VariantValues)
                    .Include(p => p.ProductVariants)
                    .ThenInclude(pv => pv.ProductVariantValues)
                    .FirstOrDefaultAsync(p => p.Id == productId);

                if (product == null)
                    throw new KeyNotFoundException("Product not found");

                // Cập nhật thông tin sản phẩm
                _mapper.Map(productDto, product);
                product.DateUpdated = DateTime.UtcNow;
                _context.Products!.Update(product);
                await _context.SaveChangesAsync();

                // Cập nhật hoặc xóa Variant Types và Values
                var allVariantValues = new List<VariantValue>();

                foreach (var variantDto in productDto.VariantTypes)
                {
                    var existingVariantType = product.VariantTypes.FirstOrDefault(vt => vt.Name == variantDto.Name);

                    if (existingVariantType == null)
                    {
                        // Thêm mới Variant Type nếu chưa tồn tại
                        var newVariantType = new VariantType
                        {
                            Name = variantDto.Name,
                            ProductId = product.Id
                        };
                        await _context.VariantTypes!.AddAsync(newVariantType);
                        await _context.SaveChangesAsync(); // Để lấy ID

                        // Thêm Variant Values
                        var variantValues = variantDto.Values.Select(value => new VariantValue
                        {
                            TypeId = newVariantType.Id,
                            Value = value
                        }).ToList();

                        await _context.VariantValues!.AddRangeAsync(variantValues);
                        await _context.SaveChangesAsync();

                        allVariantValues.AddRange(variantValues);
                    }
                    else
                    {
                        // Cập nhật Variant Type nếu tồn tại
                        existingVariantType.Name = variantDto.Name;
                        _context.VariantTypes!.Update(existingVariantType);
                        await _context.SaveChangesAsync();

                        // Xóa các Variant Value cũ không còn trong danh sách mới
                        var existingValues = existingVariantType.VariantValues.ToList();
                        var newValues = variantDto.Values.ToList();

                        var valuesToRemove = existingValues.Where(ev => !newValues.Contains(ev.Value)).ToList();
                        _context.VariantValues!.RemoveRange(valuesToRemove);

                        // Thêm mới các Variant Value nếu chưa tồn tại
                        var valuesToAdd = newValues.Except(existingValues.Select(ev => ev.Value))
                            .Select(value => new VariantValue
                            {
                                TypeId = existingVariantType.Id,
                                Value = value
                            }).ToList();

                        await _context.VariantValues.AddRangeAsync(valuesToAdd);
                        await _context.SaveChangesAsync();

                        allVariantValues.AddRange(existingVariantType.VariantValues);
                    }
                }

                // Cập nhật hoặc thêm Product Variants
                foreach (var variantDto in productDto.ProductVariants)
                {
                    var existingVariant = product.ProductVariants.FirstOrDefault(pv => pv.SKU == variantDto.SKU);

                    if (existingVariant == null)
                    {
                        // Thêm mới Product Variant
                        var newVariant = new ProductVariant
                        {
                            ProductId = product.Id,
                            Price = variantDto.Price,
                            DiscountPrice = variantDto.DiscountPrice,
                            Stock = variantDto.Stock,
                            Weight = variantDto.Weight,
                            Materials = variantDto.Materials,
                            SKU = variantDto.SKU
                        };

                        await _context.ProductVariants!.AddAsync(newVariant);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        // Cập nhật Product Variant
                        existingVariant.Price = variantDto.Price;
                        existingVariant.DiscountPrice = variantDto.DiscountPrice;
                        existingVariant.Stock = variantDto.Stock;
                        existingVariant.Weight = variantDto.Weight;
                        existingVariant.Materials = variantDto.Materials;

                        _context.ProductVariants!.Update(existingVariant);
                        await _context.SaveChangesAsync();
                    }
                }

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