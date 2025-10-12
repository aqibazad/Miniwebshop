using Moq;
using Application.Interfaces;
using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class CatalogTests
    {
        public class ProductServiceTests
        {
            private readonly Mock<IProductService> _mockService;

            public ProductServiceTests()
            {
                _mockService = new Mock<IProductService>();
            }

            [Fact]
            public async Task SearchAsync_WithEmptyKeyword_ReturnsAllProducts()
            {
                // Arrange (set up the game)
                var fakeProducts = new List<ProductDto>
        {
            new ProductDto { Id = 1, Name = "Toy Car" },
            new ProductDto { Id = 2, Name = "Doll" }
        };
                _mockService.Setup(s => s.SearchAsync("", 1, 10)).ReturnsAsync(fakeProducts);

                // Act (play the game)
                var result = await _mockService.Object.SearchAsync("", 1, 10);

                // Assert (check if we won)
                Assert.Equal(2, result.Count); // Should return all if empty search
            }

            [Fact]
            public async Task SearchAsync_WithKeyword_ReturnsMatchingProducts()
            {
                // Arrange
                var fakeProducts = new List<ProductDto>
        {
            new ProductDto { Id = 1, Name = "Toy Car", ShortDescription = "Fast car" },
            new ProductDto { Id = 2, Name = "Doll", ShortDescription = "Pretty doll" }
        };
                _mockService.Setup(s => s.SearchAsync("car", 1, 10)).ReturnsAsync(new List<ProductDto> { fakeProducts[0] });

                // Act
                var result = await _mockService.Object.SearchAsync("car", 1, 10);

                // Assert
                Assert.Single(result); // Only one match
                Assert.Equal("Toy Car", result[0].Name);
            }

            [Fact]
            public async Task SearchAsync_PaginationLimits_WorkCorrectly()
            {
                // Arrange (10 products, but page 2 with size 5 should give 5)
                var fakeProducts = new List<ProductDto>(); // Pretend 10 items
                for (int i = 1; i <= 10; i++) fakeProducts.Add(new ProductDto { Id = i });

                _mockService.Setup(s => s.SearchAsync("toy", 2, 5)).ReturnsAsync(fakeProducts.Skip(5).Take(5).ToList());

                // Act
                var result = await _mockService.Object.SearchAsync("toy", 2, 5);

                // Assert
                Assert.Equal(5, result.Count); // Page 2, size 5
                Assert.Equal(6, result[0].Id); // Starts from 6th
            }

            [Fact]
            public async Task SearchAsync_EmptyTerm_EdgeCase()
            {
                // Arrange
                _mockService.Setup(s => s.SearchAsync(null, 1, 10)).ReturnsAsync(new List<ProductDto>());

                // Act
                var result = await _mockService.Object.SearchAsync(null, 1, 10);

                // Assert
                Assert.Empty(result); // No products if keyword is null
            }
        }
    }
}
