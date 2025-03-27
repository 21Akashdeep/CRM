using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View("User");
        }
    }
}
