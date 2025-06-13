using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class ZoneController : Controller
    {
        public IActionResult Index()
        {
            return View("Zone");
        }
    }
}
