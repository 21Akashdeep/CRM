using CRMApi.Models;
using CRMApi.Services;
using System.Data;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoProject
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoProject(DBCRM db)
        {
            this.db = db;
        }
        public Message GetViewOption()
        {
            Message objMsg = new Message();
            try
            {
                dynamic option = new ExpandoObject();
                var ListStatus = db.Project.Select(x => x.Status.ToString()).ToList();
                option.Status = (
                    from st in db.Setting
                    where App.ActiveStatus.Contains(st.Status) && st.Name == App.SettingName.Status && ListStatus.Contains(st.Value)
                    select new
                    {
                        st.Value,
                        st.Description,
                    }
                ).ToList();
                option.ListId = db.Project.Select(x => new { x.Id, x.Description }).ToList();
                option.Customer = db.Customer.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new { 
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
        public List<Project> List(Project? obj, User User)
        {
            List<Project> Project = new List<Project>();
            try
            {
                obj ??= new Project();
                obj.ListStatus = obj.ListStatus.Count == 0 ? App.ActiveStatus : obj.ListStatus;
                var dbProject = db.Project.Where(x => obj.ListStatus.Contains(x.Status)).AsQueryable();
                dbProject = obj.ListId.Any() ? dbProject.Where(x => obj.ListId.Contains(x.Id)).AsQueryable() : dbProject;

                Project = (
                    from dpt in dbProject
                    join sts in db.Setting on new { Value = dpt.Status.ToString(), Name = App.SettingName.Status } equals new { sts.Value, sts.Name }
                    join cust in db.Customer on dpt.CustomerId equals cust.Id
                    join cus in db.User on dpt.CreatedBy equals cus.Id
                    join uby in db.User on dpt.UpdatedBy equals uby.Id
                    select new Project
                    {
                        Id = dpt.Id,
                        Code = dpt.Code,
                        Name = dpt.Name,
                        CustomerId = dpt.CustomerId,
                        CustomerDesc = cust.Description,
                        PoNo = dpt.PoNo,
                        PoDate = dpt.PoDate,
                        StartDate = dpt.StartDate,
                        DeadLineDate = dpt.DeadLineDate,
                        TechnicalDoc = dpt.TechnicalDoc,
                        Description = dpt.Description,
                        Status = dpt.Status,
                        StatusDesc = sts.Description,
                        StatusCss = sts.CssClass ?? "",
                        CreatedBy = dpt.CreatedBy,
                        CreatedByName = cus.Name,
                        CreatedAt = dpt.CreatedAt,
                        //StartAt = dpt.StartAt,
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
            return Project;
        }
        public Message Print(Project obj, User User)
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
        public Message Export(Project obj, User User)
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
                //Get Project Group
                var Deparment = List(obj, User).Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.Name,
                    x.Description,
                    x.CustomerId,
                    x.PoNo,
                    x.PoDate,
                    x.StartDate,
                    x.DeadLineDate,
                    x.TechnicalDoc,
                    x.Status,
                    //x.StartAt,
                    x.CreatedBy,
                    x.CreatedByName,
                    x.CreatedAt,
                    x.UpdatedBy,
                    x.UpdatedByName,
                    x.UpdatedAt,
                    Customer = x.CustomerDesc,
                }).ToList();
                //Convert List To DataTable
                DataTable objDataTable = Util.ListToDataTable(Deparment);
                //Convert Datatable to base64                
                objCompany.SheetName = "Project List";
                objCompany.ReportDesc = $"Project - {DateTime.Now.ToString("dd-MMM-yyyy")}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }

        public Message Add(Project obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var Project = db.Project.Where(op => op.Status == App.Status.Approved && op.Code == obj.Code).AsEnumerable().FirstOrDefault();
                obj.Name = Util.SanitizeInput(obj.Name, null) ?? "";
                obj.Description = Util.SanitizeInput(obj.Description, null) ?? "";
                obj.PoNo = Util.SanitizeInput(obj.PoNo, null) ?? "";
                //obj.TechnicalDoc = Util.SanitizeInput(obj.TechnicalDoc, null) ?? "";

                // Checking Duplicate
                var duplicate = db.Project
                    .Where(ap => ap.Name == obj.Name || ap.Description == obj.Description)
                    .Select(ap => new { ap.Name, ap.Description })
                    .FirstOrDefault();

                if (duplicate != null)
                {
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"Project Name : {obj.Name} already exists.");
                    else if (duplicate.Description == obj.Description)
                        Message.Duplicate(ref objMsg, $"Project Description : {obj.Description} already exists.");

                    return objMsg;
                }
                // Add Document
                if (obj.Document != null && Project != null)
                {
                    obj.Document.FilePath = App.DocPath.ProjTechDoc;
                    obj.Document.FileName = $"{Project.Code}{App.DocPath.ProjTechDocSufix}";
                    objMsg = Util.AddFile(obj.Document);
                    if (objMsg.status == Message.Type.error)
                    {
                        return objMsg;
                    }
                }
                // Insert project
                obj.CreatedBy = User.Id;
                obj.UpdatedBy = User.Id;
                db.Add(obj);

                Message.Add(ref objMsg, db.SaveChanges(), "");


                if (objMsg.status == Message.Type.success)
                {
                    //Add Document
                    if (obj.Document != null)
                    {
                        obj.Document.FilePath = App.DocPath.ProjTechDoc;
                        obj.Document.FileName = $"{obj.Code}{App.DocPath.ProjTechDocSufix}";
                        Message objDocMsg = Util.AddFile(obj.Document);
                        if (objDocMsg.status == Message.Type.success)
                        {
                            var UpdateProject = db.Project.Where(op => op.Id == obj.Id && App.ActiveStatus.Contains(op.Status)).FirstOrDefault();
                            if (UpdateProject != null)
                            {
                                UpdateProject.TechnicalDoc = objDocMsg.filePath;
                                db.Update(UpdateProject);
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

        public Message Edit(Project obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var Project = List(obj, User).FirstOrDefault();
                if (Project == null)
                {
                    Message.Error(ref objMsg, "Project did not find for edit.");
                    return objMsg;
                }
                objMsg.obj = Project;
                Message.Success(ref objMsg, "Record found.");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Update(Project obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var UpdateOutPass = db.Project.Where(op => op.Id == obj.Id && App.UnderProcess.Contains(op.Status)).AsEnumerable().FirstOrDefault();
                if (UpdateOutPass == null)
                {
                    Message.Error(ref objMsg, "Out Pass did not find for update.");
                    return objMsg;
                }
                obj.Name = Util.SanitizeInput(obj.Name, null) ?? "";
                obj.Description = Util.SanitizeInput(obj.Description, null) ?? "";
                obj.PoNo = Util.SanitizeInput(obj.PoNo, null) ?? "";
                
                obj.TechnicalDoc = Util.SanitizeInput(obj.TechnicalDoc, null) ?? "";
                // Get Active Project
                var dbProject = db.Project.Where(dp => App.ActiveStatus.Contains(dp.Status)).ToList();
                // Check for duplicate Name or Description
                var duplicate = dbProject.FirstOrDefault(dp => (dp.Name == obj.Name || dp.Description == obj.Description) && dp.Id != obj.Id);
                if (duplicate != null)
                {
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"Project Name: {obj.Name} already exists.");
                    else
                        Message.Duplicate(ref objMsg, $"Project Description: {obj.Description} already exists.");
                    return objMsg;
                }
                var UpdateDept = dbProject.FirstOrDefault(dp => dp.Id == obj.Id);
                if (UpdateDept == null)
                {
                    Message.Error(ref objMsg, "Project did not find for update.");
                    return objMsg;
                }


                //Add Document                
                if (obj.Document != null)
                {
                    obj.Document.FilePath = App.DocPath.ProjTechDoc;
                    obj.Document.FileName = $"{UpdateOutPass.Code}{App.DocPath.ProjTechDocSufix}";
                    objMsg = Util.AddFile(obj.Document);
                    if (objMsg.status == Message.Type.error)
                    {
                        return objMsg;
                    }
                }

                UpdateDept.Name = obj.Name;
                UpdateDept.Description = obj.Description;
                UpdateDept.PoDate = obj.PoDate;
                UpdateDept.PoNo = obj.PoNo;
                UpdateDept.CustomerId = obj.CustomerId;
                UpdateDept.StartDate = obj.StartDate;
                UpdateDept.DeadLineDate = obj.DeadLineDate;
                //UpdateDept.TechnicalDoc = obj.TechnicalDoc;
                UpdateOutPass.TechnicalDoc = objMsg.filePath != null ? objMsg.filePath : UpdateOutPass.TechnicalDoc;

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
        public Message Delete(Project obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var DeleteDept = db.Project.FirstOrDefault(ap => App.ActiveStatus.Contains(ap.Status) && ap.Id == obj.Id);
                if (DeleteDept == null)
                {
                    Message.Error(ref objMsg, "Project did not find for delete.");
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
        public Message Enable(Project obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var DeleteDept = db.Project.FirstOrDefault(ap => ap.Status == App.Status.Delete && ap.Id == obj.Id);
                if (DeleteDept == null)
                {
                    Message.Error(ref objMsg, "Project did not find for delete.");
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
