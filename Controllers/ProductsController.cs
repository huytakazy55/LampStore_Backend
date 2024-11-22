using LampStoreProjects.Models;
using LampStoreProjects.Repositories;
using LampStoreProjects.Data;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using Microsoft.EntityFrameworkCore;

namespace LampStoreProjects.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(IProductRepository productRepository, ApplicationDbContext context, IWebHostEnvironment env)
        {
            _productRepository = productRepository;
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductModel>>> GetAllProducts()
        {
            return Ok(await _productRepository.GetAllProductAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductModel>> GetProductById(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpGet("{id}/images")]
        public async Task<ActionResult<List<ProductImageModel>>> GetProductImagesByProductId(int id)
        {
            var images = await _productRepository.GetProductImageByIdAsync(id);

            if (images == null || images.Count == 0)
            {
                return NotFound();
            }

            return Ok(images);
        }

        [HttpGet("{id}/variants")]
        public async Task<ActionResult<List<ProductVariantCreateModel>>> GetProductVariantsByProductId(int id)
        {
            var variants = await _productRepository.GetProductVariantByIdAsync(id);

            if(variants == null || variants.Count == 0)
            {
                return NotFound();
            }

            return Ok(variants);
        }

        [HttpPost("{productId}/varians")]
        public async Task<ActionResult<ProductVariantModel>> AddProductVariant(int productId, [FromBody] List<ProductVariantCreateModel> variants)
        {
            if (variants == null || variants.Count == 0)
            {
                return BadRequest("Danh sách phân loại không được để trống.");
            }

            try
            {
                await _productRepository.AddProductVariantAsync(productId, variants);
                return Ok("phân loại sản phẩm đã được thêm thành công.");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Đã xảy ra lỗi khi thêm phân loại sản phẩm: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<ProductModel>> AddProduct([FromBody] ProductModel product)
        {
            var newProduct = await _productRepository.AddProductAsync(product);
            return CreatedAtAction(nameof(GetProductById), new { id = newProduct.Id }, newProduct);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProductModel>> UpdateProduct(int id, [FromBody] ProductModel product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }
            await _productRepository.UpdateProductAsync(product);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            await _productRepository.DeleteProductAsync(id);
            return NoContent();
        }

        [HttpDelete("image/{imageId}")]
        public async Task<ActionResult> DeleteProductImage(int imageId)
        {
            await _productRepository.DeleteImageProductAsync(imageId);
            return NoContent();
        }

        [HttpDelete("variant/{variantId}")]
        public async Task<ActionResult> DeleteProductVariant(int variantId)
        {
            await _productRepository.DeleteProductVariantAsync(variantId);
            return NoContent();
        }

        [HttpPost("{productId}/images")]
        public async Task<ActionResult> UploadImages(int productId, List<IFormFile> imageFiles)
        {
            const int MAX_IMAGES = 5;
            try
            {
                if (imageFiles == null || imageFiles.Count == 0)
                {
                    return BadRequest("No image files provided.");
                }

                var existingImagesCount = await _context.ProductImages!.CountAsync(img => img.ProductId == productId);

                // Kiểm tra tổng số ảnh sau khi upload
                if (existingImagesCount + imageFiles.Count > MAX_IMAGES)
                {
                    return BadRequest($"Bạn chỉ có thể upload tối đa {MAX_IMAGES} ảnh, hiện tại đã có {existingImagesCount} ảnh.");
                }

                var product = await _productRepository.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                if (string.IsNullOrEmpty(_env.WebRootPath))
                {
                    _env.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }
                if (!Directory.Exists(_env.WebRootPath))
                {
                    Directory.CreateDirectory(_env.WebRootPath);
                }

                // Đảm bảo thư mục ImageImport tồn tại
                var uploadPath = Path.Combine(_env.WebRootPath, "ImageImport");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                foreach (var imageFile in imageFiles)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    var imageUrl = $"/ImageImport/{fileName}";          

                    var productImage = new ProductImage
                    {
                        ImagePath = imageUrl,
                        ProductId = productId
                    };
                    _context.ProductImages!.Add(productImage);
                }
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}