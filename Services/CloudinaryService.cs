using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace LampStoreProjects.Services
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder = "lamp-store");
        Task<bool> DeleteImageAsync(string publicId);
    }

    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {
            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new ArgumentException("Cloudinary configuration is missing or incomplete");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder = "lamp-store")
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

            try
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    Folder = folder,
                    Transformation = new Transformation()
                        .Width(800)
                        .Height(600)
                        .Crop("fill")
                        .Quality("auto:good")
                        .FetchFormat("auto"),
                    UniqueFilename = true,
                    UseFilename = false
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                    throw new Exception($"Cloudinary upload error: {uploadResult.Error.Message}");

                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload image to Cloudinary: {ex.Message}");
            }
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
                return false;

            try
            {
                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);
                return result.Result == "ok";
            }
            catch
            {
                return false;
            }
        }
    }
}
