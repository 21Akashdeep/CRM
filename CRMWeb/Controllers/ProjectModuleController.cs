using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class ProjectModuleController : Controller
    {
        public IActionResult Index()
        {
            return View("ProjectModule");
        }
    }
}
