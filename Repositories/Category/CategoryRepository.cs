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
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CategoryRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryModel>> GetAllAsync()
        {
            var categories = await _context.Categories!.ToListAsync();
            return _mapper.Map<IEnumerable<CategoryModel>>(categories);
        }

        public async Task<CategoryModel> GetByIdAsync(Guid id)
        {
            var category = await _context.Categories!.FindAsync(id);
            return _mapper.Map<CategoryModel>(category);
        }

        public async Task<CategoryModel> GetBySlugAsync(string slug)
        {
            var category = await _context.Categories!.FirstOrDefaultAsync(c => c.Slug == slug);
            return _mapper.Map<CategoryModel>(category);
        }

        private async Task<string> GenerateUniqueSlugAsync(string name, Guid? categoryId = null)
        {
            var baseSlug = SlugHelper.GenerateSlug(name);
            var slug = baseSlug;
            int counter = 1;

            var query = _context.Categories!.AsQueryable();
            if (categoryId.HasValue)
            {
                query = query.Where(c => c.Id != categoryId.Value);
            }

            while (await query.AnyAsync(c => c.Slug == slug))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        public async Task AddAsync(CategoryModel categoryModel)
        {
            var category = _mapper.Map<Category>(categoryModel);
            category.Slug = await GenerateUniqueSlugAsync(category.Name);
            _context.Categories!.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CategoryModel categoryModel)
        {
            var category = _mapper.Map<Category>(categoryModel);
            category.Slug = await GenerateUniqueSlugAsync(category.Name, category.Id);
            category.UpdatedAt = DateTimeHelper.VietnamNow;
            _context.Categories!.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var category = await _context.Categories!.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task BulkDeleteAsync(List<Guid> ids)
        {
            var categories = _context.Categories!.Where(c => ids.Contains(c.Id));
            _context.Categories!.RemoveRange(categories);
            await _context.SaveChangesAsync();
        }
    }
}