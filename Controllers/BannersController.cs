using Microsoft.AspNetCore.Mvc;
using LampStoreProjects.Data;
using LampStoreProjects.Repositories;
using LampStoreProjects.Models;
using LampStoreProjects.Services;
using LampStoreProjects.Helpers;

namespace LampStoreProjects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BannersController : ControllerBase
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly IImageUploadService _imageService;
        private readonly ICacheService _cacheService;

        public BannersController(IBannerRepository bannerRepository, IWebHostEnvironment environment, IImageUploadService imageService, ICacheService cacheService)
        {
            _bannerRepository = bannerRepository;
            _environment = environment;
            _imageService = imageService;
            _cacheService = cacheService;
        }

        // GET: api/banners
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BannerModel>>> GetBanners()
        {
            var cachedBanners = await _cacheService.GetAsync<IEnumerable<BannerModel>>(CacheKeys.AllBanners);
            if (cachedBanners != null)
            {
                return Ok(cachedBanners);
            }

            var banners = await _bannerRepository.GetAllAsync();
            await _cacheService.SetAsync(CacheKeys.AllBanners, banners, TimeSpan.FromMinutes(15));
            return Ok(banners);
        }

        // GET: api/banners/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<BannerModel>>> GetActiveBanners()
        {
            var cachedBanners = await _cacheService.GetAsync<IEnumerable<BannerModel>>(CacheKeys.ActiveBanners);
            if (cachedBanners != null)
            {
                return Ok(cachedBanners);
            }

            var banners = await _bannerRepository.GetActiveBannersAsync();
            await _cacheService.SetAsync(CacheKeys.ActiveBanners, banners, TimeSpan.FromMinutes(15));
            return Ok(banners);
        }

        // GET: api/banners/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BannerModel>> GetBanner(int id)
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.BANNER_NOT_FOUND));
            }

            return Ok(banner);
        }

        // POST: api/banners
        [HttpPost]
        public async Task<ActionResult<BannerModel>> CreateBanner([FromBody] BannerModel banner)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiErrorResponse.WithErrors(ErrorCodes.VALIDATION_FAILED, ModelState));
            }

            var createdBanner = await _bannerRepository.CreateAsync(banner);
            await ClearBannerCacheAsync();
            return CreatedAtAction(nameof(GetBanner), new { id = createdBanner.Id }, createdBanner);
        }

        // PUT: api/banners/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBanner(int id, [FromBody] BannerModel banner)
        {
            if (id != banner.Id)
            {
                return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.BANNER_ID_MISMATCH));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiErrorResponse.WithErrors(ErrorCodes.VALIDATION_FAILED, ModelState));
            }

            var existingBanner = await _bannerRepository.GetByIdAsync(id);
            if (existingBanner == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.BANNER_NOT_FOUND));
            }

            await _bannerRepository.UpdateAsync(banner);
            await ClearBannerCacheAsync();
            return NoContent();
        }

        // DELETE: api/banners/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.BANNER_NOT_FOUND));
            }

            await _bannerRepository.DeleteAsync(id);
            await ClearBannerCacheAsync();
            return NoContent();
        }

        // POST: api/banners/upload
        [HttpPost("upload")]
        public async Task<ActionResult<string>> UploadImage([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.BANNER_NO_FILE));
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.BANNER_INVALID_FILE_TYPE));
            }

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.BANNER_FILE_TOO_LARGE));
            }

            try
            {
                // Upload vào wwwroot/ImageImport
                var imageUrl = await _imageService.UploadImageAsync(file, "ImageImport");

                return Ok(new { imageUrl = imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiErrorResponse.FromException(ErrorCodes.INTERNAL_ERROR, ex));
            }
        }

        private async Task ClearBannerCacheAsync()
        {
            await _cacheService.RemoveAsync(CacheKeys.AllBanners);
            await _cacheService.RemoveAsync(CacheKeys.ActiveBanners);
        }
    }
}
