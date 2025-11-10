using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class ProjectDeadLineLogController : Controller
    {
        public IActionResult Index()
        {
            return View("ProjectDeadLineLog");
        }
    }
}
