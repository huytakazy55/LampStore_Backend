using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using LampStoreProjects.Models;
using LampStoreProjects.Repositories;
using LampStoreProjects.Services;
using Microsoft.Extensions.Caching.Memory;

namespace LampStoreProjects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly IImageUploadService _imageService;
        private readonly ICacheService _cacheService;

        public CategoriesController(ICategoryRepository categoryRepository, IWebHostEnvironment environment, IImageUploadService imageService, ICacheService cacheService)
        {
            _categoryRepository = categoryRepository;
            _environment = environment;
            _imageService = imageService;
            _cacheService = cacheService;
        }

        [HttpGet]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<IEnumerable<CategoryModel>>> GetCategories()
        {
            // Kiểm tra cache trước
            var cachedCategories = await _cacheService.GetAsync<IEnumerable<CategoryModel>>(CacheKeys.AllCategories);
            if (cachedCategories != null)
            {
                return Ok(cachedCategories);
            }

            var categories = await _categoryRepository.GetAllAsync();
            
            // Lưu vào cache với thời gian expire 15 phút
            await _cacheService.SetAsync(CacheKeys.AllCategories, categories, TimeSpan.FromMinutes(15));
            
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryModel>> GetCategory(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }

        [HttpPost]
        public async Task<ActionResult> CreateCategory(CategoryModel categoryModel)
        {
            await _categoryRepository.AddAsync(categoryModel);
            await _cacheService.RemoveAsync(CacheKeys.AllCategories);
            return CreatedAtAction(nameof(GetCategory), new { id = categoryModel.Id }, categoryModel);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCategory(Guid id, CategoryModel categoryModel)
        {
            if (id != categoryModel.Id)
            {
                return BadRequest();
            }
            await _categoryRepository.UpdateAsync(categoryModel);

            // Xóa cache
            await _cacheService.RemoveAsync(CacheKeys.AllCategories);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCategory(Guid id)
        {
            await _categoryRepository.DeleteAsync(id);
            await _cacheService.RemoveAsync(CacheKeys.AllCategories);
            return NoContent();
        }

        [HttpDelete("bulk")]
        public async Task<ActionResult> BulkDeleteCategories(List<Guid> ids)
        {
            await _categoryRepository.BulkDeleteAsync(ids);
            await _cacheService.RemoveAsync(CacheKeys.AllCategories);
            return NoContent();
        }

        // POST: api/categories/upload
        [HttpPost("upload")]
        public async Task<ActionResult<string>> UploadImage([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            // Validate file type - ưu tiên kiểm tra content type
            var allowedMimeTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            
            var contentType = file.ContentType.ToLowerInvariant();
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            // Kiểm tra content type trước, sau đó mới kiểm tra extension
            if (!allowedMimeTypes.Contains(contentType) && !allowedExtensions.Contains(fileExtension))
            {
                return BadRequest($"Invalid file type. File: {file.FileName}, Extension: {fileExtension}, ContentType: {contentType}. Only JPG, JPEG, PNG, and GIF are allowed.");
            }

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest("File size too large. Maximum size is 5MB.");
            }

            try
            {
                // Upload vào wwwroot/ImageImport
                var imageUrl = await _imageService.UploadImageAsync(file, "ImageImport");

                // Xóa cache danh mục
                await _cacheService.RemoveAsync(CacheKeys.AllCategories);

                return Ok(new { imageUrl = imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}