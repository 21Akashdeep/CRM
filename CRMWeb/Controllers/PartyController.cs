using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class PartyController : Controller
    {
        public IActionResult Index()
        {
            return View("Party");
        }
    }
}
