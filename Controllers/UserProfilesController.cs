using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using LampStoreProjects.Models;
using LampStoreProjects.Repositories;
using LampStoreProjects.Data;
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

        public UserProfilesController(IUserProfileRepository userprofileRepository, ApplicationDbContext context, IWebHostEnvironment env)
        {
            _userprofileRepository = userprofileRepository;
            _context = context;
            _env = env;
        }

        [HttpGet("GetUserProfiles")]
        public async Task<ActionResult<IEnumerable<UserProfileModel>>> GetUserProfiles()
        {
            var userprofiles = await _userprofileRepository.GetAllAsync();
            return Ok(userprofiles);
        }

        [HttpGet("GetUserProfile/{id}")]
        public async Task<ActionResult<UserProfileModel>> GetUserProfile(int id)
        {
            var userprofile = await _userprofileRepository.GetByIdAsync(id);
            if (userprofile == null)
            {
                return NotFound();
            }
            return Ok(userprofile);
        }

        [HttpPost("{id}/UploadAvatar")]
        public async Task<ActionResult> UploadImage(int id, IFormFile ProfileAvatar)
        {
            try
            {
                // Kiểm tra ảnh có được cung cấp không
                if (ProfileAvatar == null || ProfileAvatar.Length == 0)
                {
                    return BadRequest("No image file provided.");
                }

                var userProfile = await _context.UserProfiles!.FirstOrDefaultAsync(p => p.Id == id);

                if (userProfile == null)
                {
                    return NotFound("User not found.");
                }

                // Tạo đường dẫn để lưu ảnh
                var uploadPath = Path.Combine(_env.WebRootPath, "ImageImport");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var fileName = Guid.NewGuid() + Path.GetExtension(ProfileAvatar.FileName);
                var filePath = Path.Combine(uploadPath, fileName);

                // Lưu ảnh vào đường dẫn đã chỉ định
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileAvatar.CopyToAsync(stream);
                }

                // Tạo URL từ đường dẫn lưu ảnh
                var imageUrl = $"/ImageImport/{fileName}";

                userProfile.ProfileAvatar = imageUrl;
                _context.UserProfiles!.Update(userProfile);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("CreateUserProfile")]
        public async Task<ActionResult> CreateUserProfile(UserProfileModel userprofileModel)
        {
            await _userprofileRepository.AddAsync(userprofileModel);
            return CreatedAtAction(nameof(GetUserProfile), new { id = userprofileModel.Id }, userprofileModel);
        }

        [HttpPut("UpdateUserProfile/{id}")]
        public async Task<ActionResult> UpdateUserProfile(int id, UserProfileModel userprofileModel)
        {
            if (id != userprofileModel.Id)
            {
                return BadRequest();
            }
            await _userprofileRepository.UpdateAsync(userprofileModel);
            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult> DeleteUserProfile(int id)
        {
            await _userprofileRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}