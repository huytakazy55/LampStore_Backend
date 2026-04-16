using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LampStoreProjects.Models;
using LampStoreProjects.Repositories;
using System.Security.Claims;

namespace LampStoreProjects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WishlistsController : ControllerBase
    {
        private readonly IWishlistRepository _wishlistRepository;

        public WishlistsController(IWishlistRepository wishlistRepository)
        {
            _wishlistRepository = wishlistRepository;
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        /// <summary>
        /// Lấy danh sách yêu thích (kèm thông tin sản phẩm)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WishlistItemModel>>> GetWishlist()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var items = await _wishlistRepository.GetByUserIdAsync(userId);
            return Ok(items);
        }

        /// <summary>
        /// Lấy danh sách productId đã yêu thích (load nhanh cho UI)
        /// </summary>
        [HttpGet("ids")]
        public async Task<ActionResult<IEnumerable<Guid>>> GetWishlistIds()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var ids = await _wishlistRepository.GetWishlistProductIdsAsync(userId);
            return Ok(ids);
        }

        /// <summary>
        /// Thêm sản phẩm vào yêu thích
        /// </summary>
        [HttpPost("{productId}")]
        public async Task<ActionResult> AddToWishlist(Guid productId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _wishlistRepository.AddAsync(userId, productId);
            if (!result) return BadRequest(new { message = "Sản phẩm đã có trong danh sách yêu thích hoặc không tồn tại." });

            return Ok(new { message = "Đã thêm vào danh sách yêu thích." });
        }

        /// <summary>
        /// Xóa sản phẩm khỏi yêu thích
        /// </summary>
        [HttpDelete("{productId}")]
        public async Task<ActionResult> RemoveFromWishlist(Guid productId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _wishlistRepository.RemoveAsync(userId, productId);
            if (!result) return NotFound(new { message = "Sản phẩm không có trong danh sách yêu thích." });

            return Ok(new { message = "Đã xóa khỏi danh sách yêu thích." });
        }

        /// <summary>
        /// Kiểm tra sản phẩm có trong yêu thích không
        /// </summary>
        [HttpGet("check/{productId}")]
        public async Task<ActionResult<bool>> CheckInWishlist(Guid productId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var isInWishlist = await _wishlistRepository.IsInWishlistAsync(userId, productId);
            return Ok(isInWishlist);
        }
    }
}
