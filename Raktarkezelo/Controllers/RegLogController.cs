using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Raktarkezelo.Data;
using Raktarkezelo.Models.Entities;
using Raktarkezelo.Models.Enums;
using Raktarkezelo.Models.Viewmodel;
using System.Security.Claims;
using Raktarkezelo.Services;
using System.Text;

namespace Raktarkezelo.Controllers
{
    public class RegLogController : Controller
    {
        private readonly RaktarDb _context;
        private readonly IDistributedCache _cache;
        private readonly AuditService _audit;

        // Lock out after 5 failed attempts for 10 minutes
        private const int MaxFailedAttempts = 5;
        private const int LockoutMinutes = 10;

        public RegLogController(RaktarDb context, IDistributedCache cache, AuditService audit)
        {
            _context = context;
            _cache = cache;
            _audit = audit;
        }

        // ─── REGISTER ─────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "A két jelszó nem egyezik!");
                return View(model);
            }

            bool emailExists = await _context.Userinfo.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Ez az email cím már használatban van!");
                return View(model);
            }

            bool usernameExists = await _context.Userinfo.AnyAsync(u => u.Username == model.Username);
            if (usernameExists)
            {
                ModelState.AddModelError("Username", "Ez a felhasználónév már foglalt!");
                return View(model);
            }

            if (model.Username.Trim().ToLower() == model.Password.Trim().ToLower())
            {
                ModelState.AddModelError("Password", "A jelszó nem lehet ugyanaz, mint a felhasználónév!");
                return View(model);
            }

            var user = new Userinfo
            {
                Email = model.Email.Trim(),
                Username = model.Username.Trim(),
                Passwordhash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = UserRole.User,
                IsActive = true
            };

            _context.Userinfo.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Login));
        }

        // ─── LOGIN ────────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // ── Lockout check ──────────────────────────────────────────────────
            string lockoutKey = $"lockout:{model.Email.ToLower()}";
            string attemptsKey = $"attempts:{model.Email.ToLower()}";

            var lockoutBytes = await _cache.GetAsync(lockoutKey);
            if (lockoutBytes != null)
            {
                ModelState.AddModelError(string.Empty,
                    $"Túl sok sikertelen bejelentkezési kísérlet. Próbáld újra {LockoutMinutes} perc múlva.");
                return View(model);
            }

            // ── Find user and verify password ──────────────────────────────────
            var user = await _context.Userinfo.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Passwordhash))
            {
                // Track failed attempts
                var attemptsBytes = await _cache.GetAsync(attemptsKey);
                int attempts = attemptsBytes != null ? int.Parse(Encoding.UTF8.GetString(attemptsBytes)) : 0;
                attempts++;

                if (attempts >= MaxFailedAttempts)
                {
                    await _cache.SetAsync(lockoutKey, Encoding.UTF8.GetBytes("locked"),
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(LockoutMinutes)
                        });
                    await _cache.RemoveAsync(attemptsKey);

                    ModelState.AddModelError(string.Empty,
                        $"Túl sok sikertelen kísérlet. A fiók {LockoutMinutes} percre zárolva.");
                }
                else
                {
                    await _cache.SetAsync(attemptsKey, Encoding.UTF8.GetBytes(attempts.ToString()),
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(LockoutMinutes)
                        });

                    ModelState.AddModelError(string.Empty,
                        $"Hibás email vagy jelszó! ({attempts}/{MaxFailedAttempts} kísérlet)");
                }

                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Ez a fiók inaktív. Kérd az adminisztrátor segítségét.");
                return View(model);
            }

            // ── Successful login — clear attempt counter ────────────────────────
            await _cache.RemoveAsync(attemptsKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            await _audit.LogAsync(user.Id, user.Username, "Login", $"Sikeres bejelentkezés: {user.Email}");

            return RedirectToAction("Main", "Main");
        }

        // ─── LOGOUT ───────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            var username = User.Identity?.Name ?? "?";
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(uid, out int logoutId))
                await _audit.LogAsync(logoutId, username, "Logout", null);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
