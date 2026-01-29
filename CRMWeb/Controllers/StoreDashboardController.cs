using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class StoreDashboardController : Controller
    {
        public IActionResult Index()
        {
            return View("StoreDashboard");
        }
    }
}
