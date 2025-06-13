using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class ComplaintItemController : Controller
    {
        public IActionResult Index()
        {
            return View("ComplaintItem");
        }
    }
}
