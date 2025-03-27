using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using CRMApi.Models;
using CRMApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoApi
    {
        private readonly DbCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoApi(DbCRM _db) 
        {
            db = _db;
        }
        public Message GetViewOption() 
        {
            Message objMsg = new Message();
            try 
            {
                dynamic Option = new ExpandoObject();
                Option.Status = (
                    from ac in db.Api
                    join st in db.Setting on new { Value = ac.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                    group st by new { st.Value, st.Description } into st
                    select new
                    {
                        st.Key.Value,
                        st.Key.Description
                    }
                ).ToList();
                var dbApiGroup = db.ApiGroup.Where(ag => App.ActiveStatus.Contains(ag.Status)).ToList();
                Option.ApiGroup = dbApiGroup.Select(ag => new
                    {
                        ag.Id,
                        ag.Code,
                        ag.Name,
                        ag.Description,
                        SubText = $"{ag.Code}, {dbApiGroup.Where(ag1=> ag1.Id == ag.ParentId).Select(ag1=> ag1.Description).FirstOrDefault() ?? "NA"} / {ag.Description}"
                    }).ToList();
                Option.Api = db.Api.Where(ap => App.ActiveStatus.Contains(ap.Status))
                    .Select(ap => new
                    {
                        ap.Id,
                        ap.Code,
                        ap.Name,
                        ap.Description
                    }).ToList();
                objMsg.data = Option;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message GetAddOption()
        {
            Message objMsg = new Message();
            try
            {
                dynamic Options = new ExpandoObject();
                var dbApiGroup = db.ApiGroup.Where(ag => App.ActiveStatus.Contains(ag.Status)).ToList();
                Options.ApiGroup = dbApiGroup.Select(ag => new
                    {
                        ag.Id,
                        ag.Code,
                        ag.Name,
                        ag.Description,
                        SubText = $"{ag.Code}, {dbApiGroup.Where(ag1 => ag1.Id == ag.ParentId).Select(ag1 => ag1.Description).FirstOrDefault() ?? "NA"} / {ag.Description}"
                }).ToList();
                Options.ApiType = db.Setting.Where(st => App.ActiveStatus.Contains(st.Status) && App.SettingName.ApiType == st.Name)
                    .Select(st => new
                    {
                        st.Value,
                        st.Description
                    }).ToList();
                objMsg.data = Options;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public List<Api> List(Api? obj) 
        {
            Message objMsg = new Message();
            List<Api> Api = new List<Api>();
            try 
            {
                obj = obj == null ? new Api() : obj;
                obj.ListStatus = obj.ListStatus.Count == 0 ? App.ActiveStatus : obj.ListStatus;
                //Get Api List
                var dbApi = db.Api.Where(ap => obj.ListStatus.Contains(ap.Status)).ToList();
                dbApi = obj.ListId.Count == 0 ? dbApi : dbApi.Where(ap => obj.ListId.Contains(ap.Id)).ToList();
                dbApi = obj.ListApiGroupId.Count == 0 ? dbApi : dbApi.Where(ap => obj.ListApiGroupId.Contains(ap.ApiGroupId)).ToList();
                //Get Api Group List
                var dbApiGroup = db.ApiGroup.Where(ag => App.ActiveStatus.Contains(ag.Status)).ToList();
                //Get Setting List
                var dbSetting = db.Setting.Where(st => App.ActiveStatus.Contains(st.Status)).ToList();
                //Get User List
                var dbUser = db.User.Where(ur => App.ActiveStatus.Contains(ur.Status)).ToList();
                //Api List
                Api = (
                    from ap in dbApi                    
                    join at in dbSetting on new { Value = ap.ApiType.ToString(), Name = App.SettingName.ApiType } equals new { at.Value, at.Name }
                    join ag in dbApiGroup on ap.ApiGroupId equals ag.Id
                    join st in dbSetting on new { Value = ap.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                    join cb in dbUser on ap.CreatedBy equals cb.Id
                    join ub in dbUser on ap.UpdatedBy equals ub.Id
                    select new Api 
                    {
                        Id = ap.Id,
                        Code = ap.Code,
                        Name = ap.Name,
                        Description = ap.Description,
                        ApiType = ap.ApiType,
                        ApiTypeName = at.Description,
                        ApiGroupId = ap.ApiGroupId,
                        ApiGroupDesc = ag.Description,
                        IsApprovalRequired = ap.IsApprovalRequired,
                        SeqNo = ap.SeqNo,
                        Icon = ap.Icon,
                        Status = ap.Status,
                        StatusName = st.Description,
                        StatusCss = st.CssClass ?? "",
                        CreatedBy = ap.CreatedBy,
                        CreatedByName = cb.Name,
                        CreatedAt = ap.CreatedAt,
                        UpdatedBy = ap.UpdatedBy,
                        UpdatedByName = ub.Name,
                        UpdatedAt = ap.UpdatedAt,
                        IsEdit = ap.Status == App.Status.Enable ? true : false,
                        IsDuplicate = true,
                        IsDelete = ap.Status == App.Status.Enable ? true : false,
                        IsEnable = ap.Status == App.Status.Delete ? true : false,                        
                    }
                ).ToList();
            }
            catch (Exception ex)             
            {
                Message.Exception(ref objMsg, ex);
            }
            return Api;
        }
        public Message Print(Api obj) 
        {
            Message objMsg = new Message();
            try 
            {
                objMsg.data = List(obj);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Export(Api obj)
        {
            Message objMsg = new Message();
            try 
            {
                //Get Party
                var objCompany = db.Company.Where(pt => App.ActiveStatus.Contains(pt.Status)).AsEnumerable().FirstOrDefault();
                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Company Info did found");
                    return objMsg;
                }
                //Get Api Group
                var Api = List(obj);
                //Convert List To DataTable
                DataTable objDataTable = Util.ListToDataTable(Api);
                //Remove Un-wanted column                
                objDataTable.Columns.Remove("IsEdit");
                objDataTable.Columns.Remove("IsDelete");
                objDataTable.Columns.Remove("IsEnable");
                objDataTable.Columns.Remove("IsDuplicate");
                objDataTable.Columns.Remove("ListId");
                objDataTable.Columns.Remove("ListApiGroupId");                
                objDataTable.Columns.Remove("ListStatus");
                //Convert Datatable to base64               
                objCompany.SheetName = "Api List";
                objCompany.ReportDesc = $"Api - {DateTime.Now.ToString("dd-MMM-yyyy")}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Add(Api obj) 
        {
            Message objMsg = new Message();
            try 
            {
                //Sanitize Input
                obj.Name = Util.SanitizeInput(obj.Name, App.Regexp.AlphaNum);
                var dbApi = db.Api.Where(fm => App.ActiveStatus.Contains(fm.Status)).ToList();
                if (dbApi.Where(ap => ap.ApiGroupId == obj.ApiGroupId && ap.Name == obj.Name).Count() > 0)
                {
                    Message.Duplicate(ref objMsg, $"Api Name : {obj.Name} already mapped with given group.");
                    return objMsg;
                }                
                obj.CreatedBy = obj.User.Id;
                obj.UpdatedBy = obj.User.Id;
                db.Add(obj);
                Message.Add(ref objMsg, db.SaveChanges(), "");                
                if (objMsg.status == Message.Type.success) 
                {
                    db.Entry(obj).Reload();                    
                    obj.ListId.Add(obj.Id);
                    objMsg.obj = List(obj).FirstOrDefault();
                    objMsg.data = GetViewOption().data;
                }                    
            } 
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Edit(Api obj) 
        {
            Message objMsg = new Message();
            try 
            {                                
                var Api = List(obj).FirstOrDefault();
                if (Api == null) 
                {
                    Message.Error(ref objMsg, "Api did not find for edit.");
                    return objMsg;
                }
                objMsg.obj = Api;
                objMsg.data = GetAddOption().data;
                Message.Success(ref objMsg, "Record found.");
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Update(Api obj) 
        {
            Message objMsg = new Message();
            try 
            {
                obj.Name = Util.SanitizeInput(obj.Name, App.Regexp.AlphaNum);
                var dbApi = db.Api.Where(ap => App.ActiveStatus.Contains(ap.Status)).ToList();
                if (dbApi.Where(ap => ap.ApiGroupId == obj.ApiGroupId && ap.Name == obj.Name && ap.Id != obj.Id).Count() > 0)
                {
                    Message.Duplicate(ref objMsg, $"Api Name : {obj.Name} already mapped with given group.");
                    return objMsg;
                }
                var UpdateApi = dbApi.Where(fm => fm.Id == obj.Id).FirstOrDefault();
                if (UpdateApi == null)
                {
                    Message.Error(ref objMsg, "Api did not find for update.");
                    return objMsg;
                }
                UpdateApi.ApiGroupId = obj.ApiGroupId;
                UpdateApi.Name = obj.Name;
                UpdateApi.Description = obj.Description;
                UpdateApi.ApiType = obj.ApiType;
                UpdateApi.IsApprovalRequired = obj.IsApprovalRequired;
                UpdateApi.SeqNo = obj.SeqNo;
                UpdateApi.Icon = obj.Icon;
                UpdateApi.UpdatedBy = obj.User.Id;
                UpdateApi.UpdatedAt = DateTime.Now;
                db.Update(UpdateApi);
                Message.Update(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success) 
                {
                    obj.ListId.Add(UpdateApi.Id);
                    objMsg.obj = List(obj).FirstOrDefault();
                    objMsg.data = GetViewOption().data;
                }                   
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Delete(Api obj) 
        {
            Message objMsg = new Message();
            try 
            {
                var DeleteApi = db.Api.Where(ap => App.ActiveStatus.Contains(ap.Status) && ap.Id == obj.Id).AsEnumerable().FirstOrDefault();
                if (DeleteApi == null)
                {
                    Message.Error(ref objMsg, "Api did not find for delete.");
                    return objMsg;
                }
                DeleteApi.Status = App.Status.Delete;
                DeleteApi.UpdatedBy = obj.User.Id;
                DeleteApi.UpdatedAt = DateTime.Now;
                db.Update(DeleteApi);
                Message.Delete(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {
                    obj.ListId.Add(obj.Id);
                    obj.ListStatus.Add(App.Status.Delete);
                    objMsg.obj = List(obj).FirstOrDefault();
                    objMsg.data = GetViewOption().data;
                }
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Enable(Api obj)
        {
            Message objMsg = new Message();
            try
            {
                var EnableApi = db.Api.Where(ap => App.Status.Delete == ap.Status && ap.Id == obj.Id).AsEnumerable().FirstOrDefault();
                if (EnableApi == null)
                {
                    Message.Error(ref objMsg, "Api did not find for enable.");
                    return objMsg;
                }
                EnableApi.Status = App.Status.Enable;
                EnableApi.UpdatedBy = obj.User.Id;
                EnableApi.UpdatedAt = DateTime.Now;
                db.Update(EnableApi);
                Message.Delete(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {
                    obj.ListId.Add(obj.Id);                    
                    objMsg.obj = List(obj).FirstOrDefault();
                    objMsg.data = GetViewOption().data;
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
    }
}
