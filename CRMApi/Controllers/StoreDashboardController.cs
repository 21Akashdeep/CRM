using Microsoft.AspNetCore.Mvc;

namespace CRMApi.Controllers
{
    public class StoreDashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
