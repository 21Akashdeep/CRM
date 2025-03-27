using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class ApprovalController : Controller
    {
        public IActionResult Index()
        {
            return View("Approval");
        }
    }
}
