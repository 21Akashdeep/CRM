using CRMApi.Models;
using CRMApi.Repository;
using CRMApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CRMApi.Dto.DtoTask;

namespace CRMApi.Controllers
{
    
   
            [Route("api/[controller]/[action]")]
            [ApiController]
            [Authorize]
            public class StoreDashboardController : Controller
            {
            private readonly DBCRM db;
            private readonly RepoStoreDashboard RepoStoreDashboard;
            private readonly VoucherRepo RepoVoucher;
            public StoreDashboardController(DBCRM _db)
            {
                db = _db;
                RepoStoreDashboard = new RepoStoreDashboard(db);
                RepoVoucher = new VoucherRepo(db);
            }
            [HttpGet]
            public async Task<IActionResult> GetViewOption()
            {
                Message objMsg = new Message();
                try
                {
                    var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                    if (User == null) return Ok(objMsg);
                    objMsg = await RepoStoreDashboard.GetViewOptionAsync();
            }
                catch (Exception ex)
                {
                    Message.Exception(ref objMsg, ex);
                }
                return Ok(objMsg);
            }
        [HttpPost]
        public async Task<IActionResult> Get(Voucher obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoStoreDashboard.ListAsync(obj, User);
                
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public async Task<IActionResult> GetDataByItemGroup(Voucher obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoStoreDashboard.GetDataByGroup(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public async Task<IActionResult> GetDataByItemGroupItemSubGroup(Voucher obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoStoreDashboard.GetDataByGroupAndSubGroup(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public async Task<IActionResult> GetDataByItemId(Voucher obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoStoreDashboard.GetDataByItemId(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public async Task<IActionResult> Print(Voucher obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Print }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoStoreDashboard.PrintAsync(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public async Task<IActionResult> Export(Voucher obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Export }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                // objMsg = await RepoTask.Export(obj, User);
                objMsg = await RepoStoreDashboard.Export(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }

        //[HttpGet]
        //public async Task<IActionResult> GetItemHistory(int itemId, int storeId)
        //{
        //    Message objMsg = new Message();

        //    try
        //    {
        //        var User = Util.RequestVerify(
        //            new Request { HttpRequest = Request, ActionType = ActionType.View },
        //            db,
        //            ref objMsg
        //        );

        //        if (User == null) return Ok(objMsg);

        //        objMsg = await RepoStoreDashboard.GetItemFullHistoryAsync(itemId, storeId);
        //    }
        //    catch (Exception ex)
        //    {
        //        Message.Exception(ref objMsg, ex);
        //    }

        //    return Ok(objMsg);
        //}

        [HttpGet]
        public async Task<IActionResult> GetHistoryViewOption()
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(
                    new Request { HttpRequest = Request, ActionType = ActionType.View },
                    db, ref objMsg);

                if (User == null) return Ok(objMsg);

                objMsg = await RepoStoreDashboard.GetHistoryViewOptionAsync();
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }

        [HttpGet]
        public async Task<IActionResult> GetItemHistory(int itemId, int storeId)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(
                    new Request { HttpRequest = Request, ActionType = ActionType.View },
                    db, ref objMsg);

                if (User == null) return Ok(objMsg);

                objMsg = await RepoStoreDashboard.GetItemFullHistoryAsync(itemId, storeId);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }

        [HttpGet]
        public async Task<IActionResult> PrintItemHistory(int itemId, int storeId)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(
                    new Request { HttpRequest = Request, ActionType = ActionType.Print },
                    db, ref objMsg);

                if (User == null) return Ok(objMsg);

                objMsg = await RepoStoreDashboard.PrintItemHistoryAsync(itemId, storeId);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }

        [HttpGet]
        public async Task<IActionResult> ExportItemHistory(int itemId, int storeId)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(
                    new Request { HttpRequest = Request, ActionType = ActionType.Export },
                    db, ref objMsg);

                if (User == null) return Ok(objMsg);

                objMsg = await RepoStoreDashboard.ExportItemHistoryAsync(itemId, storeId);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
    }
}
