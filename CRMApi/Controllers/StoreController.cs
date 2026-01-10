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
    public class StoreController : ControllerBase
    {
        private readonly DBCRM db;
        private readonly RepoStore RepoStore;

        public StoreController(DBCRM _db)
        {
            db = _db;
            RepoStore = new RepoStore(db);
        }

        [HttpGet]
        public IActionResult GetViewOption()
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(
                    new Request { HttpRequest = Request, ActionType = ActionType.View },
                    db,
                    ref objMsg
                );
                if (User == null) return Ok(objMsg);

                objMsg = RepoStore.GetViewOption();
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
                var User = Util.RequestVerify(
                    new Request { HttpRequest = Request, ActionType = ActionType.View },
                    db,
                    ref objMsg
                );
                if (User == null) return Ok(objMsg);

                objMsg = RepoStore.GetAddOption();
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }

        [HttpPost]
        public IActionResult Get(Store obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(
                    new Request { HttpRequest = Request, ActionType = ActionType.View },
                    db,
                    ref objMsg
                );
                if (User == null) return Ok(objMsg);

                objMsg.data = RepoStore.List(obj, User);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }

        [HttpPost]
        public IActionResult Print(Store obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(
                    new Request { HttpRequest = Request, ActionType = ActionType.Print },
                    db,
                    ref objMsg
                );
                if (User == null) return Ok(objMsg);

                objMsg = RepoStore.Print(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }

        [HttpPost]
        public IActionResult Export(Store obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(
                    new Request { HttpRequest = Request, ActionType = ActionType.Export },
                    db,
                    ref objMsg
                );
                if (User == null) return Ok(objMsg);

                objMsg = RepoStore.Export(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }

        [HttpPost]
        public IActionResult Add(Store obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(
                    new Request { HttpRequest = Request, ActionType = ActionType.Add },
                    db,
                    ref objMsg
                );
                if (User == null) return Ok(objMsg);

                objMsg = RepoStore.Add(obj, User);
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
                var User = Util.RequestVerify(
                    new Request { HttpRequest = Request, ActionType = ActionType.View },
                    db,
                    ref objMsg
                );
                if (User == null) return Ok(objMsg);

                Store obj = new Store();
                obj.ListId.Add(Id);

                objMsg = RepoStore.Edit(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }

        [HttpPatch]
        public IActionResult Update(Store obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(
                    new Request { HttpRequest = Request, ActionType = ActionType.View },
                    db,
                    ref objMsg
                );
                if (User == null) return Ok(objMsg);

                objMsg = RepoStore.Update(obj, User);
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
                var User = Util.RequestVerify(
                    new Request { HttpRequest = Request, ActionType = ActionType.Delete },
                    db,
                    ref objMsg
                );
                if (User == null) return Ok(objMsg);

                Store obj = new Store();
                obj.Id = Id;

                objMsg = RepoStore.Delete(obj, User);
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
                var User = Util.RequestVerify(
                    new Request { HttpRequest = Request, ActionType = ActionType.Enable },
                    db,
                    ref objMsg
                );
                if (User == null) return Ok(objMsg);

                Store obj = new Store();
                obj.Id = Id;

                objMsg = RepoStore.Enable(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
    }
}
