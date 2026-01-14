using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class StockOutController : Controller
    {
        public IActionResult Index()
        {
            return View("StockOut");
        }
    }
}
