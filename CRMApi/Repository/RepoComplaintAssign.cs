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
    public class RepoComplaintAssign
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        private readonly RepoComplaint RepoComplaint;
        public RepoComplaintAssign(DBCRM _db)
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
                Option.Status = await (
                    from ca in db.ComplaintAssign
                    join st in db.Setting on new { Name = App.SettingName.Status, Value = ca.Status.ToString() } equals new { st.Name, st.Value }
                    group st by new { st.Description, st.Value } into Setting
                    select new
                    {
                        Setting.Key.Value,
                        Setting.Key.Description
                    }
                ).ToListAsync();
                var ListStatus = new List<int>() { App.Status.Pending, App.Status.Scheduled, App.Status.Processing, App.Status.Completed };
                Option.Complaint = await db.Complaint.Where(x => ListStatus.Contains(x.Status)).Select(x => new
                {
                    ComplaintId = x.Id,
                    ComplaintNo = x.Code
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
        public async Task<Message> GetComplaintAsync(Complaint? obj, User User) 
        {
            Message objMsg = new Message();
            try
            {
                obj ??= new Complaint();
                obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : new List<int>() { App.Status.Pending };
                obj.ListForwardTo = User.UserType == App.UserType.SysAdmin || User.UserType == App.UserType.Admin ? new List<int>() : new List<int>() { User.Id };
                var dbComplaint = await RepoComplaint.ListAsync(obj, User);
                objMsg.data = dbComplaint.Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.CustomerDesc,
                    x.CustomerAddress,
                    x.Problem,
                });
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> GetUserAsync(Complaint obj)
        {
            Message objMsg = new Message();
            try
            {
                obj.ListStatus = new List<int>() { App.Status.Pending, App.Status.Scheduled };
                var dbComplaint = await db.Complaint.FirstOrDefaultAsync(x => x.Id == obj.Id && obj.ListStatus.Contains(x.Status));
                if (dbComplaint == null)
                {
                    Message.Error(ref objMsg, "Complaint did not find for get user list.");
                    return objMsg;
                }
                var AssignUser = await (
                    from cs in db.ComplaintAssign
                    join u in db.User on cs.AssignTo equals u.Id
                    where App.ActiveStatus.Contains(cs.Status) && cs.ComplaintId == obj.Id
                    select new
                    {
                        cs.Id,
                        cs.ComplaintId,
                        cs.AssignTo,
                        AssignToName = u.Name,
                        IsSelected = true
                    }
                ).ToListAsync();
                var AssignUserId = AssignUser.Select(x => x.AssignTo).ToList();

                var NotAssignUser = await db.User.Where(x => App.ActiveStatus.Contains(x.Status) && x.DepartmentId == dbComplaint.DepartmentId && !AssignUserId.Contains(x.Id)).Select(x => new
                {
                    Id = 0,
                    ComplaintId = dbComplaint.Id,
                    AssignTo = x.Id,
                    AssignToName = x.Name,
                    IsSelected = false
                }).ToListAsync();
                objMsg.data = AssignUser.Union(NotAssignUser).ToList();
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<List<ComplaintAssign>> ListAsync(ComplaintAssign? obj, User User) 
        {
            obj ??= new ComplaintAssign();
            obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : App.ActiveStatus;
            var dbComplaintAssignQuery = db.ComplaintAssign.Where(cs => obj.ListStatus.Contains(cs.Status)).AsQueryable();            
            dbComplaintAssignQuery = obj.ListId.Any() ? dbComplaintAssignQuery.Where(cs => obj.ListId.Contains(cs.Id)) : dbComplaintAssignQuery;
            dbComplaintAssignQuery = obj.ListComplaintId.Any() ? dbComplaintAssignQuery.Where(cs => obj.ListComplaintId.Contains(cs.ComplaintId)) : dbComplaintAssignQuery;
            dbComplaintAssignQuery = obj.ListAssignToId.Any() ? dbComplaintAssignQuery.Where(cs => obj.ListAssignToId.Contains(cs.AssignTo)) : dbComplaintAssignQuery;

            var dbComplaintQuery = db.Complaint.Where(co => App.AllActiveStatus.Contains(co.Status)).AsQueryable();
            dbComplaintQuery = obj.ListForwardToId.Any() ? dbComplaintQuery.Where(co => obj.ListForwardToId.Contains(co.ForwardTo ?? 0)) : dbComplaintQuery;
            dbComplaintQuery = dbComplaintQuery.Where(co => obj.FromDate == DateTime.MinValue || co.Date.Date >= obj.FromDate.Date);
            dbComplaintQuery = dbComplaintQuery.Where(co => obj.ToDate == DateTime.MinValue || co.Date.Date <= obj.ToDate.Date);
                            
            var dbComplaintAssign = await (
                from cs in dbComplaintAssignQuery
                join st in db.Setting on new { Name = App.SettingName.Status, Value = cs.Status.ToString() } equals new { st.Name, st.Value }
                join co in dbComplaintQuery on cs.ComplaintId equals co.Id
                join cu in db.Customer on co.CustomerId equals cu.Id
                join ad in db.AdminDiv on co.AdminDivId equals ad.Id
                join ct in db.Country on ad.CountryId equals ct.Id
                join ab in db.User on co.ForwardTo equals ab.Id
                join at in db.User on cs.AssignTo equals at.Id
                join cb in db.User on cs.CreatedBy equals cb.Id
                join ub in db.User on cs.UpdatedBy equals ub.Id                
                select new ComplaintAssign
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
                    StartDateTime = co.StartDateTime,
                    EndDateTime = co.EndDateTime,
                    AssignBy = ab.Name,
                    AssignTo = cs.AssignTo,
                    AssignToName = at.Name,
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
                    IsEdit = cs.Status == App.Status.Enable ? true : false,
                    IsDelete = cs.Status == App.Status.Enable ? true : false,
                }
            ).ToListAsync();
            
            return dbComplaintAssign;
        }
        public async Task<Message> GetAsync(ComplaintAssign obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                obj.ListForwardToId = App.ByPassUserType.Contains(User.UserType) ? new List<int>() : new List<int>() { User.Id };
                objMsg.data = await ListAsync(obj, User);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> PrintAsync(ComplaintAssign obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                obj.ListForwardToId = App.ByPassUserType.Contains(User.UserType) ? new List<int>() : new List<int>() { User.Id };
                objMsg.data = await ListAsync(obj, User);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> ExportAsync(ComplaintAssign obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                obj.ListForwardToId = App.ByPassUserType.Contains(User.UserType) ? new List<int>() : new List<int>() { User.Id };
                var dbComplaintAssign = await ListAsync(obj, User);
                var ComplaintAssign = dbComplaintAssign.Select(cs => new 
                {                    
                    cs.ComplaintNo,
                    cs.ComplaintDate,                    
                    Customer = cs.CustomerDesc,
                    Address = cs.CustomerAddress,
                    cs.Problem,  
                    cs.StartDateTime,
                    cs.EndDateTime,
                    cs.AssignBy,
                    AssignTo = cs.AssignToName,
                    cs.Remarks,
                    Status = cs.StatusDesc,
                    CreatedBy = cs.CreatedByName,
                    cs.CreatedAt,
                }).ToList();

                DataTable objDataTable = Util.ListToDataTable(ComplaintAssign);

                var objCompany = await db.Company.FirstOrDefaultAsync(pt => App.ActiveStatus.Contains(pt.Status));
                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Company Info did found");
                    return objMsg;
                }

                objCompany.SheetName = "Complaint Schedule List";
                objCompany.ReportDesc = $"Complaint Schedule List\rGenerated On - {DateTime.Now.ToString("dd-MMM-yyyy HH:mm")}";
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
        public async Task<Message> AddAsync(Complaint obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                if (!obj.AssignTo.Any())
                {
                    Message.Error(ref objMsg, "No service engineer found to assign");
                    return objMsg;
                }
                var UpdateComplaint = await db.Complaint.FirstOrDefaultAsync(x => x.Id == obj.Id);
                if (UpdateComplaint == null)
                {
                    Message.Error(ref objMsg, "Complaint did not find for Assign to service eng.");
                    return objMsg;
                }
                UpdateComplaint.StartDateTime = obj.StartDateTime;
                UpdateComplaint.EndDateTime = obj.EndDateTime;
                UpdateComplaint.Status = App.Status.Scheduled;
                UpdateComplaint.UpdatedBy = User.Id;
                UpdateComplaint.UpdatedAt = DateTime.Now;
                //Delete Complaint Assign
                var DeleteComplaintAssign = await db.ComplaintAssign.Where(x => x.ComplaintId == obj.Id && App.ActiveStatus.Contains(x.Status)).ToListAsync();
                if (DeleteComplaintAssign.Any())
                {
                    //Update Complaint Assign Status to Delete
                    foreach (var item in DeleteComplaintAssign)
                    {
                        item.Status = App.Status.Delete;
                        item.UpdatedBy = User.Id;
                        item.UpdatedAt = DateTime.Now;
                        db.Update(item);
                    }
                }
                //Add Complaint Assign                                
                obj.AssignTo.ForEach(x =>
                {
                    x.Id = 0;                    
                    x.Status = App.Status.Enable;
                    x.CreatedBy = User.Id;
                    x.CreatedAt = DateTime.Now;
                    x.UpdatedBy = User.Id;
                    x.UpdatedAt = DateTime.Now;
                });
                db.AddRange(obj.AssignTo);
                int isSaved = await db.SaveChangesAsync();
                Message.Add(ref objMsg, isSaved, "Complaint assigned to service eng successfully.");
                if (objMsg.status == Message.Type.success)
                {
                    dynamic data = new ExpandoObject();
                    var Complaint = await GetComplaintAsync(null, User);
                    data.Complaint = Complaint.data;
                    data.ComplaintAssign = await ListAsync(new ComplaintAssign { ListId = obj.AssignTo.Select(x => x.Id).ToList() }, User);
                    objMsg.data = data;
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> EditAsync(int ComplaintId, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                Complaint obj = new Complaint();
                obj.Id = ComplaintId;
                obj.ListId.Add(ComplaintId);
                obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : new List<int>() { App.Status.Pending, App.Status.Scheduled };
                obj.ListForwardTo = App.ByPassUserType.Contains(User.UserType) ? new List<int>() : new List<int>() { User.Id };
                var dbComplaint = await RepoComplaint.ListAsync(obj, User);
                dynamic data = new ExpandoObject();
                data.Complaint = dbComplaint.Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.CustomerDesc,
                    x.CustomerAddress,
                    x.Problem,
                }).ToList();
                var AssignedUser = await GetUserAsync(obj);
                data.AssignedUser = AssignedUser.data;
                objMsg.data = data;
                Message.Success(ref objMsg, "");
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
                var DeleteComplaintAssign = await db.ComplaintAssign.FirstOrDefaultAsync(x => App.ActiveStatus.Contains(x.Status) && x.Id == Id);
                if (DeleteComplaintAssign == null) 
                {
                    Message.Error(ref objMsg, "Complaint Schedule did not find for delete.");
                    return objMsg;
                }

                //Update Complaint (Rollback Complaint Status Schedule To Pending)
                var data = db.ComplaintAssign.Where(x => App.ActiveStatus.Contains(x.Status) && x.ComplaintId == DeleteComplaintAssign.ComplaintId).ToList();
                if (db.ComplaintAssign.Where(x => App.ActiveStatus.Contains(x.Status) && x.ComplaintId == DeleteComplaintAssign.ComplaintId).Count() <= 1) 
                {
                    var dbComplaint = await db.Complaint.FirstOrDefaultAsync(x => x.Status == App.Status.Scheduled && x.Id == DeleteComplaintAssign.ComplaintId);
                    if (dbComplaint != null)
                    {
                        dbComplaint.Status = App.Status.Pending;
                        dbComplaint.UpdatedBy = User.Id;
                        dbComplaint.UpdatedAt = DateTime.Now;
                        db.Update(dbComplaint);
                    }                    
                }

                //Delete Complaint Schedule                                 
                DeleteComplaintAssign.Status = App.Status.Delete;
                DeleteComplaintAssign.UpdatedBy = User.Id;
                DeleteComplaintAssign.UpdatedAt = DateTime.Now;
                db.Update(DeleteComplaintAssign);
                int isSaved = await db.SaveChangesAsync();

                Message.Delete(ref objMsg, isSaved, "");
                if (objMsg.status == Message.Type.success)
                {
                    //Get Delete Compliant Schedule
                    DeleteComplaintAssign.ListId.Add(DeleteComplaintAssign.Id);
                    DeleteComplaintAssign.ListStatus.Add(App.Status.Delete);
                    DeleteComplaintAssign.ListForwardToId = App.ByPassUserType.Contains(User.UserType) ? new List<int>() : new List<int>() { User.Id };
                    var ComplaintAssign = await ListAsync(DeleteComplaintAssign, User);
                    objMsg.obj = ComplaintAssign.FirstOrDefault();
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
