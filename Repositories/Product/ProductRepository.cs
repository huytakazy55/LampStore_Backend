using AutoMapper;
using LampStoreProjects.Models;
using LampStoreProjects.Data;
using LampStoreProjects.Helpers;
using Microsoft.EntityFrameworkCore;
using LampStoreProjects.DTOs;
using System;

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
                .Include(p => p.ProductVariant)
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
                    DateAdded = p.CreatedAt,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Images = p.Images.Select(i => new ProductImageModel 
                    { 
                        Id = i.Id,
                        ImagePath = i.ImagePath,
                        ProductId = i.ProductId
                    }).ToList(),
                    Variant = p.ProductVariant != null ? new ProductVariantModel
                    {
                        Id = p.ProductVariant.Id,
                        ProductId = p.ProductVariant.ProductId,
                        Price = p.ProductVariant.Price,
                        DiscountPrice = p.ProductVariant.DiscountPrice,
                        Stock = p.ProductVariant.Stock,
                        Weight = p.ProductVariant.Weight,
                        Materials = p.ProductVariant.Materials,
                        SKU = p.ProductVariant.SKU
                    } : null,
                    MinPrice = p.ProductVariant != null ? p.ProductVariant.DiscountPrice : null,
                    MaxPrice = p.ProductVariant != null ? p.ProductVariant.Price : null,
                    Stock = p.ProductVariant != null ? p.ProductVariant.Stock : 0
                })
                .ToListAsync();
        }

        public async Task<ProductModel> GetProductByIdAsync(Guid id)
        {
            var product = await _context.Products!
                .Include(l => l.Images)
                .Include(l => l.ProductVariant)
                .Include(l => l.VariantTypes)
                    .ThenInclude(vt => vt.Values)
                .FirstOrDefaultAsync(l => l.Id == id);
            
            var productModel = _mapper.Map<ProductModel>(product);
            
            if (productModel != null && product != null && product.ProductVariant != null)
            {
                var v = product.ProductVariant;
                productModel.MinPrice = v.DiscountPrice > 0 ? v.DiscountPrice : v.Price;
                productModel.MaxPrice = v.Price;
                productModel.Stock = v.Stock;
            }
            
            return productModel;
        }

        public async Task<List<ProductVariantModel>> GetProductVariantByIdAsync(Guid id)
        {
            var variants = await _context.ProductVariants!.Where(l => l.ProductId == id).ToListAsync();
            return _mapper.Map<List<ProductVariantModel>>(variants);
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
                    foreach (var valueDto in variantTypeDto.Values)
                    {
                        var variantValue = new VariantValue
                        {
                            Id = Guid.NewGuid(),
                            Value = valueDto.Value,
                            AdditionalPrice = valueDto.AdditionalPrice,
                            TypeId = variantType.Id
                        };

                        await _context.VariantValues!.AddAsync(variantValue);
                    }
                }

                // Tạo 1 ProductVariant duy nhất cho sản phẩm
                var variantDto = productDto.ProductVariant;
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
                    .Include(p => p.ProductVariant)
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
                        var valuesToRemove = existingValues.Where(ev => !newValues.Any(nv => nv.Value == ev.Value)).ToList();
                        _context.VariantValues!.RemoveRange(valuesToRemove);

                        // Cập nhật giá của các giá trị hiện có
                        foreach (var existingVal in existingValues)
                        {
                            var matchingNew = newValues.FirstOrDefault(nv => nv.Value == existingVal.Value);
                            if (matchingNew != null)
                            {
                                existingVal.AdditionalPrice = matchingNew.AdditionalPrice;
                            }
                        }

                        // Thêm các giá trị mới
                        var valuesToAdd = newValues.Where(nv => !existingValues.Any(ev => ev.Value == nv.Value))
                            .Select(valueDto => new VariantValue
                            {
                                Id = Guid.NewGuid(),
                                Value = valueDto.Value,
                                AdditionalPrice = valueDto.AdditionalPrice,
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
                        var variantValues = variantTypeDto.Values.Select(valueDto => new VariantValue
                        {
                            Id = Guid.NewGuid(),
                            Value = valueDto.Value,
                            AdditionalPrice = valueDto.AdditionalPrice,
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

                // Cập nhật hoặc tạo ProductVariant duy nhất
                var variantDto = productDto.ProductVariant;
                if (product.ProductVariant != null)
                {
                    // Cập nhật variant hiện có
                    product.ProductVariant.Price = variantDto.Price;
                    product.ProductVariant.DiscountPrice = variantDto.DiscountPrice;
                    product.ProductVariant.Stock = variantDto.Stock;
                    product.ProductVariant.Weight = variantDto.Weight;
                    product.ProductVariant.Materials = variantDto.Materials;
                    product.ProductVariant.SKU = variantDto.SKU;
                }
                else
                {
                    // Tạo variant mới
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
                    await _context.ProductVariants!.AddAsync(newVariant);
                }

                // Set UpdatedAt timestamp
                product.UpdatedAt = DateTimeHelper.VietnamNow;

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
            product.UpdatedAt = DateTimeHelper.VietnamNow;
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

        public async Task<SearchResultModel> AdvancedSearchAsync(SearchCriteriaModel criteria)
        {
            var query = _context.Products!
                .Include(p => p.Images)
                .Include(p => p.ProductVariant)
                .Include(p => p.Category)
                .Include(p => p.ProductTags)
                .ThenInclude(pt => pt.Tag)
                .AsQueryable();

            // 1. Áp dụng từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                var keyword = criteria.Keyword.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(keyword) || 
                    p.Description.ToLower().Contains(keyword) ||
                    p.Category.Name.ToLower().Contains(keyword) ||
                    p.ProductTags.Any(pt => pt.Tag.Name.ToLower().Contains(keyword))
                );
            }

            // 2. Áp dụng lọc giá
            if (criteria.MinPrice.HasValue)
                query = query.Where(p => p.ProductVariant != null && p.ProductVariant.DiscountPrice >= criteria.MinPrice.Value);
            
            if (criteria.MaxPrice.HasValue)
                query = query.Where(p => p.ProductVariant != null && p.ProductVariant.DiscountPrice <= criteria.MaxPrice.Value);

            // 3. Áp dụng lọc danh mục
            if (criteria.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == criteria.CategoryId.Value);

            // 4. Áp dụng lọc tags
            if (criteria.Tags != null && criteria.Tags.Any())
            {
                query = query.Where(p => p.ProductTags.Any(pt => 
                    criteria.Tags.Contains(pt.Tag.Name)));
            }

            // 5. Áp dụng lọc trạng thái
            if (criteria.Status.HasValue)
                query = query.Where(p => p.Status == criteria.Status.Value);

            // 6. Áp dụng sắp xếp
            query = criteria.SortBy?.ToLower() switch
            {
                "price" => criteria.SortOrder == "desc" ? 
                    query.OrderByDescending(p => p.ProductVariant != null ? p.ProductVariant.DiscountPrice : 0) : 
                    query.OrderBy(p => p.ProductVariant != null ? p.ProductVariant.DiscountPrice : 0),
                "name" => criteria.SortOrder == "desc" ? 
                    query.OrderByDescending(p => p.Name) : 
                    query.OrderBy(p => p.Name),
                "viewcount" => criteria.SortOrder == "desc" ? 
                    query.OrderByDescending(p => p.ViewCount) : 
                    query.OrderBy(p => p.ViewCount),
                "createddate" => criteria.SortOrder == "desc" ? 
                    query.OrderByDescending(p => p.CreatedAt) : 
                    query.OrderBy(p => p.CreatedAt),
                _ => query.OrderBy(p => p.Name)
            };

            // 7. Đếm tổng số sản phẩm
            var totalCount = await query.CountAsync();

            // 8. Áp dụng phân trang
            var products = await query
                .Skip((criteria.Page - 1) * criteria.PageSize)
                .Take(criteria.PageSize)
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
                    DateAdded = p.CreatedAt,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Images = p.Images.Select(i => new ProductImageModel 
                    { 
                        Id = i.Id,
                        ImagePath = i.ImagePath,
                        ProductId = i.ProductId
                    }).ToList(),
                    MinPrice = p.ProductVariant != null ? p.ProductVariant.DiscountPrice : null,
                    MaxPrice = p.ProductVariant != null ? p.ProductVariant.Price : null,
                    Stock = p.ProductVariant != null ? p.ProductVariant.Stock : 0
                })
                .ToListAsync();

            // 9. Trả về kết quả
            return new SearchResultModel
            {
                Products = products,
                TotalCount = totalCount,
                Page = criteria.Page,
                PageSize = criteria.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / criteria.PageSize),
                Criteria = criteria
            };
        }

    }
}