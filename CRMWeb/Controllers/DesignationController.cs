using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class DesignationController : Controller
    {
        public IActionResult Index()
        {
            return View("Designation");
        }
    }
}
