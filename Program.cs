using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using LampStoreProjects.Data;
using LampStoreProjects.Repositories;
using System.Text;
using Microsoft.Data.SqlClient;
using System.Data;
using Serilog;
using Serilog.Formatting.Json;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

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
    });

var apiCorsPolicy = "ApiCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: apiCorsPolicy,
        builder =>
        {
            builder.WithOrigins("http://localhost:3000", "https://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
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

builder.Services.AddScoped<IProductStoreManage, ProductStoreManage>();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
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
                Error = exception.Message,
                StackTrace = exception.StackTrace
            };

            await context.Response.WriteAsJsonAsync(errorResponse);

            // Ghi log lỗi có format JSON đẹp
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
app.UseAuthentication();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
