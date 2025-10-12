using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Web.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;

        public CheckoutController(IOrderService orderService, IProductService productService)
        {
            _orderService = orderService;
            _productService = productService;
        }

        public IActionResult Index()
        {
            var cartWithValidation = GetCartWithStockValidation();
            ViewBag.HasValidStock = cartWithValidation.All(item => item.HasSufficientStock && item.Quantity > 0);
            ViewBag.CartItems = cartWithValidation;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(string shippingAddress)
        {
            if (string.IsNullOrWhiteSpace(shippingAddress))
            {
                ModelState.AddModelError("", "Shipping address is required.");
                return await LoadCartWithValidation();
            }

            var cartWithValidation = await GetCartWithStockValidationAsync();

            // Check for stock issues
            var stockIssues = cartWithValidation
                .Where(item => !item.HasSufficientStock)
                .Select(item => $"{item.ProductName}: Only {item.AvailableStock} items available")
                .ToList();

            if (stockIssues.Any())
            {
                TempData["StockError"] = string.Join(", ", stockIssues);
                ViewBag.CartItems = cartWithValidation;
                ViewBag.HasValidStock = false;
                return View();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            // Filter valid items only
            var validCartItems = cartWithValidation
                .Where(item => item.HasSufficientStock && item.Quantity > 0)
                .ToList();

            var orderId = await _orderService.PlaceOrderAsync(userId, shippingAddress, validCartItems);

            // Clear cart from session
            HttpContext.Session.Remove("Cart");

            return RedirectToAction("Details", "Orders", new { id = orderId });
        }

        private List<CartItemDto> GetCartWithStockValidation()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            var cart = string.IsNullOrEmpty(cartJson)
                ? new List<dynamic>()
                : JsonConvert.DeserializeObject<List<dynamic>>(cartJson);

            var cartWithValidation = new List<CartItemDto>();

            foreach (var item in cart)
            {
                try
                {
                    var productId = item.productId ?? item.ProductId;
                    var quantity = item.quantity ?? item.Quantity;
                    var price = item.price ?? item.Price ?? 0m;
                    var productName = item.productName ?? item.ProductName ?? "Unknown Product";

                    if (productId != null && quantity > 0)
                    {
                        var product = _productService.GetByIdAsync((int)productId).Result;
                        var hasSufficientStock = product != null && quantity <= product.Stock;

                        cartWithValidation.Add(new CartItemDto
                        {
                            ProductId = (int)productId,
                            ProductName = productName,
                            Quantity = (int)quantity,
                            Price = (decimal)price,
                            AvailableStock = product?.Stock ?? 0,
                            HasSufficientStock = hasSufficientStock
                        });
                    }
                }
                catch
                {
                    // Skip invalid items
                    continue;
                }
            }

            return cartWithValidation;
        }

        private async Task<List<CartItemDto>> GetCartWithStockValidationAsync()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            var cart = string.IsNullOrEmpty(cartJson)
                ? new List<dynamic>()
                : JsonConvert.DeserializeObject<List<dynamic>>(cartJson);

            var cartWithValidation = new List<CartItemDto>();
            var productIds = new List<int>();

            // First pass: collect product IDs
            foreach (var item in cart)
            {
                var productId = item.productId ?? item.ProductId;
                if (productId != null)
                {
                    productIds.Add((int)productId);
                }
            }

            // Get all products at once
            var products = await _productService.GetByIdsAsync(productIds.Distinct().ToList());
            var productDict = products.ToDictionary(p => p.Id, p => p);

            // Second pass: validate each item
            foreach (var item in cart)
            {
                try
                {
                    var productId = item.productId ?? item.ProductId;
                    var quantity = item.quantity ?? item.Quantity;
                    var price = item.price ?? item.Price ?? 0m;
                    var productName = item.productName ?? item.ProductName ?? "Unknown Product";

                    if (productId != null && quantity > 0)
                    {
                        var product = productDict.GetValueOrDefault((int)productId);
                        var hasSufficientStock = product != null && quantity <= product.Stock;

                        cartWithValidation.Add(new CartItemDto
                        {
                            ProductId = (int)productId,
                            ProductName = productName,
                            Quantity = (int)quantity,
                            Price = (decimal)price,
                            AvailableStock = product?.Stock ?? 0,
                            HasSufficientStock = hasSufficientStock
                        });
                    }
                }
                catch
                {
                    // Skip invalid items
                    continue;
                }
            }

            return cartWithValidation;
        }

        private async Task<IActionResult> LoadCartWithValidation()
        {
            var cartWithValidation = await GetCartWithStockValidationAsync();
            ViewBag.HasValidStock = cartWithValidation.All(item => item.HasSufficientStock && item.Quantity > 0);
            ViewBag.CartItems = cartWithValidation;
            return View("Index");
        }
    }
}