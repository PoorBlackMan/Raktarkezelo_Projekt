using Microsoft.AspNetCore.Mvc;
using Raktarkezelo.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Raktarkezelo.Models.User;


public class RegLogController : Controller
{
    private readonly RaktarDb _context;

    public RegLogController(RaktarDb context)
    {
        _context = context;
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(Userinfo model)
    {
        if (ModelState.IsValid)
        {
            // Ellenőrizzük, hogy létezik-e már az email
            if (await _context.User.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("", "Ez az email már regisztrálva van!");
                return View(model);
            }

            // Jelszó hash-elése
            model.Password = HashPassword(model.Password);

            _context.User.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        return View(model);
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        string hashedPassword = HashPassword(password);

        var user = await _context.User
            .FirstOrDefaultAsync(u => u.Email == email && u.Password == hashedPassword);

        if (user != null)
        {
            // Ide jöhet majd a session kezelés
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Hibás email vagy jelszó!");
        return View();
    }

    private string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}