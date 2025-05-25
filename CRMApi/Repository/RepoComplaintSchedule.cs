using CRMApi.Models;
using CRMApi.Services;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Oracle.EntityFrameworkCore.Storage.Internal;
using System.Data;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoComplaintSchedule
    {
        private readonly DbCRM db;
        private readonly AppSetting App = Util.AppSetting;
        private readonly RepoComplaint RepoComplaint;
        public RepoComplaintSchedule(DbCRM _db)
        {
            db = _db;
            RepoComplaint = new RepoComplaint(db);
        }
        public async Task<Message> GetViewOptionAsync()
        {
            Message objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();
                var dbComplaintSchedule = await (
                    from cs in db.ComplaintSchedule
                    join st in db.Setting on new { Name = App.SettingName.Status, Value = cs.Status.ToString() } equals new { st.Name, st.Value }
                    join co in db.Complaint on cs.ComplaintId equals co.Id
                    select new 
                    {
                        cs.Id,
                        cs.ComplaintId,
                        ComplaintNo = co.Code,
                        ComplaintDate = co.Date,
                        cs.Status,
                        StatusName = st.Name,
                        StatusDesc = st.Description
                    }
                ).ToListAsync();
                Option.Status = dbComplaintSchedule.GroupBy(x => new { x.Status, x.StatusName, x.StatusDesc }).Select(x => new
                {
                    Value = x.Key.Status,
                    Description = x.Key.StatusDesc
                }).ToList();
                Option.Complaint = dbComplaintSchedule.GroupBy(x => new { x.ComplaintId, x.ComplaintNo }).Select(x => new
                {
                    x.Key.ComplaintId,
                    x.Key.ComplaintNo
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
        public async Task<Message> GetAddOptionAsync(Complaint? obj, User User) 
        {
            Message objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();                
                obj ??= new Complaint();
                obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : new List<int>() { App.Status.Pending };
                obj.ListAssignTo = User.UserType == App.UserType.SysAdmin || User.UserType == App.UserType.Admin ? new List<int>() : new List<int>() { User.Id };
                var dbComplaint = await RepoComplaint.ListAsync(obj, User);                
                Option.Complaint = dbComplaint.Select(co => new
                {
                    co.Id,
                    co.Code,
                    co.CustomerDesc,
                    co.DepartmentId,
                    co.DepartmentDesc,
                    CustomerAddress = Util.AddressDesc(new Composite.AddressDesc
                    {
                        Add1 = co.Address1,
                        Add2 = co.Address2,
                        PinCode = co.PinCode,
                        PostOffice = co.PostOffice,
                        District = co.District,
                        State = co.AdminDivDesc,
                        Country = co.CountryDesc,
                        OtherText = co.ContactNo
                    }),
                    co.Problem,                    
                    UserList = db.User.Where(ur => App.ActiveStatus.Contains(ur.Status) && ur.DepartmentId == co.DepartmentId).Select(ur => new
                    {
                        ur.Id,
                        ur.Name,
                        IsSelected = false
                    }).ToList()
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
        public async Task<Message> GetUserAsync(int DepartmentId) 
        {
            Message objMsg = new Message();
            try 
            {
                objMsg.data = await db.User.Where(x => App.ActiveStatus.Contains(x.Status) && x.DepartmentId == DepartmentId).Select(x => new
                {
                    x.Id,
                    x.Name
                }).ToListAsync();
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<List<ComplaintSchedule>> ListAsync(ComplaintSchedule? obj, User User) 
        {
            obj ??= new ComplaintSchedule();
            obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : new List<int>() { App.Status.Scheduled };
            var dbComplaintScheduleQuery = db.ComplaintSchedule.Where(cs => obj.ListStatus.Contains(cs.Status) && cs.CompanyId == User.CompanyId).AsQueryable();            
            dbComplaintScheduleQuery = obj.ListId.Any() ? dbComplaintScheduleQuery.Where(cs => obj.ListId.Contains(cs.Id)) : dbComplaintScheduleQuery;
            dbComplaintScheduleQuery = obj.ListComplaintId.Any() ? dbComplaintScheduleQuery.Where(cs => obj.ListComplaintId.Contains(cs.ComplaintId)) : dbComplaintScheduleQuery;
            dbComplaintScheduleQuery = obj.FromDate != default ? dbComplaintScheduleQuery.Where(cs => cs.StartDateTime.Date >= obj.FromDate.Date) : dbComplaintScheduleQuery;
            dbComplaintScheduleQuery = obj.ToDate != default ? dbComplaintScheduleQuery.Where(cs => cs.StartDateTime.Date <= obj.ToDate.Date) : dbComplaintScheduleQuery;

            var dbComplaintSchedule = await dbComplaintScheduleQuery.ToListAsync();
            var dbComplaintScheduleList = (
                from cs in dbComplaintSchedule
                join st in db.Setting on new { Name = App.SettingName.Status, Value = cs.Status.ToString() } equals new { st.Name, st.Value }
                join co in db.Complaint on cs.ComplaintId equals co.Id
                join cu in db.Customer on co.CustomerId equals cu.Id
                join ad in db.AdminDiv on co.AdminDivId equals ad.Id
                join ct in db.Country on ad.CountryId equals ct.Id                                
                join cb in db.User on cs.CreatedBy equals cb.Id
                join ub in db.User on cs.UpdatedBy equals ub.Id
                select new ComplaintSchedule
                {
                    Id = cs.Id,
                    ComplaintId = cs.ComplaintId,
                    ComplaintNo = co.Code,
                    ComplaintDate = co.Date,
                    CustomerDesc = cu.Description,
                    CustomerAddress = Util.AddressDesc(new Composite.AddressDesc
                    {
                        Add1 = co.Address1,
                        Add2 = co.Address2,
                        PinCode = co.PinCode,
                        PostOffice = co.PostOffice,
                        District = co.District,
                        State = ad.Description,
                        StateCode = ad.Code,
                        Country = ct.Description,
                        OtherText = co.ContactNo
                    }),                    
                    Problem = co.Problem,
                    AssignTo = cs.AssignTo,
                    StartDateTime = cs.StartDateTime,
                    EndDateTime = cs.EndDateTime,
                    CompletedDateTime = cs.CompletedDateTime,
                    Remarks = cs.Remarks,                    
                    Status = cs.Status,
                    StatusDesc = st.Description,
                    StatusCss = st.CssClass,
                    CreatedBy = co.CreatedBy,
                    CreatedByName = cb.Name,
                    CreatedAt = co.CreatedAt,
                    UpdatedBy = co.UpdatedBy,
                    UpdatedByName = ub.Name,
                    UpdatedAt = co.UpdatedAt,
                    IsEdit = cs.Status == App.Status.Scheduled ? true : false,
                    IsDelete = cs.Status == App.Status.Scheduled ? true : false,
                }
            ).ToList();
            return dbComplaintScheduleList;
        }
        public async Task<Message> GetAsync(ComplaintSchedule obj, User User) 
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
        public async Task<Message> PrintAsync(ComplaintSchedule obj, User User) 
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
        public async Task<Message> ExportAsync(ComplaintSchedule obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var dbComplaintSchedule = await ListAsync(obj, User);
                var ComplaintSchedule = dbComplaintSchedule.Select(cs => new 
                {                    
                    cs.ComplaintNo,
                    cs.ComplaintDate,                    
                    Customer = cs.CustomerDesc,
                    Address = cs.CustomerAddress,
                    Probelm = cs.Problem,
                    cs.StartDateTime,
                    cs.EndDateTime,
                    cs.CompletedDateTime,
                    cs.Remarks,
                    AssignToUser = cs.UserName,
                    Status = cs.StatusDesc,
                    CreatedBy = cs.CreatedByName,
                    UpdatedBy = cs.UpdatedByName,
                }).ToList();

                DataTable objDataTable = Util.ListToDataTable(ComplaintSchedule);

                var objCompany = await db.Company.FirstOrDefaultAsync(pt => App.ActiveStatus.Contains(pt.Status) && pt.Id == User.CompanyId);
                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Company Info did found");
                    return objMsg;
                }

                objCompany.SheetName = "Complaint Schedule List";
                objCompany.ReportDesc = $"Complaint Schedule List, Generated On - {DateTime.Now.ToString("dd-MMM-yyyy HH:mm")}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);

                if (String.IsNullOrEmpty(objMsg.base64))
                {
                    Message.Error(ref objMsg, "Record did not find");
                }
                else
                {
                    Message.Success(ref objMsg, "Record found");
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> AddAsync(ComplaintSchedule obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                //Update Complaint 
                var dbComplaint = await db.Complaint.FirstOrDefaultAsync(x => x.Status == App.Status.Pending && x.Id == obj.ComplaintId);
                if (dbComplaint == null)
                {
                    Message.Error(ref objMsg, "Complaint did not find for update the status");
                    return objMsg;
                }
                dbComplaint.Status = App.Status.Scheduled;
                dbComplaint.UpdatedBy = User.Id;
                dbComplaint.UpdatedAt = DateTime.Now;
                db.Update(dbComplaint);
                //Add Complint Schedule
                var dbComplaintSchedule = await db.ComplaintSchedule.Where(x => App.AllActiveStatus.Contains(x.Status)).ToListAsync();
                if (dbComplaintSchedule.Any(x => x.ComplaintId == obj.ComplaintId)) 
                {
                    Message.Error(ref objMsg, "Complaint already assigned to this user.");
                    return objMsg;
                }
                obj.Status = App.Status.Scheduled;
                obj.CompanyId = User.CompanyId;
                obj.CreatedBy = User.Id;
                obj.CreatedAt = DateTime.Now;
                obj.UpdatedBy = User.Id;
                obj.UpdatedAt = DateTime.Now;
                db.Add(obj);
                int isSaved = await db.SaveChangesAsync();
                
                Message.Add(ref objMsg, isSaved, "");
                if (objMsg.status == Message.Type.success) 
                {
                    //Get Complaint Schedule
                    obj.ListId.Add(obj.Id);
                    var ComplaintSchedule = await ListAsync(obj, User);
                    objMsg.obj = ComplaintSchedule.FirstOrDefault();
                    //Get Option
                    var ViewOption = await GetViewOptionAsync();
                    var AddOption = await GetAddOptionAsync(null, User);
                    dynamic data = new ExpandoObject();
                    data.ViewOption = ViewOption.data;
                    data.AddOption = AddOption.data;
                    objMsg.data = data;
                }
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> EditAsync(int Id, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                var dbComplaintSchedule = await ListAsync(new ComplaintSchedule
                {
                    ListId = new List<int>() { Id},
                    ListStatus = new List<int> { App.Status.Scheduled}
                }, User);
                var ComplaintSchedule = dbComplaintSchedule.FirstOrDefault();
                if (ComplaintSchedule == null) 
                {
                    Message.Error(ref objMsg, "Complaint Schedule did not find for edit.");
                    return objMsg;
                }
                objMsg.obj = ComplaintSchedule;
                var AddOption = await GetAddOptionAsync(new Complaint
                {
                    ListId = new List<int> { ComplaintSchedule.ComplaintId },
                    ListStatus = new List<int> { App.Status.Scheduled }
                }, User);
                objMsg.data = AddOption.data;
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> UpdateAsync(ComplaintSchedule obj, User User)
        {
            Message objMsg = new Message();
            try
            {                
                //Updated Complaint Schedule
                var dbComplaintSchedule = await db.ComplaintSchedule.Where(x => App.AllActiveStatus.Contains(x.Status)).ToListAsync();
                if (dbComplaintSchedule.Any(x => x.ComplaintId == obj.ComplaintId && x.Id != obj.Id))
                {
                    Message.Error(ref objMsg, "Complaint already assigned to this user.");
                    return objMsg;
                }
                var UpdateComplintSchedule = dbComplaintSchedule.FirstOrDefault(x => x.Id == obj.Id);
                if (UpdateComplintSchedule == null) 
                {
                    Message.Error(ref objMsg, "Complaint Schedule did not find for edit.");
                    return objMsg;
                }
                UpdateComplintSchedule.ComplaintId = obj.ComplaintId;                
                UpdateComplintSchedule.StartDateTime = obj.StartDateTime;
                UpdateComplintSchedule.EndDateTime = obj.EndDateTime;
                UpdateComplintSchedule.AssignTo = obj.AssignTo;
                UpdateComplintSchedule.Remarks = obj.Remarks;                
                db.Update(UpdateComplintSchedule);
                int isSaved = await db.SaveChangesAsync();

                Message.Update(ref objMsg, isSaved, "");
                if (objMsg.status == Message.Type.success)
                {
                    //Get Complaint Schedule
                    obj.ListId.Add(obj.Id);
                    var ComplaintSchedule = await ListAsync(obj, User);
                    objMsg.obj = ComplaintSchedule.FirstOrDefault();
                    //Get View Option
                    var ViewOption = await GetViewOptionAsync();                    
                    objMsg.data = ViewOption.data;
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> DeleteAsync(int Id, User User)
        {
            Message objMsg = new Message();
            try
            {
                var DeleteComplaintSchedule = await db.ComplaintSchedule.FirstOrDefaultAsync(x => x.Status == App.Status.Scheduled && x.Id == Id);
                if (DeleteComplaintSchedule == null) 
                {
                    Message.Error(ref objMsg, "Complaint Schedule did not find for delete.");
                    return objMsg;
                }
                //Update Complaint (Rollback Complaint Status Schedule To Pending)
                var dbComplaint = await db.Complaint.FirstOrDefaultAsync(x => x.Status == App.Status.Scheduled && x.Id == DeleteComplaintSchedule.ComplaintId);
                if (dbComplaint == null)
                {
                    Message.Error(ref objMsg, "Complaint did not find for update the status");
                    return objMsg;
                }
                dbComplaint.Status = App.Status.Pending;
                dbComplaint.UpdatedBy = User.Id;
                dbComplaint.UpdatedAt = DateTime.Now;
                db.Update(dbComplaint);

                //Delete Complaint Schedule                                 
                DeleteComplaintSchedule.Status = App.Status.Delete;
                DeleteComplaintSchedule.UpdatedBy = User.Id;
                DeleteComplaintSchedule.UpdatedAt = DateTime.Now;
                db.Update(DeleteComplaintSchedule);
                int isSaved = await db.SaveChangesAsync();

                Message.Delete(ref objMsg, isSaved, "");
                if (objMsg.status == Message.Type.success)
                {
                    //Get Delete Compliant Schedule
                    DeleteComplaintSchedule.ListId.Add(DeleteComplaintSchedule.Id);
                    DeleteComplaintSchedule.ListStatus.Add(App.Status.Delete);
                    var ComplaintSchedule = await ListAsync(DeleteComplaintSchedule, User);
                    objMsg.obj = ComplaintSchedule.FirstOrDefault();
                    //Get View Option
                    var ViewOption = await GetViewOptionAsync();                    
                    objMsg.data = ViewOption.data;
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
