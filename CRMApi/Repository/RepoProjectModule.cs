using CRMApi.Models;
using CRMApi.Services;
using System.Data;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoProjectModule
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoProjectModule(DBCRM db)
        {
            this.db = db;
        }
        public Message GetViewOption()
        {
            Message objMsg = new Message();
            try
            {
                dynamic option = new ExpandoObject();
                var ListStatus = db.ProjectModule.Select(x => x.Status.ToString()).ToList();
                option.Status = (
                    from st in db.Setting
                    where App.ActiveStatus.Contains(st.Status) && st.Name == App.SettingName.Status && ListStatus.Contains(st.Value)
                    select new
                    {
                        st.Value,
                        st.Description,
                    }
                ).ToList();

                option.ListId = db.ProjectModule.
                    Select(x => new 
                    { 
                      x.Id,
                      x.Description

                    }).ToList();

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
        public Message GetAddOption()
        {
            Message objMsg = new Message();
            try
            {
                //Expando Object
                dynamic Option = new ExpandoObject();
                Option.Project = db.Project.Where(ig => App.ActiveStatus.Contains(ig.Status)).Select(ig => new
                {
                    ig.Id,
                    ig.Code,
                    ig.Name,
                    ig.Description
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
        public List<ProjectModule> List(ProjectModule? obj, User User)
        {
            List<ProjectModule> ProjectModule = new List<ProjectModule>();
            try
            {
                obj ??= new ProjectModule();
                obj.ListStatus = obj.ListStatus.Count == 0 ? App.ActiveStatus : obj.ListStatus;
                var dbProjectModule = db.ProjectModule.Where(x => obj.ListStatus.Contains(x.Status)).AsQueryable();
                dbProjectModule = obj.ListId.Any() ? dbProjectModule.Where(x => obj.ListId.Contains(x.Id)).AsQueryable() : dbProjectModule;
                dbProjectModule = obj.ListProjectId.Any() ? dbProjectModule.Where(x => obj.ListProjectId.Contains(x.ProjectId)).AsQueryable() : dbProjectModule;

                ProjectModule = (
                    from dpt in dbProjectModule
                    join sts in db.Setting on new { Value = dpt.Status.ToString(), Name = App.SettingName.Status } equals new { sts.Value, sts.Name }
                    join pro in db.Project on dpt.ProjectId equals pro.Id
                    join cby in db.User on dpt.CreatedBy equals cby.Id
                    join uby in db.User on dpt.UpdatedBy equals uby.Id
                    select new ProjectModule
                    {
                        Id = dpt.Id,                       
                        Name = dpt.Name,
                        Description = dpt.Description,
                        EstimatedDays = dpt.EstimatedDays,
                        TechnicalDoc = dpt.TechnicalDoc,
                        ProjectId = dpt.ProjectId,
                        ProjectDesc = pro.Description,                       
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
            return ProjectModule;
        }
        public Message Print(ProjectModule obj, User User)
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
        public Message Export(ProjectModule obj, User User)
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
                //Get ProjectModule Group
                var Deparment = List(obj, User).Select(x => new
                {
                    x.Id,                   
                    x.Name,
                    x.EstimatedDays,
                    x.TechnicalDoc,                  
                    x.Description,
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
                objCompany.SheetName = "ProjectModule List";
                objCompany.ReportDesc = $"ProjectModule - {DateTime.Now.ToString("dd-MMM-yyyy")}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Add(ProjectModule obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var ProjectModule = db.ProjectModule.Where(op => op.Status == App.Status.Approved && op.Name == obj.Name).AsEnumerable().FirstOrDefault();
                obj.Name = Util.SanitizeInput(obj.Name, null) ?? "";
                obj.Description = Util.SanitizeInput(obj.Description, null) ?? "";
                
                //obj.TechnicalDoc = Util.SanitizeInput(obj.TechnicalDoc, null) ?? "";

                // Checking Duplicate
                var duplicate = db.ProjectModule
                    .Where(ap => ap.Name == obj.Name || ap.Description == obj.Description)
                    .Select(ap => new { ap.Name, ap.Description })
                    .FirstOrDefault();

                if (duplicate != null)
                {
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"ProjectModule Name : {obj.Name} already exists.");
                    else if (duplicate.Description == obj.Description)
                        Message.Duplicate(ref objMsg, $"ProjectModule Description : {obj.Description} already exists.");

                    return objMsg;
                }
                // Add Document
                if (obj.Document != null && ProjectModule != null)
                {
                    obj.Document.FilePath = App.DocPath.ProjModTechDoc;
                    obj.Document.FileName = $"{ProjectModule.Id}{App.DocPath.ProjModTechDocSufix}";
                    objMsg = Util.AddFile(obj.Document);
                    if (objMsg.status == Message.Type.error)
                    {
                        return objMsg;
                    }
                }
                // Insert ProjectModule
                obj.CreatedBy = User.Id;
                obj.UpdatedBy = User.Id;
                db.Add(obj);

                Message.Add(ref objMsg, db.SaveChanges(), "");


                if (objMsg.status == Message.Type.success)
                {
                    //Add Document
                    if (obj.Document != null)
                    {
                        obj.Document.FilePath = App.DocPath.ProjModTechDoc;
                        obj.Document.FileName = $"{obj.Id}{App.DocPath.ProjModTechDocSufix}";
                        Message objDocMsg = Util.AddFile(obj.Document);

                        if (objDocMsg.status == Message.Type.success)
                        {
                            var UpdateProjectModule = db.ProjectModule.Where(op => op.Id == obj.Id && App.ActiveStatus.Contains(op.Status)).FirstOrDefault();
                            if (UpdateProjectModule != null)
                            {
                                UpdateProjectModule.TechnicalDoc = objDocMsg.filePath;
                                db.Update(UpdateProjectModule);
                                db.SaveChanges();
                            }
                        }
                        objMsg.status = objDocMsg.status == Message.Type.error ? Message.Type.warning : Message.Type.success;
                        objMsg.statusText += objDocMsg.status == Message.Type.error ? $"<br>{objDocMsg.statusText}" : "";
                    }

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
        public Message Edit(int Id, User User)
        {
            Message objMsg = new Message();
            try
            {
                ProjectModule obj = new ProjectModule();
                obj.ListId.Add(Id);
                var ProjectModule = List(obj, User).FirstOrDefault();
                if (ProjectModule == null)
                {
                    Message.Error(ref objMsg, "ProjectModule did find for edit.");
                    return objMsg;
                }
                objMsg.obj = ProjectModule;
                objMsg.data = GetAddOption().data;
                Message.Success(ref objMsg, "ProjectModule found.");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Update(ProjectModule obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                obj.Name = Util.SanitizeInput(obj.Name, null) ?? "";
                //obj.Description = Util.SanitizeInput(obj.Description, null) ?? "";
                // Get Active ProjectModule
                var dbProjectModule = db.ProjectModule.Where(dp => App.ActiveStatus.Contains(dp.Status)).ToList();
                // Check for duplicate Name or Description
                var duplicate = dbProjectModule.FirstOrDefault(dp => (dp.Name == obj.Name) && dp.Id != obj.Id);
                if (duplicate != null)
                {
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"ProjectModule Name: {obj.Name} already exists.");
                    else
                        //Message.Duplicate(ref objMsg, $"ProjectModule Description: {obj.Description} already exists.");
                    return objMsg;
                }
                var UpdateDept = dbProjectModule.FirstOrDefault(dp => dp.Id == obj.Id);
                if (UpdateDept == null)
                {
                    Message.Error(ref objMsg, "ProjectModule did not find for update.");
                    return objMsg;
                }
                UpdateDept.Name = obj.Name;
                UpdateDept.Description = obj.Description;
                UpdateDept.TechnicalDoc = obj.TechnicalDoc;
                UpdateDept.EstimatedDays = obj.EstimatedDays;
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
        public Message Delete(ProjectModule obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var DeleteDept = db.ProjectModule.FirstOrDefault(ap => App.ActiveStatus.Contains(ap.Status) && ap.Id == obj.Id);
                if (DeleteDept == null)
                {
                    Message.Error(ref objMsg, "ProjectModule did not find for delete.");
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
        public Message Enable(ProjectModule obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var DeleteDept = db.ProjectModule.FirstOrDefault(ap => ap.Status == App.Status.Delete && ap.Id == obj.Id);
                if (DeleteDept == null)
                {
                    Message.Error(ref objMsg, "ProjectModule did not find for delete.");
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
