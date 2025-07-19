using AutoMapper;
using LampStoreProjects.Data;
using LampStoreProjects.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace LampStoreProjects.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TagRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TagModel>> GetAllAsync()
        {
            var tags = await _context.Tags!.ToListAsync();
            return _mapper.Map<IEnumerable<TagModel>>(tags);
        }

        public async Task<TagModel> GetByIdAsync(Guid id)
        {
            var tag = await _context.Tags!.FindAsync(id);
            return _mapper.Map<TagModel>(tag);
        }

        public async Task AddAsync(TagModel tagModel)
        {
            var tag = _mapper.Map<Tag>(tagModel);
            _context.Tags!.Add(tag);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TagModel tagModel)
        {
            var tag = _mapper.Map<Tag>(tagModel);
            tag.UpdatedAt = DateTime.UtcNow;
            _context.Tags!.Update(tag);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var tag = await _context.Tags!.FindAsync(id);
            if (tag != null)
            {
                _context.Tags.Remove(tag);
                await _context.SaveChangesAsync();
            }
        }

        public async Task BulkDeleteAsync(List<Guid> ids)
        {
            var tags = _context.Tags!.Where(t => ids.Contains(t.Id));
            _context.Tags!.RemoveRange(tags);
            await _context.SaveChangesAsync();
        }
    }
}