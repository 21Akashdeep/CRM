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
    public class QualificationController : ControllerBase
    {
        private readonly DBCRM db;
        private readonly RepoQualification RepoQualification;
        public QualificationController(DBCRM _db)
        {
            db = _db;
            RepoQualification = new RepoQualification(db);
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

                objMsg = RepoQualification.GetViewOption();
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

                objMsg = RepoQualification.GetAddOption();
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }

        [HttpPost]
        public IActionResult Get(Qualification obj)
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

                objMsg.data = RepoQualification.List(obj, User);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }

        [HttpPost]
        public IActionResult Print(Qualification obj)
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

                objMsg = RepoQualification.Print(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }

        [HttpPost]
        public IActionResult Export(Qualification obj)
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

                objMsg = RepoQualification.Export(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }

        [HttpPost]
        public IActionResult Add(Qualification obj)
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

                objMsg = RepoQualification.Add(obj, User);
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

                Qualification obj = new Qualification();
                obj.ListId.Add(Id);

                objMsg = RepoQualification.Edit(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }

        [HttpPatch]
        public IActionResult Update(Qualification obj)
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

                objMsg = RepoQualification.Update(obj, User);
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

                Qualification obj = new Qualification();
                obj.Id = Id;

                objMsg = RepoQualification.Delete(obj, User);
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

                Qualification obj = new Qualification();
                obj.Id = Id;

                objMsg = RepoQualification.Enable(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
    }
}
