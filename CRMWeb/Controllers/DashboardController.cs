using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View("Dashboard");
        }
    }
}
