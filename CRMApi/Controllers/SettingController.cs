using CRMApi.Models;
using CRMApi.Repository;
using CRMApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace CRMApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class SettingController : ControllerBase
    {
        private readonly DbCRM db;
        private readonly AppSetting App = Util.AppSetting;
        private readonly RepoSetting SettingRepo;
        public SettingController(DbCRM _db)
        {
            db = _db;
            SettingRepo = new RepoSetting(db);
        }
        [HttpGet]
        public IActionResult GetViewOption() 
        {
            Message objMsg = new Message();
            try 
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                objMsg = SettingRepo.GetViewOption();
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
                objMsg = SettingRepo.GetAddOption();
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public IActionResult Get(Setting obj) 
        {
            Message objMsg = new Message();
            try 
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                obj.User = User;
                objMsg.data = SettingRepo.List(obj);
                Message.Get(ref objMsg, "");                
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public IActionResult Print(Setting obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Print }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                obj.User = User;
                objMsg.data = SettingRepo.List(obj);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public IActionResult Export(Setting obj)
        {
            Message objMsg = new Message();
            try
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Export }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                obj.User = User;
                //Get Party
                var objCompany = db.Company.FirstOrDefault(pt => App.ActiveStatus.Contains(pt.Status) && pt.Id == User.CompanyId);
                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Company Info did found");
                    return Ok(objMsg);
                }
                //Get Api Group
                var Setting = SettingRepo.List(obj);
                //Convert List To DataTable
                DataTable objDataTable = Util.ListToDataTable(Setting);
                //Remove Un-wanted column                
                objDataTable.Columns.Remove("IsEdit");
                objDataTable.Columns.Remove("IsDelete");
                objDataTable.Columns.Remove("IsEnable");
                objDataTable.Columns.Remove("IsDuplicate");
                objDataTable.Columns.Remove("ListId");
                objDataTable.Columns.Remove("ListName");
                objDataTable.Columns.Remove("ListGroup");
                objDataTable.Columns.Remove("ListStatus");
                //Convert Datatable to base64
                objCompany.SheetName = "Setting List";
                objCompany.ReportDesc = $"Setting - {DateTime.Now.ToString("dd-MMM-yyyy")}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPost]
        public IActionResult Add(Setting obj) 
        {
            Message objMsg = new Message();
            try 
            {
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Add }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                obj.User = User;
                objMsg = SettingRepo.Add(obj);
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
                Setting obj = new Setting();
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.View }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                obj.User = User;
                obj.ListId.Add(Id);
                var Setting = SettingRepo.List(obj).FirstOrDefault();
                if (Setting == null)
                {
                    Message.Error(ref objMsg, "");
                    return Ok(objMsg);
                }
                objMsg.obj = Setting;
                objMsg.data = SettingRepo.GetAddOption().data;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
        [HttpPatch]
        public IActionResult Update(Setting obj) 
        {
            Message objMsg = new Message();
            try
            {                
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Update }, db, ref objMsg);
                if (User == null) return Ok(objMsg);                
                obj.User = User;
                objMsg = SettingRepo.Update(obj);
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
                Setting obj = new Setting();
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Update }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                obj.Id = Id;
                obj.User = User;
                objMsg = SettingRepo.Delete(obj);
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
                Setting obj = new Setting();
                var User = Util.RequestVerify(new Request { HttpRequest = Request, ActionType = ActionType.Update }, db, ref objMsg);
                if (User == null) return Ok(objMsg);
                obj.Id = Id;
                obj.User = User;
                objMsg = SettingRepo.Enable(obj);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return Ok(objMsg);
        }
    }
}
