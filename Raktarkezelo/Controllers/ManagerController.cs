using Microsoft.AspNetCore.Mvc;

namespace Raktarkezelo.Controllers
{
    public class ManagerController : Controller
    {
        public IActionResult Users()
        {
            return View();
        }
    }
}