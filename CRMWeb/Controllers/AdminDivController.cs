using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class AdminDivController : Controller
    {
        public IActionResult Index()
        {
            return View("AdminDiv");
        }
    }
}
