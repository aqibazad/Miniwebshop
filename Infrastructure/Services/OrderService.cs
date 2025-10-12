using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<OrderDto>> GetUserOrdersAsync(string userId)
        {
            var orders = await _context.Orders.Where(o => o.UserId == userId).ToListAsync();
            return orders.Select(o => MapToDto(o)).ToList();
        }

        public async Task<OrderDto> GetOrderDetailsAsync(int orderId, string userId)
        {
            var order = await _context.Orders.Include(o => o.Items).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
            if (order == null) throw new Exception("Not found or not yours");
            return MapToDto(order);
        }

        public async Task<int> PlaceOrderAsync(string userId, string shippingAddress, List<CartItemDto> cartItems)
        {
            if (cartItems.Count == 0) throw new Exception("Empty cart");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = new Order
                {
                    UserId = userId,
                    Date = DateTime.UtcNow,
                    Status = "Pending",
                    ShippingAddress = shippingAddress,
                    TotalPrice = 0
                };

                foreach (var item in cartItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                        throw new Exception($"Product '{item.ProductName}' not found");

                    if (product.Stock < item.Quantity)
                    {
                        var availableStock = product.Stock;
                        throw new Exception($"Out of stock for '{item.ProductName}'. Available: {availableStock}, Requested: {item.Quantity}");
                    }

                    var orderItem = new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        PriceAtPurchase = product.Price
                    };
                    order.Items.Add(orderItem);
                    order.TotalPrice += product.Price * item.Quantity;

                    product.Stock -= item.Quantity;
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return order.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<List<OrderDto>> GetAllAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Category)
                .ToListAsync();

            return orders.Select(MapToDto).ToList();
        }

        private OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                Date = order.Date,
                Status = order.Status,
                TotalPrice = order.TotalPrice,
                ShippingAddress = order.ShippingAddress,
                Items = order.Items.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    Price = oi.PriceAtPurchase,
                    CategoryName = oi.Product.Category?.Name ?? "Unknown" // Add category name here
                }).ToList()
            };
        }
    }
}