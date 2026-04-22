using LampStoreProjects.Models;
using LampStoreProjects.Repositories;
using LampStoreProjects.Data;
using LampStoreProjects.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using Microsoft.EntityFrameworkCore;
using LampStoreProjects.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace LampStoreProjects.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductStoreManage _productStoreManage;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IImageUploadService _imageService;
        private readonly ICacheService _cacheService;
        private readonly ImageOptimizationService _imageOptimizer;

        public ProductsController(IProductRepository productRepository, IProductStoreManage productStoreManage, ApplicationDbContext context, IWebHostEnvironment env, IImageUploadService imageService, ICacheService cacheService, ImageOptimizationService imageOptimizer)
        {
            _productRepository = productRepository;
            _productStoreManage = productStoreManage;
            _context = context;
            _env = env;
            _imageService = imageService;
            _cacheService = cacheService;
            _imageOptimizer = imageOptimizer;
        }

        [HttpGet]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<IEnumerable<ProductModel>>> GetAllProducts()
        {
            // Kiểm tra cache trước
            var cachedProducts = await _cacheService.GetAsync<IEnumerable<ProductModel>>(CacheKeys.AllProducts);
            if (cachedProducts != null)
            {
                return Ok(cachedProducts);
            }

            // Nếu không có cache, lấy từ database
            var products = await _productRepository.GetAllProductAsync();
            
            // Lưu vào cache với thời gian expire 10 phút
            await _cacheService.SetAsync(CacheKeys.AllProducts, products, TimeSpan.FromMinutes(10));
            
            return Ok(products);
        }

        [HttpGet("{id}")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<ProductModel>> GetProductById(Guid id)
        {
            // Kiểm tra cache trước
            var cacheKey = CacheKeys.ProductById(id);
            var cachedProduct = await _cacheService.GetAsync<ProductModel>(cacheKey);
            if (cachedProduct != null)
            {
                return Ok(cachedProduct);
            }

            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Lưu vào cache với thời gian expire 15 phút
            await _cacheService.SetAsync(cacheKey, product, TimeSpan.FromMinutes(15));
            
            return Ok(product);
        }

        [HttpGet("slug/{slug}")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<ProductModel>> GetProductBySlug(string slug)
        {
            var cacheKey = $"product_slug_{slug}";
            var cachedProduct = await _cacheService.GetAsync<ProductModel>(cacheKey);
            if (cachedProduct != null)
            {
                return Ok(cachedProduct);
            }

            var product = await _productRepository.GetProductBySlugAsync(slug);
            if (product == null)
            {
                return NotFound();
            }

            // Lưu vào cache với thời gian expire 15 phút
            await _cacheService.SetAsync(cacheKey, product, TimeSpan.FromMinutes(15));
            
            return Ok(product);
        }

        [HttpGet("Variant/{id}")]
        public async Task<ActionResult<IEnumerable<ProductVariantModel>>> GetProductVariantById(Guid id)
        {
            var variant = await _productRepository.GetProductVariantByIdAsync(id);
            if(variant == null)
            {
                return NotFound();
            }
            return Ok(variant);
        }

        [HttpGet("VariantType/{id}")]
        public async Task<ActionResult<VariantTypeModel>> GetVariantTypeById(Guid id)
        {
            var variantType = await _productRepository.GetVariantTypeByIdAsync(id);
            if (variantType == null)
            {
                return NotFound();
            }
            return Ok(variantType);
        }

        [HttpGet("VariantValue/{id}")]
        public async Task<ActionResult<VariantValueModel>> GetVariantValueById(Guid id)
        {
            var variantvalue = await _productRepository.GetVariantValueByIdAsync(id);
            if(variantvalue == null)
            {
                return NotFound();
            }
            return Ok(variantvalue);
        }

        [HttpGet("{id}/images")]
        public async Task<ActionResult<List<ProductImageModel>>> GetProductImagesByProductId(Guid id)
        {
            var images = await _productRepository.GetProductImageByIdAsync(id);

            if (images == null || images.Count == 0)
            {
                return NotFound();
            }

            return Ok(images);
        }

        [HttpPost]
        public async Task<ActionResult<ProductModel>> CreateProduct([FromBody] ProductCreateDto productDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdProduct = await _productRepository.CreateProductAsync(productDto);
                
                // Xóa cache liên quan sau khi tạo sản phẩm mới
                await _cacheService.RemoveAsync(CacheKeys.AllProducts);
                await _cacheService.RemoveByPatternAsync($"products_category_{createdProduct.CategoryId}");
                
                return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi tạo sản phẩm: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProductModel>> UpdateProduct(Guid Id, [FromBody] ProductUpdateDto productDto)
        {
            if (Id != productDto.Id)
            {
                return BadRequest();
            }
            
            await _productRepository.UpdateProductAsync(Id, productDto);
            
            // Xóa cache liên quan sau khi cập nhật sản phẩm
            await _cacheService.RemoveAsync(CacheKeys.AllProducts);
            await _cacheService.RemoveAsync(CacheKeys.ProductById(Id));
            await _cacheService.RemoveByPatternAsync($"products_category_");
            
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(Guid id)
        {
            // Xóa file ảnh vật lý trước khi xóa sản phẩm
            var productImages = await _context.ProductImages!.Where(img => img.ProductId == id).ToListAsync();
            var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            foreach (var image in productImages)
            {
                if (!string.IsNullOrEmpty(image.ImagePath) && !image.ImagePath.StartsWith("http"))
                {
                    var filePath = Path.Combine(webRootPath, image.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }

            await _productRepository.DeleteProductAsync(id);
            
            // Xóa cache
            await _cacheService.RemoveAsync(CacheKeys.AllProducts);
            await _cacheService.RemoveAsync(CacheKeys.ProductById(id));
            await _cacheService.RemoveByPatternAsync($"products_category_");
            await _cacheService.RemoveByPatternAsync($"product_images_{id}");
            
            return NoContent();
        }

        [HttpDelete("image/{imageId}")]
        public async Task<ActionResult> DeleteProductImage(Guid imageId)
        {
            var productImage = await _context.ProductImages!.FirstOrDefaultAsync(x => x.Id == imageId);
            if (productImage == null)
            {
                return NotFound("Hình ảnh không tồn tại.");
            }

            await _productRepository.DeleteImageProductAsync(imageId);

            if (!string.IsNullOrEmpty(productImage.ImagePath) && !productImage.ImagePath.StartsWith("http"))
            {
                var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var filePath = Path.Combine(webRootPath, productImage.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            return Ok("Xóa hình ảnh sản phẩm thành công.");
        }

        [HttpPost("{productId}/images")]
        public async Task<ActionResult> UploadImages(Guid productId, [FromForm] List<IFormFile> imageFiles)
        {
            const int MAX_IMAGES = 5;
            try
            {
                if (imageFiles == null || imageFiles.Count == 0)
                {
                    return BadRequest("No image files provided.");
                }

                var existingImagesCount = await _context.ProductImages!.CountAsync(img => img.ProductId == productId);

                if (existingImagesCount + imageFiles.Count > MAX_IMAGES)
                {
                    return BadRequest($"Bạn chỉ có thể upload tối đa {MAX_IMAGES} ảnh, hiện tại đã có {existingImagesCount} ảnh.");
                }

                var product = await _productRepository.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                // Tạo thư mục uploads nếu chưa có
                var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "ImageImport");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                foreach (var imageFile in imageFiles)
                {
                    // Validate file type
                    var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                    if (!allowedTypes.Contains(imageFile.ContentType.ToLower()))
                    {
                        return BadRequest("File không hợp lệ. Chỉ chấp nhận JPEG, PNG, GIF, WebP.");
                    }

                    if (imageFile.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest("File vượt quá 5MB.");
                    }

                    // Tạo tên file unique — always .jpg for optimized output
                    var fileName = $"{Guid.NewGuid()}.jpg";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    // Optimize: resize + compress to JPEG
                    using (var stream = imageFile.OpenReadStream())
                    {
                        await _imageOptimizer.OptimizeImageAsync(stream, filePath, maxWidth: 1200, quality: 80);
                    }

                    // Lưu path tương đối vào DB
                    var relativePath = $"/ImageImport/{fileName}";
                    var productImage = new ProductImage
                    {
                        ImagePath = relativePath,
                        ProductId = productId
                    };
                    _context.ProductImages!.Add(productImage);
                }
                await _context.SaveChangesAsync();
                
                // Xóa cache
                await _cacheService.RemoveAsync(CacheKeys.AllProducts);
                await _cacheService.RemoveAsync(CacheKeys.ProductById(productId));

                return Ok(new { message = "Upload ảnh thành công!" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportProducts([FromBody] List<ProductCreateDto> products)
        {
            try
            {
                if (products == null || !products.Any())
                {
                    return BadRequest(new { message = "Không có dữ liệu sản phẩm để import" });
                }

                foreach (var product in products)
                {
                    await _productRepository.CreateProductAsync(product);
                }

                // Xóa cache
                await _cacheService.RemoveAsync(CacheKeys.AllProducts);

                return Ok(new { message = "Import sản phẩm thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra khi import sản phẩm", error = ex.Message });
            }
        }

        [HttpDelete("bulk")]
        public async Task<ActionResult> BulkDeleteProducts(List<Guid> ids)
        {
            await _productRepository.BulkDeleteAsync(ids);
            await _cacheService.RemoveAsync(CacheKeys.AllProducts);
            return NoContent();
        }

        [HttpGet("search")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<SearchResultModel>> AdvancedSearch(
            [FromQuery] string? keyword,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] Guid? categoryId,
            [FromQuery] List<string>? tags,
            [FromQuery] bool? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? sortBy = "name",
            [FromQuery] string? sortOrder = "asc")
        {
            try
            {
                var criteria = new SearchCriteriaModel
                {
                    Keyword = keyword,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    CategoryId = categoryId,
                    Tags = tags,
                    Status = status,
                    Page = page,
                    PageSize = pageSize,
                    SortBy = sortBy,
                    SortOrder = sortOrder
                };

                var searchResult = await _productRepository.AdvancedSearchAsync(criteria);
                
                // Lưu vào cache với key động
                var cacheKey = $"search_{keyword}_{minPrice}_{maxPrice}_{categoryId}_{string.Join(",", tags ?? new List<string>())}_{status}_{page}_{pageSize}_{sortBy}_{sortOrder}";
                await _cacheService.SetAsync(cacheKey, searchResult, TimeSpan.FromMinutes(10));
                
                return Ok(searchResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi tìm kiếm: {ex.Message}");
            }
        }
    }
}