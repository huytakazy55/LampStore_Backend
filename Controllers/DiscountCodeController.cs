using LampStoreProjects.Data;
using LampStoreProjects.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LampStoreProjects.Helpers;

namespace LampStoreProjects.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountCodeController : ControllerBase
    {
        private readonly IDiscountCodeRepository _discountCodeRepository;

        public DiscountCodeController(IDiscountCodeRepository discountCodeRepository)
        {
            _discountCodeRepository = discountCodeRepository;
        }

        [HttpGet("my-codes")]
        [Authorize]
        public async Task<IActionResult> GetMyCodes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var codes = await _discountCodeRepository.GetUserDiscountCodesAsync(userId);
            var result = codes.Select(c => new
            {
                c.Code,
                c.DiscountType,
                c.DiscountPercentage,
                c.DiscountAmount,
                c.MaxDiscountAmount,
                c.MinOrderAmount,
                c.ExpiryDate,
                c.Quantity
            });

            return Ok(result);
        }

        [HttpPost("apply")]
        [Authorize]
        public async Task<IActionResult> ApplyCode([FromBody] ApplyCodeRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(request.Code))
            {
                return BadRequest("Mã giảm giá không hợp lệ.");
            }

            var discountCode = await _discountCodeRepository.ValidateDiscountCodeAsync(request.Code, userId, request.OrderTotalAmount);
            if (discountCode == null)
            {
                return BadRequest("Mã giảm giá không tồn tại, đã sử dụng, hết hạn, hoặc đơn hàng chưa đạt điều kiện.");
            }

            var discountAmount = 0m;
            if (discountCode.DiscountType?.Trim().Equals("Percentage", StringComparison.OrdinalIgnoreCase) == true)
            {
                discountAmount = (request.OrderTotalAmount * discountCode.DiscountPercentage) / 100m;
                if (discountCode.MaxDiscountAmount > 0 && discountAmount > discountCode.MaxDiscountAmount)
                {
                    discountAmount = discountCode.MaxDiscountAmount;
                }
            }
            else
            {
                discountAmount = discountCode.DiscountAmount;
                if (discountAmount > request.OrderTotalAmount)
                {
                    discountAmount = request.OrderTotalAmount;
                }
            }

            Console.WriteLine($"[DEBUG ApplyCode] Code: {request.Code}, OrderTotal: {request.OrderTotalAmount}, Calculated Discount: {discountAmount}");


            return Ok(new
            {
                Code = discountCode.Code,
                DiscountAmount = discountAmount,
                DiscountType = discountCode.DiscountType,
                DiscountPercentage = discountCode.DiscountPercentage
            });
        }
        // Admin Endpoints
        [HttpGet]
        [Authorize(Roles = AppRole.Admin + "," + AppRole.SuperAdmin)]
        public async Task<IActionResult> GetAllCodes()
        {
            var codes = await _discountCodeRepository.GetAllDiscountCodesAsync();
            return Ok(codes);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = AppRole.Admin + "," + AppRole.SuperAdmin)]
        public async Task<IActionResult> GetCode(Guid id)
        {
            var code = await _discountCodeRepository.GetDiscountCodeByIdAsync(id);
            if (code == null) return NotFound();
            return Ok(code);
        }

        [HttpPost]
        [Authorize(Roles = AppRole.Admin + "," + AppRole.SuperAdmin)]
        public async Task<IActionResult> CreateCode([FromBody] DiscountCode model)
        {
            model.Id = Guid.NewGuid();
            model.CreatedAt = Helpers.DateTimeHelper.VietnamNow;
            model.UpdatedAt = Helpers.DateTimeHelper.VietnamNow;
            if (string.IsNullOrEmpty(model.Status)) model.Status = "Active";
            
            var created = await _discountCodeRepository.CreateDiscountCodeAsync(model);
            return CreatedAtAction(nameof(GetCode), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = AppRole.Admin + "," + AppRole.SuperAdmin)]
        public async Task<IActionResult> UpdateCode(Guid id, [FromBody] DiscountCode model)
        {
            if (id != model.Id) return BadRequest();
            var updated = await _discountCodeRepository.UpdateDiscountCodeAsync(model);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AppRole.Admin + "," + AppRole.SuperAdmin)]
        public async Task<IActionResult> DeleteCode(Guid id)
        {
            await _discountCodeRepository.DeleteDiscountCodeAsync(id);
            return NoContent();
        }
    }

    public class ApplyCodeRequest
    {
        public string Code { get; set; } = string.Empty;
        public decimal OrderTotalAmount { get; set; }
    }
}
