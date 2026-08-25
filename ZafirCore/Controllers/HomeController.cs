using Microsoft.AspNetCore.Mvc;

namespace ZafirCore.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
