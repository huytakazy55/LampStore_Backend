using AutoMapper;
using LampStoreProjects.Data;
using LampStoreProjects.Helpers;
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

        public async Task<IEnumerable<TagModel>> GetAllAsync(int page = 1, int pageSize = 50, string? search = null)
        {
            if (page < 1) page = 1;
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.Tags!.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                query = query.Where(t => t.Name.Contains(keyword) || t.Description.Contains(keyword));
            }

            var tags = await query
                .OrderBy(t => t.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return _mapper.Map<IEnumerable<TagModel>>(tags);
        }

        public async Task<int> CountAsync(string? search = null)
        {
            var query = _context.Tags!.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                query = query.Where(t => t.Name.Contains(keyword) || t.Description.Contains(keyword));
            }
            return await query.CountAsync();
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
            tag.UpdatedAt = DateTimeHelper.VietnamNow;
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
