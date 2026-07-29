using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using LampStoreProjects.Data;
using LampStoreProjects.Extensions;
using LampStoreProjects.Repositories;
using LampStoreProjects.Services;
using LampStoreProjects.Hubs;
using System.Text;
using Microsoft.Data.SqlClient;
using System.Data;
using Serilog;
using Serilog.Formatting.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using PayOS;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    
builder.Services.AddTransient<IDbConnection>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new SqlConnection(connectionString);
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = false;
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret is not configured.")))
        };
        
        // Cấu hình cho SignalR để nhận JWT token
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Lấy token từ query string (cho SignalR)
                var accessToken = context.Request.Query["access_token"];
                
                // Hoặc lấy từ Authorization header
                if (string.IsNullOrEmpty(accessToken))
                {
                    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                    {
                        accessToken = authHeader.Substring("Bearer ".Length).Trim();
                    }
                }

                // Nếu request đến SignalR hub và có token
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
                {
                    context.Token = accessToken;
                }
                
                return Task.CompletedTask;
            }
        };
    });

var apiCorsPolicy = "ApiCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: apiCorsPolicy,
        policyBuilder =>
        {
            // Lấy allowed origins từ config (có thể từ appsettings hoặc env vars)
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
            
            if (allowedOrigins != null && allowedOrigins.Length > 0)
            {
                // Production: Chỉ cho phép specific origins
                policyBuilder.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithExposedHeaders("X-Total-Count");
            }
            else
            {
                // Development: Cho phép localhost
                policyBuilder.WithOrigins(
                    "http://localhost:3000",
                    "https://localhost:3000",
                    "http://localhost:80",
                    "http://frontend:80", // Docker service name
                    "http://127.0.0.1:80",
                    "http://localhost:3001"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders("X-Total-Count");
            }
        });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // SECURITY: previously KnownNetworks/KnownProxies were unconditionally cleared,
    // which makes ForwardedHeadersMiddleware accept X-Forwarded-For from ANY caller
    // (not just a real reverse proxy) — trivially spoofable and used to bypass the
    // per-IP rate limiter in RateLimitExtensions.cs. We now only trust X-Forwarded-For
    // from proxies explicitly listed in the "TrustedProxies" config section. When that
    // list is empty (the safe default), KnownNetworks/KnownProxies stay at their
    // built-in defaults (loopback only), so XFF from arbitrary internet clients is
    // ignored and RemoteIpAddress keeps reflecting the real TCP peer.
    //
    // IMPORTANT (ops): when this app runs behind a real reverse proxy/load balancer in
    // production, populate "TrustedProxies" in appsettings with that proxy's IP(s) —
    // otherwise all traffic behind the LB will appear to come from one IP address
    // (a safe, if inconvenient, degradation vs. the spoofing vulnerability).
    var trustedProxies = builder.Configuration.GetSection("TrustedProxies").Get<string[]>();
    if (trustedProxies != null && trustedProxies.Length > 0)
    {
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
        foreach (var proxy in trustedProxies)
        {
            if (System.Net.IPAddress.TryParse(proxy, out var proxyIp))
            {
                options.KnownProxies.Add(proxyIp);
            }
        }
    }
});

builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment);

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();
builder.Services.AddScoped<ICheckInRepository, CheckInRepository>();
builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IBannerRepository, BannerRepository>();
builder.Services.AddScoped<LampStoreProjects.Repositories.Chat.IChatRepository, LampStoreProjects.Repositories.Chat.ChatRepository>();
builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
builder.Services.AddScoped<IProductReviewRepository, ProductReviewRepository>();
builder.Services.AddScoped<IFlashSaleRepository, FlashSaleRepository>();

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<IDiscountCodeRepository, DiscountCodeRepository>();

builder.Services.AddScoped<IProductStoreManage, ProductStoreManage>();
builder.Services.AddSingleton<ImageOptimizationService>();
builder.Services.AddScoped<IImageUploadService, LocalImageService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddHttpClient<IAnalyticsService, AnalyticsService>();

// Add Memory Cache
// NOTE: IMemoryCache is single-instance/in-process only. If this app is ever scaled
// to multiple instances behind a load balancer, this cache (and CacheService) will
// become inconsistent across instances — migrate to IDistributedCache (e.g. Redis)
// at that point.
builder.Services.AddMemoryCache();

// Add Response Caching
builder.Services.AddResponseCaching();

// Response compression (gzip/br) for JSON API responses — cheap win for payload size.
builder.Services.AddResponseCompression(o => o.EnableForHttps = true);

builder.Services.AddLampStoreRateLimiting();

// Add SignalR
// NOTE: SignalR's default backplane is also single-instance/in-process only. If this
// app is ever scaled to multiple instances, add a Redis backplane
// (AddStackExchangeRedisBackplane) so chat messages/notifications reach clients
// connected to a different instance.
builder.Services.AddSignalR();

builder.Services.AddAutoMapper(typeof(Program));

// PayOS setup
string clientId = builder.Configuration["PayOS:ClientId"] ?? "";
string apiKey = builder.Configuration["PayOS:ApiKey"] ?? "";
string checksumKey = builder.Configuration["PayOS:ChecksumKey"] ?? "";
if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(apiKey))
{
    var payOS = new PayOSClient(clientId, apiKey, checksumKey);
    builder.Services.AddSingleton(payOS);
}

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        // Pretty-printing is only useful for local debugging; it's wasted bytes on every
        // production response, and works against response compression too.
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LampStore API", Version = "v1" });
});

// Cai dat ghi log
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Error()
    .WriteTo.Console(new JsonFormatter()) // Hiển thị log dạng JSON trong console
    .WriteTo.File(new JsonFormatter(), "Logs/errors.json", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30) // Ghi log JSON vào file, giữ tối đa 30 file
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// Áp dụng migration tự động
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        context.Database.Migrate();
        Log.Information("Database migration applied successfully");

        // Seed product slugs for existing data.
        // Perf: load ALL existing slugs into an in-memory HashSet ONCE, then do
        // uniqueness checks against that set only (no per-row DB round-trip inside
        // the loop), adding each newly generated slug to the set as we go.
        var productsWithoutSlug = context.Products.Where(p => string.IsNullOrEmpty(p.Slug)).ToList();
        if (productsWithoutSlug.Any())
        {
            var existingProductSlugs = new HashSet<string>(
                context.Products.Where(p => !string.IsNullOrEmpty(p.Slug)).Select(p => p.Slug!),
                StringComparer.OrdinalIgnoreCase);

            foreach (var p in productsWithoutSlug)
            {
                var baseSlug = LampStoreProjects.Helpers.SlugHelper.GenerateSlug(p.Name);
                var slug = baseSlug;
                int counter = 1;
                while (existingProductSlugs.Contains(slug))
                {
                    slug = $"{baseSlug}-{counter}";
                    counter++;
                }
                p.Slug = slug;
                existingProductSlugs.Add(slug);
            }
            context.SaveChanges();
            Log.Information($"Seeded slugs for {productsWithoutSlug.Count} products.");
        }

        // Seed category slugs for existing data (same in-memory HashSet approach).
        var categoriesWithoutSlug = context.Categories.Where(c => string.IsNullOrEmpty(c.Slug)).ToList();
        if (categoriesWithoutSlug.Any())
        {
            var existingCategorySlugs = new HashSet<string>(
                context.Categories.Where(c => !string.IsNullOrEmpty(c.Slug)).Select(c => c.Slug!),
                StringComparer.OrdinalIgnoreCase);

            foreach (var c in categoriesWithoutSlug)
            {
                var baseSlug = LampStoreProjects.Helpers.SlugHelper.GenerateSlug(c.Name);
                var slug = baseSlug;
                int counter = 1;
                while (existingCategorySlugs.Contains(slug))
                {
                    slug = $"{baseSlug}-{counter}";
                    counter++;
                }
                c.Slug = slug;
                existingCategorySlugs.Add(slug);
            }
            context.SaveChanges();
            Log.Information($"Seeded slugs for {categoriesWithoutSlug.Count} categories.");
        }

        // Seed news slugs for existing data (same in-memory HashSet approach).
        var newsWithoutSlug = context.News.Where(n => string.IsNullOrEmpty(n.Slug)).ToList();
        if (newsWithoutSlug.Any())
        {
            var existingNewsSlugs = new HashSet<string>(
                context.News.Where(n => !string.IsNullOrEmpty(n.Slug)).Select(n => n.Slug!),
                StringComparer.OrdinalIgnoreCase);

            foreach (var n in newsWithoutSlug)
            {
                var baseSlug = LampStoreProjects.Helpers.SlugHelper.GenerateSlug(n.Title);
                var slug = baseSlug;
                int counter = 1;
                while (existingNewsSlugs.Contains(slug))
                {
                    slug = $"{baseSlug}-{counter}";
                    counter++;
                }
                n.Slug = slug;
                existingNewsSlugs.Add(slug);
            }
            context.SaveChanges();
            Log.Information($"Seeded slugs for {newsWithoutSlug.Count} news articles.");
        }

        // Batch optimize existing images in wwwroot/ImageImport
        try
        {
            var optimizer = scope.ServiceProvider.GetRequiredService<ImageOptimizationService>();
            var imgWebRootPath = app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var imageDir = Path.Combine(imgWebRootPath, "ImageImport");
            if (Directory.Exists(imageDir))
            {
                var imageFiles = Directory.GetFiles(imageDir);
                int optimizedCount = 0;
                long savedBytes = 0;
                foreach (var filePath in imageFiles)
                {
                    var beforeSize = new FileInfo(filePath).Length;
                    var wasOptimized = await optimizer.OptimizeExistingFileAsync(filePath, maxWidth: 800, quality: 65, minSizeBytes: 200 * 1024);
                    if (wasOptimized)
                    {
                        var afterSize = new FileInfo(filePath).Length;
                        savedBytes += beforeSize - afterSize;
                        optimizedCount++;
                    }
                }
                if (optimizedCount > 0)
                {
                    Log.Information($"Optimized {optimizedCount} images, saved {savedBytes / 1024}KB total.");
                }
            }
        }
        catch (Exception imgEx)
        {
            Log.Warning(imgEx, "Image batch optimization encountered errors (non-fatal).");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while applying database migrations");
    }
}

// Middleware bắt lỗi toàn cục
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (exception != null)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                Message = "Có lỗi xảy ra trên server!",
                // ✅ CHỈ trả về error message chi tiết trong Development
                Error = app.Environment.IsDevelopment() ? exception.Message : "Internal server error",
                // ✅ CHỈ trả về StackTrace trong Development
                StackTrace = app.Environment.IsDevelopment() ? exception.StackTrace : null
            };

            await context.Response.WriteAsJsonAsync(errorResponse);

            Log.Error("{@Error}", errorResponse);
        }
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LampStore API v1");
        c.RoutePrefix = string.Empty;
    });
}
else
{
    app.UseHsts();
}


app.UseForwardedHeaders();
app.UseCors(apiCorsPolicy);
app.UseHttpsRedirection();

// Response compression must run before response caching/static files so compressed
// bytes are what gets cached/served.
app.UseResponseCompression();

// Use Response Caching
app.UseResponseCaching();

app.UseAuthentication();

// Đảm bảo serve static files từ wwwroot (fix cho dotnet watch mode)
var webRootPath = app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(webRootPath))
{
    Directory.CreateDirectory(webRootPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(webRootPath),
    RequestPath = "",
    OnPrepareResponse = ctx =>
    {
        // Cache static assets (images, etc.) for 7 days, stale-while-revalidate for 30 days
        ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=604800, stale-while-revalidate=2592000");
    }
});
app.UseRouting();
app.UseRateLimiter();
app.UseAuthorization();

// Map SignalR Hub
app.MapHub<ChatHub>("/chathub");

app.MapControllers();

app.Run();
