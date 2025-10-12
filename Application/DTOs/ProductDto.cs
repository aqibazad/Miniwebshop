using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int AvailableStock { get; set; } // Available stock from database
        public bool HasSufficientStock { get; set; } // Validation flag
        public int Quantity { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string ShortDescription { get; set; } = string.Empty;
        public string LongDescription { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } // Add this property
        public decimal Subtotal => Price * Quantity;
    }
}
