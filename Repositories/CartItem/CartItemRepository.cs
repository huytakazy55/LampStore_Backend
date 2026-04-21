using AutoMapper;
using LampStoreProjects.Data;
using LampStoreProjects.Helpers;
using LampStoreProjects.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public class CartItemRepository : ICartItemRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CartItemRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CartItemModel>> GetAllAsync()
        {
            var CartItem = await _context.CartItems!.ToListAsync();
            return _mapper.Map<IEnumerable<CartItemModel>>(CartItem);
        }

        public async Task<CartItemModel> GetByIdAsync(Guid id)
        {
            var CartItem = await _context.CartItems!.FindAsync(id);
            return _mapper.Map<CartItemModel>(CartItem);
        }

        public async Task<IEnumerable<CartItemModel>> GetByCartIdAsync(Guid cartId)
        {
            var items = await _context.CartItems!
                .Include(ci => ci.Product)
                    .ThenInclude(p => p!.Images)
                .Include(ci => ci.Product)
                    .ThenInclude(p => p!.ProductVariant)
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();

            return items.Select(ci => new CartItemModel
            {
                Id = ci.Id,
                CartId = ci.CartId,
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                SelectedOptions = ci.SelectedOptions,
                ProductName = ci.Product?.Name,
                ProductImage = ci.Product?.Images?.FirstOrDefault()?.ImagePath,
                BasePrice = ci.Product?.ProductVariant?.Price,
                Weight = ci.Product?.ProductVariant?.Weight ?? 0
            });
        }

        public async Task AddAsync(CartItemModel CartItemModel)
        {
            var CartItem = _mapper.Map<CartItem>(CartItemModel);
            _context.CartItems!.Add(CartItem);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CartItemModel CartItemModel)
        {
            var tracked = await _context.CartItems!.FindAsync(CartItemModel.Id);
            if (tracked != null)
            {
                tracked.Quantity = CartItemModel.Quantity;
                tracked.SelectedOptions = CartItemModel.SelectedOptions;
                tracked.UpdatedAt = DateTimeHelper.VietnamNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            // Detach any previously tracked instance to avoid conflicts
            var tracked = _context.ChangeTracker.Entries<CartItem>()
                .FirstOrDefault(e => e.Entity.Id == id);
            if (tracked != null)
                tracked.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            var CartItem = await _context.CartItems!.FindAsync(id);
            if (CartItem != null)
            {
                _context.CartItems.Remove(CartItem);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
                {
                    // Item was already deleted by another operation — safe to ignore
                }
            }
        }

        public async Task DeleteByCartIdAsync(Guid cartId)
        {
            var items = await _context.CartItems!
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();
            if (items.Any())
            {
                _context.CartItems!.RemoveRange(items);
                await _context.SaveChangesAsync();
            }
        }
    }
}