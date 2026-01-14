using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class StockInController : Controller
    {
        public IActionResult Index()
        {
            return View("StockIn");
        }
    }
}
