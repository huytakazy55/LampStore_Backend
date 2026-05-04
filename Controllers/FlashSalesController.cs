using Microsoft.AspNetCore.Mvc;
using LampStoreProjects.Repositories;
using LampStoreProjects.Models;
using LampStoreProjects.Helpers;

namespace LampStoreProjects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlashSalesController : ControllerBase
    {
        private readonly IFlashSaleRepository _flashSaleRepository;

        public FlashSalesController(IFlashSaleRepository flashSaleRepository)
        {
            _flashSaleRepository = flashSaleRepository;
        }

        // GET: api/flashsales
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FlashSaleModel>>> GetAll()
        {
            var flashSales = await _flashSaleRepository.GetAllAsync();
            return Ok(flashSales);
        }

        // GET: api/flashsales/active
        [HttpGet("active")]
        public async Task<ActionResult<FlashSaleModel>> GetActive()
        {
            var flashSale = await _flashSaleRepository.GetActiveAsync();
            if (flashSale == null)
            {
                return Ok(new { }); // Return empty object when no active flash sale
            }
            return Ok(flashSale);
        }

        // GET: api/flashsales/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FlashSaleModel>> GetById(int id)
        {
            var flashSale = await _flashSaleRepository.GetByIdAsync(id);
            if (flashSale == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.FLASHSALE_NOT_FOUND));
            }
            return Ok(flashSale);
        }

        // POST: api/flashsales
        [HttpPost]
        public async Task<ActionResult<FlashSaleModel>> Create([FromBody] FlashSaleModel flashSale)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiErrorResponse.WithErrors(ErrorCodes.VALIDATION_FAILED, ModelState));
            }

            var created = await _flashSaleRepository.CreateAsync(flashSale);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT: api/flashsales/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FlashSaleModel flashSale)
        {
            if (id != flashSale.Id)
            {
                return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.FLASHSALE_ID_MISMATCH));
            }

            var existing = await _flashSaleRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.FLASHSALE_NOT_FOUND));
            }

            await _flashSaleRepository.UpdateAsync(flashSale);
            return NoContent();
        }

        // DELETE: api/flashsales/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _flashSaleRepository.DeleteAsync(id);
            if (!result) return NotFound(ApiErrorResponse.FromCode(ErrorCodes.FLASHSALE_NOT_FOUND));
            return NoContent();
        }

        // POST: api/flashsales/5/items
        [HttpPost("{id}/items")]
        public async Task<ActionResult<FlashSaleItemModel>> AddItem(int id, [FromBody] FlashSaleItemModel item)
        {
            var flashSale = await _flashSaleRepository.GetByIdAsync(id);
            if (flashSale == null) return NotFound(ApiErrorResponse.FromCode(ErrorCodes.FLASHSALE_NOT_FOUND));

            var created = await _flashSaleRepository.AddItemAsync(id, item);
            return Ok(created);
        }

        // PUT: api/flashsales/5/items/3
        [HttpPut("{id}/items/{itemId}")]
        public async Task<ActionResult<FlashSaleItemModel>> UpdateItem(int id, int itemId, [FromBody] FlashSaleItemModel item)
        {
            var result = await _flashSaleRepository.UpdateItemAsync(id, itemId, item);
            if (result == null) return NotFound(ApiErrorResponse.FromCode(ErrorCodes.FLASHSALE_NOT_FOUND));
            return Ok(result);
        }

        // DELETE: api/flashsales/5/items/3
        [HttpDelete("{id}/items/{itemId}")]
        public async Task<IActionResult> RemoveItem(int id, int itemId)
        {
            var result = await _flashSaleRepository.RemoveItemAsync(id, itemId);
            if (!result) return NotFound(ApiErrorResponse.FromCode(ErrorCodes.FLASHSALE_NOT_FOUND));
            return NoContent();
        }
    }
}
