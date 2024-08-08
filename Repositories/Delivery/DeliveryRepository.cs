using AutoMapper;
using LampStoreProjects.Data;
using LampStoreProjects.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public class DeliveryRepository : IDeliveryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public DeliveryRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DeliveryModel>> GetAllAsync()
        {
            var Deliveries = await _context.Deliveries!.ToListAsync();
            return _mapper.Map<IEnumerable<DeliveryModel>>(Deliveries);
        }

        public async Task<DeliveryModel> GetByIdAsync(int id)
        {
            var Delivery = await _context.Deliveries!.FindAsync(id);
            return _mapper.Map<DeliveryModel>(Delivery);
        }

        public async Task AddAsync(DeliveryModel DeliveryModel)
        {
            var Delivery = _mapper.Map<Delivery>(DeliveryModel);
            _context.Deliveries!.Add(Delivery);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DeliveryModel DeliveryModel)
        {
            var Delivery = _mapper.Map<Delivery>(DeliveryModel);
            _context.Deliveries!.Update(Delivery);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var Delivery = await _context.Deliveries!.FindAsync(id);
            if (Delivery != null)
            {
                _context.Deliveries.Remove(Delivery);
                await _context.SaveChangesAsync();
            }
        }
    }
}