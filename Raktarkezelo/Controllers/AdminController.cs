using Microsoft.AspNetCore.Mvc;

namespace Raktarkezelo.Controllers
{
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