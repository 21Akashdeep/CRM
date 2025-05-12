using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class ComplaintController : Controller
    {
        public IActionResult Index()
        {
            return View("Complaint");
        }
    }
}
