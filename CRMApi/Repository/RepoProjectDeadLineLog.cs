using CRMApi.Models;
using CRMApi.Services;
using System.Data;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoProjectDeadLineLog
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoProjectDeadLineLog(DBCRM db)
        {
            this.db = db;
        }
        public Message GetViewOption()
        {
            Message objMsg = new Message();
            try
            {
                dynamic option = new ExpandoObject();
                var ListStatus = db.ProjectDeadLineLog.Select(x => x.Status.ToString()).ToList();
                option.Status = (
                    from st in db.Setting
                    where App.ActiveStatus.Contains(st.Status) && st.Name == App.SettingName.Status && ListStatus.Contains(st.Value)
                    select new
                    {
                        st.Value,
                        st.Description,
                    }
                ).ToList();
                option.ListId = db.ProjectDeadLineLog.Select(x => new { x.Id, x.Reason}).ToList();
                option.Project = db.Project.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new {
                    x.Id,
                    x.Name,
                    x.Description,
                    x.Code,

                }).ToList();
                objMsg.data = option;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public List<ProjectDeadLineLog> List(ProjectDeadLineLog? obj, User User)
        {
            List<ProjectDeadLineLog> ProjectDeadLineLog = new List<ProjectDeadLineLog>();
            try
            {
                obj ??= new ProjectDeadLineLog();
                obj.ListStatus = obj.ListStatus.Count == 0 ? App.ActiveStatus : obj.ListStatus;
                var dbProjectDeadLineLog = db.ProjectDeadLineLog.Where(x => obj.ListStatus.Contains(x.Status)).AsQueryable();
                dbProjectDeadLineLog = obj.ListId.Any() ? dbProjectDeadLineLog.Where(x => obj.ListId.Contains(x.Id)).AsQueryable() : dbProjectDeadLineLog;

                ProjectDeadLineLog = (
                    from dpt in dbProjectDeadLineLog
                    join sts in db.Setting on new { Value = dpt.Status.ToString(), Name = App.SettingName.Status } equals new { sts.Value, sts.Name }
                    join pro in db.Project on dpt.ProjectId equals pro.Id
                    join cby in db.User on dpt.CreatedBy equals cby.Id
                    join uby in db.User on dpt.UpdatedBy equals uby.Id
                    select new ProjectDeadLineLog
                    {
                        Id = dpt.Id,
                        //Code = dpt.Code,
                        //Name = dpt.Name,
                        //Description = dpt.Description,
                        ProjectId = dpt.ProjectId,
                        ProjectDesc = pro.Description,
                        DeadLineDate = dpt.DeadLineDate,
                        Reason = dpt.Reason,
                        Status = dpt.Status,
                        StatusDesc = sts.Description,
                        StatusCss = sts.CssClass ?? "",
                        CreatedBy = dpt.CreatedBy,
                        CreatedByName = cby.Name,
                        CreatedAt = dpt.CreatedAt,
                        UpdatedBy = dpt.UpdatedBy,
                        UpdatedByName = uby.Name,
                        UpdatedAt = dpt.UpdatedAt,
                        IsEdit = dpt.Status == App.Status.Enable ? true : false,
                        IsDuplicate = true,
                        IsDelete = dpt.Status == App.Status.Enable ? true : false,
                        IsEnable = dpt.Status == App.Status.Delete ? true : false,
                    }
                ).ToList();
            }
            catch (Exception)
            {
            }
            return ProjectDeadLineLog;
        }
        public Message Print(ProjectDeadLineLog obj, User User)
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
        public Message Export(ProjectDeadLineLog obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                //Get Company
                var objCompany = db.Company.FirstOrDefault(pt => App.ActiveStatus.Contains(pt.Status));
                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Company not found.");
                    return objMsg;
                }
                //Get ProjectDeadLineLog Group
                var Deparment = List(obj, User).Select(x => new
                {
                    x.Id,
                    //x.Code,
                    x.Reason,
                    x.DeadLineDate,
                    //x.Description,
                    x.Status,
                    Project = x.ProjectDesc,
                    x.CreatedBy,
                    x.CreatedByName,
                    x.CreatedAt,
                    x.UpdatedBy,
                    x.UpdatedByName,
                    x.UpdatedAt
                }).ToList();
                //Convert List To DataTable
                DataTable objDataTable = Util.ListToDataTable(Deparment);
                //Convert Datatable to base64                
                objCompany.SheetName = "ProjectDeadLineLog List";
                objCompany.ReportDesc = $"ProjectDeadLineLog - {DateTime.Now.ToString("dd-MMM-yyyy")}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Add(ProjectDeadLineLog obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                obj.Reason = Util.SanitizeInput(obj.Reason, null) ?? "";
                //obj.Description = Util.SanitizeInput(obj.Description, null) ?? "";
                //Checking Duplicate
                var duplicate = db.ProjectDeadLineLog.Where(ap => ap.Reason == obj.Reason || ap.Reason == obj.Reason).Select(ap => new { ap.Reason }).FirstOrDefault();
                if (duplicate != null)
                {
                    if (duplicate.Reason == obj.Reason)
                        Message.Duplicate(ref objMsg, $"ProjectDeadLineLog Name : {obj.Reason} already exists.");
                    //else if (duplicate.Description == obj.Description)
                    //    Message.Duplicate(ref objMsg, $"ProjectDeadLineLog Description : {obj.Description} already exists.");
                    return objMsg;
                }
                obj.CreatedBy = User.Id;
                obj.UpdatedBy = User.Id;
                db.Add(obj);
                Message.Add(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {
                    db.Entry(obj).Reload();
                    obj.ListId.Add(obj.Id);
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
        public Message Edit(ProjectDeadLineLog obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var ProjectDeadLineLog = List(obj, User).FirstOrDefault();
                if (ProjectDeadLineLog == null)
                {
                    Message.Error(ref objMsg, "ProjectDeadLineLog did not find for edit.");
                    return objMsg;
                }
                objMsg.obj = ProjectDeadLineLog;
                Message.Success(ref objMsg, "Record found.");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Update(ProjectDeadLineLog obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                obj.Reason = Util.SanitizeInput(obj.Reason, null) ?? "";
                //obj.Description = Util.SanitizeInput(obj.Description, null) ?? "";
                // Get Active ProjectDeadLineLog
                var dbProjectDeadLineLog = db.ProjectDeadLineLog.Where(dp => App.ActiveStatus.Contains(dp.Status)).ToList();
                // Check for duplicate Name or Description
                var duplicate = dbProjectDeadLineLog.FirstOrDefault(dp => (dp.Reason == obj.Reason) && dp.Id != obj.Id);
                if (duplicate != null)
                {
                    if (duplicate.Reason == obj.Reason)
                        Message.Duplicate(ref objMsg, $"ProjectDeadLineLog Name: {obj.Reason} already exists.");
                    else
                        //Message.Duplicate(ref objMsg, $"ProjectDeadLineLog Description: {obj.Description} already exists.");
                    return objMsg;
                }
                var UpdateDept = dbProjectDeadLineLog.FirstOrDefault(dp => dp.Id == obj.Id);
                if (UpdateDept == null)
                {
                    Message.Error(ref objMsg, "ProjectDeadLineLog did not find for update.");
                    return objMsg;
                }
                UpdateDept.Reason = obj.Reason;
                UpdateDept.DeadLineDate = obj.DeadLineDate;
                //UpdateDept.Description = obj.Description;
                UpdateDept.ProjectId = obj.ProjectId;
                UpdateDept.UpdatedBy = User.Id;
                UpdateDept.UpdatedAt = DateTime.Now;
                db.Update(UpdateDept);
                Message.Update(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {
                    obj.ListId.Add(UpdateDept.Id);
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
        public Message Delete(ProjectDeadLineLog obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var DeleteDept = db.ProjectDeadLineLog.FirstOrDefault(ap => App.ActiveStatus.Contains(ap.Status) && ap.Id == obj.Id);
                if (DeleteDept == null)
                {
                    Message.Error(ref objMsg, "ProjectDeadLineLog did not find for delete.");
                    return objMsg;
                }
                DeleteDept.Status = App.Status.Delete;
                DeleteDept.UpdatedBy = User.Id;
                DeleteDept.UpdatedAt = DateTime.Now;
                db.Update(DeleteDept);
                Message.Delete(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {
                    obj.ListId.Add(obj.Id);
                    obj.ListStatus.Add(App.Status.Delete);
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
        public Message Enable(ProjectDeadLineLog obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var DeleteDept = db.ProjectDeadLineLog.FirstOrDefault(ap => ap.Status == App.Status.Delete && ap.Id == obj.Id);
                if (DeleteDept == null)
                {
                    Message.Error(ref objMsg, "ProjectDeadLineLog did not find for delete.");
                    return objMsg;
                }
                DeleteDept.Status = App.Status.Enable;
                DeleteDept.UpdatedBy = User.Id;
                DeleteDept.UpdatedAt = DateTime.Now;
                db.Update(DeleteDept);
                Message.Enable(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {
                    obj.ListId.Add(obj.Id);
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
    }
}
