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
    public class UserController : ControllerBase
    {
        private readonly DbCRM db;
        private RepoUser RepoUser;
        public UserController(DbCRM _db) 
        {
            db = _db;
            RepoUser = new RepoUser(db);
        }
        [HttpGet]
        public IActionResult GetViewOption() 
        {
            Message objMsg = new Message();
            try 
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = RepoUser.GetViewOption();
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpGet]
        public IActionResult GetAddOption(int Id)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = RepoUser.GetAddOption(Id);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]        
        public IActionResult Get(User obj) 
        {
            Message objMsg = new Message();
            try 
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg.data = RepoUser.List(obj, User);
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public IActionResult Print(User obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = RepoUser.Print(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public IActionResult Export(User obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = RepoUser.Export(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }        
        [HttpPost]
        public IActionResult Add(User obj) 
        {
            Message objMsg = new Message();
            try 
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Add }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = RepoUser.Add(obj, User);
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpGet]
        public IActionResult ReSentPassword(int Id)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Update }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                RepoUser.SentPassword(Id, ref objMsg);
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
                //for (long i = 0; i <= 999999999999999999; i++) { }
                User obj = new User();
                obj.ListId.Add(Id);
                var dbUser = RepoUser.List(obj, User).FirstOrDefault();
                if (dbUser == null) 
                {
                    Message.Error(ref objMsg, "User did not find for edit.");
                    return Ok(objMsg);
                }
                objMsg.obj = dbUser;
                objMsg.data = RepoUser.GetAddOption(Id).data;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPatch]
        public IActionResult Update(User obj) 
        {
            Message objMsg = new Message();
            try 
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Update }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = RepoUser.Update(obj, User);
            }
            catch (Exception ex) { 
                Message.Exception(ref objMsg, ex); 
            }
            return Ok(objMsg);
        }
        [HttpPatch]
        public IActionResult UpdateProfile(User obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Update }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                obj.Id = User.Id;
                objMsg = RepoUser.UpdateProfile(obj);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPatch]
        public IActionResult UpdatePassword(User obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Update }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                obj.Id = User.Id;
                objMsg = RepoUser.UpdatePassword(obj);
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
                User obj = new User();
                obj.Id = Id;
                objMsg = RepoUser.Delete(obj, User);
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
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Enable }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                User obj = new User();
                obj.Id = Id;
                objMsg = RepoUser.Enable(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
    }
}
