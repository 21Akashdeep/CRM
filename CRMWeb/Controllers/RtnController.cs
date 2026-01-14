using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class RtnController : Controller
    {
        public IActionResult Index()
        {
            return View("Rtn");
        }
    }
}
