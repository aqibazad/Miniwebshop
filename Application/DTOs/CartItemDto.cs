using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int Stock { get; set; } // Add this property
        public string ImageUrl { get; set; }
        public int AvailableStock { get; set; } = 0;
        public bool HasSufficientStock { get; set; } = true;

        public decimal Subtotal => Price * Quantity;
    }
}
