using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using LampStoreProjects.Models;
using LampStoreProjects.Repositories;

namespace LampStoreProjects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfilesController : ControllerBase
    {
        private readonly IUserProfileRepository _userprofileRepository;

        public UserProfilesController(IUserProfileRepository userprofileRepository)
        {
            _userprofileRepository = userprofileRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserProfileModel>>> GetUserProfiles()
        {
            var userprofiles = await _userprofileRepository.GetAllAsync();
            return Ok(userprofiles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserProfileModel>> GetUserProfile(int id)
        {
            var userprofile = await _userprofileRepository.GetByIdAsync(id);
            if (userprofile == null)
            {
                return NotFound();
            }
            return Ok(userprofile);
        }

        [HttpPost]
        public async Task<ActionResult> CreateUserProfile(UserProfileModel userprofileModel)
        {
            await _userprofileRepository.AddAsync(userprofileModel);
            return CreatedAtAction(nameof(GetUserProfile), new { id = userprofileModel.Id }, userprofileModel);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUserProfile(int id, UserProfileModel userprofileModel)
        {
            if (id != userprofileModel.Id)
            {
                return BadRequest();
            }
            await _userprofileRepository.UpdateAsync(userprofileModel);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUserProfile(int id)
        {
            await _userprofileRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}