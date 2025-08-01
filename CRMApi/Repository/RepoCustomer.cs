using CRMApi.Models;
using CRMApi.Services;
using System.Data;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoCustomer
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoCustomer(DBCRM _db) 
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
                    from ac in db.Customer
                    join st in db.Setting on new { Value = ac.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                    group st by new { st.Value, st.Description } into st
                    select new
                    {
                        st.Key.Value,
                        st.Key.Description
                    }
                ).ToList();
                Option.Customer = db.Customer.Where(ag => App.ActiveStatus.Contains(ag.Status)).Select(ag => new
                {
                    ag.Id,
                    ag.Code,
                    ag.Name,
                    ag.Description,                    
                }).ToList();
                Option.Location = db.Location.Where(loc => App.ActiveStatus.Contains(loc.Status)).Select(loc => new
                {
                    loc.Id,
                    loc.Description
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
                Option.Location = db.Location.Where(loc => App.ActiveStatus.Contains(loc.Status)).Select(loc => new
                {
                    loc.Id, 
                    loc.Description
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
        public List<Customer> List(Customer? obj, User User) 
        {
            obj = obj == null ? new Customer() : obj;
            obj.ListStatus = obj.ListStatus.Count == 0 ? App.ActiveStatus : obj.ListStatus;
            //Get Customer List
            var dbCustomer = db.Customer.Where(x => obj.ListStatus.Contains(x.Status)).AsQueryable();
            dbCustomer = obj.ListId.Count == 0 ? dbCustomer : dbCustomer.Where(x => obj.ListId.Contains(x.Id));
            
            //Customer List
            var Customer = (
                from co in dbCustomer
                join ad in db.AdminDiv on co.AdminDivId equals ad.Id
                join cn in db.Country on co.CountryId equals cn.Id                
                join Loc in db.Location on co.LocationId equals Loc.Id
                join st in db.Setting on new { Value = co.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                join cb in db.User on co.CreatedBy equals cb.Id
                join ub in db.User on co.UpdatedBy equals ub.Id
                select new Customer
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
                    LocationId = co.LocationId,
                    LocationDesc = Loc.Description,
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
            return Customer;
        }
        public Message Print(Customer obj, User User) 
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
        public Message Export(Customer obj, User User)
        {
            Message objMsg = new Message();
            try 
            {
                //Get Company
                var objCompany = db.Company.FirstOrDefault(pt => App.ActiveStatus.Contains(pt.Status));
                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Customer Info did found");
                    return objMsg;
                }
                //Get Customer For Export
                var Customer = List(obj, User).Select(co => new
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
                DataTable objDataTable = Util.ListToDataTable(Customer);
                //Convert Datatable to base64
                objCompany.SheetName = "Customer List";
                objCompany.ReportDesc = $"Customer - {DateTime.Now.ToString("dd-MMM-yyyy")}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Add(Customer obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                //Sanitize Input
                obj.Code = Util.SanitizeInput(obj.Code, null) ?? "";
                obj.Name = Util.SanitizeInput(obj.Name, null) ?? "";
                obj.Description = Util.SanitizeInput(obj.Description, null) ?? "";
                obj.CinNo = Util.SanitizeInput(obj.CinNo, null) ?? "";
                obj.GstNo = Util.SanitizeInput(obj.GstNo, null) ?? "";
                obj.PanNo = Util.SanitizeInput(obj.PanNo, null);
                obj.Address1 = Util.SanitizeInput(obj.Address1, null);
                obj.Address2 = Util.SanitizeInput(obj.Address2, null);
                obj.PinCode = Util.SanitizeInput(obj.PinCode, App.Regexp.Num);
                obj.PostOffice = Util.SanitizeInput(obj.PostOffice, null);
                obj.District = Util.SanitizeInput(obj.District, null);
                var duplicate = db.Customer.FirstOrDefault(ap => App.ActiveStatus.Contains(ap.Status) && (ap.Code == obj.Code || ap.Name == obj.Name || ap.Description == obj.Description));
                if(duplicate != null)
                {
                    if(duplicate.Code == obj.Code)
                        Message.Duplicate(ref objMsg, $"Customer Code : {obj.Code} already exists.");
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"Customer Name : {obj.Name} already exists.");
                    if (duplicate.Description == obj.Description)
                        Message.Duplicate(ref objMsg, $"Customer Description : {obj.Description} already exists.");
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
        public Message Edit(Customer obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {                                
                var Customer = List(obj, User).FirstOrDefault();
                if (Customer == null) 
                {
                    Message.Error(ref objMsg, "Customer did not find for edit.");
                    return objMsg;
                }
                objMsg.obj = Customer;                
                objMsg.data = GetAddOption().data;
                Message.Success(ref objMsg, "Record found.");                
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Update(Customer obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                obj.Name = Util.SanitizeInput(obj.Name, null) ?? "";
                var dbCustomer = db.Customer.Where(ap => App.ActiveStatus.Contains(ap.Status)).ToList();
                var duplicate = db.Customer.FirstOrDefault(ap => App.ActiveStatus.Contains(ap.Status) && (ap.Code == obj.Code || ap.Name == obj.Name || ap.Description == obj.Description) && ap.Id != obj.Id);
                if (duplicate != null)
                {
                    if (duplicate.Code == obj.Code)
                        Message.Duplicate(ref objMsg, $"Customer Code : {obj.Code} already exists.");
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"Customer Name : {obj.Name} already exists.");
                    if (duplicate.Description == obj.Description)
                        Message.Duplicate(ref objMsg, $"Customer Description : {obj.Description} already exists.");
                    return objMsg;
                }
                
                var UpdateCustomer = dbCustomer.Where(fm => fm.Id == obj.Id).FirstOrDefault();
                if (UpdateCustomer == null)
                {
                    Message.Error(ref objMsg, "Customer did not find for update.");
                    return objMsg;
                }
                UpdateCustomer.Code = String.IsNullOrEmpty(obj.Code) ? UpdateCustomer.Code : obj.Code;
                UpdateCustomer.Name = obj.Name;
                UpdateCustomer.Description = obj.Description;                
                UpdateCustomer.CinNo = obj.CinNo;
                UpdateCustomer.GstNo = obj.GstNo;
                UpdateCustomer.PanNo = obj.PanNo;
                UpdateCustomer.Address1 = obj.Address1;
                UpdateCustomer.Address2 = obj.Address2;
                UpdateCustomer.PinCode = obj.PinCode;
                UpdateCustomer.PostOffice = obj.PostOffice;
                UpdateCustomer.District = obj.District;
                UpdateCustomer.AdminDivId = obj.AdminDivId;
                UpdateCustomer.CountryId = obj.CountryId;
                UpdateCustomer.ContactNo = obj.ContactNo;
                UpdateCustomer.Email = obj.Email;
                UpdateCustomer.AccountNo = obj.AccountNo;
                UpdateCustomer.IfscCode = obj.IfscCode;
                UpdateCustomer.BankName = obj.BankName;
                UpdateCustomer.BankAddress = obj.BankAddress;
                UpdateCustomer.LocationId = obj.LocationId;
                UpdateCustomer.UpdatedBy = User.Id;
                UpdateCustomer.UpdatedAt = DateTime.Now;
                db.Update(UpdateCustomer);
                Message.Update(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success) 
                {
                    obj.ListId.Add(UpdateCustomer.Id);
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
        public Message Delete(Customer obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                var DeleteCustomer = db.Customer.FirstOrDefault(ap => App.ActiveStatus.Contains(ap.Status) && ap.Id == obj.Id);
                if (DeleteCustomer == null)
                {
                    Message.Error(ref objMsg, "Customer did not find for delete.");
                    return objMsg;
                }
                DeleteCustomer.Status = App.Status.Delete;
                DeleteCustomer.UpdatedBy = User.Id;
                DeleteCustomer.UpdatedAt = DateTime.Now;
                db.Update(DeleteCustomer);
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
        public Message Enable(Customer obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var EnableCustomer = db.Customer.FirstOrDefault(ap => App.Status.Delete == ap.Status && ap.Id == obj.Id);
                if (EnableCustomer == null)
                {
                    Message.Error(ref objMsg, "Customer did not find for enable.");
                    return objMsg;
                }
                EnableCustomer.Status = App.Status.Enable;
                EnableCustomer.UpdatedBy = User.Id;
                EnableCustomer.UpdatedAt = DateTime.Now;
                db.Update(EnableCustomer);
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
