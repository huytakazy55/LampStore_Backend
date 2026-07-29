using Microsoft.AspNetCore.Authorization;
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
        public async Task<ActionResult<IEnumerable<TagModel>>> GetTags([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? search = null)
        {
            if (page < 1) page = 1;
            pageSize = Math.Clamp(pageSize, 1, 100);

            search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
            var totalCount = await _tagRepository.CountAsync(search);
            Response.Headers["X-Total-Count"] = totalCount.ToString();

            var tags = await _tagRepository.GetAllAsync(page, pageSize, search);
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
        [Authorize(Roles = AppRole.Admin)]
        public async Task<ActionResult> CreateTag(TagModel tagModel)
        {
            await _tagRepository.AddAsync(tagModel);
            return CreatedAtAction(nameof(GetTag), new { id = tagModel.Id }, tagModel);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = AppRole.Admin)]
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
        [Authorize(Roles = AppRole.Admin)]
        public async Task<ActionResult> DeleteTag(Guid id)
        {
            await _tagRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpDelete("bulk")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<ActionResult> BulkDeleteTags(List<Guid> ids)
        {
            await _tagRepository.BulkDeleteAsync(ids);
            return NoContent();
        }
    }
}
