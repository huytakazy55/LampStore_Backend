using LampStoreProjects.Models;
using LampStoreProjects.Repositories;
using LampStoreProjects.Data;
using LampStoreProjects.Services;
using LampStoreProjects.Helpers;
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
        [ResponseCache(Duration = 900, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "id" })]
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
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.PRODUCT_NOT_FOUND));
            }

            // Lưu vào cache 15 phút (chỉ data tĩnh, không có stats)
            await _cacheService.SetAsync(cacheKey, product, TimeSpan.FromMinutes(15));
            
            return Ok(product);
        }

        [HttpGet("slug/{slug}")]
        [ResponseCache(Duration = 900, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "slug" })]
        public async Task<ActionResult<ProductModel>> GetProductBySlug(string slug)
        {
            // Kiểm tra cache trước
            var cacheKey = $"product_slug_{slug}";
            var cachedProduct = await _cacheService.GetAsync<ProductModel>(cacheKey);
            if (cachedProduct != null)
            {
                return Ok(cachedProduct);
            }

            var product = await _productRepository.GetProductBySlugAsync(slug);
            if (product == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.PRODUCT_NOT_FOUND));
            }

            // Lưu vào cache 15 phút (chỉ data tĩnh, không có stats)
            await _cacheService.SetAsync(cacheKey, product, TimeSpan.FromMinutes(15));
            
            return Ok(product);
        }

        /// <summary>
        /// Lấy thông tin realtime: số đánh giá, rating, lượt bán, tồn kho
        /// </summary>
        [HttpGet("{id}/stats")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<ProductStatsModel>> GetProductStats(Guid id)
        {
            var stats = await _productRepository.GetProductStatsAsync(id);
            return Ok(stats);
        }

        [HttpGet("Variant/{id}")]
        public async Task<ActionResult<IEnumerable<ProductVariantModel>>> GetProductVariantById(Guid id)
        {
            var variant = await _productRepository.GetProductVariantByIdAsync(id);
            if(variant == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.PRODUCT_VARIANT_NOT_FOUND));
            }
            return Ok(variant);
        }

        [HttpGet("VariantType/{id}")]
        public async Task<ActionResult<VariantTypeModel>> GetVariantTypeById(Guid id)
        {
            var variantType = await _productRepository.GetVariantTypeByIdAsync(id);
            if (variantType == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.PRODUCT_VARIANT_NOT_FOUND));
            }
            return Ok(variantType);
        }

        [HttpGet("VariantValue/{id}")]
        public async Task<ActionResult<VariantValueModel>> GetVariantValueById(Guid id)
        {
            var variantvalue = await _productRepository.GetVariantValueByIdAsync(id);
            if(variantvalue == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.PRODUCT_VARIANT_NOT_FOUND));
            }
            return Ok(variantvalue);
        }

        [HttpGet("{id}/images")]
        public async Task<ActionResult<List<ProductImageModel>>> GetProductImagesByProductId(Guid id)
        {
            var images = await _productRepository.GetProductImageByIdAsync(id);

            if (images == null || images.Count == 0)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.PRODUCT_IMAGE_NOT_FOUND));
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
                    return BadRequest(ApiErrorResponse.WithErrors(ErrorCodes.VALIDATION_FAILED, ModelState));
                }

                var createdProduct = await _productRepository.CreateProductAsync(productDto);
                
                // Xóa cache liên quan sau khi tạo sản phẩm mới
                await _cacheService.RemoveAsync(CacheKeys.AllProducts);
                await _cacheService.RemoveByPatternAsync($"products_category_{createdProduct.CategoryId}");
                
                return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiErrorResponse.FromException(ErrorCodes.PRODUCT_CREATE_FAILED, ex));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProductModel>> UpdateProduct(Guid Id, [FromBody] ProductUpdateDto productDto)
        {
            if (Id != productDto.Id)
            {
                return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.PRODUCT_ID_MISMATCH));
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

            var productForVideo = await _context.Products!.FirstOrDefaultAsync(p => p.Id == id);
            if (productForVideo != null && !string.IsNullOrEmpty(productForVideo.VideoPath) && !productForVideo.VideoPath.StartsWith("http"))
            {
                var videoPath = Path.Combine(webRootPath, productForVideo.VideoPath.TrimStart('/'));
                if (System.IO.File.Exists(videoPath))
                {
                    System.IO.File.Delete(videoPath);
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
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.PRODUCT_IMAGE_NOT_FOUND));
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

            return Ok(new ApiSuccessResponse("Xóa hình ảnh sản phẩm thành công."));
        }

        // Upload nhanh ảnh từ modal tạo/sửa option (không lưu DB)
        [HttpPost("UploadVariantImage")]
        public async Task<ActionResult<object>> UploadVariantImage([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.PRODUCT_NO_FILE));

            try
            {
                var imageUrl = await _imageService.UploadImageAsync(file, "ImageImport");
                return Ok(new { imageUrl });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiErrorResponse.FromException(ErrorCodes.PRODUCT_INVALID_FILE_TYPE, ex));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiErrorResponse.FromException(ErrorCodes.INTERNAL_ERROR, ex));
            }
        }

        [HttpPost("{productId}/images")]
        public async Task<ActionResult> UploadImages(Guid productId, [FromForm] List<IFormFile> imageFiles)
        {
            const int MAX_IMAGES = 5;
            try
            {
                if (imageFiles == null || imageFiles.Count == 0)
                {
                    return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.PRODUCT_NO_FILE));
                }

                var existingImagesCount = await _context.ProductImages!.CountAsync(img => img.ProductId == productId);

                if (existingImagesCount + imageFiles.Count > MAX_IMAGES)
                {
                    return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.PRODUCT_MAX_IMAGES, $"Bạn chỉ có thể upload tối đa {MAX_IMAGES} ảnh, hiện tại đã có {existingImagesCount} ảnh."));
                }

                var product = await _productRepository.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return NotFound(ApiErrorResponse.FromCode(ErrorCodes.PRODUCT_NOT_FOUND));
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
                        return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.PRODUCT_INVALID_FILE_TYPE));
                    }

                    if (imageFile.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.PRODUCT_FILE_TOO_LARGE));
                    }

                    // Tạo tên file unique — always .jpg for optimized output
                    var fileName = $"{Guid.NewGuid()}.jpg";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    // Optimize: resize + compress to JPEG
                    using (var stream = imageFile.OpenReadStream())
                    {
                        await _imageOptimizer.OptimizeImageAsync(stream, filePath, maxWidth: 800, quality: 55);
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

                return Ok(new ApiSuccessResponse("Upload ảnh thành công!"));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ApiErrorResponse.FromException(ErrorCodes.INTERNAL_ERROR, ex));
            }
        }

        [HttpPost("{productId}/video")]
        [RequestSizeLimit(21 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 21 * 1024 * 1024)]
        public async Task<ActionResult> UploadVideo(Guid productId, [FromForm] IFormFile videoFile, CancellationToken cancellationToken)
        {
            try
            {
                if (videoFile == null || videoFile.Length == 0)
                {
                    return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.PRODUCT_NO_FILE, "Vui lòng chọn file video."));
                }

                var product = await _productRepository.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return NotFound(ApiErrorResponse.FromCode(ErrorCodes.PRODUCT_NOT_FOUND));
                }

                // Validate file type
                var fileExtension = Path.GetExtension(videoFile.FileName).ToLowerInvariant();
                var allowedVideoTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    [".mp4"] = "video/mp4",
                    [".webm"] = "video/webm"
                };
                if (!allowedVideoTypes.TryGetValue(fileExtension, out var expectedContentType) ||
                    !string.Equals(videoFile.ContentType, expectedContentType, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.PRODUCT_INVALID_FILE_TYPE, "Chỉ hỗ trợ định dạng .mp4 hoặc .webm"));
                }

                if (videoFile.Length > 20 * 1024 * 1024)
                {
                    return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.PRODUCT_FILE_TOO_LARGE, "Kích thước video không được vượt quá 20MB."));
                }

                // Tạo thư mục uploads nếu chưa có
                var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "VideoImport");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                // Tạo tên file unique
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsDir, fileName);

                await using (var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true))
                {
                    await videoFile.CopyToAsync(stream, cancellationToken);
                }

                // Lưu path tương đối vào DB
                var relativePath = $"/VideoImport/{fileName}";

                // Fetch product entity to update
                var productEntity = await _context.Products!.FirstOrDefaultAsync(p => p.Id == productId);
                if (productEntity != null)
                {
                    // Xóa video cũ nếu có
                    if (!string.IsNullOrEmpty(productEntity.VideoPath) && !productEntity.VideoPath.StartsWith("http"))
                    {
                        var oldFilePath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), productEntity.VideoPath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    productEntity.VideoPath = relativePath;
                    try
                    {
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                    catch
                    {
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                        throw;
                    }
                }

                // Xóa cache
                await _cacheService.RemoveAsync(CacheKeys.AllProducts);
                await _cacheService.RemoveAsync(CacheKeys.ProductById(productId));

                return Ok(new { success = true, message = "Upload video thành công!", videoPath = relativePath });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ApiErrorResponse.FromException(ErrorCodes.INTERNAL_ERROR, ex));
            }
        }

        [HttpDelete("{productId}/video")]
        public async Task<ActionResult> DeleteProductVideo(Guid productId)
        {
            var productEntity = await _context.Products!.FirstOrDefaultAsync(p => p.Id == productId);
            if (productEntity == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.PRODUCT_NOT_FOUND));
            }

            if (!string.IsNullOrEmpty(productEntity.VideoPath))
            {
                if (!productEntity.VideoPath.StartsWith("http"))
                {
                    var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var filePath = Path.Combine(webRootPath, productEntity.VideoPath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                productEntity.VideoPath = null;
                await _context.SaveChangesAsync();

                // Xóa cache
                await _cacheService.RemoveAsync(CacheKeys.AllProducts);
                await _cacheService.RemoveAsync(CacheKeys.ProductById(productId));
            }

            return Ok(new ApiSuccessResponse("Xóa video sản phẩm thành công."));
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportProducts([FromBody] List<ProductCreateDto> products)
        {
            try
            {
                if (products == null || !products.Any())
                {
                    return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.PRODUCT_IMPORT_EMPTY));
                }

                foreach (var product in products)
                {
                    await _productRepository.CreateProductAsync(product);
                }

                // Xóa cache
                await _cacheService.RemoveAsync(CacheKeys.AllProducts);

                return Ok(new ApiSuccessResponse("Import sản phẩm thành công."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiErrorResponse.FromException(ErrorCodes.PRODUCT_IMPORT_FAILED, ex));
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
                return StatusCode(500, ApiErrorResponse.FromException(ErrorCodes.PRODUCT_SEARCH_FAILED, ex));
            }
        }

        [HttpPost("recompress-images")]
        public async Task<ActionResult> RecompressAllImages([FromQuery] int quality = 55, [FromQuery] int maxWidth = 800)
        {
            try
            {
                var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var imageDir = Path.Combine(webRootPath, "ImageImport");

                if (!Directory.Exists(imageDir))
                {
                    return NotFound(ApiErrorResponse.FromCode(ErrorCodes.PRODUCT_IMAGE_DIR_NOT_FOUND));
                }

                var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
                var files = Directory.GetFiles(imageDir)
                    .Where(f => imageExtensions.Contains(Path.GetExtension(f).ToLower()))
                    .ToList();

                int optimized = 0;
                int skipped = 0;
                long savedBytes = 0;

                foreach (var file in files)
                {
                    var originalSize = new FileInfo(file).Length;
                    var result = await _imageOptimizer.OptimizeExistingFileAsync(file, maxWidth, quality, minSizeBytes: 0);
                    if (result)
                    {
                        var newSize = new FileInfo(file).Length;
                        savedBytes += originalSize - newSize;
                        optimized++;
                    }
                    else
                    {
                        skipped++;
                    }
                }

                // Xóa cache ảnh
                await _cacheService.RemoveAsync(CacheKeys.AllProducts);

                return Ok(new
                {
                    message = $"Hoàn tất nén ảnh: {optimized} ảnh đã nén, {skipped} ảnh bỏ qua",
                    totalFiles = files.Count,
                    optimized,
                    skipped,
                    savedMB = Math.Round(savedBytes / 1024.0 / 1024.0, 2)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiErrorResponse.FromException(ErrorCodes.PRODUCT_COMPRESS_FAILED, ex));
            }
        }
    }
}
