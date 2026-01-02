using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class TaskActionController : Controller
    {
        public IActionResult Index()
        {
            return View("TaskAction");
        }
    }
}
