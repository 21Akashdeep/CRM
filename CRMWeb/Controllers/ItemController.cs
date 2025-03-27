using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class ItemController : Controller
    {
        public IActionResult Index()
        {
            return View("Item");
        }
    }
}
