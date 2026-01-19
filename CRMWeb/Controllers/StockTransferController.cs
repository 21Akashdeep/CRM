using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class StockTransferController : Controller
    {
        public IActionResult Index()
        {
            return View("StockTransfer");
        }
    }
}
