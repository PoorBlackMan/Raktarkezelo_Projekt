using Microsoft.AspNetCore.Mvc;
using Raktarkezelo.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Raktarkezelo.Models.Stock;
using Raktarkezelo.Models.Enums;
using Raktarkezelo.Models.User;


namespace Raktarkezelo.Controllers
{
    public class MainController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Main()
        {
            return View();
        }
        
    }
}
