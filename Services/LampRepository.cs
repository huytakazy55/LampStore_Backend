using LampStoreProjects.Models;
using LampStoreProjects.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public class LampRepository : ILampRepository
    {
        private readonly ApplicationDbContext _context;

        public LampRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LampModel>> GetAllLampsAsync()
        {
            return await _context.Lamps.Include(l => l.Images).ToListAsync();
        }

        public async Task<LampModel> GetLampByIdAsync(int id)
        {
            return await _context.Lamps.Include(l => l.Images).FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<LampModel> AddLampAsync(LampModel lamp)
        {
            _context.Lamps.Add(lamp);
            await _context.SaveChangesAsync();
            return lamp;
        }

        public async Task<LampModel> UpdateLampAsync(LampModel lamp)
        {
            _context.Lamps.Update(lamp);
            await _context.SaveChangesAsync();
            return lamp;
        }

        public async Task DeleteLampAsync(int id)
        {
            var lamp = await _context.Lamps.FindAsync(id);
            if (lamp != null)
            {
                _context.Lamps.Remove(lamp);
                await _context.SaveChangesAsync();
            }
        }
    }
}