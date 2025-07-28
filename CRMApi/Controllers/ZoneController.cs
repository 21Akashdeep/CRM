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
    public class ZoneController : ControllerBase
    {
        private readonly DBCRM db;
        private readonly RepoZone RepoZone;
        public ZoneController(DBCRM _db) 
        {
            db = _db;
            RepoZone = new RepoZone(db);
        }
        [HttpGet]
        public IActionResult GetViewOption()
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = RepoZone.GetViewOption();
            }
            catch (Exception ex)
            {
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
                objMsg = RepoZone.GetAddOption();
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public IActionResult Get(Zone obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg.data = RepoZone.List(obj, User);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public IActionResult Print(Zone obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Print }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = RepoZone.Print(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public IActionResult Export(Zone obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Export }, db, ref objMsg);
                if (User == null) return Ok(objMsg);                
                objMsg = RepoZone.Export(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public IActionResult Add(Zone obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Export }, db, ref objMsg);
                if (User == null) return Ok(objMsg);                
                objMsg = RepoZone.Add(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpGet]
        public IActionResult Edit(int Id)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                Zone obj = new Zone();
                obj.ListId.Add(Id);                
                objMsg = RepoZone.Edit(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPatch]
        public IActionResult Update(Zone obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);                
                objMsg = RepoZone.Update(obj, User);
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
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                Zone obj = new Zone();
                obj.Id = Id;                
                objMsg = RepoZone.Delete(obj, User);
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
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                Zone obj = new Zone();
                obj.Id = Id;                
                objMsg = RepoZone.Enable(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
    }
}
