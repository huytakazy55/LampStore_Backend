using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using LampStoreProjects.Data;
using LampStoreProjects.Repositories;
using LampStoreProjects.Services;
using LampStoreProjects.Hubs;
using System.Text;
using Microsoft.Data.SqlClient;
using System.Data;
using Serilog;
using Serilog.Formatting.Json;
using Microsoft.AspNetCore.Diagnostics;

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
                    .AllowCredentials();
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
                .AllowCredentials();
            }
            
            // ❌ XÓA: .SetIsOriginAllowed((host) => true) - Quá nguy hiểm
            // ❌ XÓA: .WithExposedHeaders("*") - Không cần thiết
        });
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

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IProductStoreManage, ProductStoreManage>();
builder.Services.AddSingleton<ImageOptimizationService>();
builder.Services.AddScoped<IImageUploadService, LocalImageService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// Add Memory Cache
builder.Services.AddMemoryCache();

// Add Response Caching
builder.Services.AddResponseCaching();

// Add SignalR
builder.Services.AddSignalR();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
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
    .WriteTo.File(new JsonFormatter(), "Logs/errors.json", rollingInterval: RollingInterval.Day) // Ghi log JSON vào file
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

        // Seed product slugs for existing data
        var productsWithoutSlug = context.Products.Where(p => string.IsNullOrEmpty(p.Slug)).ToList();
        if (productsWithoutSlug.Any())
        {
            foreach (var p in productsWithoutSlug)
            {
                var baseSlug = LampStoreProjects.Helpers.SlugHelper.GenerateSlug(p.Name);
                var slug = baseSlug;
                int counter = 1;
                while (context.Products.Any(x => x.Slug == slug && x.Id != p.Id) || productsWithoutSlug.Any(x => x.Slug == slug && x.Id != p.Id))
                {
                    slug = $"{baseSlug}-{counter}";
                    counter++;
                }
                p.Slug = slug;
            }
            context.SaveChanges();
            Log.Information($"Seeded slugs for {productsWithoutSlug.Count} products.");
        }

        // Seed category slugs for existing data
        var categoriesWithoutSlug = context.Categories.Where(c => string.IsNullOrEmpty(c.Slug)).ToList();
        if (categoriesWithoutSlug.Any())
        {
            foreach (var c in categoriesWithoutSlug)
            {
                var baseSlug = LampStoreProjects.Helpers.SlugHelper.GenerateSlug(c.Name);
                var slug = baseSlug;
                int counter = 1;
                while (context.Categories.Any(x => x.Slug == slug && x.Id != c.Id) || categoriesWithoutSlug.Any(x => x.Slug == slug && x.Id != c.Id))
                {
                    slug = $"{baseSlug}-{counter}";
                    counter++;
                }
                c.Slug = slug;
            }
            context.SaveChanges();
            Log.Information($"Seeded slugs for {categoriesWithoutSlug.Count} categories.");
        }

        // Seed news slugs for existing data
        var newsWithoutSlug = context.News.Where(n => string.IsNullOrEmpty(n.Slug)).ToList();
        if (newsWithoutSlug.Any())
        {
            foreach (var n in newsWithoutSlug)
            {
                var baseSlug = LampStoreProjects.Helpers.SlugHelper.GenerateSlug(n.Title);
                var slug = baseSlug;
                int counter = 1;
                while (context.News.Any(x => x.Slug == slug && x.Id != n.Id) || newsWithoutSlug.Any(x => x.Slug == slug && x.Id != n.Id))
                {
                    slug = $"{baseSlug}-{counter}";
                    counter++;
                }
                n.Slug = slug;
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
                    var wasOptimized = await optimizer.OptimizeExistingFileAsync(filePath, maxWidth: 1200, quality: 80, minSizeBytes: 200 * 1024);
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
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseCors(apiCorsPolicy);
app.UseHttpsRedirection();

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
app.UseAuthorization();

// Map SignalR Hub
app.MapHub<ChatHub>("/chathub");

app.MapControllers();

app.Run();
