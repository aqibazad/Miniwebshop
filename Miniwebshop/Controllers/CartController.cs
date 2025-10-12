using Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Web.Controllers
{
    public class CartController : Controller
    {
        private const string CartSessionKey = "Cart";

        private List<CartItemDto> GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            return string.IsNullOrEmpty(cartJson) ? new List<CartItemDto>() : JsonConvert.DeserializeObject<List<CartItemDto>>(cartJson);
        }

        private void SaveCart(List<CartItemDto> cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonConvert.SerializeObject(cart));
        }

        [HttpPost]
        public IActionResult Add(int productId, int quantity = 1)
        {
            var cart = GetCart();
            var item = cart.Find(i => i.ProductId == productId);
            if (item != null) item.Quantity += quantity;
            else cart.Add(new CartItemDto { ProductId = productId, Quantity = quantity });
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Update(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.Find(i => i.ProductId == productId);
            if (item != null) item.Quantity = quantity;
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Remove(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(i => i.ProductId == productId);
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart); // You'll need to fetch product details in view if needed
        }
    }
}