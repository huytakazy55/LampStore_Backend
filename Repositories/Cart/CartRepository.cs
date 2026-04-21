using AutoMapper;
using LampStoreProjects.Data;
using LampStoreProjects.Helpers;
using LampStoreProjects.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace LampStoreProjects.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CartRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CartModel>> GetAllAsync()
        {
            var carts = await _context.Carts!.ToListAsync();
            return _mapper.Map<IEnumerable<CartModel>>(carts);
        }

        public async Task<CartModel> GetByIdAsync(Guid id)
        {
            var Cart = await _context.Carts!.FindAsync(id);
            return _mapper.Map<CartModel>(Cart);
        }

        public async Task<CartModel?> GetByUserIdAsync(string userId)
        {
            var cart = await _context.Carts!
                .FirstOrDefaultAsync(c => c.UserId == userId);
            return cart == null ? null : _mapper.Map<CartModel>(cart);
        }

        public async Task<CartModel> GetOrCreateByUserIdAsync(string userId)
        {
            var cart = await _context.Carts!
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTimeHelper.VietnamNow
                };
                _context.Carts!.Add(cart);
                await _context.SaveChangesAsync();
            }

            return _mapper.Map<CartModel>(cart);
        }

        public async Task AddAsync(CartModel CartModel)
        {
            var Cart = _mapper.Map<Cart>(CartModel);
            _context.Carts!.Add(Cart);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CartModel CartModel)
        {
            var Cart = _mapper.Map<Cart>(CartModel);
            Cart.UpdatedAt = DateTimeHelper.VietnamNow;
            _context.Carts!.Update(Cart);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var Cart = await _context.Carts!.FindAsync(id);
            if (Cart != null)
            {
                _context.Carts.Remove(Cart);
                await _context.SaveChangesAsync();
            }
        }
    }
}