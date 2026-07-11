using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LampStoreProjects.Repositories;
using LampStoreProjects.Helpers;
using System.Security.Claims;

namespace LampStoreProjects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductReviewsController : ControllerBase
    {
        private readonly IProductReviewRepository _reviewRepository;

        public ProductReviewsController(IProductReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        /// <summary>
        /// Lấy danh sách đánh giá theo sản phẩm (public)
        /// </summary>
        [HttpGet("{productId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductReviewModel>>> GetByProductId(Guid productId)
        {
            var reviews = await _reviewRepository.GetByProductIdAsync(productId);
            return Ok(reviews);
        }

        /// <summary>
        /// Lấy danh sách đánh giá mới nhất trên toàn hệ thống
        /// </summary>
        [HttpGet("recent")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductReviewModel>>> GetRecentReviews([FromQuery] int limit = 6)
        {
            var reviews = await _reviewRepository.GetRecentReviewsAsync(limit);
            return Ok(reviews);
        }

        /// <summary>
        /// Gửi đánh giá sản phẩm (yêu cầu đăng nhập)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> SubmitReview([FromBody] ProductReviewModel model)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiErrorResponse.FromCode(ErrorCodes.UNAUTHORIZED));

            if (model.ProductId == null)
                return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.REVIEW_PRODUCT_ID_REQUIRED));

            // Validate rating
            if (model.Rating < 1 || model.Rating > 5)
                return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.REVIEW_INVALID_RATING));

            // Check if user has purchased this product
            var hasPurchased = await _reviewRepository.HasPurchasedProductAsync(userId, model.ProductId.Value);
            if (!hasPurchased)
                return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.REVIEW_PURCHASE_REQUIRED));

            // Check if already reviewed
            var hasReviewed = await _reviewRepository.HasReviewedAsync(userId, model.ProductId.Value);
            if (hasReviewed)
                return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.REVIEW_ALREADY_REVIEWED));

            var result = await _reviewRepository.AddAsync(userId, model);
            if (result == null)
                return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.REVIEW_PRODUCT_NOT_FOUND));

            return Ok(result);
        }

        /// <summary>
        /// Kiểm tra trạng thái đánh giá của user (đã mua? đã đánh giá?)
        /// </summary>
        [HttpGet("status/{productId}")]
        [Authorize]
        public async Task<ActionResult> GetReviewStatus(Guid productId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiErrorResponse.FromCode(ErrorCodes.UNAUTHORIZED));

            var hasPurchased = await _reviewRepository.HasPurchasedProductAsync(userId, productId);
            var hasReviewed = await _reviewRepository.HasReviewedAsync(userId, productId);

            return Ok(new { hasPurchased, hasReviewed });
        }
    }
}
