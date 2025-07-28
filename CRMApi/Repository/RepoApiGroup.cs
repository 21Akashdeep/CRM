using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using CRMApi.Models;
using CRMApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace CRMApi.Repository
{
    public class RepoApiGroup
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoApiGroup(DBCRM _db) 
        {
            db = _db;
        }
        public Message GetViewOption() 
        {
            Message objMsg = new Message();
            try 
            {
                //Setting 
                var dbSetting = db.Setting.Where(st => App.ActiveStatus.Contains(st.Status) && st.Name == App.SettingName.Status)
                    .Select(st => new
                    {
                        st.Value,
                        st.Description
                    }).ToList();
                //Api Group                
                var dbApiGroup = db.ApiGroup.Where(ag => App.ActiveStatus.Contains(ag.Status))
                    .Select(ag => new
                    {
                        ag.Id,
                        ag.Code,
                        ag.Name,
                        ag.Description,
                        ag.ParentId,
                        SubText = ""
                    }).ToList();
                dbApiGroup = dbApiGroup.Select(ag => new
                {
                    ag.Id,
                    ag.Code,
                    ag.Name,
                    ag.Description,
                    ag.ParentId,
                    SubText = $"{ag.Code}, {dbApiGroup.Where(ag1 => ag1.Id == ag.ParentId).Select(ag1 => ag1.Description).FirstOrDefault() ?? "NA"} / {ag.Description}"
                }).ToList();
                //Options
                dynamic Options = new ExpandoObject();
                Options.Status = dbSetting.ToList();
                Options.ApiGroup = dbApiGroup.ToList();
                Options.ApiGroupParent = dbApiGroup.Where(ag => ag.ParentId == 0).ToList();
                objMsg.data = Options;
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
                //Api Group                               
                var dbApiGroup = db.ApiGroup.Where(ag => App.ActiveStatus.Contains(ag.Status))
                    .Select(ag => new
                    {
                        ag.Id,
                        ag.Code,
                        ag.Name,
                        ag.Description,
                        ag.ParentId,
                        SubText = ""
                    }).ToList();
                dbApiGroup = dbApiGroup.Select(ag => new
                {
                    ag.Id,
                    ag.Code,
                    ag.Name,
                    ag.Description,
                    ag.ParentId,
                    SubText = $"{ag.Code}, {dbApiGroup.Where(ag1 => ag1.Id == ag.ParentId).Select(ag1 => ag1.Description).FirstOrDefault() ?? "NA"} / {ag.Description}"
                }).ToList();

                //Options
                dynamic Options = new ExpandoObject();
                Options.ApiGroup = dbApiGroup.ToList();
                Options.ApiGroupParent = dbApiGroup.Where(ag => ag.ParentId == 0).ToList();
                
                objMsg.data = Options;
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public List<ApiGroup> List(ApiGroup? obj, User User) 
        {
            obj = obj == null ? new ApiGroup() : obj;
            obj.ListStatus = obj.ListStatus.Count == 0 ? App.ActiveStatus : obj.ListStatus;
            var dbApiGroup = db.ApiGroup.Where(ag => obj.ListStatus.Contains(ag.Status)).ToList();
            dbApiGroup = obj.ListId.Count == 0 ? dbApiGroup : dbApiGroup.Where(ag => obj.ListId.Contains(ag.Id)).ToList();
            dbApiGroup = obj.ListParentId.Count == 0 ? dbApiGroup : dbApiGroup.Where(ag => obj.ListParentId.Contains(ag.ParentId)).ToList();

            var dbSetting = db.Setting.Where(st=> App.ActiveStatus.Contains(st.Status)).ToList();
            var dbUser = db.User.Where(ur => App.ActiveStatus.Contains(ur.Status)).ToList();

            var ListApiGroup = (
                from apg in dbApiGroup
                join set in dbSetting on new { Value = apg.Status.ToString(), Name = App.SettingName.Status } equals new { set.Value, set.Name }
                join cby in dbUser on apg.CreatedBy equals cby.Id
                join uby in dbUser on apg.UpdatedBy equals uby.Id
                select new ApiGroup 
                {
                    Id = apg.Id,
                    Code = apg.Code,
                    Name = apg.Name,
                    Description = apg.Description,
                    IsReserved = apg.IsReserved,
                    ParentId = apg.ParentId,
                    ParentDesc = dbApiGroup.Where(gp1 => gp1.Id == apg.ParentId).Select(gp1 => gp1.Description).FirstOrDefault(),
                    SeqNo = apg.SeqNo,
                    Icon = apg.Icon,                    
                    Status = apg.Status,
                    StatusName = set.Description,
                    StatusIcon = set.Icon,
                    StatusCss = set.CssClass,
                    CreatedByName = cby.Name,
                    CreatedBy = apg.CreatedBy,
                    UpdatedByName = uby.Name,
                    UpdatedBy = apg.UpdatedBy,
                    CreatedAt = apg.CreatedAt,
                    UpdatedAt = apg.UpdatedAt,
                    IsEdit = apg.Status > 0 ? true : false,
                    IsDuplicate = true,
                    IsDelete = apg.Status > 0 ? true : false,
                    IsEnable = apg.Status == 0 ? true : false,
                }
            ).ToList();
            return ListApiGroup;
        }
        public Message Print(ApiGroup obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                objMsg.data = List(obj, User);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Export(ApiGroup obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                //Get Party
                var objCompany = db.Company.FirstOrDefault(pt => App.ActiveStatus.Contains(pt.Status));
                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Company Info did found");
                    return objMsg;
                }
                //Get Api Group
                var ApiGroup = List(obj, User);
                //Convert List To DataTable
                DataTable objDataTable = Util.ListToDataTable(ApiGroup);
                //Remove Un-wanted column                
                objDataTable.Columns.Remove("IsEdit");
                objDataTable.Columns.Remove("IsDelete");
                objDataTable.Columns.Remove("IsEnable");
                objDataTable.Columns.Remove("IsDuplicate");
                objDataTable.Columns.Remove("ListId");
                objDataTable.Columns.Remove("ListParentId");
                objDataTable.Columns.Remove("ListStatus");
                objDataTable.Columns.Remove("User");
                //Convert Datatable to base64
                objCompany.SheetName = "Api Group List";
                objCompany.ReportDesc = $"Api Group - {DateTime.Now.ToString("dd-MMM-yyyy")}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Add(ApiGroup obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {                
                //Sanitize Input
                obj.Name = Util.SanitizeInput(obj.Name, $"{App.Regexp.Alpha} ")!;
                //Get Group
                var dbApiGroup = db.ApiGroup.Where(gp => App.ActiveStatus.Contains(gp.Status) && gp.ParentId == obj.ParentId).ToList();
                //Prevent Duplicate Name
                if (dbApiGroup.Where(agp => agp.Name == obj.Name && agp.ParentId == obj.ParentId).Count() > 0)
                {
                    Message.Duplicate(ref objMsg, $"Name : {obj.Name}");
                    return objMsg;
                }
                //Prevent Duplicate Description
                if (dbApiGroup.Where(agp => agp.Description == obj.Description && agp.ParentId == obj.ParentId).Count() > 0)
                {
                    Message.Duplicate(ref objMsg, $"Description : {obj.Description}");
                    return objMsg;
                }
                //Add Api Group                
                obj.CreatedBy = User.Id;
                obj.UpdatedBy = User.Id;
                db.Add(obj);
                Message.Add(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success) 
                {
                    db.Entry(obj).Reload();                    
                    obj.ListId.Add(obj.Id);
                    objMsg.obj = List(obj, User).FirstOrDefault();
                    objMsg.data = GetAddOption().data;
                }                
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Edit(int Id, User User)
        {
            Message objMsg = new Message();
            try 
            {
                var ApiGroup = List(new ApiGroup { ListId = new List<int> { Id} }, User).FirstOrDefault();
                if (ApiGroup == null)
                {
                    Message.Error(ref objMsg, "Api Group did not found for edit.");
                    return objMsg;
                }
                objMsg.obj = ApiGroup;
                objMsg.data = GetAddOption().data;
                Message.Success(ref objMsg, "Api Group find for edit.");
            } 
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Update(ApiGroup obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                //Sanitize Input
                obj.Name = Util.SanitizeInput(obj.Name, $"{App.Regexp.Alpha} ")!;
                //Get Group
                var dbApiGroup = db.ApiGroup.Where(gp => App.ActiveStatus.Contains(gp.Status)).ToList();
                //Prevent Duplicate Name
                if (dbApiGroup.Where(gp => gp.Name == obj.Name && gp.ParentId == obj.ParentId && gp.Id != obj.Id).Count() > 0)
                {
                    Message.Duplicate(ref objMsg, $"Name : {obj.Name}");
                    return objMsg;
                }
                //Prevent Duplicate Description
                if (dbApiGroup.Where(gp => gp.Description == obj.Description && gp.ParentId == obj.ParentId && gp.Id != obj.Id).Count() > 0)
                {
                    Message.Duplicate(ref objMsg, $"Description : {obj.Description}");
                    return objMsg;
                }
                //Update Group
                var UpdateApiGroup = dbApiGroup.Where(gp => gp.Id == obj.Id).FirstOrDefault();
                if (UpdateApiGroup == null)
                {
                    Message.Error(ref objMsg, "Api Group did find for update");
                    return objMsg;
                }
                UpdateApiGroup.Name = UpdateApiGroup.IsReserved ? UpdateApiGroup.Name : obj.Name;
                UpdateApiGroup.Description = obj.Description;
                UpdateApiGroup.IsReserved = UpdateApiGroup.IsReserved ? UpdateApiGroup.IsReserved : obj.IsReserved;
                UpdateApiGroup.ParentId = obj.ParentId;
                UpdateApiGroup.SeqNo = obj.SeqNo;
                UpdateApiGroup.Icon = obj.Icon;
                UpdateApiGroup.UpdatedAt = DateTime.Now;
                UpdateApiGroup.UpdatedBy = User.Id;
                db.Update(UpdateApiGroup);
                Message.Update(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success) 
                {
                    obj.ListId.Add(UpdateApiGroup.Id);
                    objMsg.obj = List(obj, User).FirstOrDefault();
                    objMsg.data = GetViewOption().data;
                }                
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Delete(int Id, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                var DeleteApiGroup = db.ApiGroup.Where(gp => gp.Status > 0 && gp.Id == Id).AsEnumerable().FirstOrDefault();
                if (DeleteApiGroup == null)
                {
                    Message.Error(ref objMsg, "Api Group did not find for delete");
                    return objMsg;
                }
                if (DeleteApiGroup.IsReserved)
                {
                    Message.Error(ref objMsg, $"Api Group Name : {DeleteApiGroup.Name} is reserved so you can not delete this.");
                    return objMsg;
                }
                DeleteApiGroup.Status = 0;
                DeleteApiGroup.UpdatedBy = User.Id;
                DeleteApiGroup.UpdatedAt = DateTime.Now;
                db.Update(DeleteApiGroup);
                Message.Delete(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success) 
                {
                    DeleteApiGroup.ListId.Add(DeleteApiGroup.Id);
                    DeleteApiGroup.ListStatus.Add(App.Status.Delete);
                    objMsg.obj = List(DeleteApiGroup, User).FirstOrDefault();
                    objMsg.data = GetViewOption().data;
                }                
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Enable(int Id, User User)
        {
            Message objMsg = new Message();
            try
            {
                //Get Api Group
                var dbApiGroup = db.ApiGroup.ToList();
                var EnableApiGroup = dbApiGroup.Where(gp => gp.Status == 0 && gp.Id == Id).FirstOrDefault();
                if (EnableApiGroup == null)
                {
                    Message.Error(ref objMsg, "Api Group did not find for enable");
                    return objMsg;
                }
                //Prevent Duplicate Name
                var ApiGroup = dbApiGroup.Where(ag => App.ActiveStatus.Contains(ag.Status)).ToList();
                if (ApiGroup.Where(gp => gp.Name == EnableApiGroup.Name && gp.ParentId == EnableApiGroup.ParentId && gp.Id != EnableApiGroup.Id).Count() > 0)
                {
                    Message.Duplicate(ref objMsg, $"Api Group Name : {EnableApiGroup.Name}");
                    return objMsg;
                }
                //Prevent Duplicate Description
                if (ApiGroup.Where(gp => gp.Description == EnableApiGroup.Description && gp.ParentId == EnableApiGroup.ParentId && gp.Id != EnableApiGroup.Id).Count() > 0)
                {
                    Message.Duplicate(ref objMsg, $"Api Group Description : {EnableApiGroup.Description}");
                    return objMsg;
                }
                EnableApiGroup.Status = App.Status.Enable;
                EnableApiGroup.UpdatedBy = User.Id;
                EnableApiGroup.UpdatedAt = DateTime.Now;
                db.Update(EnableApiGroup);
                Message.Enable(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success) 
                {
                    EnableApiGroup.ListId.Add(EnableApiGroup.Id);
                    objMsg.obj = List(EnableApiGroup, User).FirstOrDefault();
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
