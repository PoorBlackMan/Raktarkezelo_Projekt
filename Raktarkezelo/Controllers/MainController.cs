using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raktarkezelo.Models.Enums;

namespace Raktarkezelo.Controllers
{
    public class MainController : Controller
    {
        [Authorize]
        public IActionResult Main()
        {
            return View();
        }

        [Authorize]
        public IActionResult Products()
        {
            return View();
        }


    }
}
