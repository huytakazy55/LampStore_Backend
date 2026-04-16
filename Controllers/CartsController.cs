using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Security.Claims;
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
        private readonly ICartItemRepository _cartitemRepository;

        public CartsController(ICartRepository cartRepository, ICartItemRepository cartitemRepository)
        {
            _cartRepository = cartRepository;
            _cartitemRepository = cartitemRepository;
        }

        // ===== Existing CRUD endpoints =====

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

        // ===== NEW: User-specific cart endpoints =====

        /// <summary>
        /// Get current user's cart items (with product info)
        /// </summary>
        [Authorize]
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<CartItemModel>>> GetMyCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cart = await _cartRepository.GetByUserIdAsync(userId);
            if (cart == null)
                return Ok(new List<CartItemModel>());

            var items = await _cartitemRepository.GetByCartIdAsync(cart.Id);
            return Ok(items);
        }

        /// <summary>
        /// Sync localStorage cart items into user's backend cart.
        /// Merges items: adds quantity if same product+options exists, creates new item otherwise.
        /// Returns the full updated cart.
        /// </summary>
        [Authorize]
        [HttpPost("sync")]
        public async Task<ActionResult<IEnumerable<CartItemModel>>> SyncCart([FromBody] List<CartSyncItemDto> localItems)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cart = await _cartRepository.GetOrCreateByUserIdAsync(userId);

            if (localItems != null && localItems.Count > 0)
            {
                // Get existing items in user's cart
                var existingItems = (await _cartitemRepository.GetByCartIdAsync(cart.Id)).ToList();

                foreach (var localItem in localItems)
                {
                    // Find matching item by ProductId + SelectedOptions
                    var existing = existingItems.FirstOrDefault(e =>
                        e.ProductId == localItem.ProductId &&
                        (e.SelectedOptions ?? "") == (localItem.SelectedOptions ?? ""));

                    if (existing != null)
                    {
                        // Cộng thêm quantity
                        existing.Quantity += localItem.Quantity;
                        await _cartitemRepository.UpdateAsync(existing);
                    }
                    else
                    {
                        // Thêm item mới
                        var newItem = new CartItemModel
                        {
                            Id = Guid.NewGuid(),
                            CartId = cart.Id,
                            ProductId = localItem.ProductId,
                            Quantity = localItem.Quantity,
                            SelectedOptions = localItem.SelectedOptions
                        };
                        await _cartitemRepository.AddAsync(newItem);
                    }
                }
            }

            // Return full cart with product info
            var updatedItems = await _cartitemRepository.GetByCartIdAsync(cart.Id);
            return Ok(updatedItems);
        }

        /// <summary>
        /// Clear all items in the current user's cart
        /// </summary>
        [Authorize]
        [HttpDelete("my")]
        public async Task<ActionResult> ClearMyCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cart = await _cartRepository.GetByUserIdAsync(userId);
            if (cart != null)
            {
                await _cartitemRepository.DeleteByCartIdAsync(cart.Id);
            }

            return NoContent();
        }
    }
}