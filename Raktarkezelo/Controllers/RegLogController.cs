using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Raktarkezelo.Data;
using Raktarkezelo.Models.Entities;
using Raktarkezelo.Models.User;
using Raktarkezelo.Models.Enums;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Raktarkezelo.Controllers
{
    public class RegLogController : Controller
    {
        private readonly RaktarDb _context;

        public RegLogController(RaktarDb context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new Userinfo
            {
                Email = model.Email,
                Username = model.Username,
                Passwordhash = HashPassword(model.Password),

                // 🔥 ENUM STRING
                Role = UserRole.User,

                IsActive = true
            };

            _context.Userinfo.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Login() => View(new LoginViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string hashedPassword = HashPassword(model.Password);

            var user = await _context.Userinfo
                .FirstOrDefaultAsync(u =>
                    u.Email == model.Email &&
                    u.Passwordhash == hashedPassword);

            if (user == null)
            {
                ModelState.AddModelError("", "Hibás email vagy jelszó!");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "A fiók inaktív!");
                return View(model);
            }

            // 🔐 COOKIE LOGIN

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal);

            return RedirectToAction("Main", "Main");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}