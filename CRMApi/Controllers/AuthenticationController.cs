using CRMApi.Models;
using CRMApi.Repository;
using CRMApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRMApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly DBCRM db;        
        private RepoAuthentication RepoAuthentication;
        public AuthenticationController(DBCRM _db)
        {
            db = _db;
            RepoAuthentication = new RepoAuthentication(db);
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AppInfo()
        {
            Message objMsg = new Message();
            try
            {
                objMsg = RepoAuthentication.AppInfo();
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(User obj)
        {
            Message objMsg = new Message();
            try
            {
                objMsg = await RepoAuthentication.Auth(obj);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Test() 
        {
            Message objMsg = new Message();            
            objMsg.obj = new {
                EncryptedCode = Util.Encrypt("SysAdmin@New"),
                DecryptedCode = Util.Decrypt(Util.Encrypt("SysAdmin@New")),
            };
            return Ok(objMsg);
        }
    }
}
