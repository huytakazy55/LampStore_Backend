using AutoMapper;
using LampStoreProjects.Data;
using LampStoreProjects.Models;
using LampStoreProjects.Helpers;
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
            var orders = await _context.Orders!
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(o => MapOrderToModel(o)).ToList();
        }

        public async Task<IEnumerable<OrderModel>> GetByUserIdAsync(string userId)
        {
            var orders = await _context.Orders!
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(o => MapOrderToModel(o)).ToList();
        }

        public async Task<OrderModel?> GetByIdAsync(Guid id)
        {
            var order = await _context.Orders!
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return null;
            return MapOrderToModel(order);
        }

        public async Task<OrderModel> CreateOrderAsync(OrderModel orderModel)
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = orderModel.UserId,
                GuestToken = orderModel.GuestToken,
                OrderDate = DateTimeHelper.VietnamNow,
                Status = "Pending",
                FullName = orderModel.FullName,
                Phone = orderModel.Phone,
                Email = orderModel.Email,
                Address = orderModel.Address,
                City = orderModel.City,
                District = orderModel.District,
                Ward = orderModel.Ward,
                Note = orderModel.Note,
                PaymentMethod = orderModel.PaymentMethod,
                TotalAmount = orderModel.TotalAmount,
                ShippingFee = orderModel.ShippingFee,
                OrderItems = orderModel.OrderItems?.Select(oi => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    ProductImage = oi.ProductImage,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    SelectedOptions = oi.SelectedOptions
                }).ToList()
            };

            _context.Orders!.Add(order);
            await _context.SaveChangesAsync();

            orderModel.Id = order.Id;
            orderModel.OrderDate = order.OrderDate;
            orderModel.Status = order.Status;
            return orderModel;
        }

        public async Task UpdateStatusAsync(Guid id, string status)
        {
            var order = await _context.Orders!.FindAsync(id);
            if (order != null)
            {
                order.Status = status;
                order.UpdatedAt = DateTimeHelper.VietnamNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var order = await _context.Orders!
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order != null)
            {
                if (order.OrderItems != null)
                    _context.OrderItems!.RemoveRange(order.OrderItems);
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }

        private OrderModel MapOrderToModel(Order order)
        {
            return new OrderModel
            {
                Id = order.Id,
                UserId = order.UserId,
                GuestToken = order.GuestToken,
                OrderDate = order.OrderDate,
                Status = order.Status,
                FullName = order.FullName,
                Phone = order.Phone,
                Email = order.Email,
                Address = order.Address,
                City = order.City,
                District = order.District,
                Ward = order.Ward,
                Note = order.Note,
                PaymentMethod = order.PaymentMethod,
                TotalAmount = order.TotalAmount,
                ShippingFee = order.ShippingFee,
                OrderItems = order.OrderItems?.Select(oi => new OrderItemModel
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName ?? oi.Product?.Name ?? "",
                    ProductImage = oi.ProductImage ?? oi.Product?.Images?.FirstOrDefault()?.ImagePath,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    SelectedOptions = oi.SelectedOptions
                }).ToList()
            };
        }

        public async Task<IEnumerable<OrderModel>> GetByGuestTokenAsync(string guestToken)
        {
            var orders = await _context.Orders!
                .Where(o => o.GuestToken == guestToken)
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(o => MapOrderToModel(o)).ToList();
        }

        public async Task<int> ClaimGuestOrdersAsync(string guestToken, string userId)
        {
            var guestOrders = await _context.Orders!
                .Where(o => o.GuestToken == guestToken && o.UserId == null)
                .ToListAsync();

            foreach (var order in guestOrders)
            {
                order.UserId = userId;
                order.GuestToken = null;
                order.UpdatedAt = DateTimeHelper.VietnamNow;
            }

            await _context.SaveChangesAsync();
            return guestOrders.Count;
        }
    }
}