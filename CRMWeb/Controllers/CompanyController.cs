using Microsoft.AspNetCore.Mvc;

namespace CRMWeb.Controllers
{
    public class CompanyController : Controller
    {
        public IActionResult Index()
        {
            return View("Company");
        }
    }
}
