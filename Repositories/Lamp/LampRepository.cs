using AutoMapper;
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
        private readonly IMapper _mapper;

        public LampRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LampModel>> GetAllLampsAsync()
        {
            var lamps = await _context.Lamps.Include(l => l.Images).ToListAsync();
            return _mapper.Map<IEnumerable<LampModel>>(lamps);
        }

        public async Task<LampModel> GetLampByIdAsync(int id)
        {
            var lamp = await _context.Lamps.Include(l => l.Images).FirstOrDefaultAsync(l => l.Id == id);
            return _mapper.Map<LampModel>(lamp);
        }

        public async Task<LampModel> AddLampAsync(LampModel lampModel)
        {
            var lamp = _mapper.Map<Lamp>(lampModel);
            _context.Lamps.Add(lamp);
            await _context.SaveChangesAsync();
            return _mapper.Map<LampModel>(lamp);
        }

        public async Task<LampModel> UpdateLampAsync(LampModel lampModel)
        {
            var lamp = _mapper.Map<Lamp>(lampModel);
            _context.Lamps.Update(lamp);
            await _context.SaveChangesAsync();
            return _mapper.Map<LampModel>(lamp);
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