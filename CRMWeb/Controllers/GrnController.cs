using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class GrnController : Controller
    {
        public IActionResult Index()
        {
            return View("Grn");
        }
    }
}
