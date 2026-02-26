using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Raktarkezelo.Data;
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

        // ===== REGISTER =====

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

            
            // Email már létezik?
            bool emailExists = await _context.User.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Ez az email már regisztrálva van!");
                return View(model);
            }

            // (Opcionális) Username már létezik?
            bool usernameExists = await _context.User.AnyAsync(u => u.Username == model.Username);
            if (usernameExists)
            {
                ModelState.AddModelError("Username", "Ez a felhasználónév már foglalt!");
                return View(model);
            }
            if (model.Username == model.Password) 
            {
                ModelState.AddModelError("Password", "A felhasználónév nem lehet ugyan az, mint a jelszó");
                return View(model);
            }

            var user = new Userinfo
            {
                Email = model.Email,
                Username = model.Username,
                Password = HashPassword(model.Password)
            };

            _context.User.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Login));
        }

        // ===== LOGIN =====

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

            var user = await _context.User.FirstOrDefaultAsync(u =>
                u.Email == model.Email && u.Password == hashedPassword);

            if (user == null)
            {
                // Globális hiba (nem mezőhöz kötött)
                ModelState.AddModelError("", "Hibás email vagy jelszó!");
                return View(model);
            }

            // Itt majd később: Cookie Auth / Session
            return RedirectToAction("Main", "Main");
        }

        // ===== HASH =====

        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}