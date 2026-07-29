using LampStoreProjects.Data;
using LampStoreProjects.DTOs;
using LampStoreProjects.Services;
using LampStoreProjects.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LampStoreProjects.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController(ApplicationDbContext context, IImageUploadService imageService) : ControllerBase
    {
        // GET: api/News
        // NOTE on pagination defaults: this endpoint is shared by the public storefront
        // (activeOnly=true, no page/pageSize passed today — it expects the full active list
        // in one call) and the admin news management screen (which passes its own explicit
        // page/pageSize). To avoid silently truncating the public listing if active-news
        // count grows past a typical "page", the default pageSize here is 50 rather than the
        // usual 20 used elsewhere. Admin callers that explicitly pass page/pageSize are
        // unaffected by this default.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NewsDto>>> GetNews([FromQuery] bool activeOnly = true, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            if (page < 1) page = 1;
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = context.News!.AsQueryable();

            if (activeOnly)
            {
                query = query.Where(n => n.IsActive);
            }

            var totalCount = await query.CountAsync();
            Response.Headers["X-Total-Count"] = totalCount.ToString();

            var newsList = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NewsDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Slug = n.Slug,
                    Excerpt = n.Excerpt,
                    Content = n.Content,
                    ImageUrl = n.ImageUrl,
                    Category = n.Category,
                    IsActive = n.IsActive,
                    CreatedAt = n.CreatedAt,
                    UpdatedAt = n.UpdatedAt,
                    ViewCount = n.ViewCount
                })
                .ToListAsync();

            return Ok(newsList);
        }

        private async Task<string> GenerateUniqueNewsSlugAsync(string title, Guid? newsId = null)
        {
            var baseSlug = LampStoreProjects.Helpers.SlugHelper.GenerateSlug(title);
            var slug = baseSlug;
            int counter = 1;

            var query = context.News!.AsQueryable();
            if (newsId.HasValue)
            {
                query = query.Where(n => n.Id != newsId.Value);
            }

            while (await query.AnyAsync(n => n.Slug == slug))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        // GET: api/News/slug/5
        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<NewsDto>> GetNewsItemBySlug(string slug)
        {
            var news = await context.News!.FirstOrDefaultAsync(n => n.Slug == slug);

            if (news == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.NEWS_NOT_FOUND));
            }

            news.ViewCount += 1;
            await context.SaveChangesAsync();

            return new NewsDto
            {
                Id = news.Id,
                Title = news.Title,
                Slug = news.Slug,
                Excerpt = news.Excerpt,
                Content = news.Content,
                ImageUrl = news.ImageUrl,
                Category = news.Category,
                IsActive = news.IsActive,
                CreatedAt = news.CreatedAt,
                UpdatedAt = news.UpdatedAt,
                ViewCount = news.ViewCount
            };
        }

        // GET: api/News/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NewsDto>> GetNewsItem(Guid id)
        {
            var news = await context.News!.FindAsync(id);

            if (news == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.NEWS_NOT_FOUND));
            }

            news.ViewCount += 1;
            await context.SaveChangesAsync();

            return new NewsDto
            {
                Id = news.Id,
                Title = news.Title,
                Slug = news.Slug,
                Excerpt = news.Excerpt,
                Content = news.Content,
                ImageUrl = news.ImageUrl,
                Category = news.Category,
                IsActive = news.IsActive,
                CreatedAt = news.CreatedAt,
                UpdatedAt = news.UpdatedAt,
                ViewCount = news.ViewCount
            };
        }

        // POST: api/News
        [HttpPost]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<ActionResult<NewsDto>> PostNews(NewsCreateDto dto)
        {
            var news = new News
            {
                Title = dto.Title,
                Slug = await GenerateUniqueNewsSlugAsync(dto.Title),
                Excerpt = dto.Excerpt,
                Content = dto.Content,
                ImageUrl = dto.ImageUrl,
                Category = dto.Category,
                IsActive = dto.IsActive
            };

            context.News!.Add(news);
            await context.SaveChangesAsync();

            var result = new NewsDto
            {
                Id = news.Id,
                Title = news.Title,
                Slug = news.Slug,
                Excerpt = news.Excerpt,
                Content = news.Content,
                ImageUrl = news.ImageUrl,
                Category = news.Category,
                IsActive = news.IsActive,
                CreatedAt = news.CreatedAt,
                ViewCount = news.ViewCount
            };

            return CreatedAtAction("GetNewsItem", new { id = news.Id }, result);
        }

        // PUT: api/News/5
        [HttpPut("{id}")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> PutNews(Guid id, NewsUpdateDto dto)
        {
            var news = await context.News!.FindAsync(id);

            if (news == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.NEWS_NOT_FOUND));
            }

            news.Title = dto.Title;
            news.Slug = await GenerateUniqueNewsSlugAsync(dto.Title, news.Id);
            news.Excerpt = dto.Excerpt;
            news.Content = dto.Content;
            news.ImageUrl = dto.ImageUrl;
            news.Category = dto.Category;
            news.IsActive = dto.IsActive;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NewsExists(id))
                {
                    return NotFound(ApiErrorResponse.FromCode(ErrorCodes.NEWS_NOT_FOUND));
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/News/5
        [HttpDelete("{id}")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> DeleteNews(Guid id)
        {
            var news = await context.News!.FindAsync(id);
            if (news == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.NEWS_NOT_FOUND));
            }

            context.News.Remove(news);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool NewsExists(Guid id)
        {
            return context.News!.Any(e => e.Id == id);
        }

        // POST: api/News/upload
        [HttpPost("upload")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<ActionResult<object>> UploadImage([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.NEWS_NO_FILE));

            try
            {
                var imageUrl = await imageService.UploadImageAsync(file, "NewsImages");
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
    }
}
