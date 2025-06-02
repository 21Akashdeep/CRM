using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class Department : Controller
    {
        public IActionResult Index()
        {
            return View("Department");
        }
    }
}
