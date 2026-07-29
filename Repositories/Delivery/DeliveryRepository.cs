using AutoMapper;
using LampStoreProjects.Data;
using LampStoreProjects.Models;
using Microsoft.EntityFrameworkCore;
using System;
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

        public async Task<IEnumerable<DeliveryModel>> GetAllAsync(int page = 1, int pageSize = 20, string? search = null)
        {
            if (page < 1) page = 1;
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.Orders!
                .AsNoTracking()
                .Where(o => o.Status == "Shipping");

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                query = query.Where(o =>
                    o.FullName.Contains(keyword) ||
                    o.Phone.Contains(keyword) ||
                    o.Address.Contains(keyword) ||
                    o.OrderCode.ToString().Contains(keyword) ||
                    o.Id.ToString().Contains(keyword));
            }

            var orders = await query
                .Include(o => o.Deliveries)
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return orders.Select(o =>
            {
                var delivery = o.Deliveries?.OrderByDescending(d => d.CreatedAt).FirstOrDefault();
                return new DeliveryModel
                {
                    Id = delivery?.Id ?? Guid.Empty,
                    OrderId = o.Id,
                    DeliveryDate = delivery?.DeliveryDate ?? o.OrderDate,
                    DeliveryStatus = delivery?.DeliveryStatus ?? o.Status,
                    OrderCode = o.OrderCode,
                    OrderDate = o.OrderDate,
                    FullName = o.FullName,
                    Phone = o.Phone,
                    Address = o.Address,
                    City = o.City,
                    District = o.District,
                    Ward = o.Ward,
                    PaymentMethod = o.PaymentMethod,
                    PaymentStatus = o.PaymentStatus,
                    TotalAmount = o.TotalAmount,
                    OrderItems = o.OrderItems?.Select(oi => _mapper.Map<OrderItemModel>(oi)).ToList() ?? new()
                };
            }).ToList();
        }

        public async Task<int> CountAsync(string? search = null)
        {
            var query = _context.Orders!.Where(o => o.Status == "Shipping");
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                query = query.Where(o =>
                    o.FullName.Contains(keyword) ||
                    o.Phone.Contains(keyword) ||
                    o.Address.Contains(keyword) ||
                    o.OrderCode.ToString().Contains(keyword) ||
                    o.Id.ToString().Contains(keyword));
            }
            return await query.CountAsync();
        }

        public async Task<DeliveryModel> GetByIdAsync(Guid id)
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

        public async Task DeleteAsync(Guid id)
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
