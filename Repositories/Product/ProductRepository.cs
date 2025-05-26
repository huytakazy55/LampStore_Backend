using AutoMapper;
using LampStoreProjects.Models;
using LampStoreProjects.Data;
using Microsoft.EntityFrameworkCore;
using LampStoreProjects.DTOs;

namespace LampStoreProjects.Repositories
{
    public class ProductRepository(ApplicationDbContext context, IMapper mapper) : IProductRepository
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<ProductModel>> GetAllProductAsync()
        {
            return await _context.Products!
                .Include(p => p.Images)
                .Include(p => p.ProductVariants)
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
                    Images = p.Images.Select(i => new ProductImageModel 
                    { 
                        Id = i.Id,
                        ImagePath = i.ImagePath,
                        ProductId = i.ProductId
                    }).ToList(),
                    MinPrice = p.ProductVariants.Min(v => v.DiscountPrice),
                    MaxPrice = p.ProductVariants.Max(v => v.Price),
                    Stock = p.ProductVariants.Sum(s => s.Stock)
                })
                .ToListAsync();
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
                // Tạo sản phẩm mới
                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = productDto.Name,
                    Description = productDto.Description,
                    ReviewCount = productDto.ReviewCount,
                    Tags = productDto.Tags,
                    ViewCount = productDto.ViewCount,
                    Favorites = productDto.Favorites,
                    SellCount = productDto.SellCount,
                    CategoryId = productDto.CategoryId,
                    DateAdded = productDto.DateAdded,
                    Status = productDto.Status == 1,
                };

                await _context.Products!.AddAsync(product);

                // Xử lý các loại biến thể (VariantType)
                foreach (var variantTypeDto in productDto.VariantTypes)
                {
                    var variantType = new VariantType
                    {
                        Id = Guid.NewGuid(),
                        Name = variantTypeDto.Name,
                        ProductId = product.Id
                    };

                    await _context.VariantTypes!.AddAsync(variantType);

                    // Xử lý các giá trị của loại biến thể
                    foreach (var value in variantTypeDto.Values)
                    {
                        var variantValue = new VariantValue
                        {
                            Id = Guid.NewGuid(),
                            Value = value,
                            TypeId = variantType.Id
                        };

                        await _context.VariantValues!.AddAsync(variantValue);
                    }
                }

                // Xử lý các biến thể sản phẩm (ProductVariant)
                foreach (var variantDto in productDto.ProductVariants)
                {
                    var productVariant = new ProductVariant
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        Price = variantDto.Price,
                        DiscountPrice = variantDto.DiscountPrice,
                        Stock = variantDto.Stock,
                        Weight = variantDto.Weight,
                        Materials = variantDto.Materials,
                        SKU = variantDto.SKU
                    };

                    await _context.ProductVariants!.AddAsync(productVariant);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<ProductModel>(product);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ProductModel> UpdateProductAsync(Guid productId, ProductUpdateDto productDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lấy sản phẩm hiện tại với các quan hệ
                var product = await _context.Products!
                    .Include(p => p.VariantTypes)
                        .ThenInclude(vt => vt.Values)
                    .Include(p => p.ProductVariants)
                    .FirstOrDefaultAsync(p => p.Id == productId);

                if (product == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy sản phẩm");
                }

                // Cập nhật thông tin cơ bản của sản phẩm
                product.Name = productDto.Name;
                product.Description = productDto.Description;
                product.ReviewCount = productDto.ReviewCount;
                product.Tags = productDto.Tags;
                product.ViewCount = productDto.ViewCount;
                product.Favorites = productDto.Favorites;
                product.SellCount = productDto.SellCount;
                product.CategoryId = productDto.CategoryId;
                product.Status = productDto.Status == true;

                // Xử lý các loại biến thể (VariantType)
                var existingVariantTypes = product.VariantTypes.ToList();
                var newVariantTypes = new List<VariantType>();

                foreach (var variantTypeDto in productDto.VariantTypes)
                {
                    var existingType = existingVariantTypes.FirstOrDefault(vt => vt.Name == variantTypeDto.Name);
                    
                    if (existingType != null)
                    {
                        // Cập nhật giá trị của loại biến thể hiện có
                        var existingValues = existingType.Values.ToList();
                        var newValues = variantTypeDto.Values.ToList();

                        // Xóa các giá trị không còn tồn tại
                        var valuesToRemove = existingValues.Where(ev => !newValues.Contains(ev.Value)).ToList();
                        _context.VariantValues!.RemoveRange(valuesToRemove);

                        // Thêm các giá trị mới
                        var valuesToAdd = newValues.Except(existingValues.Select(ev => ev.Value))
                            .Select(value => new VariantValue
                            {
                                Id = Guid.NewGuid(),
                                Value = value,
                                TypeId = existingType.Id
                            }).ToList();

                        await _context.VariantValues!.AddRangeAsync(valuesToAdd);
                    }
                    else
                    {
                        // Tạo loại biến thể mới
                        var newVariantType = new VariantType
                        {
                            Id = Guid.NewGuid(),
                            Name = variantTypeDto.Name,
                            ProductId = product.Id
                        };

                        newVariantTypes.Add(newVariantType);
                        await _context.VariantTypes!.AddAsync(newVariantType);

                        // Thêm các giá trị cho loại biến thể mới
                        var variantValues = variantTypeDto.Values.Select(value => new VariantValue
                        {
                            Id = Guid.NewGuid(),
                            Value = value,
                            TypeId = newVariantType.Id
                        }).ToList();

                        await _context.VariantValues!.AddRangeAsync(variantValues);
                    }
                }

                // Xóa các loại biến thể không còn tồn tại
                var typesToRemove = existingVariantTypes
                    .Where(et => !productDto.VariantTypes.Any(vt => vt.Name == et.Name))
                    .ToList();

                _context.VariantTypes!.RemoveRange(typesToRemove);

                // Xử lý các biến thể sản phẩm (ProductVariant)
                var existingVariants = product.ProductVariants.ToList();
                var newVariants = new List<ProductVariant>();

                foreach (var variantDto in productDto.ProductVariants)
                {
                    var existingVariant = existingVariants.FirstOrDefault(v => v.SKU == variantDto.SKU);
                    
                    if (existingVariant != null)
                    {
                        // Cập nhật biến thể hiện có
                        existingVariant.Price = variantDto.Price;
                        existingVariant.DiscountPrice = variantDto.DiscountPrice;
                        existingVariant.Stock = variantDto.Stock;
                        existingVariant.Weight = variantDto.Weight;
                        existingVariant.Materials = variantDto.Materials;
                        existingVariant.SKU = variantDto.SKU;

                        _context.ProductVariants!.Update(existingVariant);
                    }
                    else
                    {
                        // Tạo biến thể mới
                        var newVariant = new ProductVariant
                        {
                            Id = Guid.NewGuid(),
                            ProductId = product.Id,
                            Price = variantDto.Price,
                            DiscountPrice = variantDto.DiscountPrice,
                            Stock = variantDto.Stock,
                            Weight = variantDto.Weight,
                            Materials = variantDto.Materials,
                            SKU = variantDto.SKU
                        };

                        newVariants.Add(newVariant);
                        await _context.ProductVariants!.AddAsync(newVariant);
                    }
                }

                // Xóa các biến thể không còn tồn tại
                var variantsToRemove = existingVariants
                    .Where(ev => !productDto.ProductVariants.Any(v => v.SKU == ev.SKU))
                    .ToList();

                _context.ProductVariants!.RemoveRange(variantsToRemove);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<ProductModel>(product);
            }
            catch (Exception)
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

        public async Task BulkDeleteAsync(List<Guid> ids)
        {
            var products = _context.Products!.Where(p => ids.Contains(p.Id));
            _context.Products!.RemoveRange(products);
            await _context.SaveChangesAsync();
        }

    }
}