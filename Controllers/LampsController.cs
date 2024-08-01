using LampStoreProjects.Models;
using LampStoreProjects.Repositories;
using LampStoreProjects.Data;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;

namespace LampStoreProjects.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LampsController : ControllerBase
    {
        private readonly ILampRepository _lampRepository;
        private readonly ApplicationDbContext _context;

        public LampsController(ILampRepository lampRepository, ApplicationDbContext context)
        {
            _lampRepository = lampRepository;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LampModel>>> GetAllLamps()
        {
            return Ok(await _lampRepository.GetAllLampsAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LampModel>> GetLampById(int id)
        {
            var lamp = await _lampRepository.GetLampByIdAsync(id);
            if (lamp == null)
            {
                return NotFound();
            }
            return Ok(lamp);
        }

        [HttpPost]
        public async Task<ActionResult<LampModel>> AddLamp([FromBody] LampModel lamp)
        {
            var newLamp = await _lampRepository.AddLampAsync(lamp);
            return CreatedAtAction(nameof(GetLampById), new { id = newLamp.Id }, newLamp);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<LampModel>> UpdateLamp(int id, [FromBody] LampModel lamp)
        {
            if (id != lamp.Id)
            {
                return BadRequest();
            }
            await _lampRepository.UpdateLampAsync(lamp);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteLamp(int id)
        {
            await _lampRepository.DeleteLampAsync(id);
            return NoContent();
        }

        [HttpPost("{lampId}/images")]
        public async Task<ActionResult> UploadImages(int lampId, List<IFormFile> imageFiles)
        {
            if (imageFiles == null || imageFiles.Count == 0)
            {
                return BadRequest("No image files provided.");
            }

            var lamp = await _lampRepository.GetLampByIdAsync(lampId);
            if (lamp == null)
            {
                return NotFound("Lamp not found.");
            }

            foreach (var imageFile in imageFiles)
            {
                var filePath = Path.Combine("ImageImport", Guid.NewGuid() + Path.GetExtension(imageFile.FileName));

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                var lampImage = new LampImage
                {
                    ImagePath = filePath,
                    LampId = lampId
                };

                _context.LampImages.Add(lampImage);
            }
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}