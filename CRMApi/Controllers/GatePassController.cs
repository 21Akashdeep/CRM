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
    public class GatePassController : ControllerBase
    {
        private readonly DBCRM db;
        private readonly RepoGatePass RepoGatePass;
        public GatePassController(DBCRM _db)
        {
            db = _db;
            RepoGatePass = new RepoGatePass(db);
        }
        [HttpGet]
        public async Task<IActionResult> GetViewOption()
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoGatePass.GetViewOptionAsync();
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpGet]
        public async Task<IActionResult> GetAddOption()
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoGatePass.GetAddOptionAsync();
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public async Task<IActionResult> Get(GatePass obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoGatePass.GetAsync(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public async Task<IActionResult> Print(GatePass obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Print }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoGatePass.PrintAsync(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        //[HttpPost]
        //public async Task<IActionResult> Export(GatePass obj)
        //{
        //    Message objMsg = new Message();
        //    try
        //    {
        //        var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Export }, db, ref objMsg);
        //        if (User == null) return Ok(objMsg);
        //        objMsg = await RepoGatePass.ExportAsync(obj, User);
        //    }
        //    catch (Exception ex)
        //    {
        //        Message.Exception(ref objMsg, ex);
        //    }
        //    return Ok(objMsg);
        //}

        [HttpPost]
        public async Task<IActionResult> Add(GatePass obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Export }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoGatePass.AddAsync(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoGatePass.EditAsync(Id, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPatch]
        public async Task<IActionResult> Update(GatePass obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoGatePass.UpdateAsync(obj, User);
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
                objMsg = await RepoGatePass.DeleteAsync(Id, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        public IActionResult Enable(int Id)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Enable }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = RepoGatePass.Enable(Id, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }

    }

}
