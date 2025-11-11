using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class ProjectController : Controller
    {
        public IActionResult Index()
        {
            return View("Project");
        }
    }
}
