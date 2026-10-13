using Microsoft.AspNetCore.Mvc;

namespace AspNetCore_ProductCatalog_MVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
