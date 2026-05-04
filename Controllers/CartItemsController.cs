using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using LampStoreProjects.Models;
using LampStoreProjects.Repositories;
using LampStoreProjects.Helpers;

namespace LampStoreProjects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartItemsController : ControllerBase
    {
        private readonly ICartItemRepository _cartitemRepository;

        public CartItemsController(ICartItemRepository cartitemRepository)
        {
            _cartitemRepository = cartitemRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItemModel>>> GetCartItems()
        {
            var cartitems = await _cartitemRepository.GetAllAsync();
            return Ok(cartitems);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CartItemModel>> GetCartItem(Guid id)
        {
            var cartitem = await _cartitemRepository.GetByIdAsync(id);
            if (cartitem == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.CART_ITEM_NOT_FOUND));
            }
            return Ok(cartitem);
        }

        [HttpPost]
        public async Task<ActionResult> CreateCartItem(CartItemModel cartitemModel)
        {
            await _cartitemRepository.AddAsync(cartitemModel);
            return CreatedAtAction(nameof(GetCartItem), new { id = cartitemModel.Id }, cartitemModel);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCartItem(Guid id, CartItemModel cartitemModel)
        {
            if (id != cartitemModel.Id)
            {
                return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.CART_ID_MISMATCH));
            }
            await _cartitemRepository.UpdateAsync(cartitemModel);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCartItem(Guid id)
        {
            await _cartitemRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}