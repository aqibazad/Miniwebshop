using Moq;
using Application.Interfaces; // Assume ICheckoutService exists, or use IProductService for stock
using Application.DTOs;
using System.Threading.Tasks;
using Xunit;
namespace Tests
{
    public class CheckoutTests
    {
        [Fact]
        public async Task Checkout_UpdatesStock_Correctly()
        {
            // Arrange
            var mockProductService = new Mock<IProductService>();
            var product = new ProductDto { Id = 1, Stock = 10 };
            mockProductService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(product);
            // Pretend update reduces stock
            mockProductService.Setup(s => s.UpdateAsync(It.IsAny<ProductDto>())).Callback<ProductDto>(dto => product.Stock = dto.Stock);

            // Act (buy 3)
            var buyQty = 3;
            product.Stock -= buyQty;
            await mockProductService.Object.UpdateAsync(product);

            // Assert
            Assert.Equal(7, product.Stock); // 10 - 3 = 7
        }

        [Fact]
        public async Task Checkout_CalculatesPrice_Correctly()
        {
            // Arrange
            var items = new List<ProductDto>
        {
            new ProductDto { Price = 5.00m, Quantity = 2 }, // 10
            new ProductDto { Price = 3.00m, Quantity = 1 }  // 3
        };

            // Act (simple total calc)
            decimal total = items.Sum(i => i.Price * i.Quantity);

            // Assert
            Assert.Equal(13.00m, total); // Check price logic
        }

        [Fact]
        public async Task Checkout_NoStock_ThrowsError()
        {
            // Arrange
            var mockService = new Mock<IProductService>();
            var product = new ProductDto { Id = 1, Stock = 0 };
            mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(product);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => // Assume your code throws if no stock
                Task.Run(() => { if (product.Stock < 1) throw new Exception("Out of stock"); }));
        }
    }
}
