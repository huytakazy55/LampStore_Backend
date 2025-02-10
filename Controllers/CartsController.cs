using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using LampStoreProjects.Models;
using LampStoreProjects.Repositories;

namespace LampStoreProjects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartsController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;

        public CartsController(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartModel>>> GetCarts()
        {
            var carts = await _cartRepository.GetAllAsync();
            return Ok(carts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CartModel>> GetCart(Guid id)
        {
            var cart = await _cartRepository.GetByIdAsync(id);
            if (cart == null)
            {
                return NotFound();
            }
            return Ok(cart);
        }

        [HttpPost]
        public async Task<ActionResult> CreateCart(CartModel cartModel)
        {
            await _cartRepository.AddAsync(cartModel);
            return CreatedAtAction(nameof(GetCart), new { id = cartModel.Id }, cartModel);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCart(Guid id, CartModel cartModel)
        {
            if (id != cartModel.Id)
            {
                return BadRequest();
            }
            await _cartRepository.UpdateAsync(cartModel);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCart(Guid id)
        {
            await _cartRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}