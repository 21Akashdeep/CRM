using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class UnitController : Controller
    {
        public IActionResult Index()
        {
            return View("Unit");
        }
    }
}
