using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class GdnController : Controller
    {
        public IActionResult Index()
        {
            return View("Gdn");
        }
    }
}
