using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;

        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } 
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
    public class CustomerDashboardViewModel
    {
        public UserProfileDto UserProfile { get; set; } = new UserProfileDto();
        public OrderStatsDto OrderStats { get; set; } = new OrderStatsDto();
    }

    public class OrderStatsDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int PendingOrders { get; set; }
    }
}
