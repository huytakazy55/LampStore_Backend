using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using LampStoreProjects.Models;
using LampStoreProjects.Repositories;

namespace LampStoreProjects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _environment;

        public CategoriesController(ICategoryRepository categoryRepository, IWebHostEnvironment environment)
        {
            _categoryRepository = categoryRepository;
            _environment = environment;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryModel>>> GetCategories()
        {
            var categories = await _categoryRepository.GetAllAsync();
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
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCategory(Guid id)
        {
            await _categoryRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpDelete("bulk")]
        public async Task<ActionResult> BulkDeleteCategories(List<Guid> ids)
        {
            await _categoryRepository.BulkDeleteAsync(ids);
            return NoContent();
        }

        // POST: api/categories/upload
        [HttpPost("upload")]
        public async Task<ActionResult<string>> UploadImage(IFormFile file)
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
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "CategoryImages");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Guid.NewGuid().ToString() + fileExtension;
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var imageUrl = $"/CategoryImages/{fileName}";
                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}