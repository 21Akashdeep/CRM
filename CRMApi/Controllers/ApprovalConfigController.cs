using CRMApi.Models;
using CRMApi.Repository;
using CRMApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ApprovalConfigController : ControllerBase
    {
        private readonly DBCRM db;
        private readonly RepoApprovalConfig RepoApprovalConfig;
        public ApprovalConfigController(DBCRM _db) 
        {
            db = _db;
            RepoApprovalConfig = new RepoApprovalConfig(db);
        }
        [HttpGet]
        public IActionResult GetViewOption() 
        {
            Message objMsg = new Message();
            try 
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = RepoApprovalConfig.GetViewOption();
            }
            catch (Exception ex) { 
                Message.Exception(ref objMsg, ex); 
            }
            return Ok(objMsg);
        }
        [HttpGet]
        public IActionResult GetAddOption()
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = RepoApprovalConfig.GetAddOption();
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public IActionResult Get(ApprovalConfig obj) 
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = RepoApprovalConfig.Get(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public IActionResult Print(ApprovalConfig obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = RepoApprovalConfig.Print(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public IActionResult Export(ApprovalConfig obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = RepoApprovalConfig.Export(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public IActionResult Add(ApprovalConfig obj) 
        {
            Message objMsg = new Message();
            
            try 
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = RepoApprovalConfig.Add(obj, User);
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpDelete]
        public IActionResult Delete(int Id) 
        {
            Message objMsg = new Message();
            try 
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Delete }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = RepoApprovalConfig.Delete(new ApprovalConfig { Id = Id }, User);
            } 
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPatch]
        public IActionResult Enable(int Id)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Delete }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = RepoApprovalConfig.Enable(new ApprovalConfig { Id = Id }, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
    }
}
