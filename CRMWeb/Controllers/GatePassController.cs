using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class GatePassController : Controller
    {
        public IActionResult Index()
        {
            return View("GatePass");
        }
    }
}
