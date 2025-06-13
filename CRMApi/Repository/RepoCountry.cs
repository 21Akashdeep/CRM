using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using CRMApi.Models;
using CRMApi.Services;
using System.Data;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoCountry
    {
        private readonly DbCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoCountry(DbCRM _db) 
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
                    from ac in db.Country
                    join st in db.Setting on new { Value = ac.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                    group st by new { st.Value, st.Description } into st
                    select new
                    {
                        st.Key.Value,
                        st.Key.Description
                    }
                ).ToList();
                Option.Country = db.Country.Where(ag => App.ActiveStatus.Contains(ag.Status)).Select(ag => new
                {
                    ag.Id,
                    ag.Code,
                    ag.Name,
                    ag.Description,
                    ag.AdminDivType,
                    ag.PostalType
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
                var dbSetting = db.Setting.Where(x => App.ActiveStatus.Contains(x.Status) && (x.Name == App.SettingName.AdminDivType || x.Name == App.SettingName.PostalType)).Select(x => new
                {
                    x.Name,
                    x.Value,
                    x.Description
                }).ToList();
                dynamic Option = new ExpandoObject();
                Option.AdminDivType = dbSetting.Where(x => x.Name == App.SettingName.AdminDivType).ToList();
                Option.PostalType = dbSetting.Where(x => x.Name == App.SettingName.PostalType).ToList();
                objMsg.data = Option;
                Message.Success(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public List<Country> List(Country? obj, User User) 
        {
            Message objMsg = new Message();
            List<Country> Country = new List<Country>();
            obj = obj == null ? new Country() : obj;
            obj.ListStatus = obj.ListStatus.Count == 0 ? App.ActiveStatus : obj.ListStatus;
            //Get Country List
            var dbCountry = db.Country.Where(ap => obj.ListStatus.Contains(ap.Status)).AsQueryable();
            dbCountry = obj.ListId.Count == 0 ? dbCountry : dbCountry.Where(ap => obj.ListId.Contains(ap.Id));            
            //Country List
            Country = (
                from ct in dbCountry
                join st in db.Setting on new { Value = ct.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                join cb in db.User on ct.CreatedBy equals cb.Id
                join ub in db.User on ct.UpdatedBy equals ub.Id
                select new Country
                {
                    Id = ct.Id,
                    Code = ct.Code,
                    Name = ct.Name,
                    Description = ct.Description,
                    AdminDivType = ct.AdminDivType,
                    PostalType = ct.PostalType,
                    Status = ct.Status,
                    StatusName = st.Description,
                    StatusCss = st.CssClass ?? "",
                    CreatedBy = ct.CreatedBy,
                    CreatedByName = cb.Name,
                    CreatedAt = ct.CreatedAt,
                    UpdatedBy = ct.UpdatedBy,
                    UpdatedByName = ub.Name,
                    UpdatedAt = ct.UpdatedAt,
                    IsEdit = ct.Status == App.Status.Enable ? true : false,
                    IsDuplicate = true,
                    IsDelete = ct.Status == App.Status.Enable ? true : false,
                    IsEnable = ct.Status == App.Status.Delete ? true : false,
                }
            ).ToList();
            return Country;
        }
        public Message Print(Country obj, User User) 
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
        public Message Export(Country obj, User User)
        {
            Message objMsg = new Message();
            try 
            {
                //Get Company
                var objCompany = db.Company.FirstOrDefault(pt => App.ActiveStatus.Contains(pt.Status) && pt.Id == User.CompanyId);
                if(objCompany == null)
                {
                    Message.Error(ref objMsg, "Company did not find for export.");
                    return objMsg;
                }
                //Get Country Group
                var Country = List(obj, User).Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.Name,
                    x.Description,
                    x.AdminDivType,
                    x.PostalType,
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
                DataTable objDataTable = Util.ListToDataTable(Country);
                //Convert Datatable to base64                               
                objCompany.SheetName = "Country List";
                objCompany.ReportDesc = $"Country - {DateTime.Now.ToString("dd-MMM-yyyy")}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Add(Country obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                //Sanitize Input
                obj.Code = Util.SanitizeInput(obj.Code, null) ?? "";
                obj.Name = Util.SanitizeInput(obj.Name, null) ?? "";
                obj.Description = Util.SanitizeInput(obj.Description, null) ?? "";
                var duplicate = db.Country.FirstOrDefault(ap => App.ActiveStatus.Contains(ap.Status) && (ap.Code == obj.Code || ap.Name == obj.Name || ap.Description == obj.Description));
                if(duplicate != null)
                {
                    if(duplicate.Code == obj.Code)
                        Message.Duplicate(ref objMsg, $"Country Code : {obj.Code} already exists.");
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"Country Name : {obj.Name} already exists.");
                    if (duplicate.Description == obj.Description)
                        Message.Duplicate(ref objMsg, $"Country Description : {obj.Description} already exists.");
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
        public Message Edit(Country obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {                                
                var Country = List(obj, User).FirstOrDefault();
                if (Country == null) 
                {
                    Message.Error(ref objMsg, "Country did not find for edit.");
                    return objMsg;
                }
                objMsg.obj = Country;  
                objMsg.data = GetAddOption().data;
                Message.Success(ref objMsg, "Record found.");
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Update(Country obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                obj.Name = Util.SanitizeInput(obj.Name, App.Regexp.AlphaNum) ?? "";
                var dbCountry = db.Country.Where(ap => App.ActiveStatus.Contains(ap.Status)).ToList();
                var duplicate = db.Country.FirstOrDefault(ap => App.ActiveStatus.Contains(ap.Status) && (ap.Code == obj.Code || ap.Name == obj.Name || ap.Description == obj.Description) && ap.Id != obj.Id);
                if (duplicate != null)
                {
                    if (duplicate.Code == obj.Code)
                        Message.Duplicate(ref objMsg, $"Country Code : {obj.Code} already exists.");
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"Country Name : {obj.Name} already exists.");
                    if (duplicate.Description == obj.Description)
                        Message.Duplicate(ref objMsg, $"Country Description : {obj.Description} already exists.");
                    return objMsg;
                }
                
                var UpdateCountry = dbCountry.Where(fm => fm.Id == obj.Id).FirstOrDefault();
                if (UpdateCountry == null)
                {
                    Message.Error(ref objMsg, "Country did not find for update.");
                    return objMsg;
                }
                UpdateCountry.Code = obj.Code;
                UpdateCountry.Name = obj.Name;
                UpdateCountry.Description = obj.Description;
                UpdateCountry.AdminDivType = obj.AdminDivType;
                UpdateCountry.PostalType = obj.PostalType;
                UpdateCountry.UpdatedBy = User.Id;
                UpdateCountry.UpdatedAt = DateTime.Now;
                db.Update(UpdateCountry);
                Message.Update(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success) 
                {
                    obj.ListId.Add(UpdateCountry.Id);
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
        public Message Delete(Country obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                var DeleteCountry = db.Country.FirstOrDefault(ap => App.ActiveStatus.Contains(ap.Status) && ap.Id == obj.Id);
                if (DeleteCountry == null)
                {
                    Message.Error(ref objMsg, "Country did not find for delete.");
                    return objMsg;
                }
                DeleteCountry.Status = App.Status.Delete;
                DeleteCountry.UpdatedBy = User.Id;
                DeleteCountry.UpdatedAt = DateTime.Now;
                db.Update(DeleteCountry);
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
        public Message Enable(Country obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var EnableCountry = db.Country.FirstOrDefault(ap => App.Status.Delete == ap.Status && ap.Id == obj.Id);
                if (EnableCountry == null)
                {
                    Message.Error(ref objMsg, "Country did not find for enable.");
                    return objMsg;
                }
                EnableCountry.Status = App.Status.Enable;
                EnableCountry.UpdatedBy = User.Id;
                EnableCountry.UpdatedAt = DateTime.Now;
                db.Update(EnableCountry);
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
