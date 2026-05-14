using LampStoreProjects.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LampStoreProjects.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public AnalyticsService(ApplicationDbContext context, HttpClient httpClient, IMemoryCache cache)
        {
            _context = context;
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task TrackVisitAsync(string sessionId, string ipAddress, string path, Guid? productId)
        {
            ipAddress = NormalizeIp(ipAddress);

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

        public async Task<object> GetVisitorLocationsAsync(int days = 30, int limit = 100)
        {
            days = Math.Clamp(days, 1, 365);
            limit = Math.Clamp(limit, 1, 100);
            var fromDate = DateTime.UtcNow.AddDays(-days);

            var ipGroups = await _context.SiteVisits
                .Where(v => v.VisitedAt >= fromDate && v.IpAddress != null && v.IpAddress != "")
                .GroupBy(v => v.IpAddress)
                .Select(g => new
                {
                    IpAddress = g.Key,
                    VisitCount = g.Count(),
                    UniqueVisitors = g.Select(v => v.SessionId).Distinct().Count(),
                    FirstVisit = g.Min(v => v.VisitedAt),
                    LastVisit = g.Max(v => v.VisitedAt)
                })
                .OrderByDescending(x => x.VisitCount)
                .Take(limit)
                .ToListAsync();

            var publicIps = ipGroups
                .Select(g => NormalizeIp(g.IpAddress))
                .Where(ip => IsPublicIp(ip))
                .Distinct()
                .Take(100)
                .ToList();

            var locations = await ResolveIpLocationsAsync(publicIps);

            var items = ipGroups.Select(g =>
            {
                var ip = NormalizeIp(g.IpAddress);
                locations.TryGetValue(ip, out var geo);

                return new
                {
                    ipAddress = MaskIp(ip),
                    rawIpAddress = ip,
                    g.VisitCount,
                    g.UniqueVisitors,
                    g.FirstVisit,
                    g.LastVisit,
                    isPublicIp = IsPublicIp(ip),
                    country = geo?.Country,
                    countryCode = geo?.CountryCode,
                    region = geo?.RegionName,
                    city = geo?.City,
                    latitude = geo?.Lat,
                    longitude = geo?.Lon,
                    isp = geo?.Isp,
                    status = geo?.Status ?? (IsPublicIp(ip) ? "unresolved" : "private")
                };
            }).ToList();

            var locationSummary = items
                .Where(x => x.latitude != null && x.longitude != null)
                .GroupBy(x => new { x.country, x.region, x.city, x.latitude, x.longitude })
                .Select(g => new
                {
                    g.Key.country,
                    g.Key.region,
                    g.Key.city,
                    g.Key.latitude,
                    g.Key.longitude,
                    visitCount = g.Sum(x => x.VisitCount),
                    uniqueVisitors = g.Sum(x => x.UniqueVisitors),
                    ipCount = g.Count()
                })
                .OrderByDescending(x => x.visitCount)
                .ToList();

            return new
            {
                days,
                totalIpCount = ipGroups.Count,
                resolvedIpCount = items.Count(x => x.latitude != null && x.longitude != null),
                privateIpCount = items.Count(x => !x.isPublicIp),
                unresolvedIpCount = items.Count(x => x.isPublicIp && (x.latitude == null || x.longitude == null)),
                locations = locationSummary,
                ipVisits = items
            };
        }

        private async Task<Dictionary<string, IpGeoResult>> ResolveIpLocationsAsync(List<string> ips)
        {
            var results = new Dictionary<string, IpGeoResult>();
            var missingIps = new List<string>();

            foreach (var ip in ips)
            {
                if (_cache.TryGetValue(CacheKey(ip), out IpGeoResult cached))
                {
                    results[ip] = cached;
                }
                else
                {
                    missingIps.Add(ip);
                }
            }

            if (missingIps.Count == 0)
            {
                return results;
            }

            try
            {
                var fields = "status,message,country,countryCode,regionName,city,lat,lon,isp,query";
                var response = await _httpClient.PostAsJsonAsync($"http://ip-api.com/batch?fields={fields}", missingIps);
                if (response.IsSuccessStatusCode)
                {
                    var geoItems = await response.Content.ReadFromJsonAsync<List<IpGeoResult>>();
                    foreach (var item in geoItems ?? new List<IpGeoResult>())
                    {
                        if (string.IsNullOrWhiteSpace(item.Query))
                        {
                            continue;
                        }

                        var ip = NormalizeIp(item.Query);
                        results[ip] = item;
                        _cache.Set(CacheKey(ip), item, TimeSpan.FromHours(12));
                    }
                }
            }
            catch
            {
                // Keep analytics usable even when the external geo provider is unavailable.
            }

            return results;
        }

        private static string NormalizeIp(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return "unknown";
            }

            var ip = ipAddress.Split(',')[0].Trim();
            if (ip.StartsWith("::ffff:", StringComparison.OrdinalIgnoreCase))
            {
                ip = ip.Substring("::ffff:".Length);
            }

            return ip;
        }

        private static bool IsPublicIp(string ipAddress)
        {
            if (!IPAddress.TryParse(ipAddress, out var ip))
            {
                return false;
            }

            if (IPAddress.IsLoopback(ip))
            {
                return false;
            }

            var bytes = ip.GetAddressBytes();
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return bytes[0] switch
                {
                    10 => false,
                    172 when bytes[1] >= 16 && bytes[1] <= 31 => false,
                    192 when bytes[1] == 168 => false,
                    169 when bytes[1] == 254 => false,
                    _ => true
                };
            }

            return !ip.IsIPv6LinkLocal && !ip.IsIPv6SiteLocal && !ip.IsIPv6Multicast;
        }

        private static string MaskIp(string ipAddress)
        {
            if (!IPAddress.TryParse(ipAddress, out var ip))
            {
                return ipAddress;
            }

            var bytes = ip.GetAddressBytes();
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && bytes.Length == 4)
            {
                return $"{bytes[0]}.{bytes[1]}.{bytes[2]}.x";
            }

            var parts = ipAddress.Split(':');
            return parts.Length > 4 ? string.Join(':', parts.Take(4)) + ":xxxx" : ipAddress;
        }

        private static string CacheKey(string ipAddress) => $"analytics:geo:{ipAddress}";

        private class IpGeoResult
        {
            [JsonPropertyName("status")]
            public string Status { get; set; }

            [JsonPropertyName("message")]
            public string Message { get; set; }

            [JsonPropertyName("country")]
            public string Country { get; set; }

            [JsonPropertyName("countryCode")]
            public string CountryCode { get; set; }

            [JsonPropertyName("regionName")]
            public string RegionName { get; set; }

            [JsonPropertyName("city")]
            public string City { get; set; }

            [JsonPropertyName("lat")]
            public double? Lat { get; set; }

            [JsonPropertyName("lon")]
            public double? Lon { get; set; }

            [JsonPropertyName("isp")]
            public string Isp { get; set; }

            [JsonPropertyName("query")]
            public string Query { get; set; }
        }
    }
}
