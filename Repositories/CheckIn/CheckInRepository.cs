using AutoMapper;
using LampStoreProjects.Data;
using LampStoreProjects.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public class CheckInRepository : ICheckInRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CheckInRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CheckInModel>> GetAllAsync()
        {
            var CheckIns = await _context.CheckIns.ToListAsync();
            return _mapper.Map<IEnumerable<CheckInModel>>(CheckIns);
        }

        public async Task<CheckInModel> GetByIdAsync(int id)
        {
            var CheckIn = await _context.CheckIns.FindAsync(id);
            return _mapper.Map<CheckInModel>(CheckIn);
        }

        public async Task AddAsync(CheckInModel CheckInModel)
        {
            var CheckIn = _mapper.Map<CheckIn>(CheckInModel);
            _context.CheckIns.Add(CheckIn);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CheckInModel CheckInModel)
        {
            var CheckIn = _mapper.Map<CheckIn>(CheckInModel);
            _context.CheckIns.Update(CheckIn);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var CheckIn = await _context.CheckIns.FindAsync(id);
            if (CheckIn != null)
            {
                _context.CheckIns.Remove(CheckIn);
                await _context.SaveChangesAsync();
            }
        }
    }
}