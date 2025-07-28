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
    [Authorize]
    public class ApprovalController : ControllerBase
    {
        private readonly DBCRM db;        
        private RepoApproval RepoApproval;
        public ApprovalController(DBCRM _db) 
        {
            db = _db;            
            RepoApproval = new RepoApproval(db);
        }
        [HttpGet]
        public IActionResult GetViewOption()
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = RepoApproval.GetViewOption();
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public IActionResult Get(Approval obj) 
        {
            Message objMsg = new Message();
            try 
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);                
                obj.ListUserId.Add(User.Id);
                objMsg.data = RepoApproval.List(obj, User);
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public IActionResult GetReqInfo(Approval obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = RepoApproval.GetReqInfo(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public IActionResult Approve(Approval obj) 
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Update }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = RepoApproval.Approve(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public IActionResult Reject(Approval obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = RepoApproval.Reject(obj, User);
                //objMsg = RepoOutPass.GetViewOption();
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }

    }
}
