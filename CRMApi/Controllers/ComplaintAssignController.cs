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
    public class ComplaintAssignController : ControllerBase
    {
        private readonly DbCRM db;
        private readonly RepoComplaintAssign RepoComplaintSchedule;
        public ComplaintAssignController(DbCRM _db) 
        {
            db = _db;
            RepoComplaintSchedule = new RepoComplaintAssign(db);
        }
        [HttpGet]
        public async Task<IActionResult> GetViewOption()
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoComplaintSchedule.GetViewOptionAsync();
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetComplaint()
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoComplaintSchedule.GetComplaintAsync(null, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }        

        [HttpPost]
        public async Task<IActionResult> GetUser(Complaint obj) 
        {
            Message objMsg = new Message();
            try 
            {
                objMsg = await RepoComplaintSchedule.GetUserAsync(obj);
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        
        [HttpPost]
        public async Task<IActionResult> Get(ComplaintAssign obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoComplaintSchedule.GetAsync(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public async Task<IActionResult> Print(ComplaintAssign obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Print }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoComplaintSchedule.PrintAsync(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public async Task<IActionResult> Export(ComplaintAssign obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Export }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoComplaintSchedule.ExportAsync(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public async Task<IActionResult> Add(Complaint obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Export }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoComplaintSchedule.AddAsync(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int ComplaintId)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoComplaintSchedule.EditAsync(ComplaintId, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }        
        [HttpDelete]
        public async Task<IActionResult> Delete(int Id)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);                
                objMsg = await RepoComplaintSchedule.DeleteAsync(Id, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }        
    }
}
