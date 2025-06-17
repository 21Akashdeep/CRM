using CRMApi.Models;
using CRMApi.Services;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Data;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoComplaint
    {
        private readonly DbCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoComplaint(DbCRM _db)
        {
            db = _db;
        }
        public async Task<Message> GetViewOptionAsync() 
        {
            Message objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();
                var ListStatus = await db.Complaint.GroupBy(x => new { x.Status }).Select(x => x.Key.Status.ToString()).ToListAsync();
                Option.Status = await db.Setting.Where(st => App.ActiveStatus.Contains(st.Status) && st.Name == App.SettingName.Status && ListStatus.Contains(st.Value)).Select(st => new
                {
                    st.Value,
                    st.Description
                }).ToListAsync();                
                Option.Complaint = await db.Complaint.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.Code
                }).ToListAsync();
                Option.Customer = await db.Customer.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.Description,
                    SubText = Util.AddressDesc(new Composite.AddressDesc { Add1 = x.Address1, Add2 = x.Address2, PinCode = x.PinCode, PostOffice = x.PostOffice, OtherText = x.GstNo })
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
        public async Task<Message> GetAddOptionAsync(Complaint obj)
        {
            Message objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();
                Option.SupportMode = await db.Setting.Where(x => App.ActiveStatus.Contains(x.Status) && x.Name == App.SettingName.SupportMode).Select(x => new
                {
                    Id = x.Value,
                    x.Description
                }).ToListAsync();
                Option.Customer = await db.Customer.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.Description,
                    x.Address1,
                    x.Address2,
                    x.PinCode,
                    x.PostOffice,
                    x.District,
                    x.AdminDivId,
                    x.CountryId,
                    SubText = Util.AddressDesc(new Composite.AddressDesc { Add1 = x.Address1, Add2 = x.Address2, PinCode = x.PinCode, PostOffice = x.PostOffice, OtherText = x.GstNo })
                }).ToListAsync();
                Option.AdminDiv = await db.AdminDiv.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.Description
                }).ToListAsync();
                Option.Country = await db.Country.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.Description
                }).ToListAsync();
                Option.Department = await db.Department.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.Description
                }).ToListAsync();                
                Option.Priority = await db.Setting.Where(x => App.ActiveStatus.Contains(x.Status) && x.Name == App.SettingName.Priority).Select(x => new
                {
                    Id = x.Value,
                    x.Description
                }).ToListAsync();
                var ForwardTo = await (
                    from usr in db.User
                    join dpt in db.Department on usr.DepartmentId equals dpt.Id
                    join deg in db.Designation on usr.DesignationId equals deg.Id
                    where App.ActiveStatus.Contains(usr.Status) && usr.DepartmentId == obj.DepartmentId
                    select new
                    {
                        usr.Id,
                        usr.Name,
                        DepartmentDesc = dpt.Description,
                        DesignationDesc = deg.Description,
                        usr.ApprovalRole,
                        SubText = $"{dpt.Code} - {dpt.Description}<br>{deg.Code} - {deg.Description}"
                    }
                ).ToListAsync();                
                Option.ForwardTo = ForwardTo.Where(x => x.ApprovalRole.Any(x => x.ApprovalRoleDesc == App.AprvRole.Supervisor)).ToList();
                objMsg.data = Option;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }        
        public async Task<List<Complaint>> ListAsync(Complaint? obj, User User) 
        {
            //filter Complaint data
            obj = obj == null ? new Complaint() : obj;
            obj.ListStatus = obj.ListStatus.Count == 0 ? new List<int> { App.Status.SaveAsDraft, App.Status.Pending, App.Status.Scheduled, App.Status.Processing } : obj.ListStatus;
            var dbComplaintQuery = db.Complaint.AsQueryable();

            dbComplaintQuery = db.Complaint.Where(co => obj.ListStatus.Contains(co.Status) && co.CompanyId == User.CompanyId).AsQueryable();
            
            if (obj.FromDate != default(DateTime)) 
                dbComplaintQuery = dbComplaintQuery.Where(co => co.Date.Date >= obj.FromDate.Date);
            if (obj.ToDate != default(DateTime)) 
                dbComplaintQuery = dbComplaintQuery.Where(co => co.Date.Date <= obj.ToDate.Date);
            if (obj.ListId.Any()) 
                dbComplaintQuery = dbComplaintQuery.Where(co => obj.ListId.Contains(co.Id));
            if (obj.ListSupportMode.Any()) 
                dbComplaintQuery = dbComplaintQuery.Where(co => obj.ListSupportMode.Contains(co.SupportMode));
            if (obj.ListCustomerId.Any()) 
                dbComplaintQuery = dbComplaintQuery.Where(co => obj.ListCustomerId.Contains(co.CustomerId));
            if (obj.ListDeparmentId.Any()) 
                dbComplaintQuery = dbComplaintQuery.Where(co => obj.ListDeparmentId.Contains(co.DepartmentId));
            if (obj.ListPriority.Any()) 
                dbComplaintQuery = dbComplaintQuery.Where(co => obj.ListPriority.Contains(co.Priority));
            if (obj.ListForwardTo.Any()) 
                dbComplaintQuery = dbComplaintQuery.Where(co => obj.ListForwardTo.Contains(co.ForwardTo ?? 0));

            //Get Complaint 
            var dbComplaint = await dbComplaintQuery.ToListAsync();

            //Get Assing Complaint
            var dbComplaintId = dbComplaint.Select(x => x.Id).ToList();
            
            var dbComplaintAssign = await (
                from ca in db.ComplaintAssign
                join at in db.User on ca.AssignTo equals at.Id
                where App.ActiveStatus.Contains(ca.Status) &&  dbComplaintId.Contains(ca.ComplaintId)
                select new ComplaintAssign
                {
                    Id = ca.Id,
                    ComplaintId = ca.ComplaintId,
                    AssignTo = ca.AssignTo,
                    AssignToName = at.Name,
                    Status = ca.Status,
                    CreatedBy = ca.CreatedBy,
                    CreatedAt = ca.CreatedAt,
                    UpdatedBy = ca.UpdatedBy,
                    UpdatedAt = ca.UpdatedAt
                }
            ).ToListAsync();

            //Get Complaint List
            var Complaint = (
                from co in dbComplaint
                join sm in db.Setting on new { Name = App.SettingName.SupportMode, Value = co.SupportMode.ToString() } equals new { sm.Name, sm.Value }
                join cu in db.Customer on co.CustomerId equals cu.Id
                join ad in db.AdminDiv on co.AdminDivId equals ad.Id
                join cn in db.Country on co.CountryId equals cn.Id
                join de in db.Department on co.DepartmentId equals de.Id
                join pr in db.Setting on new { Name = App.SettingName.Priority, Value = co.Priority.ToString() } equals new { pr.Name, pr.Value }
                join st in db.Setting on new { Name = App.SettingName.Status, Value = co.Status.ToString() } equals new { st.Name, st.Value }
                join at in db.User on co.ForwardTo equals at.Id into dbForwardTo
                from at in dbForwardTo.DefaultIfEmpty()
                join cb in db.User on co.CreatedBy equals cb.Id
                join ub in db.User on co.UpdatedBy equals ub.Id
                select new Complaint
                {
                    Id = co.Id,
                    Code = co.Code,
                    Date = co.Date,
                    SupportMode = co.SupportMode,
                    SupportModeDesc = sm.Description,
                    CustomerId = co.CustomerId,
                    CustomerDesc = cu.Description,
                    Address1 = co.Address1,
                    Address2 = co.Address2,
                    PinCode = co.PinCode,
                    PostOffice = co.PostOffice,
                    District = co.District,
                    AdminDivId = co.AdminDivId,
                    AdminDivDesc = ad.Description,
                    CountryId = co.CountryId,
                    CountryDesc = cn.Description,
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
                    ContactNo = co.ContactNo,
                    Email = co.Email,
                    DepartmentId = co.DepartmentId,
                    DepartmentDesc = de.Description,
                    ForwardTo = co.ForwardTo,
                    ForwardToName = at?.Name,
                    Priority = co.Priority,
                    PriorityDesc = pr.Description,
                    Problem = co.Problem,                    
                    AssignTo = dbComplaintAssign.Where(ca=> ca.ComplaintId == co.Id).ToList(),
                    CompanyId = co.CompanyId,
                    Status = co.Status,
                    StatusDesc = st.Description,
                    StatusCss = st.CssClass,
                    CreatedBy = co.CreatedBy,
                    CreatedByName = cb.Name,
                    CreatedAt = co.CreatedAt,
                    UpdatedBy = co.UpdatedBy,
                    UpdatedByName = ub.Name,
                    UpdatedAt = co.UpdatedAt,                    
                    IsEdit = co.Status == App.Status.SaveAsDraft || co.Status == App.Status.Pending || App.ByPassUserType.Contains(User.UserType) ? true : false,
                    IsDelete = co.Status == App.Status.SaveAsDraft || co.Status == App.Status.Pending || App.ByPassUserType.Contains(User.UserType) ? true : false,
                    IsDuplicate = co.Status != App.Status.Delete ? true : false,
                    IsClose = App.ByPassUserType.Contains(User.UserType) ? true : false,
                    IsAddStatus = co.Status == App.Status.Scheduled || co.Status == App.Status.Processing ? true : false,
                }
            ).ToList();
            return Complaint;
        }        
        private List<Complaint> ListSample(Complaint? obj, User User, int page = 1, int pageSize = 50, string sortBy = "Date", bool sortDesc = true, bool exportAll = false)
        {
            obj ??= new Complaint();
            obj.ListStatus = obj.ListStatus.Count == 0 ? App.ActiveStatus : obj.ListStatus;

            var query = db.Complaint.Where(co => obj.ListStatus.Contains(co.Status));

            if (obj.FromDate != default)
                query = query.Where(co => co.Date.Date >= obj.FromDate.Date);

            if (obj.ToDate != default)
                query = query.Where(co => co.Date.Date <= obj.ToDate.Date);

            if (obj.ListId.Any())
                query = query.Where(co => obj.ListId.Contains(co.Id));

            if (obj.ListSupportMode.Any())
                query = query.Where(co => obj.ListSupportMode.Contains(co.SupportMode));

            if (obj.ListCustomerId.Any())
                query = query.Where(co => obj.ListCustomerId.Contains(co.CustomerId));

            if (obj.ListDeparmentId.Any())
                query = query.Where(co => obj.ListDeparmentId.Contains(co.DepartmentId));

            if (obj.ListPriority.Any())
                query = query.Where(co => obj.ListPriority.Contains(co.Priority));

            var dbComplaint = query.ToList();
            

            var Complaint = (
                from co in dbComplaint
                join sm in db.Setting on new { Name = App.SettingName.SupportMode, Value = co.SupportMode.ToString() } equals new { sm.Name, sm.Value }
                join cu in db.Customer on co.CustomerId equals cu.Id
                join ad in db.AdminDiv on co.AdminDivId equals ad.Id
                join cn in db.Country on co.CountryId equals cn.Id
                join de in db.Department on co.DepartmentId equals de.Id
                join pr in db.Setting on new { Name = App.SettingName.Priority, Value = co.Priority.ToString() } equals new { pr.Name, pr.Value }
                join st in db.Setting on new { Name = App.SettingName.Status, Value = co.Status.ToString() } equals new { st.Name, st.Value }
                join cb in db.User on co.CreatedBy equals cb.Id
                join ub in db.User on co.UpdatedBy equals ub.Id
                select new Complaint
                {
                    Id = co.Id,
                    Code = co.Code,
                    Date = co.Date,
                    SupportMode = co.SupportMode,
                    SupportModeDesc = sm.Description,
                    CustomerId = co.CustomerId,
                    CustomerDesc = cu.Description,
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
                    DepartmentId = co.DepartmentId,
                    DepartmentDesc = de.Description,
                    ForwardTo = co.ForwardTo,
                    Priority = co.Priority,
                    PriorityDesc = pr.Description,
                    CompanyId = co.CompanyId,
                    Status = co.Status,
                    StatusDesc = st.Description,
                    StatusCss = st.CssClass,
                    CreatedBy = co.CreatedBy,
                    CreatedByName = cb.Name,
                    CreatedAt = co.CreatedAt,
                    UpdatedBy = co.UpdatedBy,
                    UpdatedByName = ub.Name,
                    UpdatedAt = co.UpdatedAt,                    
                }
            ).AsQueryable();

            // Sorting
            Complaint = sortDesc
                ? Complaint.OrderByDescending(x => EF.Property<object>(x, sortBy))
                : Complaint.OrderBy(x => EF.Property<object>(x, sortBy));

            // Pagination
            if (!exportAll)
                Complaint = Complaint.Skip((page - 1) * pageSize).Take(pageSize);

            return Complaint.ToList();
        }
        public async Task<Message> GetAsync(Complaint obj, User User) 
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
        public async Task<Message> PrintAsync(Complaint obj, User User)
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
        public async Task<Message> ExportAsync(Complaint obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var dbComplaint = await ListAsync(obj, User);
                var Complaint = dbComplaint.Select(co => new
                {
                    co.Date,
                    ComplaintNo = co.Code,
                    SupportMode = co.SupportModeDesc,
                    Customer = co.CustomerDesc,
                    co.Address1,
                    co.Address2,
                    co.PinCode,
                    co.PostOffice,
                    State = co.AdminDivDesc,
                    Country = co.CountryDesc,
                    co.ContactNo,
                    co.Email,
                    Priority = co.PriorityDesc,
                    ForwardTo = co.ForwardToName,                    
                }).ToList();
                
                DataTable objDataTable = Util.ListToDataTable(Complaint);

                var objCompany = await db.Company.FirstOrDefaultAsync(pt => App.ActiveStatus.Contains(pt.Status) && pt.Id == User.CompanyId);
                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Company Info did found");
                    return objMsg;
                }
                
                objCompany.SheetName = "Complaint Item Wise List";
                objCompany.ReportDesc = $"Complaint Item Wise List Generated On - {DateTime.Now.ToString("dd-MMM-yyyy HH:mm")}";
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
                if (!String.IsNullOrEmpty(obj.ContactNo) && obj.ContactNo.Length != 10) 
                {
                    Message.Error(ref objMsg, "Contact No. should be 10 digit");
                    return objMsg;
                }
                obj.CompanyId = User.CompanyId;
                obj.Status = obj.ForwardTo == null ? App.Status.SaveAsDraft : App.Status.Pending;
                obj.CreatedBy = User.Id;
                obj.UpdatedBy = User.Id;               
                db.Add(obj);
                int isSaved = await db.SaveChangesAsync();
                Message.Add(ref objMsg, isSaved, "");
                if (objMsg.status == Message.Type.success) 
                {
                    //Reload Object & Assing Id
                    await db.Entry(obj).ReloadAsync();
                    obj.ListId.Add(obj.Id);
                    if (obj.ForwardTo != null)
                    {
                        obj.ListStatus.Add(App.Status.Pending);
                        obj.ListStatus.AddRange(App.ActiveStatus);
                    }                    
                    var Complaints = await ListAsync(obj, User);                                        
                    var ViewOption = await GetViewOptionAsync();
                    
                    objMsg.obj = Complaints.FirstOrDefault();
                    objMsg.data = ViewOption.data;
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
                Complaint? obj = new Complaint();
                obj.ListId.Add(Id);
                obj.ListStatus.AddRange(App.AllActiveStatus);
                var dbComplaint = await ListAsync(obj, User);
                obj = dbComplaint.FirstOrDefault();
                if (obj == null) 
                {
                    Message.Error(ref objMsg, "Complaint did not find for edit.");
                    return objMsg;
                }
                var AddOption = await GetAddOptionAsync(obj);
                objMsg.obj = obj;
                objMsg.data = AddOption.data;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> UpdateAsync(Complaint obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var UpdateComplaint = await db.Complaint.FindAsync(obj.Id);
                if (UpdateComplaint == null)
                {
                    Message.Error(ref objMsg, "Complaint did not find for update.");
                    return objMsg;
                }                
                UpdateComplaint.Date = obj.Date;
                UpdateComplaint.SupportMode = obj.SupportMode;
                UpdateComplaint.CustomerId = obj.CustomerId;
                UpdateComplaint.Address1 = obj.Address1;
                UpdateComplaint.Address2 = obj.Address2;
                UpdateComplaint.PinCode = obj.PinCode;
                UpdateComplaint.PostOffice = obj.PostOffice;
                UpdateComplaint.District = obj.District;
                UpdateComplaint.AdminDivId = obj.AdminDivId;
                UpdateComplaint.CountryId = obj.CountryId;
                UpdateComplaint.ContactNo = obj.ContactNo;
                UpdateComplaint.Email = obj.Email;
                UpdateComplaint.DepartmentId = obj.DepartmentId;
                UpdateComplaint.ForwardTo = obj.ForwardTo;
                UpdateComplaint.Priority = obj.Priority;                
                UpdateComplaint.Problem = obj.Problem;
                UpdateComplaint.Status = obj.ForwardTo == null ? App.Status.SaveAsDraft : App.Status.Pending;
                UpdateComplaint.UpdatedBy = User.Id;
                UpdateComplaint.UpdatedAt = DateTime.Now;
                db.Update(UpdateComplaint);

                int isSaveChanges = await db.SaveChangesAsync();
                Message.Update(ref objMsg, isSaveChanges, "");
                if (objMsg.status == Message.Type.success)
                {                    
                    obj.ListId.Add(obj.Id);
                    if (obj.ForwardTo != null) 
                    {
                        obj.ListStatus.Add(App.Status.Pending);
                        obj.ListStatus.AddRange(App.ActiveStatus);
                    }
                    //Get Complaint
                    var Complaints = await ListAsync(obj, User);
                    objMsg.obj = Complaints.FirstOrDefault();
                    
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
                var DeleteComplaint = await db.Complaint.FindAsync(Id);
                if (DeleteComplaint == null)
                {
                    Message.Error(ref objMsg, "Complaint did not find for update.");
                    return objMsg;
                }
                DeleteComplaint.Status = App.Status.Delete;
                DeleteComplaint.UpdatedBy = User.Id;
                DeleteComplaint.UpdatedAt = DateTime.Now;               
                db.Update(DeleteComplaint);
                //Delete Complaint Assign
                var DeleteComplaintAssign = await db.ComplaintAssign.Where(x => x.ComplaintId == Id).ToListAsync();
                if (DeleteComplaintAssign.Any())
                {
                    foreach (var item in DeleteComplaintAssign)
                    {
                        item.Status = App.Status.Delete;
                        item.UpdatedBy = User.Id;
                        item.UpdatedAt = DateTime.Now;
                        db.Update(item);
                    }
                }
                //Delete Complaint Status
                var DeleteComplaintStatus = await db.ComplaintStatus.Where(x => x.ComplaintId == Id).ToListAsync();
                if (DeleteComplaintStatus.Any())
                {
                    foreach (var item in DeleteComplaintStatus)
                    {
                        item.Status = App.Status.Delete;
                        item.UpdatedBy = User.Id;
                        item.UpdatedAt = DateTime.Now;
                        db.Update(item);
                    }
                }
                int isSaveChanges = await db.SaveChangesAsync();
                Message.Delete(ref objMsg, isSaveChanges, "");
                if (objMsg.status == Message.Type.success)
                {
                    DeleteComplaint.ListId.Add(Id);
                    DeleteComplaint.ListStatus.Add(App.Status.Delete);
                    //Get Complaint
                    var Complaints = await ListAsync(DeleteComplaint, User);
                    objMsg.obj = Complaints.FirstOrDefault();
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
