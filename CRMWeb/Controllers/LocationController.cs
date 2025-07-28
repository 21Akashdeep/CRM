using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class LocationController : Controller
    {
        public IActionResult Index()
        {
            return View("Location");
        }
    }
}
