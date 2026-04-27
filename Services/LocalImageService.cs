namespace LampStoreProjects.Services
{
    public interface IImageUploadService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder = "ImageImport");
        Task<bool> DeleteImageAsync(string imagePath);
    }

    public class LocalImageService : IImageUploadService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ImageOptimizationService _optimizer;

        public LocalImageService(IWebHostEnvironment env, ImageOptimizationService optimizer)
        {
            _env = env;
            _optimizer = optimizer;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder = "ImageImport")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null");

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                throw new ArgumentException("Invalid file type. Only JPEG, PNG, GIF, and WebP are allowed.");

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                throw new ArgumentException("File size exceeds 5MB limit");

            var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsDir = Path.Combine(webRootPath, folder);
            
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            // Always output as .jpg for consistency and best compression
            var fileName = $"{Guid.NewGuid()}.jpg";
            var filePath = Path.Combine(uploadsDir, fileName);

            // Optimize: resize + compress to JPEG
            using (var stream = file.OpenReadStream())
            {
                await _optimizer.OptimizeImageAsync(stream, filePath, maxWidth: 800, quality: 65);
            }

            return $"/{folder}/{fileName}";
        }

        public Task<bool> DeleteImageAsync(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || imagePath.StartsWith("http"))
                return Task.FromResult(false);

            try
            {
                var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var filePath = Path.Combine(webRootPath, imagePath.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
    }
}

