using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Raktarkezelo.Data;
using Raktarkezelo.Models.Viewmodel;
using Raktarkezelo.Services;
using System.Security.Claims;

namespace Raktarkezelo.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly RaktarDb _context;
        private readonly AuditService _audit;

        public ProfileController(RaktarDb context, AuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        // ─── VIEW PROFILE ─────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _context.Userinfo.FindAsync(GetUserId());
            if (user == null) return NotFound();
            return View(user);
        }

        // ─── CHANGE USERNAME ──────────────────────────────────────────────────

        [HttpGet]
        public IActionResult ChangeUsername() => View(new ChangeUsernameViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUsername(ChangeUsernameViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _context.Userinfo.FindAsync(GetUserId());
            if (user == null) return NotFound();

            bool taken = await _context.Userinfo.AnyAsync(u => u.Username == model.NewUsername && u.Id != user.Id);
            if (taken)
            {
                ModelState.AddModelError("NewUsername", "Ez a felhasználónév már foglalt.");
                return View(model);
            }

            string oldUsername = user.Username;
            user.Username = model.NewUsername.Trim();
            await _context.SaveChangesAsync();

            await _audit.LogAsync(user.Id, user.Username, "ProfileEdit",
                $"Felhasználónév megváltoztatva: '{oldUsername}' → '{user.Username}'");

            // Re-issue cookie with updated username claim
            await RefreshClaims(user);

            TempData["SuccessMessage"] = "Felhasználónév sikeresen megváltoztatva.";
            return RedirectToAction(nameof(Index));
        }

        // ─── CHANGE PASSWORD ──────────────────────────────────────────────────

        [HttpGet]
        public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _context.Userinfo.FindAsync(GetUserId());
            if (user == null) return NotFound();

            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Passwordhash))
            {
                ModelState.AddModelError("CurrentPassword", "A jelenlegi jelszó helytelen.");
                return View(model);
            }

            user.Passwordhash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            await _audit.LogAsync(user.Id, user.Username, "ProfileEdit", "Jelszó megváltoztatva.");

            TempData["SuccessMessage"] = "Jelszó sikeresen megváltoztatva.";
            return RedirectToAction(nameof(Index));
        }

        // ─── HELPERS ─────────────────────────────────────────────────────────

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private async Task RefreshClaims(Models.Entities.Userinfo user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }
    }
}
