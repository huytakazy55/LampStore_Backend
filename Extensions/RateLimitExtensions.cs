using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace LampStoreProjects.Extensions
{
    public static class RateLimitExtensions
    {
        public static IServiceCollection AddLampStoreRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";

                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds).ToString();
                    }

                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        Message = "Bạn thao tác quá nhanh. Vui lòng thử lại sau ít phút."
                    }, cancellationToken);
                };

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var partitionKey = GetRateLimitPartitionKey(httpContext);
                    var rateLimit = GetRateLimitProfile(httpContext);

                    return RateLimitPartition.GetFixedWindowLimiter(
                        $"{rateLimit.Name}:{partitionKey}",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rateLimit.PermitLimit,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = rateLimit.QueueLimit,
                            AutoReplenishment = true
                        });
                });
            });

            return services;
        }

        private static string GetRateLimitPartitionKey(HttpContext context)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                return $"user:{userId}";
            }

            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            var ipAddress = forwardedFor?.Split(',').FirstOrDefault()?.Trim();

            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            }

            return $"ip:{ipAddress}";
        }

        private static RateLimitProfile GetRateLimitProfile(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
            var method = context.Request.Method;

            if (path.StartsWith("/api/account/signin")
                || path.StartsWith("/api/account/requestsignupotp")
                || path.StartsWith("/api/account/signupverifyotp")
                || path.StartsWith("/api/account/signup")
                || path.StartsWith("/api/account/googlesignin")
                || path.StartsWith("/api/account/facebooksignin")
                || path.StartsWith("/api/account/forgotpassword"))
            {
                return new RateLimitProfile("auth-sensitive", 10, 0);
            }

            if (path.StartsWith("/api/account/refresh"))
            {
                return new RateLimitProfile("auth-refresh", 60, 2);
            }

            if (path.Contains("/upload")
                || path.Contains("/import")
                || path.Contains("/recompress-images")
                || path.StartsWith("/api/products/uploadvariantimage"))
            {
                return new RateLimitProfile("upload", 10, 0);
            }

            if (path.StartsWith("/api/orders")
                || path.StartsWith("/api/carts")
                || path.StartsWith("/api/discountcode/apply")
                || path.StartsWith("/api/payments"))
            {
                return new RateLimitProfile("checkout", 60, 2);
            }

            if (path.StartsWith("/api/chat") || path.StartsWith("/chathub"))
            {
                return new RateLimitProfile("chat", 120, 5);
            }

            if (path.StartsWith("/api/analytics/track"))
            {
                return new RateLimitProfile("analytics-track", 60, 0);
            }

            if (path.StartsWith("/api/products/search"))
            {
                return new RateLimitProfile("search", 60, 2);
            }

            if (HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsDelete(method) || HttpMethods.IsPatch(method))
            {
                return new RateLimitProfile("write", 30, 1);
            }

            if (path.StartsWith("/api"))
            {
                return new RateLimitProfile("public-api", 180, 10);
            }

            return new RateLimitProfile("default", 300, 10);
        }

        private readonly record struct RateLimitProfile(string Name, int PermitLimit, int QueueLimit);
    }
}
