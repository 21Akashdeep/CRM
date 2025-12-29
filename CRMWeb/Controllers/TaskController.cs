using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class TaskController : Controller
    {
        public IActionResult Index()
        {
            return View("Task");
        }
    }
}
