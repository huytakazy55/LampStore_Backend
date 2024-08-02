using AutoMapper;
using LampStoreProjects.Data;
using LampStoreProjects.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public OrderItemRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrderItemModel>> GetAllAsync()
        {
            var OrderItems = await _context.OrderItems.ToListAsync();
            return _mapper.Map<IEnumerable<OrderItemModel>>(OrderItems);
        }

        public async Task<OrderItemModel> GetByIdAsync(int id)
        {
            var OrderItem = await _context.OrderItems.FindAsync(id);
            return _mapper.Map<OrderItemModel>(OrderItem);
        }

        public async Task AddAsync(OrderItemModel OrderItemModel)
        {
            var OrderItem = _mapper.Map<OrderItem>(OrderItemModel);
            _context.OrderItems.Add(OrderItem);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(OrderItemModel OrderItemModel)
        {
            var OrderItem = _mapper.Map<OrderItem>(OrderItemModel);
            _context.OrderItems.Update(OrderItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var OrderItem = await _context.OrderItems.FindAsync(id);
            if (OrderItem != null)
            {
                _context.OrderItems.Remove(OrderItem);
                await _context.SaveChangesAsync();
            }
        }
    }
}