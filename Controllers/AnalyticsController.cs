using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using LampStoreProjects.Services;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace LampStoreProjects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        public class TrackRequest
        {
            public string SessionId { get; set; }
            public string Path { get; set; }
            public Guid? ProductId { get; set; }
        }

        [HttpPost("track")]
        public async Task<IActionResult> TrackVisit([FromBody] TrackRequest req)
        {
            var ip = GetClientIpAddress();

            await _analyticsService.TrackVisitAsync(req.SessionId, ip, req.Path, req.ProductId);
            
            return Ok(new { success = true });
        }

        [HttpGet("overview")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOverview()
        {
            var data = await _analyticsService.GetDashboardOverviewAsync();
            return Ok(data);
        }

        [HttpGet("sales-overview")]
        public async Task<IActionResult> GetSalesOverview()
        {
            var data = await _analyticsService.GetSalesOverviewAsync();
            return Ok(data);
        }

        [HttpGet("visitor-locations")]
        public async Task<IActionResult> GetVisitorLocations([FromQuery] int days = 30, [FromQuery] int limit = 100)
        {
            var data = await _analyticsService.GetVisitorLocationsAsync(days, limit);
            return Ok(data);
        }

        private string GetClientIpAddress()
        {
            var candidateHeaders = new[]
            {
                "CF-Connecting-IP",
                "True-Client-IP",
                "X-Real-IP",
                "X-Forwarded-For"
            };

            foreach (var header in candidateHeaders)
            {
                if (!Request.Headers.TryGetValue(header, out var values))
                {
                    continue;
                }

                foreach (var value in values)
                {
                    var ips = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var publicIp = ips.FirstOrDefault(IsPublicIp);
                    if (!string.IsNullOrWhiteSpace(publicIp))
                    {
                        return publicIp;
                    }

                    var firstValidIp = ips.FirstOrDefault(ip => IPAddress.TryParse(NormalizeIp(ip), out _));
                    if (!string.IsNullOrWhiteSpace(firstValidIp))
                    {
                        return NormalizeIp(firstValidIp);
                    }
                }
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private static string NormalizeIp(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return "unknown";
            }

            var ip = ipAddress.Trim();
            return ip.StartsWith("::ffff:", StringComparison.OrdinalIgnoreCase)
                ? ip.Substring("::ffff:".Length)
                : ip;
        }

        private static bool IsPublicIp(string ipAddress)
        {
            ipAddress = NormalizeIp(ipAddress);
            if (!IPAddress.TryParse(ipAddress, out var ip) || IPAddress.IsLoopback(ip))
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
    }
}
