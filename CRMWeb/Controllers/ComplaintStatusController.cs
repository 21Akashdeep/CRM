using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class ComplaintStatusController : Controller
    {
        public IActionResult Index()
        {
            return View("ComplaintStatus");
        }
    }
}
