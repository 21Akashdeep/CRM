using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class AuthenticationController : Controller
    {
        public IActionResult Index()
        {
            return View("Authentication");
        }
    }
}
