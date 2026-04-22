using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace LampStoreProjects.Services
{
    public class ImageOptimizationService
    {
        /// <summary>
        /// Optimize an image: resize to maxWidth (keeping aspect ratio) and compress to JPEG quality.
        /// </summary>
        public async Task OptimizeImageAsync(Stream inputStream, string outputPath, int maxWidth = 1200, int quality = 80)
        {
            using var image = await Image.LoadAsync(inputStream);

            // Only resize if wider than maxWidth
            if (image.Width > maxWidth)
            {
                var ratio = (double)maxWidth / image.Width;
                var newHeight = (int)(image.Height * ratio);
                image.Mutate(x => x.Resize(maxWidth, newHeight));
            }

            var encoder = new JpegEncoder
            {
                Quality = quality
            };

            await image.SaveAsync(outputPath, encoder);
        }

        /// <summary>
        /// Optimize an existing file in-place. Returns true if the file was optimized.
        /// </summary>
        public async Task<bool> OptimizeExistingFileAsync(string filePath, int maxWidth = 1200, int quality = 80, long minSizeBytes = 200 * 1024)
        {
            if (!File.Exists(filePath))
                return false;

            var fileInfo = new FileInfo(filePath);

            // Skip small files
            if (fileInfo.Length <= minSizeBytes)
                return false;

            // Skip non-image files
            var ext = fileInfo.Extension.ToLower();
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
            if (!imageExtensions.Contains(ext))
                return false;

            try
            {
                var tempPath = filePath + ".tmp";

                using (var inputStream = File.OpenRead(filePath))
                {
                    await OptimizeImageAsync(inputStream, tempPath, maxWidth, quality);
                }

                // Only replace if the optimized file is actually smaller
                var optimizedInfo = new FileInfo(tempPath);
                if (optimizedInfo.Length < fileInfo.Length)
                {
                    File.Delete(filePath);
                    File.Move(tempPath, filePath);
                    return true;
                }
                else
                {
                    // Optimized version is larger or same, keep original
                    File.Delete(tempPath);
                    return false;
                }
            }
            catch
            {
                // If optimization fails (corrupted image, etc.), skip silently
                var tempPath = filePath + ".tmp";
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
                return false;
            }
        }
    }
}
