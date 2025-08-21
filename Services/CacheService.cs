using Microsoft.Extensions.Caching.Memory;

namespace LampStoreProjects.Services
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
        Task RemoveAsync(string key);
        Task RemoveByPatternAsync(string pattern);
    }

    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheService> _logger;
        private readonly HashSet<string> _cacheKeys = new();

        public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out T? cachedValue))
                {
                    _logger.LogInformation("Cache HIT for key: {Key}", key);
                    return cachedValue;
                }

                _logger.LogInformation("Cache MISS for key: {Key}", key);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache for key: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30), // Default 30 minutes
                    SlidingExpiration = TimeSpan.FromMinutes(5), // Refresh if accessed within 5 minutes
                    Priority = CacheItemPriority.Normal
                };

                // Set callback để remove khỏi tracking khi cache expire
                cacheOptions.RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    _cacheKeys.Remove(key.ToString() ?? string.Empty);
                    _logger.LogInformation("Cache expired for key: {Key}, Reason: {Reason}", key, reason);
                });

                _memoryCache.Set(key, value, cacheOptions);
                _cacheKeys.Add(key);

                _logger.LogInformation("Cache SET for key: {Key}, Expiration: {Expiration}", key, expiration);
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                _memoryCache.Remove(key);
                _cacheKeys.Remove(key);
                _logger.LogInformation("Cache REMOVED for key: {Key}", key);
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache for key: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                var keysToRemove = _cacheKeys.Where(key => key.Contains(pattern)).ToList();
                
                foreach (var key in keysToRemove)
                {
                    _memoryCache.Remove(key);
                    _cacheKeys.Remove(key);
                }
                
                _logger.LogInformation("Cache REMOVED by pattern: {Pattern}, Count: {Count}", pattern, keysToRemove.Count);
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache by pattern: {Pattern}", pattern);
            }
        }
    }

    // Cache Keys Constants
    public static class CacheKeys
    {
        public const string AllProducts = "products_all";
        public const string AllCategories = "categories_all";
        public const string AllBanners = "banners_all";
        public const string ActiveBanners = "banners_active";
        
        public static string ProductById(Guid id) => $"product_{id}";
        public static string CategoryById(Guid id) => $"category_{id}";
        public static string ProductsByCategory(Guid categoryId) => $"products_category_{categoryId}";
        public static string ProductImages(Guid productId) => $"product_images_{productId}";
    }
}
