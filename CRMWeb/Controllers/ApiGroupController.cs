using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class ApiGroupController : Controller
    {
        public IActionResult Index()
        {
            return View("ApiGroup");
        }
    }
}
