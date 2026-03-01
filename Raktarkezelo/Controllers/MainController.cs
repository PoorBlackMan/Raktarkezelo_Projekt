using Microsoft.AspNetCore.Mvc;

namespace Raktarkezelo.Controllers
{
    public class MainController : Controller
    {
        public IActionResult Main()
        {
            return View();
        }

        public IActionResult Products()
        {
            return View();
        }
    }
}