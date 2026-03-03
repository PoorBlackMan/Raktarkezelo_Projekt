using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raktarkezelo.Models.Enums;

namespace Raktarkezelo.Controllers
{
    public class ManagerController : Controller
    {
        [Authorize(Roles =nameof(UserRole.Manager))]
        public IActionResult Users()
        {
            return View();
        }
    }
}