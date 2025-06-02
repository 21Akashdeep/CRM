using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {
            return View("Customer");
        }
    }
}
