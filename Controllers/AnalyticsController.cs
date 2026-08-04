using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using LampStoreProjects.Services;
using System;
using Microsoft.AspNetCore.Authorization;
using LampStoreProjects.Helpers;
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
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> GetOverview()
        {
            var data = await _analyticsService.GetDashboardOverviewAsync();
            return Ok(data);
        }

        [HttpGet("sales-overview")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> GetSalesOverview()
        {
            var data = await _analyticsService.GetSalesOverviewAsync();
            return Ok(data);
        }

        [HttpGet("visitor-locations")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> GetVisitorLocations([FromQuery] int days = 30, [FromQuery] int limit = 100)
        {
            var data = await _analyticsService.GetVisitorLocationsAsync(days, limit);
            return Ok(data);
        }

        // SECURITY: do NOT read raw client headers (X-Forwarded-For, CF-Connecting-IP, etc.)
        // here — they are client-controlled and trivially spoofable, letting any caller
        // inject a fake location into the visitor map or hide their real IP. Use
        // HttpContext.Connection.RemoteIpAddress instead: UseForwardedHeaders() (configured
        // in Program.cs) only rewrites this value from X-Forwarded-For when the request came
        // through a proxy listed in the "TrustedProxies" config, so for direct/untrusted
        // traffic this always reflects the real TCP peer. Matches GetRateLimitPartitionKey
        // in RateLimitExtensions.cs.
        private string GetClientIpAddress()
        {
            var ip = HttpContext.Connection.RemoteIpAddress;
            if (ip == null)
            {
                return "unknown";
            }

            if (ip.IsIPv4MappedToIPv6)
            {
                ip = ip.MapToIPv4();
            }

            return ip.ToString();
        }
    }
}
