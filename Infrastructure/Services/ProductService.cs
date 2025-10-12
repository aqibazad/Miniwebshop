using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductDto>> GetByCategoryAsync(int categoryId, int page, int pageSize, string sortBy = "price")
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize > 20 ? 20 : pageSize; // Bound page size

            IQueryable<Product> query = _context.Products
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category);

            // Sorting
            switch (sortBy.ToLower())
            {
                case "price":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "newest":
                    query = query.OrderByDescending(p => p.Id);
                    break;
                default:
                    query = query.OrderBy(p => p.Id);
                    break;
            }

            // Paging
            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return products.Select(MapToDto).ToList();
        }

        public async Task<List<ProductDto>> SearchAsync(string keyword, int page, int pageSize)
        {
            if (string.IsNullOrEmpty(keyword)) return new List<ProductDto>();

            page = page < 1 ? 1 : page;
            pageSize = pageSize > 20 ? 20 : pageSize;

            var query = _context.Products.Where(p => p.Name.Contains(keyword) || p.ShortDescription.Contains(keyword) || p.LongDescription.Contains(keyword)).Include(p => p.Category);

            var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return products.Select(p => MapToDto(p)).ToList();
        }

        public async Task<ProductDto> GetByIdAsync(int id)
        {
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) throw new Exception("Not found");
            return MapToDto(product);
        }

        public async Task AddAsync(ProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Slug = dto.Slug.ToLower(),
                Price = dto.Price,
                Stock = dto.Stock,
                ShortDescription = dto.ShortDescription,
                LongDescription = dto.LongDescription,
                ImageUrl = dto.ImageUrl,
                CategoryId = dto.CategoryId
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductDto dto)
        {
            var product = await _context.Products.FindAsync(dto.Id);
            if (product == null) throw new Exception("Not found");
            product.Name = dto.Name;
            product.Slug = dto.Slug.ToLower();
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.ShortDescription = dto.ShortDescription;
            product.LongDescription = dto.LongDescription;
            product.ImageUrl = dto.ImageUrl;
            product.CategoryId = dto.CategoryId;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) throw new Exception("Not found");
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ProductDto>> GetAllAsync()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return products.Select(MapToDto).ToList();
        }
        public async Task<List<ProductDto>> GetByIdsAsync(List<int> productIds)
        {
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .Include(p => p.Category)
                .ToListAsync();
            return products.Select(MapToDto).ToList();
        }
        private ProductDto MapToDto(Product p)
        {
            return new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Price = p.Price,
                Stock = p.Stock,
                ShortDescription = p.ShortDescription,
                LongDescription = p.LongDescription,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name // Maps the category name
            };
        }
    }
}