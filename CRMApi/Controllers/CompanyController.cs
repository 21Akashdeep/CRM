using CRMApi.Models;
using CRMApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRMApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class CompanyController : ControllerBase
    {
        private readonly DbCRM db;
        public CompanyController(DbCRM _db) {
            db = _db;
        }
        [HttpGet]
        public IActionResult GetViewOption(Company obj) 
        {
            Message objMSg = new Message();
            return Ok(objMSg);
        }
    }
}
