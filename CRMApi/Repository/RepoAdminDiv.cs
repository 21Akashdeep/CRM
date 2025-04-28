using CRMApi.Models;
using CRMApi.Services;
using System.Data;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoAdminDiv
    {
        private readonly DbCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoAdminDiv(DbCRM _db) 
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
                    from ac in db.AdminDiv
                    join st in db.Setting on new { Value = ac.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                    group st by new { st.Value, st.Description } into st
                    select new
                    {
                        st.Key.Value,
                        st.Key.Description
                    }
                ).ToList();
                Option.AdminDiv = db.AdminDiv.Where(ag => App.ActiveStatus.Contains(ag.Status)).Select(ag => new
                {
                    ag.Id,
                    ag.Code,
                    ag.Name,
                    ag.Description,                    
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
                dynamic Option = new ExpandoObject();                
                Option.Country = db.Country.Where(ag => App.ActiveStatus.Contains(ag.Status)).Select(ag => new
                {
                    ag.Id,
                    ag.Code,
                    ag.Name,
                    ag.Description,
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
        public List<AdminDiv> List(AdminDiv? obj, User User) 
        {
            obj = obj == null ? new AdminDiv() : obj;
            obj.ListStatus = obj.ListStatus.Count == 0 ? App.ActiveStatus : obj.ListStatus;
            //Get AdminDiv List
            var dbAdminDiv = db.AdminDiv.Where(ap => obj.ListStatus.Contains(ap.Status)).AsQueryable();
            dbAdminDiv = obj.ListId.Count == 0 ? dbAdminDiv : dbAdminDiv.Where(ap => obj.ListId.Contains(ap.Id));
            
            //AdminDiv List
            var AdminDiv = (
                from ad in dbAdminDiv
                join zn in db.Zone on ad.ZoneId equals zn.Id
                join cn in db.Country on ad.CountryId equals cn.Id
                join st in db.Setting on new { Value = ad.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                join cb in db.User on ad.CreatedBy equals cb.Id
                join ub in db.User on ad.UpdatedBy equals ub.Id
                select new AdminDiv
                {
                    Id = ad.Id,
                    Code = ad.Code,
                    Name = ad.Name,
                    Description = ad.Description,
                    ZoneId = ad.ZoneId,
                    ZoneDesc = zn.Description,
                    CountryId = ad.CountryId,
                    CountryDesc = cn.Description,
                    Status = ad.Status,
                    StatusName = st.Description,
                    StatusCss = st.CssClass ?? "",
                    CreatedBy = ad.CreatedBy,
                    CreatedByName = cb.Name,
                    CreatedAt = ad.CreatedAt,
                    UpdatedBy = ad.UpdatedBy,
                    UpdatedByName = ub.Name,
                    UpdatedAt = ad.UpdatedAt,
                    IsEdit = ad.Status == App.Status.Enable ? true : false,
                    IsDuplicate = true,
                    IsDelete = ad.Status == App.Status.Enable ? true : false,
                    IsEnable = ad.Status == App.Status.Delete ? true : false,
                }
            ).ToList();
            return AdminDiv;
        }
        public Message Print(AdminDiv obj, User User) 
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
        public Message Export(AdminDiv obj, User User)
        {
            Message objMsg = new Message();
            try
            {   //Get Party
                var objCompany = db.Company.FirstOrDefault(pt => App.ActiveStatus.Contains(pt.Status) && pt.Id == User.CompanyId);
                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Company Info did found");
                    return objMsg;
                }
                //Get AdminDiv Group
                var AdminDiv = List(obj, User).Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.Name,
                    x.Description,
                    x.ZoneDesc,
                    x.CountryDesc,                    
                    Status = x.StatusName,
                    x.CreatedBy,
                    x.CreatedByName,
                    x.CreatedAt,
                    x.UpdatedBy,
                    x.UpdatedByName,
                    x.UpdatedAt
                }).ToList();
                //Convert List To DataTable
                DataTable objDataTable = Util.ListToDataTable(AdminDiv);
                objCompany.SheetName = "State / Admin. Div. List";
                objCompany.ReportDesc = $"State / Admin. Div. - {DateTime.Now.ToString("dd-MMM-yyyy")}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Add(AdminDiv obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                //Sanitize Input
                obj.Code = Util.SanitizeInput(obj.Code, null) ?? "";
                obj.Name = Util.SanitizeInput(obj.Name, null) ?? "";
                obj.Description = Util.SanitizeInput(obj.Description, null) ?? "";
                var duplicate = db.AdminDiv.FirstOrDefault(ap => App.ActiveStatus.Contains(ap.Status) && (ap.Code == obj.Code || ap.Name == obj.Name || ap.Description == obj.Description));
                if(duplicate != null)
                {
                    if(duplicate.Code == obj.Code)
                        Message.Duplicate(ref objMsg, $"AdminDiv Code : {obj.Code} already exists.");
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"AdminDiv Name : {obj.Name} already exists.");
                    if (duplicate.Description == obj.Description)
                        Message.Duplicate(ref objMsg, $"AdminDiv Description : {obj.Description} already exists.");
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
        public Message Edit(AdminDiv obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {                                
                var AdminDiv = List(obj, User).FirstOrDefault();
                if (AdminDiv == null) 
                {
                    Message.Error(ref objMsg, "AdminDiv did not find for edit.");
                    return objMsg;
                }
                objMsg.obj = AdminDiv;                
                Message.Success(ref objMsg, "Record found.");
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Update(AdminDiv obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                obj.Name = Util.SanitizeInput(obj.Name, App.Regexp.AlphaNum) ?? "";
                var dbAdminDiv = db.AdminDiv.Where(ap => App.ActiveStatus.Contains(ap.Status)).ToList();
                var duplicate = db.AdminDiv.FirstOrDefault(ap => App.ActiveStatus.Contains(ap.Status) && (ap.Code == obj.Code || ap.Name == obj.Name || ap.Description == obj.Description) && ap.Id != obj.Id);
                if (duplicate != null)
                {
                    if (duplicate.Code == obj.Code)
                        Message.Duplicate(ref objMsg, $"AdminDiv Code : {obj.Code} already exists.");
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"AdminDiv Name : {obj.Name} already exists.");
                    if (duplicate.Description == obj.Description)
                        Message.Duplicate(ref objMsg, $"AdminDiv Description : {obj.Description} already exists.");
                    return objMsg;
                }
                
                var UpdateAdminDiv = dbAdminDiv.Where(fm => fm.Id == obj.Id).FirstOrDefault();
                if (UpdateAdminDiv == null)
                {
                    Message.Error(ref objMsg, "AdminDiv did not find for update.");
                    return objMsg;
                }
                UpdateAdminDiv.Code = obj.Code;
                UpdateAdminDiv.Name = obj.Name;
                UpdateAdminDiv.Description = obj.Description;
                UpdateAdminDiv.ZoneId = obj.ZoneId;
                UpdateAdminDiv.CountryId = obj.CountryId;
                UpdateAdminDiv.UpdatedBy = User.Id;
                UpdateAdminDiv.UpdatedAt = DateTime.Now;
                db.Update(UpdateAdminDiv);
                Message.Update(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success) 
                {
                    obj.ListId.Add(UpdateAdminDiv.Id);
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
        public Message Delete(AdminDiv obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                var DeleteAdminDiv = db.AdminDiv.FirstOrDefault(ap => App.ActiveStatus.Contains(ap.Status) && ap.Id == obj.Id);
                if (DeleteAdminDiv == null)
                {
                    Message.Error(ref objMsg, "AdminDiv did not find for delete.");
                    return objMsg;
                }
                DeleteAdminDiv.Status = App.Status.Delete;
                DeleteAdminDiv.UpdatedBy = User.Id;
                DeleteAdminDiv.UpdatedAt = DateTime.Now;
                db.Update(DeleteAdminDiv);
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
        public Message Enable(AdminDiv obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var EnableAdminDiv = db.AdminDiv.FirstOrDefault(ap => App.Status.Delete == ap.Status && ap.Id == obj.Id);
                if (EnableAdminDiv == null)
                {
                    Message.Error(ref objMsg, "AdminDiv did not find for enable.");
                    return objMsg;
                }
                EnableAdminDiv.Status = App.Status.Enable;
                EnableAdminDiv.UpdatedBy = User.Id;
                EnableAdminDiv.UpdatedAt = DateTime.Now;
                db.Update(EnableAdminDiv);
                Message.Delete(ref objMsg, db.SaveChanges(), "");
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
