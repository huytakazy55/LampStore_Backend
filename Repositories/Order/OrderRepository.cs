using AutoMapper;
using LampStoreProjects.Data;
using LampStoreProjects.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public OrderRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrderModel>> GetAllAsync()
        {
            var Orders = await _context.Orders.ToListAsync();
            return _mapper.Map<IEnumerable<OrderModel>>(Orders);
        }

        public async Task<OrderModel> GetByIdAsync(int id)
        {
            var Order = await _context.Orders.FindAsync(id);
            return _mapper.Map<OrderModel>(Order);
        }

        public async Task AddAsync(OrderModel OrderModel)
        {
            var Order = _mapper.Map<Order>(OrderModel);
            _context.Orders.Add(Order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(OrderModel OrderModel)
        {
            var Order = _mapper.Map<Order>(OrderModel);
            _context.Orders.Update(Order);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var Order = await _context.Orders.FindAsync(id);
            if (Order != null)
            {
                _context.Orders.Remove(Order);
                await _context.SaveChangesAsync();
            }
        }
    }
}