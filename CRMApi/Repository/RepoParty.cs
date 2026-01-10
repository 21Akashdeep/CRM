using CRMApi.Models;
using CRMApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoParty
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoParty(DBCRM _db) 
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
                    from ac in db.Party
                    join st in db.Setting on new { Value = ac.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                    group st by new { st.Value, st.Description } into st
                    select new
                    {
                        st.Key.Value,
                        st.Key.Description
                    }
                ).ToListAsync();
                Option.Party = await db.Party.Where(ag => App.ActiveStatus.Contains(ag.Status)).Select(ag => new
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
                Option.AccountGroup = await db.AccountGroup.Where(ag => App.ActiveStatus.Contains(ag.Status)
                 && new[] { 17, 18 }.Contains(ag.Id))
                .Select(ag => new
                {
                    ag.Id,
                    ag.Description,
                })
                  .ToListAsync();
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
                Option.AccountGroup = await db.AccountGroup.Where(ag => App.ActiveStatus.Contains(ag.Status)
                  && new[] { 17, 18 }.Contains(ag.Id))
                 .Select(ag => new
                {
                    ag.Id,
                    ag.Description,
                   })
                   .ToListAsync();

                objMsg.data = Option;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<List<Party>> ListAsync(Party? obj, User User) 
        {
            obj = obj == null ? new Party() : obj;
            obj.ListStatus = obj.ListStatus.Count == 0 ? App.ActiveStatus : obj.ListStatus;
            //Get Party List
            var dbParty = db.Party.Where(x => obj.ListStatus.Contains(x.Status)).AsQueryable();
            if (obj.ListId.Any())
                dbParty = dbParty.Where(x => obj.ListId.Contains(x.Id));
            if (obj.ListLocationId.Any())
                dbParty = dbParty.Where(x => obj.ListLocationId.Contains(x.Id));
            if (obj.ListAccountGroupId.Any())
                dbParty = dbParty.Where(x => obj.ListAccountGroupId.Contains(x.AccountGroupId));

            //Party List
            var Party = await (
                from co in dbParty
                join ad in db.AdminDiv on co.AdminDivId equals ad.Id
                join cn in db.Country on co.CountryId equals cn.Id                             
                join lo in db.Location on co.LocationId equals lo.Id
                join ag in db.AccountGroup on co.AccountGroupId equals ag.Id
                join st in db.Setting on new { Value = co.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                join cb in db.User on co.CreatedBy equals cb.Id
                join ub in db.User on co.UpdatedBy equals ub.Id                
                select new Party
                {
                    Id = co.Id,
                    Code = co.Code,
                    Name = co.Name,
                    Description = co.Description,
                    AccountGroupId=co.AccountGroupId,
                    AccountGroupDesc=ag.Description,
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
            return Party;
        }
        public async Task<Message> PrintAsync(Party obj, User User) 
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
        public async Task<Message> ExportAsync(Party obj, User User)
        {
            Message objMsg = new Message();
            try 
            {
                //Get Company
                var objCompany = await db.Company.FirstOrDefaultAsync(pt => App.ActiveStatus.Contains(pt.Status));
                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Party Info did found");
                    return objMsg;
                }
                //Get Party For Export
                var Party = (await ListAsync(obj, User)).Select(co => new
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
                DataTable objDataTable = Util.ListToDataTable(Party);
                //Convert Datatable to base64
                objCompany.SheetName = "Party List";
                objCompany.ReportDesc = $"Party - {DateTime.Now.ToString("dd-MMM-yyyy")}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> AddAsync(Party obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                //Sanitize Input
                obj.Code = Util.SanitizeInput(obj.Code, null) ?? "";
                obj.Name = Util.SanitizeInput(obj.Name, null) ?? "";
                obj.Description = Util.SanitizeInput(obj.Description, null) ?? "";
                //Check duplicate
                var duplicate = await db.Party.FirstOrDefaultAsync(ap => App.ActiveStatus.Contains(ap.Status) && (ap.Code == obj.Code || ap.Name == obj.Name || ap.Description == obj.Description));
                if(duplicate != null)
                {
                    if(duplicate.Code == obj.Code)
                        Message.Duplicate(ref objMsg, $"Party Code : {obj.Code} already exists.");
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"Party Name : {obj.Name} already exists.");
                    if (duplicate.Description == obj.Description)
                        Message.Duplicate(ref objMsg, $"Party Description : {obj.Description} already exists.");
                    return objMsg;
                }
                //Add Party
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
        public async Task<Message> EditAsync(Party obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {                                
                var Party = (await ListAsync(obj, User)).FirstOrDefault();
                if (Party == null) 
                {
                    Message.Error(ref objMsg, "Party did not find for edit.");
                    return objMsg;
                }
                objMsg.obj = Party;                
                objMsg.data = (await GetAddOptionAsync()).data;
                Message.Success(ref objMsg, "Record found.");                
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> UpdateAsync(Party obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                //Sanitize Input
                obj.Code = Util.SanitizeInput(obj.Code, null) ?? "";
                obj.Name = Util.SanitizeInput(obj.Name, null) ?? "";
                obj.Description = Util.SanitizeInput(obj.Description, null) ?? "";
                //Get Party List
                var dbParty = await db.Party.Where(ap => App.ActiveStatus.Contains(ap.Status)).ToListAsync();
                //Check duplicate
                var duplicate = dbParty.FirstOrDefault(ap => App.ActiveStatus.Contains(ap.Status) && (ap.Code == obj.Code || ap.Name == obj.Name || ap.Description == obj.Description) && ap.Id != obj.Id);
                if (duplicate != null)
                {
                    if (duplicate.Code == obj.Code)
                        Message.Duplicate(ref objMsg, $"Party Code : {obj.Code} already exists.");
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"Party Name : {obj.Name} already exists.");
                    if (duplicate.Description == obj.Description)
                        Message.Duplicate(ref objMsg, $"Party Description : {obj.Description} already exists.");
                    return objMsg;
                }

                var UpdateParty = dbParty.FirstOrDefault(fm => fm.Id == obj.Id);
                if (UpdateParty == null)
                {
                    Message.Error(ref objMsg, "Party did not find for update.");
                    return objMsg;
                }
                UpdateParty.Code = String.IsNullOrEmpty(obj.Code) ? UpdateParty.Code : obj.Code;
                UpdateParty.Name = obj.Name;
                UpdateParty.Description = obj.Description;
                UpdateParty.AccountGroupId = obj.AccountGroupId;
                UpdateParty.CinNo = obj.CinNo;
                UpdateParty.GstNo = obj.GstNo;
                UpdateParty.PanNo = obj.PanNo;
                UpdateParty.Address1 = obj.Address1;
                UpdateParty.Address2 = obj.Address2;
                UpdateParty.PinCode = obj.PinCode;
                UpdateParty.PostOffice = obj.PostOffice;
                UpdateParty.District = obj.District;
                UpdateParty.AdminDivId = obj.AdminDivId;
                UpdateParty.CountryId = obj.CountryId;
                UpdateParty.ContactNo = obj.ContactNo;
                UpdateParty.Email = obj.Email;
                UpdateParty.AccountNo = obj.AccountNo;
                UpdateParty.IfscCode = obj.IfscCode;
                UpdateParty.BankName = obj.BankName;
                UpdateParty.BankAddress = obj.BankAddress;
                UpdateParty.LocationId = obj.LocationId;
                UpdateParty.CountryId = obj.CountryId;
                UpdateParty.UpdatedBy = User.Id;
                UpdateParty.UpdatedAt = DateTime.Now;
                db.Update(UpdateParty);
                Message.Update(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success) 
                {
                    obj.ListId.Add(UpdateParty.Id);
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
        public async Task<Message> DeleteAsync(Party obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                var DeleteParty = await db.Party.FirstOrDefaultAsync(ap => App.ActiveStatus.Contains(ap.Status) && ap.Id == obj.Id);
                if (DeleteParty == null)
                {
                    Message.Error(ref objMsg, "Party did not find for delete.");
                    return objMsg;
                }
                DeleteParty.Status = App.Status.Delete;
                DeleteParty.UpdatedBy = User.Id;
                DeleteParty.UpdatedAt = DateTime.Now;
                db.Update(DeleteParty);
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
        public async Task<Message> EnableAsync(Party obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var EnableParty = await db.Party.FirstOrDefaultAsync(ap => App.Status.Delete == ap.Status && ap.Id == obj.Id);
                if (EnableParty == null)
                {
                    Message.Error(ref objMsg, "Party did not find for enable.");
                    return objMsg;
                }
                EnableParty.Status = App.Status.Enable;
                EnableParty.UpdatedBy = User.Id;
                EnableParty.UpdatedAt = DateTime.Now;
                db.Update(EnableParty);
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
