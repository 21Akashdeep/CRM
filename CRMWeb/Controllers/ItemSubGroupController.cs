using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class ItemSubGroupController : Controller
    {
        public IActionResult Index()
        {
            return View("ItemSubGroup");
        }
    }
}
