using Microsoft.AspNetCore.Mvc;

namespace YourNamespace.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string userName)
        {
            ViewBag.UserName = userName;
            return View("Greet");
        }
    }
}
