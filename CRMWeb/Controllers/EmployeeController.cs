using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class EmployeeController : Controller
    {
        public IActionResult Index()
        {
            return View("Employee");
        }
    }
}
