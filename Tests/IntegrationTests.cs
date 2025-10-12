using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public IntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext registration
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database
                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });

                    // Add test authentication
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
                });
            });
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task PlaceOrder_EndToEnd_Works()
        {
            // Arrange: Reset in-memory DB
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            context.Products.Add(new Product
            {
                Id = 1,
                Name = "Test",
                Price = 10m,
                Stock = 5
            });
            await context.SaveChangesAsync();

            // Act
            var content = new StringContent(
                "{ \"productId\": 1, \"quantity\": 2 }",
                Encoding.UTF8,
                "application/json"
            );
            var response = await _client.PostAsync("/api/orders", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var updatedProduct = await context.Products.FindAsync(1);
            Assert.Equal(3, updatedProduct.Stock);
        }
    }
}