using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raktarkezelo.Models.Enums;

namespace Raktarkezelo.Controllers
{
    [Authorize(Roles = nameof(UserRole.Admin) + "," + nameof(UserRole.Manager))]
    public class AdminController : Controller
    {
        public IActionResult Inbound()
        {
            return View();
        }
        public IActionResult Outbound()
        {
            return View();
        }
        public IActionResult Adjustments()
        {
            return View();
        }
        public IActionResult Reports()
        {
            return View();
        }
    }
}