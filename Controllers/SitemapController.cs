using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LampStoreProjects.Data;
using System.Text;

namespace LampStoreProjects.Controllers
{
    [ApiController]
    public class SitemapController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private const string SITE_URL = "https://capylumine.com";

        public SitemapController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("/sitemap-dynamic.xml")]
        [ResponseCache(Duration = 3600)] // Cache 1 giờ
        public async Task<IActionResult> GetSitemap()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            // Trang tĩnh
            AppendUrl(sb, $"{SITE_URL}/", "daily", "1.0");
            AppendUrl(sb, $"{SITE_URL}/categories", "weekly", "0.8");
            AppendUrl(sb, $"{SITE_URL}/news", "weekly", "0.7");

            // Tất cả sản phẩm
            var products = await _context.Products!
                .Where(p => p.Status == true)
                .OrderByDescending(p => p.UpdatedAt)
                .Select(p => new { p.Id, p.UpdatedAt })
                .ToListAsync();

            foreach (var p in products)
            {
                var lastmod = p.UpdatedAt?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
                AppendUrl(sb, $"{SITE_URL}/product/{p.Id}", "weekly", "0.8", lastmod);
            }

            // Tất cả danh mục
            var categories = await _context.Categories!
                .Where(c => c.IsDisplayed == true)
                .Select(c => new { c.Id })
                .ToListAsync();

            foreach (var c in categories)
            {
                AppendUrl(sb, $"{SITE_URL}/categories/{c.Id}", "weekly", "0.7");
            }

            // Tất cả bài viết
            var newsList = await _context.News!
                .Where(n => n.IsActive)
                .OrderByDescending(n => n.UpdatedAt)
                .Select(n => new { n.Id, n.UpdatedAt })
                .ToListAsync();

            foreach (var n in newsList)
            {
                var lastmod = n.UpdatedAt?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
                AppendUrl(sb, $"{SITE_URL}/news/{n.Id}", "monthly", "0.6", lastmod);
            }

            sb.AppendLine("</urlset>");

            return Content(sb.ToString(), "application/xml", Encoding.UTF8);
        }

        private static void AppendUrl(StringBuilder sb, string loc, string changefreq, string priority, string? lastmod = null)
        {
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{loc}</loc>");
            if (lastmod != null)
                sb.AppendLine($"    <lastmod>{lastmod}</lastmod>");
            sb.AppendLine($"    <changefreq>{changefreq}</changefreq>");
            sb.AppendLine($"    <priority>{priority}</priority>");
            sb.AppendLine("  </url>");
        }
    }
}
