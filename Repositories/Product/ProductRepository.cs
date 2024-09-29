using AutoMapper;
using LampStoreProjects.Models;
using LampStoreProjects.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ProductRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductModel>> GetAllProductAsync()
        {
            var products = await _context.Products!.Include(l => l.Images).ToListAsync();
            return _mapper.Map<IEnumerable<ProductModel>>(products);
        }

        public async Task<ProductModel> GetProductByIdAsync(int id)
        {
            var product = await _context.Products!.Include(l => l.Images).FirstOrDefaultAsync(l => l.Id == id);
            return _mapper.Map<ProductModel>(product);
        }

        public async Task<ProductModel> AddProductAsync(ProductModel ProductModel)
        {
            var product = _mapper.Map<Product>(ProductModel);
            _context.Products!.Add(product);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductModel>(product);
        }

        public async Task<ProductModel> UpdateProductAsync(ProductModel ProductModel)
        {
            var product = _mapper.Map<Product>(ProductModel);
            _context.Products!.Update(product);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductModel>(product);
        }
        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products!.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

    }
}