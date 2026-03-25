using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Raktarkezelo.Data;
using Raktarkezelo.Models.Entities;
using Raktarkezelo.Models.Viewmodel;

namespace Raktarkezelo.Controllers
{
    public class MainController : Controller
    {
        private readonly RaktarDb _context;

        public MainController(RaktarDb context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Main()
        {
            var products = await _context.Products
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(products);
        }

        [Authorize]
        public async Task<IActionResult> Products()
        {
            var products = await _context.Products
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();

            return View(products);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public IActionResult CreateProduct()
        {
            return View(new ProductViewModel());
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var product = new Products
            {
                Name = model.Name.Trim(),
                Category = model.Category.Trim(),
                Stock = model.Stock,
                MinStock = model.MinStock,
                Note = model.Note?.Trim() ?? string.Empty
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Products));
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            var model = new ProductViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Category = product.Category,
                Stock = product.Stock,
                MinStock = product.MinStock,
                Note = product.Note
            };

            return View(model);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(ProductViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var product = await _context.Products.FindAsync(model.ProductId);
            if (product == null)
                return NotFound();

            product.Name = model.Name.Trim();
            product.Category = model.Category.Trim();
            product.Stock = model.Stock;
            product.MinStock = model.MinStock;
            product.Note = model.Note?.Trim() ?? string.Empty;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Products));
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Products));
        }
    }
}