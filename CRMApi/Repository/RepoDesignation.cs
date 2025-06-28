using CRMApi.Models;
using CRMApi.Services;
using System.Data;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoDesignation
    {
        private readonly DbCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoDesignation(DbCRM db)
        {
            this.db = db;
        }
        public Message GetViewOption()
        {
            Message objMsg = new Message();
            try
            {
                dynamic option = new ExpandoObject();
                var dbDesignation = db.Designation.ToList();
                var ListStatus = dbDesignation.Select(x => x.Status.ToString()).ToList();
                option.Status = (
                    from st in db.Setting                    
                    where App.ActiveStatus.Contains(st.Status) && st.Name == App.SettingName.Status && ListStatus.Contains(st.Value)
                    select new 
                    {
                        st.Value,
                        st.Description,
                    }
                ).ToList();
                option.ListId = db.Designation.Select(x => new { x.Id,  x.Description }).ToList();
                objMsg.data = option;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public List<Designation> List(Designation? obj, User User)
        {
            List<Designation> Designation = new List<Designation>();
            try 
            {
                obj ??= new Designation();
                obj.ListStatus = obj.ListStatus.Count == 0 ? App.ActiveStatus : obj.ListStatus;
                var dbDesignation = db.Designation.Where(x => obj.ListStatus.Contains(x.Status)).AsQueryable();
                dbDesignation = obj.ListId.Any() ? dbDesignation.Where(x => obj.ListId.Contains(x.Id)).AsQueryable() : dbDesignation;

                Designation = (
                    from dpt in dbDesignation
                    join sts in db.Setting on new { Value = dpt.Status.ToString(), Name = App.SettingName.Status } equals new { sts.Value, sts.Name }
                    join cby in db.User on dpt.CreatedBy equals cby.Id
                    join uby in db.User on dpt.UpdatedBy equals uby.Id
                    select new Designation
                    {
                        Id = dpt.Id,
                        Code = dpt.Code,
                        Name = dpt.Name,
                        Description = dpt.Description,
                        Status = dpt.Status,
                        StatusName = sts.Description,
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
            return Designation;
        }
        public Message Print(Designation obj, User User) 
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
        public Message Export(Designation obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                //Get Company
                var objCompany = db.Company.FirstOrDefault(pt => App.ActiveStatus.Contains(pt.Status) && pt.Id == User.CompanyId);
                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Company not found.");
                    return objMsg;
                }
                //Get Designation Group
                var Deparment = List(obj, User).Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.Name,
                    x.Description,
                    x.Status,
                    x.StatusName,
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
                objCompany.SheetName = "Designation List";
                objCompany.ReportDesc = $"Designation - {DateTime.Now.ToString("dd-MMM-yyyy")}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Add(Designation obj, User User)
        {
            Message objMsg = new Message();
            try
            {                
                obj.Name = Util.SanitizeInput(obj.Name, null) ?? "";
                obj.Description = Util.SanitizeInput(obj.Description, null) ?? "";
                //Checking Duplicate
                var duplicate = db.Designation.Where(ap => ap.Name == obj.Name || ap.Description == obj.Description).Select(ap => new { ap.Name, ap.Description }).FirstOrDefault();
                if (duplicate != null)
                {
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"Designation Name : {obj.Name} already exists.");
                    else if (duplicate.Description == obj.Description)
                        Message.Duplicate(ref objMsg, $"Designation Description : {obj.Description} already exists.");
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
        public Message Edit(Designation obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                var Designation = List(obj, User).FirstOrDefault();
                if (Designation == null)
                {
                    Message.Error(ref objMsg, "Designation did not find for edit.");
                    return objMsg;
                }
                objMsg.obj = Designation;                
                Message.Success(ref objMsg, "Record found.");
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Update(Designation obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                obj.Name = Util.SanitizeInput(obj.Name, null) ?? "";
                obj.Description = Util.SanitizeInput(obj.Description, null) ?? "";
                // Get Active Designation
                var dbDesignation = db.Designation.Where(dp => App.ActiveStatus.Contains(dp.Status)).ToList();
                // Check for duplicate Name or Description
                var duplicate = dbDesignation.FirstOrDefault(dp => (dp.Name == obj.Name || dp.Description == obj.Description) && dp.Id != obj.Id);
                if (duplicate != null)
                {
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"Designation Name: {obj.Name} already exists.");
                    else
                        Message.Duplicate(ref objMsg, $"Designation Description: {obj.Description} already exists.");
                    return objMsg;
                }
                var UpdateDept = dbDesignation.FirstOrDefault(dp => dp.Id == obj.Id);
                if (UpdateDept == null)
                {
                    Message.Error(ref objMsg, "Designation did not find for update.");
                    return objMsg;
                }                
                UpdateDept.Name = obj.Name;
                UpdateDept.Description = obj.Description;                
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
        public Message Delete(Designation obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var DeleteDept = db.Designation.FirstOrDefault(ap => App.ActiveStatus.Contains(ap.Status) && ap.Id == obj.Id);
                if (DeleteDept == null)
                {
                    Message.Error(ref objMsg, "Designation did not find for delete.");
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
        public Message Enable(Designation obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var DeleteDept = db.Designation.FirstOrDefault(ap => ap.Status == App.Status.Delete && ap.Id == obj.Id);
                if (DeleteDept == null)
                {
                    Message.Error(ref objMsg, "Designation did not find for delete.");
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
