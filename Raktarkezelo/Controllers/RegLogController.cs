using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Raktarkezelo.Data;
using Raktarkezelo.Models.Entities;
using Raktarkezelo.Models.Enums;
using Raktarkezelo.Models.User;
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
                Passwordhash = HashPassword(model.Password),
                Role = UserRole.User,
                IsActive = true
            };

            _context.Userinfo.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Login));
        }

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

            string hashedPassword = HashPassword(model.Password);

            var user = await _context.Userinfo
                .FirstOrDefaultAsync(u =>
                    u.Email == model.Email &&
                    u.Passwordhash == hashedPassword);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Hibás email vagy jelszó!");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "A fiók inaktív!");
                return View(model);
            }

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

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal);

            return RedirectToAction("Main", "Main");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}