using LampStoreProjects.Data;
using LampStoreProjects.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

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

        public async Task<IEnumerable<BannerModel>> GetAllAsync()
        {
            var banners = await _context.Banners!
                .OrderBy(b => b.Order)
                .ThenBy(b => b.CreatedAt)
                .ToListAsync();
            return _mapper.Map<IEnumerable<BannerModel>>(banners);
        }

        public async Task<IEnumerable<BannerModel>> GetActiveBannersAsync()
        {
            var banners = await _context.Banners!
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
            bannerEntity.CreatedAt = DateTime.UtcNow;
            _context.Banners!.Add(bannerEntity);
            await _context.SaveChangesAsync();
            return _mapper.Map<BannerModel>(bannerEntity);
        }

        public async Task<BannerModel> UpdateAsync(BannerModel banner)
        {
            var bannerEntity = _mapper.Map<Banner>(banner);
            bannerEntity.UpdatedAt = DateTime.UtcNow;
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