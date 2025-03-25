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
using LampStoreProjects.DTOs;

namespace LampStoreProjects.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductStoreManage _productStoreManage;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(IProductRepository productRepository, IProductStoreManage productStoreManage, ApplicationDbContext context, IWebHostEnvironment env)
        {
            _productRepository = productRepository;
            _productStoreManage = productStoreManage;
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductModel>>> GetAllProducts()
        {
            return Ok(await _productRepository.GetAllProductAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductModel>> GetProductById(Guid id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpGet("Variant/{id}")]
        public async Task<ActionResult<IEnumerable<ProductVariantModel>>> GetProductVariantById(Guid id)
        {
            var variant = await _productRepository.GetProductVariantByIdAsync(id);
            if(variant == null)
            {
                return NotFound();
            }
            return Ok(variant);
        }

        [HttpGet("VariantType/{id}")]
        public async Task<ActionResult<VariantTypeModel>> GetVariantTypeById(Guid id)
        {
            var variantType = await _productRepository.GetVariantTypeByIdAsync(id);
            if (variantType == null)
            {
                return NotFound();
            }
            return Ok(variantType);
        }

        [HttpGet("VariantValue/{id}")]
        public async Task<ActionResult<VariantValueModel>> GetVariantValueById(Guid id)
        {
            var variantvalue = await _productRepository.GetVariantValueByIdAsync(id);
            if(variantvalue == null)
            {
                return NotFound();
            }
            return Ok(variantvalue);
        }

        [HttpGet("{id}/images")]
        public async Task<ActionResult<List<ProductImageModel>>> GetProductImagesByProductId(Guid id)
        {
            var images = await _productRepository.GetProductImageByIdAsync(id);

            if (images == null || images.Count == 0)
            {
                return NotFound();
            }

            return Ok(images);
        }

        [HttpPost("CreateProduct")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDto productDto)
        {
            var createdProduct = await _productRepository.CreateProductAsync(productDto);
            return CreatedAtAction(nameof(CreateProduct), new { id = createdProduct.Id }, createdProduct);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProductModel>> UpdateProduct(Guid Id, [FromBody] ProductUpdateDto productDto)
        {
            if (Id != productDto.Id)
            {
                return BadRequest();
            }
            await _productRepository.UpdateProductAsync(Id, productDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(Guid id)
        {
            await _productRepository.DeleteProductAsync(id);
            return NoContent();
        }

        [HttpDelete("image/{imageId}")]
        public async Task<ActionResult> DeleteProductImage(Guid imageId)
        {
            var productImage = await _context.ProductImages!.FirstOrDefaultAsync(x => x.Id == imageId);
            if (productImage == null)
            {
                return NotFound("Hình ảnh không tồn tại.");
            }

            await _productRepository.DeleteImageProductAsync(imageId);

            if (!string.IsNullOrEmpty(productImage.ImagePath))
            {
                var filePath = Path.Combine(_env.WebRootPath, productImage.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            return Ok("Xóa hình ảnh sản phẩm thành công.");
        }

        [HttpPost("{productId}/images")]
        public async Task<ActionResult> UploadImages(Guid productId, List<IFormFile> imageFiles)
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