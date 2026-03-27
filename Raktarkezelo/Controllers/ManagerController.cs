using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Raktarkezelo.Data;
using Raktarkezelo.Models.Enums;

namespace Raktarkezelo.Controllers
{
    [Authorize(Roles = nameof(UserRole.Manager) + "," + nameof(UserRole.Admin))]
    public class ManagerController : Controller
    {
        private readonly RaktarDb _context;

        public ManagerController(RaktarDb context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _context.Userinfo
                .OrderBy(u => u.Username)
                .ToListAsync();

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Userinfo.FindAsync(id);
            if (user == null)
                return NotFound();

            if (user.Role == UserRole.Admin)
                return RedirectToAction(nameof(Users));

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Users));
        }
    }
}