using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class ApiController : Controller
    {
        public IActionResult Index()
        {
            return View("Api");
        }
    }
}
