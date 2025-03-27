using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class SettingController : Controller
    {
        public IActionResult Index()
        {
            return View("Setting");
        }
    }
}
