using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using LampStoreProjects.Models;
using LampStoreProjects.Repositories;
using LampStoreProjects.Helpers;

namespace LampStoreProjects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly ITagRepository _tagRepository;

        public TagsController(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagModel>>> GetTags()
        {
            var tags = await _tagRepository.GetAllAsync();
            return Ok(tags);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TagModel>> GetTag(Guid id)
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.TAG_NOT_FOUND));
            }
            return Ok(tag);
        }

        [HttpPost]
        public async Task<ActionResult> CreateTag(TagModel tagModel)
        {
            await _tagRepository.AddAsync(tagModel);
            return CreatedAtAction(nameof(GetTag), new { id = tagModel.Id }, tagModel);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateTag(Guid id, TagModel tagModel)
        {
            if (id != tagModel.Id)
            {
                return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.TAG_ID_MISMATCH));
            }
            await _tagRepository.UpdateAsync(tagModel);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTag(Guid id)
        {
            await _tagRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpDelete("bulk")]
        public async Task<ActionResult> BulkDeleteTags(List<Guid> ids)
        {
            await _tagRepository.BulkDeleteAsync(ids);
            return NoContent();
        }
    }
}