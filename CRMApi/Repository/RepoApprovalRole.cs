using CRMApi.Models;
using CRMApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoApprovalRole
    {
        private readonly DbCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoApprovalRole(DbCRM _db) 
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
                //Approvale Role
                var dbApprovalRole = db.ApprovalRole.Where(ag => App.ActiveStatus.Contains(ag.Status))
                    .Select(ag => new
                    {
                        ag.Id,
                        ag.Code,
                        ag.Name,
                        ag.Description,                        
                    }).ToList();
                //Options
                dynamic Options = new ExpandoObject();
                Options.Status = dbSetting.ToList();
                Options.ApprovalRole = dbApprovalRole.ToList();                
                objMsg.data = Options;
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }        
        public List<ApprovalRole> List(ApprovalRole? obj)
        {
            obj = obj == null ? new ApprovalRole() : obj;
            obj.ListStatus = obj.ListStatus.Count == 0 ? App.ActiveStatus : obj.ListStatus;
            var dbApprovalRole = db.ApprovalRole.Where(ag => obj.ListStatus.Contains(ag.Status)).ToList();
            dbApprovalRole = obj.ListId.Count == 0 ? dbApprovalRole : dbApprovalRole.Where(ag => obj.ListId.Contains(ag.Id)).ToList();
            
            var dbSetting = db.Setting.Where(st => App.ActiveStatus.Contains(st.Status)).ToList();
            var dbUser = db.User.Where(ur => App.ActiveStatus.Contains(ur.Status)).ToList();

            var ListApprovalRole = (
                from apr in dbApprovalRole
                join set in dbSetting on new { Value = apr.Status.ToString(), Name = App.SettingName.Status } equals new { set.Value, set.Name }
                join cby in dbUser on apr.CreatedBy equals cby.Id
                join uby in dbUser on apr.UpdatedBy equals uby.Id
                select new ApprovalRole
                {
                    Id = apr.Id,
                    Code = apr.Code,
                    Name = apr.Name,
                    Description = apr.Description,                                        
                    Status = apr.Status,
                    StatusName = set.Description,
                    StatusIcon = set.Icon,
                    StatusCss = set.CssClass,
                    CreatedByName = cby.Name,
                    CreatedBy = apr.CreatedBy,
                    UpdatedByName = uby.Name,
                    UpdatedBy = apr.UpdatedBy,
                    CreatedAt = apr.CreatedAt,
                    UpdatedAt = apr.UpdatedAt,
                    IsEdit = apr.Status > 0 ? true : false,
                    IsDuplicate = true,
                    IsDelete = apr.Status > 0 ? true : false,
                    IsEnable = apr.Status == 0 ? true : false,
                }
            ).ToList();
            return ListApprovalRole;
        }
        public Message Print(ApprovalRole obj)
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
        public Message Export(ApprovalRole obj)
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
                //Get Approval Role
                var ApprovalRole = List(obj);
                //Convert List To DataTable
                DataTable objDataTable = Util.ListToDataTable(ApprovalRole);
                //Remove Un-wanted column                
                objDataTable.Columns.Remove("IsEdit");
                objDataTable.Columns.Remove("IsDelete");
                objDataTable.Columns.Remove("IsEnable");
                objDataTable.Columns.Remove("IsDuplicate");
                objDataTable.Columns.Remove("ListId");                
                objDataTable.Columns.Remove("ListStatus");
                objDataTable.Columns.Remove("User");
                //Convert Datatable to base64               
                objCompany.SheetName = "Approval Role";
                objCompany.ReportDesc = $"Approval Role - {DateTime.Now.ToString("dd-MMM-yyyy")}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Add(ApprovalRole obj)
        {
            Message objMsg = new Message();
            try
            {                
                //Get Group
                var dbApprovalRole = db.ApprovalRole.Where(gp => App.ActiveStatus.Contains(gp.Status)).ToList();
                //Prevent Duplicate Name
                if (dbApprovalRole.Where(agp => agp.Name == obj.Name).Count() > 0)
                {
                    Message.Duplicate(ref objMsg, $"Name : {obj.Name}");
                    return objMsg;
                }
                //Prevent Duplicate Description
                if (dbApprovalRole.Where(agp => agp.Description == obj.Description).Count() > 0)
                {
                    Message.Duplicate(ref objMsg, $"Description : {obj.Description}");
                    return objMsg;
                }
                //Add Approval Role                
                obj.CreatedBy = obj.User.Id;
                obj.UpdatedBy = obj.User.Id;
                db.Add(obj);
                Message.Add(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {                    
                    db.Entry(obj).CurrentValues.SetValues(
                        db.Database.SqlQueryRaw<ApprovalRole>("SELECT * FROM APPROVALROLE WHERE ID = {0} AND ROWNUM = 1", obj.Id).AsEnumerable().FirstOrDefault() ?? obj
                    );
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
        public Message Edit(ApprovalRole obj)
        {
            Message objMsg = new Message();
            try
            {
                var ApprovalRole = List(obj).FirstOrDefault();
                if (ApprovalRole == null)
                {
                    Message.Error(ref objMsg, "Approval Role did not found for edit.");
                    return objMsg;
                }
                objMsg.obj = ApprovalRole;                
                Message.Success(ref objMsg, "Approval Role find for edit.");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Update(ApprovalRole obj)
        {
            Message objMsg = new Message();
            try
            {                
                //Get Group
                var dbApprovalRole = db.ApprovalRole.Where(gp => App.ActiveStatus.Contains(gp.Status)).ToList();
                //Prevent Duplicate Name
                if (dbApprovalRole.Where(gp => gp.Name == obj.Name && gp.Id != obj.Id).Count() > 0)
                {
                    Message.Duplicate(ref objMsg, $"Name : {obj.Name}");
                    return objMsg;
                }
                //Prevent Duplicate Description
                if (dbApprovalRole.Where(gp => gp.Description == obj.Description && gp.Id != obj.Id).Count() > 0)
                {
                    Message.Duplicate(ref objMsg, $"Description : {obj.Description}");
                    return objMsg;
                }
                //Update Group
                var UpdateApprovalRole = dbApprovalRole.Where(gp => gp.Id == obj.Id).FirstOrDefault();
                if (UpdateApprovalRole == null)
                {
                    Message.Error(ref objMsg, "Approval Role did find for update");
                    return objMsg;
                }
                UpdateApprovalRole.Name = obj.Name;
                UpdateApprovalRole.Description = obj.Description;
                UpdateApprovalRole.UpdatedAt = DateTime.Now;
                UpdateApprovalRole.UpdatedBy = obj.User.Id;
                db.Update(UpdateApprovalRole);
                Message.Update(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {
                    obj.ListId.Add(UpdateApprovalRole.Id);
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
        public Message Delete(ApprovalRole obj)
        {
            Message objMsg = new Message();
            try
            {
                var DeleteApprovalRole = db.ApprovalRole.Where(gp => gp.Status > 0 && gp.Id == obj.Id).AsEnumerable().FirstOrDefault();
                if (DeleteApprovalRole == null)
                {
                    Message.Error(ref objMsg, "Approval Role did not find for delete");
                    return objMsg;
                }                
                DeleteApprovalRole.Status = App.Status.Delete;
                DeleteApprovalRole.UpdatedBy = obj.User.Id;
                DeleteApprovalRole.UpdatedAt = DateTime.Now;
                db.Update(DeleteApprovalRole);
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
        public Message Enable(ApprovalRole obj)
        {
            Message objMsg = new Message();
            try
            {
                //Get Approval Role
                var dbApprovalRole = db.ApprovalRole.ToList();
                var EnableApprovalRole = dbApprovalRole.Where(gp => gp.Status == 0 && gp.Id == obj.Id).FirstOrDefault();
                if (EnableApprovalRole == null)
                {
                    Message.Error(ref objMsg, "Approval Role did not find for enable");
                    return objMsg;
                }
                //Prevent Duplicate Name
                var ApprovalRole = dbApprovalRole.Where(ag => App.ActiveStatus.Contains(ag.Status)).ToList();
                if (ApprovalRole.Where(gp => gp.Name == EnableApprovalRole.Name && gp.Id != EnableApprovalRole.Id).Count() > 0)
                {
                    Message.Duplicate(ref objMsg, $"Approval Role Name : {EnableApprovalRole.Name}");
                    return objMsg;
                }
                //Prevent Duplicate Description
                if (ApprovalRole.Where(gp => gp.Description == EnableApprovalRole.Description && gp.Id != EnableApprovalRole.Id).Count() > 0)
                {
                    Message.Duplicate(ref objMsg, $"Approval Role Description : {EnableApprovalRole.Description}");
                    return objMsg;
                }
                EnableApprovalRole.Status = App.Status.Enable;
                EnableApprovalRole.UpdatedBy = obj.User.Id;
                EnableApprovalRole.UpdatedAt = DateTime.Now;
                db.Update(EnableApprovalRole);
                Message.Enable(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {
                    obj.ListId.Add(EnableApprovalRole.Id);
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
