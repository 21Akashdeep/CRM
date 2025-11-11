using CRMApi.Models;
using CRMApi.Services;
using Microsoft.EntityFrameworkCore;
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
        public async Task<Message> GetViewOptionAsync() 
        {
            Message objMsg = new Message();
            try 
            {
                dynamic Option = new ExpandoObject();
                Option.Status = await (
                    from ac in db.Customer
                    join st in db.Setting on new { Value = ac.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                    group st by new { st.Value, st.Description } into st
                    select new
                    {
                        st.Key.Value,
                        st.Key.Description
                    }
                ).ToListAsync();
                Option.Customer = await db.Customer.Where(ag => App.ActiveStatus.Contains(ag.Status)).Select(ag => new
                {
                    ag.Id,
                    ag.Code,
                    ag.Name,
                    ag.Description,                    
                }).ToListAsync();
                Option.Location = await db.Location.Where(ag => App.ActiveStatus.Contains(ag.Status)).Select(ag => new
                {
                    ag.Id,
                    ag.Code,
                    ag.Name,
                    ag.Description,
                }).ToListAsync();
                objMsg.data = Option;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> GetAddOptionAsync()
        {
            Message objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();
                Option.AdminDiv = await db.AdminDiv.Where(ag => App.ActiveStatus.Contains(ag.Status)).Select(ag => new
                {
                    ag.Id,
                    ag.Code,
                    ag.Name,
                    ag.Description,
                }).ToListAsync();
                Option.Country = await db.Country.Where(ag => App.ActiveStatus.Contains(ag.Status)).Select(ag => new
                {
                    ag.Id,
                    ag.Code,
                    ag.Name,
                    ag.Description,
                    ag.AdminDivType,
                    ag.PostalType
                }).ToListAsync();
                Option.Location = await db.Location.Where(ag => App.ActiveStatus.Contains(ag.Status)).Select(ag => new
                {
                    ag.Id,
                    ag.Code,
                    ag.Name,
                    ag.Description,
                }).ToListAsync();
                objMsg.data = Option;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<List<Customer>> ListAsync(Customer? obj, User User) 
        {
            obj = obj == null ? new Customer() : obj;
            obj.ListStatus = obj.ListStatus.Count == 0 ? App.ActiveStatus : obj.ListStatus;
            //Get Customer List
            var dbCustomer = db.Customer.Where(x => obj.ListStatus.Contains(x.Status)).AsQueryable();
            if (obj.ListId.Any())
                dbCustomer = dbCustomer.Where(x => obj.ListId.Contains(x.Id));
            if (obj.ListLocationId.Any())
                dbCustomer = dbCustomer.Where(x => obj.ListLocationId.Contains(x.Id));

            //Customer List
            var Customer = await (
                from co in dbCustomer
                join ad in db.AdminDiv on co.AdminDivId equals ad.Id
                join cn in db.Country on co.CountryId equals cn.Id                             
                join lo in db.Location on co.LocationId equals lo.Id
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
                    ContactNo = co.ContactNo,
                    Email = co.Email,
                    AccountNo = co.AccountNo,
                    IfscCode = co.IfscCode,
                    BankName = co.BankName,
                    BankAddress = co.BankAddress,                    
                    LocationId = co.LocationId,
                    LocationDesc = lo.Description,
                    Status = co.Status,
                    StatusDesc = st.Description,
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
            ).ToListAsync();
            return Customer;
        }
        public async Task<Message> PrintAsync(Customer obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                objMsg.data = await ListAsync(obj, User);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> ExportAsync(Customer obj, User User)
        {
            Message objMsg = new Message();
            try 
            {
                //Get Company
                var objCompany = await db.Company.FirstOrDefaultAsync(pt => App.ActiveStatus.Contains(pt.Status));
                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Customer Info did found");
                    return objMsg;
                }
                //Get Customer For Export
                var Customer = (await ListAsync(obj, User)).Select(co => new
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
                    State = co.AdminDivDesc,
                    Country = co.CountryDesc,
                    co.ContactNo,
                    co.Email,
                    co.AccountNo,
                    co.IfscCode,
                    co.BankName,
                    co.BankAddress,
                    Location = co.LocationDesc,
                    co.StatusDesc,                                        
                    co.CreatedByName,
                    co.CreatedAt,                    
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
        public async Task<Message> AddAsync(Customer obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                //Sanitize Input
                obj.Code = Util.SanitizeInput(obj.Code, null) ?? "";
                obj.Name = Util.SanitizeInput(obj.Name, null) ?? "";
                obj.Description = Util.SanitizeInput(obj.Description, null) ?? "";
                //Check duplicate
                var duplicate = await db.Customer.FirstOrDefaultAsync(ap => App.ActiveStatus.Contains(ap.Status) && (ap.Code == obj.Code || ap.Name == obj.Name || ap.Description == obj.Description));
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
                //Add Customer
                obj.CreatedBy = User.Id;
                obj.UpdatedBy = User.Id;
                db.Add(obj);
                Message.Add(ref objMsg, (await db.SaveChangesAsync()), "");                
                if (objMsg.status == Message.Type.success) 
                {
                    db.Entry(obj).Reload();                    
                    obj.ListId.Add(obj.Id);
                    objMsg.obj = (await ListAsync(obj, User)).FirstOrDefault();
                    objMsg.data = (await GetViewOptionAsync()).data;
                }                    
            } 
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> EditAsync(Customer obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {                                
                var Customer = (await ListAsync(obj, User)).FirstOrDefault();
                if (Customer == null) 
                {
                    Message.Error(ref objMsg, "Customer did not find for edit.");
                    return objMsg;
                }
                objMsg.obj = Customer;                
                objMsg.data = (await GetAddOptionAsync()).data;
                Message.Success(ref objMsg, "Record found.");                
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> UpdateAsync(Customer obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                //Sanitize Input
                obj.Code = Util.SanitizeInput(obj.Code, null) ?? "";
                obj.Name = Util.SanitizeInput(obj.Name, null) ?? "";
                obj.Description = Util.SanitizeInput(obj.Description, null) ?? "";
                //Get Customer List
                var dbCustomer = await db.Customer.Where(ap => App.ActiveStatus.Contains(ap.Status)).ToListAsync();
                //Check duplicate
                var duplicate = dbCustomer.FirstOrDefault(ap => App.ActiveStatus.Contains(ap.Status) && (ap.Code == obj.Code || ap.Name == obj.Name || ap.Description == obj.Description) && ap.Id != obj.Id);
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

                var UpdateCustomer = dbCustomer.FirstOrDefault(fm => fm.Id == obj.Id);
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
                UpdateCustomer.CountryId = obj.CountryId;
                UpdateCustomer.UpdatedBy = User.Id;
                UpdateCustomer.UpdatedAt = DateTime.Now;
                db.Update(UpdateCustomer);
                Message.Update(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success) 
                {
                    obj.ListId.Add(UpdateCustomer.Id);
                    objMsg.obj = (await ListAsync(obj, User)).FirstOrDefault();
                    objMsg.data = (await GetViewOptionAsync()).data;
                }                   
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> DeleteAsync(Customer obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                var DeleteCustomer = await db.Customer.FirstOrDefaultAsync(ap => App.ActiveStatus.Contains(ap.Status) && ap.Id == obj.Id);
                if (DeleteCustomer == null)
                {
                    Message.Error(ref objMsg, "Customer did not find for delete.");
                    return objMsg;
                }
                DeleteCustomer.Status = App.Status.Delete;
                DeleteCustomer.UpdatedBy = User.Id;
                DeleteCustomer.UpdatedAt = DateTime.Now;
                db.Update(DeleteCustomer);
                Message.Delete(ref objMsg, (await db.SaveChangesAsync()), "");
                if (objMsg.status == Message.Type.success)
                {
                    obj.ListId.Add(obj.Id);
                    obj.ListStatus.Add(App.Status.Delete);
                    objMsg.obj = (await ListAsync(obj, User)).FirstOrDefault();
                    objMsg.data = (await GetViewOptionAsync()).data;
                }
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> EnableAsync(Customer obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var EnableCustomer = await db.Customer.FirstOrDefaultAsync(ap => App.Status.Delete == ap.Status && ap.Id == obj.Id);
                if (EnableCustomer == null)
                {
                    Message.Error(ref objMsg, "Customer did not find for enable.");
                    return objMsg;
                }
                EnableCustomer.Status = App.Status.Enable;
                EnableCustomer.UpdatedBy = User.Id;
                EnableCustomer.UpdatedAt = DateTime.Now;
                db.Update(EnableCustomer);
                Message.Delete(ref objMsg, (await db.SaveChangesAsync()), "");
                if (objMsg.status == Message.Type.success)
                {
                    obj.ListId.Add(obj.Id);                    
                    objMsg.obj = (await ListAsync(obj, User)).FirstOrDefault();
                    objMsg.data = (await GetViewOptionAsync()).data;
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
