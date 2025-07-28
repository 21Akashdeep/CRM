using CRMApi.Models;
using CRMApi.Services;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Identity.Client;
using System.Data;
using System.Dynamic;
using static QRCoder.PayloadGenerator.SwissQrCode;

namespace CRMApi.Repository
{
    public class RepoCompany
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoCompany(DBCRM _db) 
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
                    from ac in db.Company
                    join st in db.Setting on new { Value = ac.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                    group st by new { st.Value, st.Description } into st
                    select new
                    {
                        st.Key.Value,
                        st.Key.Description
                    }
                ).ToList();
                Option.Company = db.Company.Where(ag => App.ActiveStatus.Contains(ag.Status)).Select(ag => new
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
                Option.AdminDiv = db.AdminDiv.Where(ag => App.ActiveStatus.Contains(ag.Status)).Select(ag => new
                {
                    ag.Id,
                    ag.Code,
                    ag.Name,
                    ag.Description,
                }).ToList();
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
        public List<Company> List(Company? obj, User User) 
        {
            obj = obj == null ? new Company() : obj;
            obj.ListStatus = obj.ListStatus.Count == 0 ? App.ActiveStatus : obj.ListStatus;
            //Get Company List
            var dbCompany = db.Company.Where(x => obj.ListStatus.Contains(x.Status)).AsQueryable();
            dbCompany = obj.ListId.Count == 0 ? dbCompany : dbCompany.Where(x => obj.ListId.Contains(x.Id));
            
            //Company List
            var Company = (
                from co in dbCompany
                join ad in db.AdminDiv on co.AdminDivId equals ad.Id
                join cn in db.Country on co.CountryId equals cn.Id
                join st in db.Setting on new { Value = co.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                join cb in db.User on co.CreatedBy equals cb.Id
                join ub in db.User on co.UpdatedBy equals ub.Id
                select new Company
                {
                    Id = co.Id,
                    Code = co.Code,
                    Name = co.Name,
                    Description = co.Description,
                    CinNo = co.CinNo,
                    GstNo = co.GstNo,
                    PanNo = co.PanNo,
                    Address1 = co.Address1,
                    Address2 = co.Address2,
                    PinCode = co.PinCode,
                    PostOffice = co.PostOffice,
                    District = co.District,
                    AdminDivId = co.AdminDivId,
                    AdminDivDesc = ad.Description,
                    CountryId = co.CountryId,                   
                    CountryDesc = cn.Description,
                    ContactNo = co.ContactNo,
                    Email = co.Email,
                    AccountNo = co.AccountNo,
                    IfscCode = co.IfscCode,
                    BankName = co.BankName,
                    BankAddress = co.BankAddress,
                    Status = co.Status,
                    StatusName = st.Description,
                    StatusCss = st.CssClass ?? "",
                    CreatedBy = co.CreatedBy,
                    CreatedByName = cb.Name,
                    CreatedAt = co.CreatedAt,
                    UpdatedBy = co.UpdatedBy,
                    UpdatedByName = ub.Name,
                    UpdatedAt = co.UpdatedAt,
                    IsEdit = co.Status == App.Status.Enable ? true : false,
                    IsDuplicate = true,
                    IsDelete = co.Status == App.Status.Enable ? true : false,
                    IsEnable = co.Status == App.Status.Delete ? true : false,
                }
            ).ToList();
            return Company;
        }
        public Message Print(Company obj, User User) 
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
        public Message Export(Company obj, User User)
        {
            Message objMsg = new Message();
            try 
            {
                //Get Company
                var objCompany = db.Company.FirstOrDefault(pt => App.ActiveStatus.Contains(pt.Status));
                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Company Info did found");
                    return objMsg;
                }
                //Get Company For Export
                var Company = List(obj, User).Select(co => new
                {
                    co.Id,
                    co.Code,
                    co.Name,
                    co.Description,
                    co.CinNo,
                    co.GstNo,
                    co.PanNo,
                    co.Address1,
                    co.Address2,
                    co.PinCode,
                    co.PostOffice,
                    co.District,
                    StateId = co.AdminDivId,
                    State = co.AdminDivDesc,
                    co.CountryId,
                    Country = co.CountryDesc,
                    co.ContactNo,
                    co.Email,
                    co.AccountNo,
                    co.IfscCode,
                    co.BankName,
                    co.BankAddress,
                    co.StatusName,                    
                    co.CreatedBy,
                    co.CreatedByName,
                    co.CreatedAt,
                    co.UpdatedBy,
                    co.UpdatedByName,
                    co.UpdatedAt,                    
                }).ToList();
                //Convert List To DataTable
                DataTable objDataTable = Util.ListToDataTable(Company);
                //Convert Datatable to base64
                objCompany.SheetName = "Company List";
                objCompany.ReportDesc = $"Company - {DateTime.Now.ToString("dd-MMM-yyyy")}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Add(Company obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                //Sanitize Input
                obj.Code = Util.SanitizeInput(obj.Code, null) ?? "";
                obj.Name = Util.SanitizeInput(obj.Name, null) ?? "";
                obj.Description = Util.SanitizeInput(obj.Description, null) ?? "";
                obj.CinNo = Util.SanitizeInput(obj.CinNo, null);
                obj.GstNo = Util.SanitizeInput(obj.GstNo, null);
                obj.PanNo = Util.SanitizeInput(obj.PanNo, null);
                obj.Address1 = Util.SanitizeInput(obj.Address1, null);
                obj.Address2 = Util.SanitizeInput(obj.Address2, null);
                obj.PinCode = Util.SanitizeInput(obj.PinCode, App.Regexp.Num);
                obj.PostOffice = Util.SanitizeInput(obj.PostOffice, null);
                obj.District = Util.SanitizeInput(obj.District, null);
                var duplicate = db.Company.FirstOrDefault(ap => App.ActiveStatus.Contains(ap.Status) && (ap.Code == obj.Code || ap.Name == obj.Name || ap.Description == obj.Description));
                if(duplicate != null)
                {
                    if(duplicate.Code == obj.Code)
                        Message.Duplicate(ref objMsg, $"Company Code : {obj.Code} already exists.");
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"Company Name : {obj.Name} already exists.");
                    if (duplicate.Description == obj.Description)
                        Message.Duplicate(ref objMsg, $"Company Description : {obj.Description} already exists.");
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
        public Message Edit(Company obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {                                
                var Company = List(obj, User).FirstOrDefault();
                if (Company == null) 
                {
                    Message.Error(ref objMsg, "Company did not find for edit.");
                    return objMsg;
                }
                objMsg.obj = Company;                
                objMsg.data = GetAddOption().data;
                Message.Success(ref objMsg, "Record found.");
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Update(Company obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                obj.Name = Util.SanitizeInput(obj.Name, App.Regexp.AlphaNum) ?? "";
                var dbCompany = db.Company.Where(ap => App.ActiveStatus.Contains(ap.Status)).ToList();
                var duplicate = db.Company.FirstOrDefault(ap => App.ActiveStatus.Contains(ap.Status) && (ap.Code == obj.Code || ap.Name == obj.Name || ap.Description == obj.Description) && ap.Id != obj.Id);
                if (duplicate != null)
                {
                    if (duplicate.Code == obj.Code)
                        Message.Duplicate(ref objMsg, $"Company Code : {obj.Code} already exists.");
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"Company Name : {obj.Name} already exists.");
                    if (duplicate.Description == obj.Description)
                        Message.Duplicate(ref objMsg, $"Company Description : {obj.Description} already exists.");
                    return objMsg;
                }
                
                var UpdateCompany = dbCompany.Where(fm => fm.Id == obj.Id).FirstOrDefault();
                if (UpdateCompany == null)
                {
                    Message.Error(ref objMsg, "Company did not find for update.");
                    return objMsg;
                }                
                UpdateCompany.Name = obj.Name;
                UpdateCompany.Description = obj.Description;
                UpdateCompany.CinNo = obj.CinNo;
                UpdateCompany.GstNo = obj.GstNo;
                UpdateCompany.PanNo = obj.PanNo;
                UpdateCompany.Address1 = obj.Address1;
                UpdateCompany.Address2 = obj.Address2;
                UpdateCompany.PinCode = obj.PinCode;
                UpdateCompany.PostOffice = obj.PostOffice;
                UpdateCompany.District = obj.District;
                UpdateCompany.AdminDivId = obj.AdminDivId;
                UpdateCompany.CountryId = obj.CountryId;
                UpdateCompany.ContactNo = obj.ContactNo;
                UpdateCompany.Email = obj.Email;
                UpdateCompany.AccountNo = obj.AccountNo;
                UpdateCompany.IfscCode = obj.IfscCode;
                UpdateCompany.BankName = obj.BankName;
                UpdateCompany.BankAddress = obj.BankAddress;
                UpdateCompany.CountryId = obj.CountryId;
                UpdateCompany.UpdatedBy = User.Id;
                UpdateCompany.UpdatedAt = DateTime.Now;
                db.Update(UpdateCompany);
                Message.Update(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success) 
                {
                    obj.ListId.Add(UpdateCompany.Id);
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
        public Message Delete(Company obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                var DeleteCompany = db.Company.FirstOrDefault(ap => App.ActiveStatus.Contains(ap.Status) && ap.Id == obj.Id);
                if (DeleteCompany == null)
                {
                    Message.Error(ref objMsg, "Company did not find for delete.");
                    return objMsg;
                }
                DeleteCompany.Status = App.Status.Delete;
                DeleteCompany.UpdatedBy = User.Id;
                DeleteCompany.UpdatedAt = DateTime.Now;
                db.Update(DeleteCompany);
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
        public Message Enable(Company obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var EnableCompany = db.Company.FirstOrDefault(ap => App.Status.Delete == ap.Status && ap.Id == obj.Id);
                if (EnableCompany == null)
                {
                    Message.Error(ref objMsg, "Company did not find for enable.");
                    return objMsg;
                }
                EnableCompany.Status = App.Status.Enable;
                EnableCompany.UpdatedBy = User.Id;
                EnableCompany.UpdatedAt = DateTime.Now;
                db.Update(EnableCompany);
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
