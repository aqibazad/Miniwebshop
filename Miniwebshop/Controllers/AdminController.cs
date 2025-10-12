using Application.DTOs;
using Application.Interface;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Web.Controllers
{
    [Authorize(Roles = "Admin")] // Ensures only users with the "Admin" role can access this controller
    public class AdminController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ICategoryService categoryService, IProductService productService, IOrderService orderService, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _categoryService = categoryService;
            _productService = productService;
            _orderService = orderService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Orders()
        {
            var orders = await _orderService.GetAllAsync();
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // Total Revenue (sum of all order totals)
            var allOrders = await _orderService.GetAllAsync();
            var totalRevenue = allOrders.Sum(o => o.TotalPrice);

            // Total Orders
            var totalOrders = allOrders.Count;

            // Total Products
            var allProducts = await _productService.GetAllAsync();
            var totalProducts = allProducts.Count;

            // Total Users
            var allUsers = await _userManager.Users.ToListAsync();
            var totalUsers = allUsers.Count;

            // Changes (compare to last month)
            var lastMonthStart = DateTime.Now.AddMonths(-1).Date;
            var lastMonthEnd = DateTime.Now.Date;
            var lastMonthOrders = allOrders.Where(o => o.Date >= lastMonthStart && o.Date <= lastMonthEnd).ToList();
            var lastMonthRevenue = lastMonthOrders.Sum(o => o.TotalPrice);
            var lastMonthOrdersCount = lastMonthOrders.Count;
            var lastMonthProducts = allProducts.Count(p => p.CreatedDate >= lastMonthStart);
            var lastMonthUsers = allUsers.Count(u => u.CreatedDate >= lastMonthStart);

            var revenueChange = lastMonthRevenue > 0 ? ((totalRevenue - lastMonthRevenue) / lastMonthRevenue * 100) : 0;
            var ordersChange = lastMonthOrdersCount > 0 ? ((totalOrders - lastMonthOrdersCount) / (double)lastMonthOrdersCount * 100) : 0;
            var productsChange = lastMonthProducts > 0 ? ((totalProducts - lastMonthProducts) / (double)lastMonthProducts * 100) : 0;
            var usersChange = lastMonthUsers > 0 ? ((totalUsers - lastMonthUsers) / (double)lastMonthUsers * 100) : 0;

            // Recent Orders (last 3) with dynamic customer names
            var recentOrders = allOrders
                .OrderByDescending(o => o.Date)
                .Take(3) // Limit to last 3 orders
                .Select(o => new
                {
                    Id = o.Id,
                    Customer = allUsers.FirstOrDefault(u => u.Id == o.UserId)?.Name ?? "Unknown", // Assuming Order has UserId
                    Amount = o.TotalPrice
                })
                .ToList();

            // Top Products (top 3 by sales)
            var topProducts = allOrders
                .SelectMany(o => o.Items)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new
                {
                    Product = allProducts.FirstOrDefault(p => p.Id == g.Key),
                    Sales = g.Sum(oi => oi.Quantity)
                })
                .Where(p => p.Product != null)
                .OrderByDescending(p => p.Sales)
                .Take(3)
                .Select(p => new
                {
                    Name = p.Product!.Name,
                    Sales = p.Sales
                })
                .ToList();

            ViewBag.TotalRevenue = totalRevenue.ToString("C");
            ViewBag.RevenueChange = revenueChange.ToString("F1");
            ViewBag.TotalOrders = totalOrders;
            ViewBag.OrdersChange = ordersChange.ToString("F1");
            ViewBag.TotalProducts = totalProducts;
            ViewBag.ProductsChange = productsChange.ToString("F1");
            ViewBag.TotalUsers = totalUsers;
            ViewBag.UsersChange = usersChange.ToString("F1");
            ViewBag.RecentOrders = recentOrders;
            ViewBag.TopProducts = topProducts;

            return View();
        }
        public async Task<IActionResult> Categories()
        {
            var categories = await _categoryService.GetAllAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult AddCategory()
        {
            return View(new CategoryDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(CategoryDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _categoryService.AddAsync(dto);
                    return RedirectToAction("Categories");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            try
            {
                var category = await _categoryService.GetByIdAsync(id);
                return View(category);
            }
            catch (Exception ex)
            {
                return NotFound($"Category not found: {ex.Message}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(CategoryDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _categoryService.UpdateAsync(dto);
                    return RedirectToAction("Categories");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                await _categoryService.DeleteAsync(id);
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        public async Task<IActionResult> Products()
        {
            var products = await _productService.GetAllAsync();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            var categories = await _categoryService.GetAllAsync();
            ViewData["Categories"] = categories;
            return View(new ProductDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(ProductDto dto, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var extension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("ImageFile", "Only JPG, JPEG, PNG, and GIF files are allowed.");
                        }
                        else
                        {
                            var fileName = Guid.NewGuid().ToString() + extension;
                            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");
                            var filePath = Path.Combine(directoryPath, fileName);

                            if (!Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await ImageFile.CopyToAsync(stream);
                            }

                            dto.ImageUrl = $"/images/products/{fileName}";
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("ImageFile", "Please upload an image.");
                    }

                    if (ModelState.IsValid)
                    {
                        await _productService.AddAsync(dto);
                        return RedirectToAction("Products");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            var categories = await _categoryService.GetAllAsync();
            ViewData["Categories"] = categories;
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                var categories = await _categoryService.GetAllAsync();
                ViewData["Categories"] = categories;
                return View(product);
            }
            catch (Exception ex)
            {
                return NotFound($"Product not found: {ex.Message}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(ProductDto dto, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var extension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("ImageFile", "Only JPG, JPEG, PNG, and GIF files are allowed.");
                        }
                        else
                        {
                            var fileName = Guid.NewGuid().ToString() + extension;
                            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");
                            var filePath = Path.Combine(directoryPath, fileName);

                            if (!Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await ImageFile.CopyToAsync(stream);
                            }

                            dto.ImageUrl = $"/images/products/{fileName}";
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        await _productService.UpdateAsync(dto);
                        return RedirectToAction("Products");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            var categories = await _categoryService.GetAllAsync();
            ViewData["Categories"] = categories;
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                await _productService.DeleteAsync(id);
                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        // New Action Methods for Managing Customers
        public async Task<IActionResult> Customers()
        {
            var customers = await _userManager.GetUsersInRoleAsync("Customer");
            return View(customers);
        }

        [HttpGet]
        public IActionResult AddCustomer()
        {
            return View(new RegisterViewModel()); // Custom view model for registration
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCustomer(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = new ApplicationUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        Name = model.FullName // Assuming a custom property
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "Customer");
                        return RedirectToAction("Customers");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditCustomer(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound("User not found");
                }
                var model = new EditUserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.Name // Assuming a custom property
                };
                return View(model);
            }
            catch (Exception ex)
            {
                return NotFound($"Error: {ex.Message}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCustomer(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(model.Id);
                    if (user == null)
                    {
                        return NotFound("User not found");
                    }

                    user.Email = model.Email;
                    user.Name = model.FullName; // Assuming a custom property
                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var result = await _userManager.ResetPasswordAsync(user, token, model.Password);
                        if (!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                            return View(model);
                        }
                    }

                    var resultUpdate = await _userManager.UpdateAsync(user);
                    if (resultUpdate.Succeeded)
                    {
                        return RedirectToAction("Customers");
                    }
                    foreach (var error in resultUpdate.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound("User not found");
                }
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Customers");
                }
                return BadRequest("Failed to delete user");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }

    // View Models for Customer Management
    public class RegisterViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}