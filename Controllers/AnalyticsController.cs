using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using LampStoreProjects.Services;
using System;
using Microsoft.AspNetCore.Authorization;

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
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            
            // Note: Cloudflare or proxies might hide the real IP. 
            // In a real prod environment we'd check headers like X-Forwarded-For
            if (HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
                ip = HttpContext.Request.Headers["X-Forwarded-For"];

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
    }
}
