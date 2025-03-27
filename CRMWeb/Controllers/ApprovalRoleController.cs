using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class ApprovalRoleController : Controller
    {
        public IActionResult Index()
        {
            return View("ApprovalRole");
        }
    }
}
