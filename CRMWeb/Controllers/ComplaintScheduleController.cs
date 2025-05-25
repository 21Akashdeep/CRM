using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class ComplaintScheduleController : Controller
    {
        public IActionResult Index()
        {
            return View("ComplaintSchedule");
        }
    }
}
