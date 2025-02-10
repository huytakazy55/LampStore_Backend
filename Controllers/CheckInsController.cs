using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using LampStoreProjects.Models;
using LampStoreProjects.Repositories;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CheckInModel>>> GetCheckIns()
        {
            var checkins = await _checkinRepository.GetAllAsync();
            return Ok(checkins);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CheckInModel>> GetCheckIn(Guid id)
        {
            var checkin = await _checkinRepository.GetByIdAsync(id);
            if (checkin == null)
            {
                return NotFound();
            }
            return Ok(checkin);
        }

        [HttpPost]
        public async Task<ActionResult> CreateCheckIn(CheckInModel checkinModel)
        {
            await _checkinRepository.AddAsync(checkinModel);
            return CreatedAtAction(nameof(GetCheckIn), new { id = checkinModel.Id }, checkinModel);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCheckIn(Guid id, CheckInModel checkinModel)
        {
            if (id != checkinModel.Id)
            {
                return BadRequest();
            }
            await _checkinRepository.UpdateAsync(checkinModel);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCheckIn(Guid id)
        {
            await _checkinRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}