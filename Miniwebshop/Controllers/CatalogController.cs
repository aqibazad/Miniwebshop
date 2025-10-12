using Application.Interface;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Miniwebshop.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;

        public CatalogController(ICategoryService categoryService, IProductService productService)
        {
            _categoryService = categoryService;
            _productService = productService;
        }

        public async Task<IActionResult> Categories()
        {
            var categories = await _categoryService.GetAllAsync();
            return View(categories);
        }

        public async Task<IActionResult> Products(int categoryId, int page = 1, string sortBy = "price")
        {
            var products = await _productService.GetByCategoryAsync(categoryId, page, 10, sortBy);
            ViewBag.CategoryId = categoryId;
            ViewBag.Page = page;
            ViewBag.SortBy = sortBy;
            return View(products);
        }

        public async Task<IActionResult> Search(string keyword, int page = 1)
        {
            var products = await _productService.SearchAsync(keyword, page, 10);
            ViewBag.Keyword = keyword;
            ViewBag.Page = page;
            return View("Products", products); // Reuse view
        }
    }
}