using LampStoreProjects.Data;
using LampStoreProjects.DTOs;
using LampStoreProjects.Services;
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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NewsDto>>> GetNews([FromQuery] bool activeOnly = true)
        {
            var query = context.News!.AsQueryable();
            
            if (activeOnly)
            {
                query = query.Where(n => n.IsActive);
            }

            var newsList = await query
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NewsDto
                {
                    Id = n.Id,
                    Title = n.Title,
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

        // GET: api/News/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NewsDto>> GetNewsItem(Guid id)
        {
            var news = await context.News!.FindAsync(id);

            if (news == null)
            {
                return NotFound();
            }

            news.ViewCount += 1;
            await context.SaveChangesAsync();

            return new NewsDto
            {
                Id = news.Id,
                Title = news.Title,
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
        public async Task<ActionResult<NewsDto>> PostNews(NewsCreateDto dto)
        {
            var news = new News
            {
                Title = dto.Title,
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
        public async Task<IActionResult> PutNews(Guid id, NewsUpdateDto dto)
        {
            var news = await context.News!.FindAsync(id);

            if (news == null)
            {
                return NotFound();
            }

            news.Title = dto.Title;
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
                    return NotFound();
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
        public async Task<IActionResult> DeleteNews(Guid id)
        {
            var news = await context.News!.FindAsync(id);
            if (news == null)
            {
                return NotFound();
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
        public async Task<ActionResult<object>> UploadImage([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            try
            {
                var imageUrl = await imageService.UploadImageAsync(file, "NewsImages");
                return Ok(new { imageUrl });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
