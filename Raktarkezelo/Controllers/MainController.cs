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

        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> Main()
        {
            var products = await _context.Products.ToListAsync();

            ViewBag.TotalProducts = products.Count;
            ViewBag.LowStockCount = products.Count(p => p.Stock <= p.MinStock);

            ViewBag.LastTransactions = await _context.StockTransactions
                .Include(x => x.Product)
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Chart data: stock per category
            var categoryData = products
                .GroupBy(p => p.Category)
                .Select(g => new { Category = g.Key, Total = g.Sum(p => p.Stock) })
                .OrderBy(x => x.Category)
                .ToList();
            ViewBag.ChartLabels = System.Text.Json.JsonSerializer.Serialize(categoryData.Select(x => x.Category).ToList());
            ViewBag.ChartValues = System.Text.Json.JsonSerializer.Serialize(categoryData.Select(x => x.Total).ToList());

            return View(products);
        }

        [Authorize]
        public async Task<IActionResult> Products(string search, string category, bool lowStockOnly = false)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            if (lowStockOnly)
            {
                query = query.Where(p => p.Stock <= p.MinStock);
            }

            var products = await query.ToListAsync();

            ViewBag.Categories = await _context.Products
                .Select(p => p.Category)
                .Distinct()
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
            product.MinStock = model.MinStock;
            product.Note = model.Note?.Trim();
            // Stock is intentionally not updated here — use Inbound/Outbound/Adjustments for stock changes.

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "A termék adatai sikeresen frissítve lettek.";
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

            bool hasTransactions = await _context.StockTransactions
                .AnyAsync(x => x.ProductId == id);

            if (hasTransactions)
            {
                TempData["ErrorMessage"] = "A termék nem törölhető, mert már tartoznak hozzá készletmozgások.";
                return RedirectToAction(nameof(Products));
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "A termék sikeresen törölve lett.";
            return RedirectToAction(nameof(Products));
        }
    }
}