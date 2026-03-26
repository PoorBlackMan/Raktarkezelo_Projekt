using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Raktarkezelo.Data;
using Raktarkezelo.Models.Entities;
using Raktarkezelo.Models.Enums;
using Raktarkezelo.Models.Viewmodel;
using System.Security.Claims;

namespace Raktarkezelo.Controllers
{
    [Authorize(Roles = nameof(UserRole.Admin) + "," + nameof(UserRole.Manager))]
    public class AdminController : Controller
    {
        private readonly RaktarDb _context;

        public AdminController(RaktarDb context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Inbound()
        {
            var model = new StockTransactionViewModel
            {
                Products = await GetProductsSelectList()
            };

            ViewBag.RecentTransactions = await _context.StockTransactions
                .Include(x => x.Product)
                .Include(x => x.User)
                .Where(x => x.Type == "IN")
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .ToListAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inbound(StockTransactionViewModel model)
        {
            model.Products = await GetProductsSelectList();

            if (!ModelState.IsValid)
            {
                ViewBag.RecentTransactions = await GetRecentTransactions("IN");
                return View(model);
            }

            var product = await _context.Products.FindAsync(model.ProductId);
            if (product == null)
            {
                ModelState.AddModelError("", "A kiválasztott termék nem található.");
                ViewBag.RecentTransactions = await GetRecentTransactions("IN");
                return View(model);
            }

            var userId = GetCurrentUserId();
            if (userId == null)
                return Forbid();

            product.Stock += model.Quantity;

            var transaction = new StockTransactions
            {
                ProductId = product.ProductId,
                UserId = userId.Value,
                Type = "IN",
                Quantity = model.Quantity,
                Note = model.Note ?? string.Empty,
                CreatedAt = DateTime.Now
            };

            _context.StockTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Inbound));
        }

        [HttpGet]
        public async Task<IActionResult> Outbound()
        {
            var model = new StockTransactionViewModel
            {
                Products = await GetProductsSelectList()
            };

            ViewBag.RecentTransactions = await _context.StockTransactions
                .Include(x => x.Product)
                .Include(x => x.User)
                .Where(x => x.Type == "OUT")
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .ToListAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Outbound(StockTransactionViewModel model)
        {
            model.Products = await GetProductsSelectList();

            if (!ModelState.IsValid)
            {
                ViewBag.RecentTransactions = await GetRecentTransactions("OUT");
                return View(model);
            }

            var product = await _context.Products.FindAsync(model.ProductId);
            if (product == null)
            {
                ModelState.AddModelError("", "A kiválasztott termék nem található.");
                ViewBag.RecentTransactions = await GetRecentTransactions("OUT");
                return View(model);
            }

            if (product.Stock < model.Quantity)
            {
                ModelState.AddModelError("Quantity", "Nincs elegendő készlet.");
                ViewBag.RecentTransactions = await GetRecentTransactions("OUT");
                return View(model);
            }

            var userId = GetCurrentUserId();
            if (userId == null)
                return Forbid();

            product.Stock -= model.Quantity;

            var transaction = new StockTransactions
            {
                ProductId = product.ProductId,
                UserId = userId.Value,
                Type = "OUT",
                Quantity = model.Quantity,
                Note = model.Note ?? string.Empty,
                CreatedAt = DateTime.Now
            };

            _context.StockTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Outbound));
        }

        [HttpGet]
        public async Task<IActionResult> Adjustments()
        {
            var model = new AdjustmentViewModel
            {
                Products = await GetProductsSelectList()
            };

            ViewBag.RecentTransactions = await _context.StockTransactions
                .Include(x => x.Product)
                .Include(x => x.User)
                .Where(x => x.Type == "ADJUST")
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .ToListAsync();

            return View(model);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adjustment(AdjustmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.RecentTransactions = await GetRecentTransactions("ADJUST");
                return View(model);
            }

            var product = await _context.Products.FindAsync(model.ProductId);
            if (product == null)
                return NotFound();

            int oldStock = product.Stock;
            int newStock = model.NewQuantity;
            int difference = newStock - oldStock;

            if (difference == 0)
            {
                ModelState.AddModelError("NewQuantity", "Az új készletérték nem tér el a jelenlegi készlettől.");
                ViewBag.RecentTransactions = await GetRecentTransactions("ADJUST");
                return View(model);
            }

            product.Stock = newStock;

            _context.StockTransactions.Add(new StockTransactions
            {
                ProductId = product.ProductId,
                UserId = int.Parse(User.FindFirst("UserId")!.Value),
                Type = "ADJUST",
                Quantity = Math.Abs(difference),
                CreatedAt = DateTime.Now,
                Note = $"Korrigálva {oldStock}-ról {newStock}-ra"
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "A készletkorrekció sikeresen megtörtént.";
            return RedirectToAction(nameof(Adjustment));
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> Reports(DateTime? fromDate, DateTime? toDate, string? type, int? productId)
        {
            var query = _context.StockTransactions
                .Include(x => x.Product)
                .Include(x => x.User)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.CreatedAt < toDate.Value.Date.AddDays(1));
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(x => x.Type == type);
            }

            if (productId.HasValue)
            {
                query = query.Where(x => x.ProductId == productId.Value);
            }

            var transactions = await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            ViewBag.Products = await _context.Products
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewBag.SelectedFromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.SelectedToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.SelectedType = type;
            ViewBag.SelectedProductId = productId;

            return View(transactions);
        }

        private async Task<List<SelectListItem>> GetProductsSelectList()
        {
            return await _context.Products
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem
                {
                    Value = p.ProductId.ToString(),
                    Text = p.Name + " (" + p.Category + ") - Készlet: " + p.Stock
                })
                .ToListAsync();
        }

        private async Task<List<StockTransactions>> GetRecentTransactions(string type)
        {
            return await _context.StockTransactions
                .Include(x => x.Product)
                .Include(x => x.User)
                .Where(x => x.Type == type)
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .ToListAsync();
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(claim, out int userId))
                return userId;

            return null;
        }
    }
}