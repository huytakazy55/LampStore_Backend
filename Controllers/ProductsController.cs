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

                var existingImagesCount = await _context.ProductImages.CountAsync(img => img.ProductId == productId);

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

                foreach (var imageFile in imageFiles)
                {
                    var uploadPath = Path.Combine(_env.WebRootPath, "ImageImport");
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