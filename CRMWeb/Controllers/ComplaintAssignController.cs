using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class ComplaintAssignController : Controller
    {
        public IActionResult Index()
        {
            return View("ComplaintAssign");
        }
    }
}
