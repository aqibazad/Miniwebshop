using Application.DTOs;
using Application.Interface;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            var categories = await _context.Categories.ToListAsync();
            return categories.Select(c => new CategoryDto { Id = c.Id, Name = c.Name, Slug = c.Slug }).ToList();
        }

        public async Task<CategoryDto> GetByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) throw new Exception("Not found");
            return new CategoryDto { Id = category.Id, Name = category.Name, Slug = category.Slug };
        }

        public async Task AddAsync(CategoryDto dto)
        {
            var category = new Category { Name = dto.Name, Slug = dto.Slug.ToLower() };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CategoryDto dto)
        {
            var category = await _context.Categories.FindAsync(dto.Id);
            if (category == null) throw new Exception("Not found");
            category.Name = dto.Name;
            category.Slug = dto.Slug.ToLower();
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) throw new Exception("Not found");
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}