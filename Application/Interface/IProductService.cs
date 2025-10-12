using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetByIdsAsync(List<int> productIds); // Add this line
        Task<List<ProductDto>> GetByCategoryAsync(int categoryId, int page, int pageSize, string sortBy = "price");
        Task<List<ProductDto>> SearchAsync(string keyword, int page, int pageSize);
        Task<ProductDto> GetByIdAsync(int id);
        Task AddAsync(ProductDto dto);
        Task UpdateAsync(ProductDto dto);
        Task DeleteAsync(int id);

        Task<List<ProductDto>> GetAllAsync();
    }
}