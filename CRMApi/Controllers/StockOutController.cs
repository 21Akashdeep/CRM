using CRMApi.Dto;
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
    public class StockOutController : Controller
    {
        private readonly DBCRM db;
        private readonly RepoStockOut RepoStockOut;
        private readonly VoucherRepo RepoVoucher;
        public StockOutController(DBCRM _db)
        {
            db = _db;
            RepoStockOut = new RepoStockOut(db);
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
                objMsg = await RepoStockOut.GetViewOptionAsync();
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
                objMsg = await RepoStockOut.GetAddOptionAsync();
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
                objMsg.data = await RepoStockOut.ListAsync(obj, User);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }

        [HttpPost]
        public async Task<IActionResult> GetStockItem([FromBody] StockItemFltrDto obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(
                    new Request { HttpRequest = Request, ActionType = ActionType.Export },
                    db, ref objMsg);

                if (User == null) return Ok(objMsg);


                objMsg = await RepoStockOut.GetStockItemAsync(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public async Task<IActionResult> GetStockItemWithSerialNo([FromBody] StockItemFltrDto obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(
                    new Request
                    {
                        HttpRequest = Request,
                        ActionType = ActionType.Export
                    },
                    db, ref objMsg);

                if (User == null) return Ok(objMsg);


                objMsg = await RepoStockOut.GetStockItemWithSerialNoAsync(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Voucher obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Export }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = await RepoStockOut.AddAsync(obj, User);
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
                objMsg = await RepoStockOut.EditAsync(Id, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPatch]
        public async Task<IActionResult> Update([FromBody] Voucher obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
               
                objMsg = await RepoStockOut.UpdateAsync(obj, User);
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
                objMsg = await RepoVoucher.DeleteAsync(Id, User);
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
                objMsg = await RepoStockOut.PrintAsync(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }

        //[HttpPost]
        //public async Task<IActionResult> Export(Voucher obj)
        //{
        //    Message objMsg = new Message();
        //    try
        //    {
        //        var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Export }, db, ref objMsg);
        //        if (User == null) return Ok(objMsg);
        //        // objMsg = await RepoStockOut.ExportAsync(obj, User);
        //    }
        //    catch (Exception ex)
        //    {
        //        Message.Exception(ref objMsg, ex);
        //    }
        //    return Ok(objMsg);
        //}
        [HttpPost]
        public async Task<IActionResult> Export(Voucher obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(
                    new Request { HttpRequest = Request, ActionType = ActionType.Export },
                    db, ref objMsg);

                if (User == null) return Ok(objMsg);

                objMsg = await RepoStockOut.ExportAsync(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
    }
}
