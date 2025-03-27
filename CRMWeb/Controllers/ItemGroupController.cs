using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class ItemGroupController : Controller
    {
        public IActionResult Index()
        {
            return View("ItemGroup");
        }
    }
}
