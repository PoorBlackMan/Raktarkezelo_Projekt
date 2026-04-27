using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Raktarkezelo.Data;
using Raktarkezelo.Models.Entities;
using Raktarkezelo.Models.Enums;
using Raktarkezelo.Models.Viewmodel;
using Raktarkezelo.Models.Entities;
using Raktarkezelo.Services;
using System.Security.Claims;

namespace Raktarkezelo.Controllers
{
    // Only Managers can perform write actions — Admins get read-only access via the Users() GET only
    public class ManagerController : Controller
    {
        private readonly RaktarDb _context;
        private readonly AuditService _audit;

        public ManagerController(RaktarDb context, AuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        // ─── LIST (Admin + Manager can view) ─────────────────────────────────

        [Authorize(Roles = "Manager,Admin")]
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _context.Userinfo
                .OrderBy(u => u.Role)
                .ThenBy(u => u.Username)
                .ToListAsync();

            return View(users);
        }

        // ─── CREATE (Manager only) ────────────────────────────────────────────

        [Authorize(Roles = "Manager")]
        [HttpGet]
        public IActionResult CreateUser()
        {
            return View(new CreateUserViewModel());
        }

        [Authorize(Roles = "Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool emailExists = await _context.Userinfo.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Ez az email cím már használatban van.");
                return View(model);
            }

            bool usernameExists = await _context.Userinfo.AnyAsync(u => u.Username == model.Username);
            if (usernameExists)
            {
                ModelState.AddModelError("Username", "Ez a felhasználónév már foglalt.");
                return View(model);
            }

            var user = new Userinfo
            {
                Email = model.Email.Trim(),
                Username = model.Username.Trim(),
                Passwordhash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = model.Role,
                IsActive = true
            };

            _context.Userinfo.Add(user);
            await _context.SaveChangesAsync();

            await _audit.LogAsync(GetCurrentUserId(), User.Identity!.Name!, "UserCreate", $"Létrehozva: {user.Username} ({user.Role})");
            TempData["SuccessMessage"] = $"A(z) '{user.Username}' felhasználó sikeresen létrehozva.";
            return RedirectToAction(nameof(Users));
        }

        // ─── EDIT (Manager only) ──────────────────────────────────────────────

        [Authorize(Roles = "Manager")]
        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _context.Userinfo.FindAsync(id);
            if (user == null)
                return NotFound();

            // Manager cannot edit another Manager's account (can edit admins and users)
            if (user.Role == UserRole.Manager && user.Id != GetCurrentUserId())
            {
                TempData["ErrorMessage"] = "Másik manager fiókját nem szerkesztheted.";
                return RedirectToAction(nameof(Users));
            }

            return View(new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Role = user.Role,
                IsActive = user.IsActive
            });
        }

        [Authorize(Roles = "Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Userinfo.FindAsync(model.Id);
            if (user == null)
                return NotFound();

            if (user.Role == UserRole.Manager && user.Id != GetCurrentUserId())
            {
                TempData["ErrorMessage"] = "Másik manager fiókját nem szerkesztheted.";
                return RedirectToAction(nameof(Users));
            }

            // Prevent demoting the last Manager
            if (user.Role == UserRole.Manager && model.Role != UserRole.Manager)
            {
                int managerCount = await _context.Userinfo.CountAsync(u => u.Role == UserRole.Manager);
                if (managerCount <= 1)
                {
                    ModelState.AddModelError("Role", "Nem lehet az utolsó manager szerepkörét megváltoztatni.");
                    return View(model);
                }
            }

            bool emailTaken = await _context.Userinfo.AnyAsync(u => u.Email == model.Email && u.Id != model.Id);
            if (emailTaken)
            {
                ModelState.AddModelError("Email", "Ez az email cím már használatban van.");
                return View(model);
            }

            bool usernameTaken = await _context.Userinfo.AnyAsync(u => u.Username == model.Username && u.Id != model.Id);
            if (usernameTaken)
            {
                ModelState.AddModelError("Username", "Ez a felhasználónév már foglalt.");
                return View(model);
            }

            user.Email = model.Email.Trim();
            user.Username = model.Username.Trim();
            user.Role = model.Role;
            user.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            await _audit.LogAsync(GetCurrentUserId(), User.Identity!.Name!, "UserEdit", $"Módosítva: {user.Username}");
            TempData["SuccessMessage"] = $"A(z) '{user.Username}' felhasználó adatai sikeresen frissítve.";
            return RedirectToAction(nameof(Users));
        }

        // ─── TOGGLE STATUS (Manager only) ─────────────────────────────────────

        [Authorize(Roles = "Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Userinfo.FindAsync(id);
            if (user == null)
                return NotFound();

            // Cannot deactivate yourself
            if (user.Id == GetCurrentUserId())
            {
                TempData["ErrorMessage"] = "A saját fiókodat nem tilthatod le.";
                return RedirectToAction(nameof(Users));
            }

            // Cannot deactivate another Manager
            if (user.Role == UserRole.Manager)
            {
                TempData["ErrorMessage"] = "Másik manager fiókját nem tilthatod le.";
                return RedirectToAction(nameof(Users));
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            string toggleAction = user.IsActive ? "aktiválva" : "letiltva";
            await _audit.LogAsync(GetCurrentUserId(), User.Identity!.Name!, "UserToggle", $"{user.Username} fiókja {toggleAction}");
            TempData["SuccessMessage"] = user.IsActive
                ? $"'{user.Username}' fiókja aktiválva."
                : $"'{user.Username}' fiókja letiltva.";

            return RedirectToAction(nameof(Users));
        }

        // ─── RESET PASSWORD (Manager only) ────────────────────────────────────

        [Authorize(Roles = "Manager")]
        [HttpGet]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _context.Userinfo.FindAsync(id);
            if (user == null)
                return NotFound();

            if (user.Role == UserRole.Manager && user.Id != GetCurrentUserId())
            {
                TempData["ErrorMessage"] = "Másik manager jelszavát nem állíthatod vissza.";
                return RedirectToAction(nameof(Users));
            }

            return View(new ResetPasswordViewModel
            {
                UserId = user.Id,
                Username = user.Username
            });
        }

        [Authorize(Roles = "Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Userinfo.FindAsync(model.UserId);
            if (user == null)
                return NotFound();

            if (user.Role == UserRole.Manager && user.Id != GetCurrentUserId())
            {
                TempData["ErrorMessage"] = "Másik manager jelszavát nem állíthatod vissza.";
                return RedirectToAction(nameof(Users));
            }

            user.Passwordhash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            await _audit.LogAsync(GetCurrentUserId(), User.Identity!.Name!, "PasswordReset", $"Jelszó visszaállítva: {user.Username}");
            TempData["SuccessMessage"] = $"'{user.Username}' jelszava sikeresen visszaállítva.";
            return RedirectToAction(nameof(Users));
        }

        // ─── DELETE (Manager only) ────────────────────────────────────────────

        [Authorize(Roles = "Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Userinfo.FindAsync(id);
            if (user == null)
                return NotFound();

            // Cannot delete yourself
            if (user.Id == GetCurrentUserId())
            {
                TempData["ErrorMessage"] = "A saját fiókodat nem törölheted.";
                return RedirectToAction(nameof(Users));
            }

            // Cannot delete another Manager
            if (user.Role == UserRole.Manager)
            {
                TempData["ErrorMessage"] = "Másik manager fiókját nem törölheted.";
                return RedirectToAction(nameof(Users));
            }

            bool hasTransactions = await _context.StockTransactions.AnyAsync(t => t.UserId == id);
            if (hasTransactions)
            {
                TempData["ErrorMessage"] = $"'{user.Username}' nem törölhető, mert készletmozgások tartoznak hozzá.";
                return RedirectToAction(nameof(Users));
            }

            string deletedName = user.Username;
            _context.Userinfo.Remove(user);
            await _context.SaveChangesAsync();
            await _audit.LogAsync(GetCurrentUserId(), User.Identity!.Name!, "UserDelete", $"Törölve: {deletedName}");
            TempData["SuccessMessage"] = $"'{deletedName}' felhasználó sikeresen törölve.";
            return RedirectToAction(nameof(Users));
        }

        // ─── AUDIT LOG ────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> AuditLog(string? action, string? username, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(action))
                query = query.Where(a => a.Action == action);
            if (!string.IsNullOrWhiteSpace(username))
                query = query.Where(a => a.Username.Contains(username));
            if (fromDate.HasValue)
                query = query.Where(a => a.CreatedAt >= fromDate.Value.Date);
            if (toDate.HasValue)
                query = query.Where(a => a.CreatedAt < toDate.Value.Date.AddDays(1));

            var logs = await query
                .OrderByDescending(a => a.CreatedAt)
                .Take(500)
                .ToListAsync();

            ViewBag.SelectedAction   = action;
            ViewBag.SelectedUsername = username;
            ViewBag.SelectedFromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.SelectedToDate   = toDate?.ToString("yyyy-MM-dd");

            return View(logs);
        }

        // ─── HELPER ───────────────────────────────────────────────────────────

        private int GetCurrentUserId()
            => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
