using AutoMapper;
using LampStoreProjects.Data;
using LampStoreProjects.Models;
using Microsoft.EntityFrameworkCore;

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

        public async Task AddAsync(CartModel CartModel)
        {
            var Cart = _mapper.Map<Cart>(CartModel);
            _context.Carts!.Add(Cart);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CartModel CartModel)
        {
            var Cart = _mapper.Map<Cart>(CartModel);
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