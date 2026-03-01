using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Raktarkezelo.Data;
using Raktarkezelo.Models.Entities;
using Raktarkezelo.Models.User;
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

            bool emailExists = await _context.Userinfos.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Ez az email már regisztrálva van!");
                return View(model);
            }

            bool usernameExists = await _context.Userinfos.AnyAsync(u => u.Username == model.Username);
            if (usernameExists)
            {
                ModelState.AddModelError("Username", "Ez a felhasználónév már foglalt!");
                return View(model);
            }

            if (model.Username == model.Password)
            {
                ModelState.AddModelError("Password", "A felhasználónév nem lehet ugyanaz, mint a jelszó!");
                return View(model);
            }

            var user = new Userinfo
            {
                Email = model.Email,
                Username = model.Username,
                Passwordhash = HashPassword(model.Password),
                Role = "Felhasználó",
                IsActive = true
            };

            _context.Userinfos.Add(user);
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

            var user = await _context.Userinfos.FirstOrDefaultAsync(u =>
                u.Email == model.Email && u.Passwordhash == hashedPassword);

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

            return RedirectToAction("Main", "Main");
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