using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderDto>> GetUserOrdersAsync(string userId);
        Task<OrderDto> GetOrderDetailsAsync(int orderId, string userId); // Validate ownership
        Task<int> PlaceOrderAsync(string userId, string shippingAddress, List<CartItemDto> cartItems); // Returns orderId

        Task<List<OrderDto>> GetAllAsync();
    }
}