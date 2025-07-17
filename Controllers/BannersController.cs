using Microsoft.AspNetCore.Mvc;
using LampStoreProjects.Data;
using LampStoreProjects.Repositories;
using LampStoreProjects.Models;

namespace LampStoreProjects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BannersController : ControllerBase
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly IWebHostEnvironment _environment;

        public BannersController(IBannerRepository bannerRepository, IWebHostEnvironment environment)
        {
            _bannerRepository = bannerRepository;
            _environment = environment;
        }

        // GET: api/banners
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BannerModel>>> GetBanners()
        {
            var banners = await _bannerRepository.GetAllAsync();
            return Ok(banners);
        }

        // GET: api/banners/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<BannerModel>>> GetActiveBanners()
        {
            var banners = await _bannerRepository.GetActiveBannersAsync();
            return Ok(banners);
        }

        // GET: api/banners/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BannerModel>> GetBanner(int id)
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null)
            {
                return NotFound();
            }

            return Ok(banner);
        }

        // POST: api/banners
        [HttpPost]
        public async Task<ActionResult<BannerModel>> CreateBanner([FromBody] BannerModel banner)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdBanner = await _bannerRepository.CreateAsync(banner);
            return CreatedAtAction(nameof(GetBanner), new { id = createdBanner.Id }, createdBanner);
        }

        // PUT: api/banners/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBanner(int id, [FromBody] BannerModel banner)
        {
            if (id != banner.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingBanner = await _bannerRepository.GetByIdAsync(id);
            if (existingBanner == null)
            {
                return NotFound();
            }

            await _bannerRepository.UpdateAsync(banner);
            return NoContent();
        }

        // DELETE: api/banners/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null)
            {
                return NotFound();
            }

            await _bannerRepository.DeleteAsync(id);
            return NoContent();
        }

        // POST: api/banners/upload
        [HttpPost("upload")]
        public async Task<ActionResult<string>> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Invalid file type. Only JPG, JPEG, PNG, and GIF are allowed.");
            }

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest("File size too large. Maximum size is 5MB.");
            }

            try
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "BannerImages");
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

                var imageUrl = $"/BannerImages/{fileName}";
                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
} 