using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using LampStoreProjects.Models;
using LampStoreProjects.Repositories;
using LampStoreProjects.Data;
using LampStoreProjects.Helpers;
using LampStoreProjects.Services;
using Microsoft.EntityFrameworkCore;

namespace LampStoreProjects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfilesController : ControllerBase
    {
        private readonly IUserProfileRepository _userprofileRepository;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IImageUploadService _imageService;

        public UserProfilesController(IUserProfileRepository userprofileRepository, ApplicationDbContext context, IWebHostEnvironment env, IImageUploadService imageService)
        {
            _userprofileRepository = userprofileRepository;
            _context = context;
            _env = env;
            _imageService = imageService;
        }

        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        private bool IsAdmin() => User.IsInRole(AppRole.Admin);

        [HttpGet("GetUserProfiles")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<ActionResult<IEnumerable<UserProfileModel>>> GetUserProfiles()
        {
            var userprofiles = await _userprofileRepository.GetAllAsync();
            return Ok(userprofiles);
        }

        [HttpGet("GetUserProfile/{id}")]
        public async Task<ActionResult<UserProfileModel>> GetUserProfile(Guid id)
        {
            var userprofile = await _userprofileRepository.GetByIdAsync(id);
            if (userprofile == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.PROFILE_USER_NOT_FOUND));
            }
            return Ok(userprofile);
        }

        [HttpPost("{id}/UploadAvatar")]
        [Authorize]
        public async Task<ActionResult> UploadImage(Guid id, IFormFile ProfileAvatar)
        {
            try
            {
                // Kiểm tra ảnh có được cung cấp không
                if (ProfileAvatar == null || ProfileAvatar.Length == 0)
                {
                    return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.PROFILE_NO_FILE));
                }

                var userProfile = await _context.UserProfiles!.FirstOrDefaultAsync(p => p.Id == id);

                if (userProfile == null)
                {
                    return NotFound(ApiErrorResponse.FromCode(ErrorCodes.PROFILE_USER_NOT_FOUND));
                }

                var currentUserId = GetUserId();
                if (string.IsNullOrEmpty(currentUserId)) return Unauthorized(ApiErrorResponse.FromCode(ErrorCodes.UNAUTHORIZED));
                if (!IsAdmin() && userProfile.UserId != currentUserId)
                {
                    return Forbid();
                }

                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(userProfile.ProfileAvatar))
                {
                    await _imageService.DeleteImageAsync(userProfile.ProfileAvatar);
                }

                // Upload qua IImageUploadService: content-type allowlist, size limit (5MB) và
                // re-encode ảnh (giống luồng upload ảnh sản phẩm), thay vì lưu file thô với
                // extension client gửi lên.
                string imageUrl;
                try
                {
                    imageUrl = await _imageService.UploadImageAsync(ProfileAvatar, "ImageImport");
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(ApiErrorResponse.FromException(ErrorCodes.PRODUCT_INVALID_FILE_TYPE, ex));
                }

                // Cập nhật thông tin người dùng
                userProfile.ProfileAvatar = imageUrl;
                _context.UserProfiles!.Update(userProfile);
                await _context.SaveChangesAsync();

                return Ok(new ApiSuccessResponse("Cập nhật ảnh đại diện thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiErrorResponse.FromException(ErrorCodes.INTERNAL_ERROR, ex));
            }
        }

        [HttpPost("CreateUserProfile")]
        [Authorize]
        public async Task<ActionResult> CreateUserProfile(UserProfileModel userprofileModel)
        {
            // Force ownership to the authenticated caller — never trust a client-supplied UserId.
            var currentUserId = GetUserId();
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized(ApiErrorResponse.FromCode(ErrorCodes.UNAUTHORIZED));
            userprofileModel.UserId = currentUserId;

            await _userprofileRepository.AddAsync(userprofileModel);
            return CreatedAtAction(nameof(GetUserProfile), new { id = userprofileModel.Id }, userprofileModel);
        }

        [HttpPut("UpdateUserProfile/{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateUserProfile(Guid id, UserProfileModel userprofileModel)
        {
            if (id != userprofileModel.Id)
            {
                return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.PROFILE_ID_MISMATCH));
            }

            var existing = await _userprofileRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.PROFILE_USER_NOT_FOUND));
            }

            var currentUserId = GetUserId();
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized(ApiErrorResponse.FromCode(ErrorCodes.UNAUTHORIZED));
            if (!IsAdmin() && existing.UserId != currentUserId)
            {
                return Forbid();
            }

            await _userprofileRepository.UpdateAsync(userprofileModel);
            return NoContent();
        }

        [HttpDelete("DeleteAvatar/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteUserAvatar(Guid id)
        {
            var userProfile = await _context.UserProfiles!.FirstOrDefaultAsync(p => p.Id == id);

            if (userProfile == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.PROFILE_USER_NOT_FOUND));
            }

            var currentUserId = GetUserId();
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized(ApiErrorResponse.FromCode(ErrorCodes.UNAUTHORIZED));
            if (!IsAdmin() && userProfile.UserId != currentUserId)
            {
                return Forbid();
            }

            var avatarPath = userProfile.ProfileAvatar;
            userProfile.ProfileAvatar = "";

            _context.UserProfiles!.Update(userProfile);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(avatarPath))
            {
                await _imageService.DeleteImageAsync(avatarPath);
            }

            return Ok(new ApiSuccessResponse("Xóa ảnh đại diện thành công."));
        }

        [HttpDelete("Delete/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteUserProfile(Guid id)
        {
            var existing = await _userprofileRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.PROFILE_USER_NOT_FOUND));
            }

            var currentUserId = GetUserId();
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized(ApiErrorResponse.FromCode(ErrorCodes.UNAUTHORIZED));
            if (!IsAdmin() && existing.UserId != currentUserId)
            {
                return Forbid();
            }

            await _userprofileRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
