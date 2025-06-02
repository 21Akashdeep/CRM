using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class Country : Controller
    {
        public IActionResult Index()
        {
            return View("Country");
        }
    }
}
