using LampStoreProjects.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LampStoreProjects.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task TrackVisitAsync(string sessionId, string ipAddress, string path, Guid? productId)
        {
            // Auto-resolve ProductId from slug if path is a product page
            bool isProductPage = false;
            if (!string.IsNullOrEmpty(path) && path.StartsWith("/product/"))
            {
                isProductPage = true;
                if (productId == null)
                {
                    var slug = path.Substring("/product/".Length).Split('?')[0].Trim('/');
                    if (!string.IsNullOrEmpty(slug))
                    {
                        var product = await _context.Products
                            .Where(p => p.Slug == slug)
                            .Select(p => new { p.Id })
                            .FirstOrDefaultAsync();
                        if (product != null)
                        {
                            productId = product.Id;
                        }
                    }
                }
            }

            // Với trang thường (không phải sản phẩm): chỉ ghi 1 lần/session/path
            if (!isProductPage)
            {
                var alreadyTracked = await _context.SiteVisits
                    .AnyAsync(v => v.SessionId == sessionId && v.Path == path);
                if (alreadyTracked)
                    return;
            }

            var visit = new SiteVisit
            {
                SessionId = sessionId ?? "anonymous",
                IpAddress = ipAddress,
                Path = path,
                ProductId = productId,
                VisitedAt = DateTime.UtcNow
            };

            _context.SiteVisits.Add(visit);
            await _context.SaveChangesAsync();
        }

        public async Task<object> GetDashboardOverviewAsync()
        {
            // Calculate total unique visitors (by SessionId) - 30 days
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var totalVisits = await _context.SiteVisits
                .CountAsync();

            var visits30Days = await _context.SiteVisits
                .Where(v => v.VisitedAt >= thirtyDaysAgo)
                .Select(v => v.SessionId)
                .Distinct()
                .CountAsync();

            // Lấy ra Top sản phẩm xem nhiều nhất trong 30 ngày
            var topViewedProducts = await _context.SiteVisits
                .Where(v => v.ProductId != null && v.VisitedAt >= thirtyDaysAgo)
                .GroupBy(v => v.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    ViewCount = g.Count()
                })
                .OrderByDescending(x => x.ViewCount)
                .Take(10)
                .Join(_context.Products, 
                    v => v.ProductId, 
                    p => p.Id, 
                    (v, p) => new { p.Id, p.Name, v.ViewCount })
                .ToListAsync();

            // Lấy ra Top trang truy cập nhiều nhất trong 30 ngày
            var topViewedPaths = await _context.SiteVisits
                .Where(v => v.Path != null && v.VisitedAt >= thirtyDaysAgo)
                .GroupBy(v => v.Path)
                .Select(g => new
                {
                    Path = g.Key,
                    ViewCount = g.Count()
                })
                .OrderByDescending(x => x.ViewCount)
                .Take(10)
                .ToListAsync();

            // Lượt truy cập theo ngày (30 ngày gần nhất) - cho biểu đồ
            var dailyVisitsRaw = await _context.SiteVisits
                .Where(v => v.VisitedAt >= thirtyDaysAgo)
                .GroupBy(v => v.VisitedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Visits = g.Count(),
                    Sessions = g.Select(x => x.SessionId).Distinct().ToList(),
                    ProductViews = g.Count(x => x.ProductId != null)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var dailyVisits = dailyVisitsRaw.Select(d => new
            {
                Date = d.Date.ToString("dd/MM"),
                d.Visits,
                UniqueVisitors = d.Sessions.Count,
                d.ProductViews
            }).ToList();

            var productCount = await _context.Products.CountAsync();
            var categoryCount = await _context.Categories.CountAsync();
            var orderCount = await _context.Orders.CountAsync();

            return new
            {
                siteVisits = totalVisits,
                uniqueVisits30Days = visits30Days,
                productCount,
                categoryCount,
                orderCount,
                topProducts = topViewedProducts,
                topPaths = topViewedPaths,
                dailyVisits
            };
        }

        public async Task<object> GetSalesOverviewAsync()
        {
            var now = DateTime.UtcNow;
            var thirtyDaysAgo = now.AddDays(-30);
            var yearStart = new DateTime(now.Year, 1, 1);

            // Doanh số theo ngày (30 ngày gần nhất) - cho Line Chart
            var dailySalesRaw = await _context.Orders
                .Where(o => o.OrderDate >= thirtyDaysAgo)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var dailySales = dailySalesRaw.Select(d => new
            {
                Name = d.Date.ToString("dd/MM"),
                d.Revenue,
                d.OrderCount
            }).ToList();

            // Doanh số theo tháng (năm hiện tại) - cho Pie Chart
            var monthlySalesRaw = await _context.Orders
                .Where(o => o.OrderDate >= yearStart)
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            var monthNames = new[] { "", "Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6",
                "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12" };

            var monthlySales = monthlySalesRaw.Select(m => new
            {
                Name = monthNames[m.Month],
                Value = m.Revenue,
                m.OrderCount
            }).ToList();

            // Doanh số theo tuần (4 tuần gần nhất)
            var fourWeeksAgo = now.AddDays(-28);
            var weeklySalesRaw = await _context.Orders
                .Where(o => o.OrderDate >= fourWeeksAgo)
                .ToListAsync();

            var weeklySales = weeklySalesRaw
                .GroupBy(o => (int)((now - o.OrderDate).TotalDays / 7))
                .Select(g => new
                {
                    Name = g.Key == 0 ? "Tuần này" : $"Tuần -{g.Key}",
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count(),
                    WeekIndex = g.Key
                })
                .OrderByDescending(x => x.WeekIndex)
                .ToList();

            return new
            {
                dailySales,
                weeklySales,
                monthlySales
            };
        }
    }
}
