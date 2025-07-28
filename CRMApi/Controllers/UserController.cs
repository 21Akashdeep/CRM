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
        private readonly DBCRM db;
        private RepoUser RepoUser;
        public UserController(DBCRM _db) 
        {
            db = _db;
            RepoUser = new RepoUser(db);
        }
        [HttpGet]
        public async Task<IActionResult> GetViewOption() 
        {
            Message objMsg = new Message();
            try 
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoUser.GetViewOption();
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpGet]
        public async Task<IActionResult> GetAddOption(int Id)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoUser.GetAddOption(Id);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]        
        public async Task<IActionResult> Get(User obj) 
        {
            Message objMsg = new Message();
            try 
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg.data = await RepoUser.ListAsync(obj, User);
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public async Task<IActionResult> Print(User obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoUser.Print(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public async Task<IActionResult> Export(User obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoUser.Export(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }        
        [HttpPost]
        public async Task<IActionResult> Add(User obj) 
        {
            Message objMsg = new Message();
            try 
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Add }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoUser.Add(obj, User);
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
        public async Task<IActionResult> Edit(int Id) 
        {
            Message objMsg = new Message();
            try 
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);                
                User obj = new User();
                obj.ListId.Add(Id);
                var dbUser = (await RepoUser.ListAsync(obj, User)).FirstOrDefault();
                if (dbUser == null) 
                {
                    Message.Error(ref objMsg, "User did not find for edit.");
                    return Ok(objMsg);
                }
                objMsg.obj = dbUser;
                objMsg.data = (await RepoUser.GetAddOption(Id)).data;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPatch]
        public async Task<IActionResult> Update(User obj) 
        {
            Message objMsg = new Message();
            try 
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Update }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoUser.Update(obj, User);
            }
            catch (Exception ex) { 
                Message.Exception(ref objMsg, ex); 
            }
            return Ok(objMsg);
        }
        [HttpPatch]
        public async Task<IActionResult> UpdateProfile(User obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Update }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                obj.Id = User.Id;
                objMsg = await RepoUser.UpdateProfile(obj);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPatch]
        public async Task<IActionResult> UpdatePassword(User obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Update }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                obj.Id = User.Id;
                objMsg = await RepoUser.UpdatePassword(obj);
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
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Delete }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                User obj = new User();
                obj.Id = Id;
                objMsg = await RepoUser.Delete(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPatch]
        public async Task<IActionResult> Enable(int Id)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Enable }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                User obj = new User();
                obj.Id = Id;
                objMsg = await RepoUser.Enable(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
    }
}
