using LampStoreProjects.Data;
using LampStoreProjects.Helpers;
using LampStoreProjects.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System;

namespace LampStoreProjects.Repositories
{
    public class BannerRepository : IBannerRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public BannerRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BannerModel>> GetAllAsync(int page = 1, int pageSize = 50, string? search = null)
        {
            if (page < 1) page = 1;
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.Banners!.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                query = query.Where(b => b.Title.Contains(keyword) ||
                    (b.Description != null && b.Description.Contains(keyword)));
            }

            var banners = await query
                .OrderBy(b => b.Order)
                .ThenBy(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return _mapper.Map<IEnumerable<BannerModel>>(banners);
        }

        public async Task<int> CountAsync(string? search = null)
        {
            var query = _context.Banners!.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                query = query.Where(b => b.Title.Contains(keyword) ||
                    (b.Description != null && b.Description.Contains(keyword)));
            }
            return await query.CountAsync();
        }

        public async Task<IEnumerable<BannerModel>> GetActiveBannersAsync()
        {
            var banners = await _context.Banners!
                .AsNoTracking()
                .Where(b => b.IsActive)
                .OrderBy(b => b.Order)
                .ThenBy(b => b.CreatedAt)
                .ToListAsync();
            return _mapper.Map<IEnumerable<BannerModel>>(banners);
        }

        public async Task<BannerModel?> GetByIdAsync(int id)
        {
            var banner = await _context.Banners!.FindAsync(id);
            return _mapper.Map<BannerModel>(banner);
        }

        public async Task<BannerModel> CreateAsync(BannerModel banner)
        {
            var bannerEntity = _mapper.Map<Banner>(banner);
            bannerEntity.CreatedAt = DateTimeHelper.VietnamNow;
            _context.Banners!.Add(bannerEntity);
            await _context.SaveChangesAsync();
            return _mapper.Map<BannerModel>(bannerEntity);
        }

        public async Task<BannerModel> UpdateAsync(BannerModel banner)
        {
            var bannerEntity = _mapper.Map<Banner>(banner);
            bannerEntity.UpdatedAt = DateTimeHelper.VietnamNow;
            _context.Banners!.Update(bannerEntity);
            await _context.SaveChangesAsync();
            return _mapper.Map<BannerModel>(bannerEntity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var banner = await _context.Banners!.FindAsync(id);
            if (banner == null)
                return false;

            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Banners!.AnyAsync(b => b.Id == id);
        }
    }
} 
