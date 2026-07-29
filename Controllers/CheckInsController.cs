using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using LampStoreProjects.Models;
using LampStoreProjects.Repositories;
using LampStoreProjects.Helpers;

namespace LampStoreProjects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckInsController : ControllerBase
    {
        private readonly ICheckInRepository _checkinRepository;

        public CheckInsController(ICheckInRepository checkinRepository)
        {
            _checkinRepository = checkinRepository;
        }

        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        private bool IsAdmin() => User.IsInRole(AppRole.Admin);

        // Admin/management: list every user's check-ins.
        [HttpGet]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<ActionResult<IEnumerable<CheckInModel>>> GetCheckIns()
        {
            var checkins = await _checkinRepository.GetAllAsync();
            return Ok(checkins);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<CheckInModel>> GetCheckIn(Guid id)
        {
            var checkin = await _checkinRepository.GetByIdAsync(id);
            if (checkin == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.CHECKIN_NOT_FOUND));
            }

            var currentUserId = GetUserId();
            if (!IsAdmin() && (string.IsNullOrEmpty(currentUserId) || checkin.UserId != currentUserId))
            {
                return Forbid();
            }

            return Ok(checkin);
        }

        // Check-in-as-self: any authenticated user, always scoped to their own UserId
        // (never trust a client-supplied UserId here).
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> CreateCheckIn(CheckInModel checkinModel)
        {
            var currentUserId = GetUserId();
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized(ApiErrorResponse.FromCode(ErrorCodes.UNAUTHORIZED));

            checkinModel.UserId = currentUserId;

            await _checkinRepository.AddAsync(checkinModel);
            return CreatedAtAction(nameof(GetCheckIn), new { id = checkinModel.Id }, checkinModel);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateCheckIn(Guid id, CheckInModel checkinModel)
        {
            if (id != checkinModel.Id)
            {
                return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.CHECKIN_ID_MISMATCH));
            }

            var existing = await _checkinRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.CHECKIN_NOT_FOUND));
            }

            var currentUserId = GetUserId();
            if (!IsAdmin() && (string.IsNullOrEmpty(currentUserId) || existing.UserId != currentUserId))
            {
                return Forbid();
            }

            // Never let the caller reassign a check-in to another user.
            checkinModel.UserId = existing.UserId;

            await _checkinRepository.UpdateAsync(checkinModel);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteCheckIn(Guid id)
        {
            var existing = await _checkinRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.CHECKIN_NOT_FOUND));
            }

            var currentUserId = GetUserId();
            if (!IsAdmin() && (string.IsNullOrEmpty(currentUserId) || existing.UserId != currentUserId))
            {
                return Forbid();
            }

            await _checkinRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
