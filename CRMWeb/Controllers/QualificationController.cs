using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class QualificationController : Controller
    {
        public IActionResult Index()
        {
            return View("Qualification");
        }
    }
}
