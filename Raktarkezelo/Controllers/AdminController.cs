using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Raktarkezelo.Data;
using Raktarkezelo.Models.Entities;
using Raktarkezelo.Models.Enums;
using Raktarkezelo.Models.Viewmodel;
using Raktarkezelo.Services;
using System.Security.Claims;

namespace Raktarkezelo.Controllers
{
    [Authorize(Roles = nameof(UserRole.Admin) + ",Manager")]
    public class AdminController : Controller
    {
        private readonly RaktarDb _context;
        private readonly AuditService _audit;

        public AdminController(RaktarDb context, AuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        // ─── INBOUND ──────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Inbound()
        {
            var model = new StockTransactionViewModel { Products = await GetProductsSelectList() };
            ViewBag.RecentTransactions = await GetRecentTransactions("IN");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inbound(StockTransactionViewModel model)
        {
            model.Products = await GetProductsSelectList();
            if (!ModelState.IsValid) { ViewBag.RecentTransactions = await GetRecentTransactions("IN"); return View(model); }

            var product = await _context.Products.FindAsync(model.ProductId);
            if (product == null) { ModelState.AddModelError("", "A kiválasztott termék nem található."); ViewBag.RecentTransactions = await GetRecentTransactions("IN"); return View(model); }

            var userId = GetCurrentUserId();
            if (userId == null) return Forbid();

            product.Stock += model.Quantity;
            _context.StockTransactions.Add(new StockTransactions { ProductId = product.ProductId, UserId = userId.Value, Type = "IN", Quantity = model.Quantity, Note = model.Note ?? string.Empty, CreatedAt = DateTime.Now });
            await _context.SaveChangesAsync();

            await _audit.LogAsync(userId, User.Identity!.Name!, "StockIn", $"Bevételezés: {product.Name} +{model.Quantity} db");
            return RedirectToAction(nameof(Inbound));
        }

        // ─── OUTBOUND ─────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Outbound()
        {
            var model = new StockTransactionViewModel { Products = await GetProductsSelectList() };
            ViewBag.RecentTransactions = await GetRecentTransactions("OUT");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Outbound(StockTransactionViewModel model)
        {
            model.Products = await GetProductsSelectList();
            if (!ModelState.IsValid) { ViewBag.RecentTransactions = await GetRecentTransactions("OUT"); return View(model); }

            var product = await _context.Products.FindAsync(model.ProductId);
            if (product == null) { ModelState.AddModelError("", "A kiválasztott termék nem található."); ViewBag.RecentTransactions = await GetRecentTransactions("OUT"); return View(model); }
            if (product.Stock < model.Quantity) { ModelState.AddModelError("Quantity", "Nincs elegendő készlet."); ViewBag.RecentTransactions = await GetRecentTransactions("OUT"); return View(model); }

            var userId = GetCurrentUserId();
            if (userId == null) return Forbid();

            product.Stock -= model.Quantity;
            _context.StockTransactions.Add(new StockTransactions { ProductId = product.ProductId, UserId = userId.Value, Type = "OUT", Quantity = model.Quantity, Note = model.Note ?? string.Empty, CreatedAt = DateTime.Now });
            await _context.SaveChangesAsync();

            await _audit.LogAsync(userId, User.Identity!.Name!, "StockOut", $"Kiadás: {product.Name} -{model.Quantity} db");
            return RedirectToAction(nameof(Outbound));
        }

        // ─── ADJUSTMENTS ─────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Adjustments()
        {
            var model = new AdjustmentViewModel { Products = await GetProductsSelectList() };
            ViewBag.RecentTransactions = await GetRecentTransactions("ADJUST");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adjustments(AdjustmentViewModel model)
        {
            model.Products = await GetProductsSelectList();
            if (!ModelState.IsValid) { ViewBag.RecentTransactions = await GetRecentTransactions("ADJUST"); return View(model); }

            var product = await _context.Products.FindAsync(model.ProductId);
            if (product == null) return NotFound();

            int oldStock = product.Stock;
            int newStock = model.NewQuantity;
            int difference = newStock - oldStock;

            if (difference == 0) { ModelState.AddModelError("NewQuantity", "Az új készletérték nem tér el a jelenlegi készlettől."); ViewBag.RecentTransactions = await GetRecentTransactions("ADJUST"); return View(model); }

            var userId = GetCurrentUserId();
            product.Stock = newStock;
            _context.StockTransactions.Add(new StockTransactions { ProductId = product.ProductId, UserId = userId!.Value, Type = "ADJUST", Quantity = Math.Abs(difference), CreatedAt = DateTime.Now, Note = $"Korrigálva {oldStock}-ról {newStock}-ra" });
            await _context.SaveChangesAsync();

            await _audit.LogAsync(userId, User.Identity!.Name!, "StockAdjust", $"Korrekció: {product.Name} {oldStock} → {newStock}");
            TempData["SuccessMessage"] = "A készletkorrekció sikeresen megtörtént.";
            return RedirectToAction(nameof(Adjustments));
        }

        // ─── REPORTS ─────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Reports(DateTime? fromDate, DateTime? toDate, string? type, int? productId)
        {
            var transactions = await BuildReportQuery(fromDate, toDate, type, productId).ToListAsync();

            ViewBag.Products = await _context.Products.OrderBy(p => p.Name).ToListAsync();
            ViewBag.SelectedFromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.SelectedToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.SelectedType = type;
            ViewBag.SelectedProductId = productId;

            return View(transactions);
        }

        // ─── EXCEL EXPORT ─────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> ExportExcel(DateTime? fromDate, DateTime? toDate, string? type, int? productId)
        {
            var transactions = await BuildReportQuery(fromDate, toDate, type, productId).ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Készletmozgások");

            // Header row
            var headers = new[] { "Dátum", "Termék", "Kategória", "Típus", "Mennyiség", "Felhasználó", "Megjegyzés" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
                cell.Style.Font.FontColor = XLColor.White;
            }

            // Data rows
            for (int r = 0; r < transactions.Count; r++)
            {
                var t = transactions[r];
                string typeLabel = t.Type switch { "IN" => "Bevételezés", "OUT" => "Kiadás", _ => "Korrekció" };
                ws.Cell(r + 2, 1).Value = t.CreatedAt.ToString("yyyy.MM.dd HH:mm");
                ws.Cell(r + 2, 2).Value = t.Product?.Name ?? "";
                ws.Cell(r + 2, 3).Value = t.Product?.Category ?? "";
                ws.Cell(r + 2, 4).Value = typeLabel;
                ws.Cell(r + 2, 5).Value = t.Quantity;
                ws.Cell(r + 2, 6).Value = t.User?.Username ?? "";
                ws.Cell(r + 2, 7).Value = t.Note ?? "";
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            string filename = $"keszletmozgasok_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
        }

        // ─── HELPERS ─────────────────────────────────────────────────────────

        private IQueryable<StockTransactions> BuildReportQuery(DateTime? fromDate, DateTime? toDate, string? type, int? productId)
        {
            var query = _context.StockTransactions.Include(x => x.Product).Include(x => x.User).AsQueryable();
            if (fromDate.HasValue) query = query.Where(x => x.CreatedAt >= fromDate.Value.Date);
            if (toDate.HasValue) query = query.Where(x => x.CreatedAt < toDate.Value.Date.AddDays(1));
            if (!string.IsNullOrWhiteSpace(type)) query = query.Where(x => x.Type == type);
            if (productId.HasValue) query = query.Where(x => x.ProductId == productId.Value);
            return query.OrderByDescending(x => x.CreatedAt);
        }

        private async Task<List<SelectListItem>> GetProductsSelectList() =>
            await _context.Products.OrderBy(p => p.Name)
                .Select(p => new SelectListItem { Value = p.ProductId.ToString(), Text = p.Name + " (" + p.Category + ") - Készlet: " + p.Stock })
                .ToListAsync();

        private async Task<List<StockTransactions>> GetRecentTransactions(string type) =>
            await _context.StockTransactions.Include(x => x.Product).Include(x => x.User)
                .Where(x => x.Type == type).OrderByDescending(x => x.CreatedAt).Take(10).ToListAsync();

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out int id) ? id : null;
        }
    }
}
