using CRMApi.Models;
using CRMApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;
using System.Net.Mail;
using System.Text;

namespace CRMApi.Repository
{
    public class RepoComplaint
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoComplaint(DBCRM _db)
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
                    x.ContactNo,
                    x.Email,
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
                Option.Department = await db.Complaint.Where(x => App.AllActiveStatus.Contains(x.Status)).GroupBy(x => new { x.Department }).Select(x => new
                {
                    Value = x.Key.Department,
                    Description = x.Key.Department
                }).ToListAsync();
                Option.Priority = await db.Setting.Where(x => App.ActiveStatus.Contains(x.Status) && x.Name == App.SettingName.Priority).Select(x => new
                {
                    Id = x.Value,
                    x.Description
                }).ToListAsync();
                var dbComplaintAssign = db.ComplaintAssign.Where(x => App.AllActiveStatus.Contains(x.Status)).AsQueryable();
                Option.ComplaintAssing = await (
                    from usr in db.User
                    join ulo in db.UserLocation on usr.Id equals ulo.UserId
                    join cus in db.Customer on ulo.LocationId equals cus.LocationId
                    join coa in dbComplaintAssign on new {UserId = usr.Id, ComplaintId = obj.Id} equals new { coa.UserId, coa.ComplaintId } into ComplaintAssign
                    from coa in ComplaintAssign.DefaultIfEmpty()
                    where App.ActiveStatus.Contains(usr.Status) && cus.Id == obj.CustomerId
                    select new
                    {
                        Id = coa != null ? coa.Id : 0,
                        ComplaintId = coa != null ? coa.ComplaintId : 0,
                        UserId = usr.Id,
                        UserName = usr.Name,
                        IsAdded = coa != null,
                    }
                ).ToListAsync();                
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
            obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : App.AllActiveStatus;
            var dbComplaintQuery = db.Complaint.Where(co => obj.ListStatus.Contains(co.Status)).AsQueryable();            
            
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
            if (obj.ListPriority.Any()) 
                dbComplaintQuery = dbComplaintQuery.Where(co => obj.ListPriority.Contains(co.Priority));
            if (obj.ListAssignTo.Any()) 
            {
                var ListAssignedComplaintId = await db.ComplaintAssign.Where(ca => App.AllActiveStatus.Contains(ca.Status) && obj.ListAssignTo.Contains(ca.UserId)).Select(ca=> ca.ComplaintId).ToListAsync();
                dbComplaintQuery = dbComplaintQuery.Where(co => ListAssignedComplaintId.Contains(co.Id));
            }
            var dbComplaint = await dbComplaintQuery.ToListAsync();
            //Get Complaint List
            var Complaint = (
                from co in dbComplaint
                join sm in db.Setting on new { Name = App.SettingName.SupportMode, Value = co.SupportMode } equals new { sm.Name, sm.Value }
                join cu in db.Customer on co.CustomerId equals cu.Id
                join lo in db.Location on cu.LocationId equals lo.Id
                join ad in db.AdminDiv on co.AdminDivId equals ad.Id
                join cn in db.Country on co.CountryId equals cn.Id                
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
                    CustomerAddress = Util.AddressDesc(new Composite.AddressDesc
                    {
                        Add1 = co.Address1,
                        Add2 = co.Address2,
                        PinCode = co.PinCode,
                        PostOffice = co.PostOffice,
                        District = co.District,
                        State = co.AdminDivDesc,
                        Country = co.CountryDesc,
                        OtherText = ""
                    }),
                    CustomerLocation = lo.Description,
                    ContactPerson = co.ContactPerson,
                    ContactNo = co.ContactNo,
                    Email = co.Email,
                    Department = co.Department,
                    Priority = co.Priority,
                    PriorityDesc = pr.Description,
                    Problem = co.Problem,
                    StartDateTime = co.StartDateTime,
                    EndDateTime = co.EndDateTime,
                    CompletedDateTime = co.CompletedDateTime,                    
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
                    IsAddStatus = co.Status == App.Status.Pending || co.Status == App.Status.Processing ? true : false,
                    IsDetailedView = true
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

            if (obj.ListPriority.Any())
                query = query.Where(co => obj.ListPriority.Contains(co.Priority));

            var dbComplaint = query.ToList();
            

            var Complaint = (
                from co in dbComplaint
                join sm in db.Setting on new { Name = App.SettingName.SupportMode, Value = co.SupportMode.ToString() } equals new { sm.Name, sm.Value }
                join cu in db.Customer on co.CustomerId equals cu.Id
                join ad in db.AdminDiv on co.AdminDivId equals ad.Id
                join cn in db.Country on co.CountryId equals cn.Id                
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
                    Department = co.Department,                    
                    Priority = co.Priority,
                    PriorityDesc = pr.Description,                    
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
                }).ToList();
                
                DataTable objDataTable = Util.ListToDataTable(Complaint);

                var objCompany = await db.Company.FirstOrDefaultAsync(pt => App.ActiveStatus.Contains(pt.Status));
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
        public async Task<Message> DetailedViewAsync(int Id, User User)
        {
            Message objMsg = new Message();
            try
            {                
                var dbComplaint = await ListAsync(new Complaint { ListId = new List<int> { Id }, ListStatus = App.AllActiveStatus }, User);
                var dbComplaintStatus = await new RepoComplaintStatus(db).ListAsync(new ComplaintStatus { ListComplaintId = new List<int> { Id } }, User);
                var dbComplaintItem = await new RepoComplaintItem(db).ListAsync(new ComplaintItem { ListComplaintId = new List<int> { Id } }, User);
                var Complaint = dbComplaint.Select(Complaint => new
                {
                    Complaint,
                    ComplaintStatus = dbComplaintStatus,
                    ComplaintItem = dbComplaintItem
                }).FirstOrDefault();
                objMsg.obj = Complaint;                
                if (Complaint != null)
                {
                    Message.Success(ref objMsg, "Record found.");
                }
                else 
                {
                    Message.Error(ref objMsg, "Record did not find.");
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
                obj.Status = App.Status.Pending;
                obj.CreatedBy = User.Id;
                obj.UpdatedBy = User.Id;
                obj.ComplaintAssign.ForEach(x =>
                {
                    x.Status = obj.Status;
                    x.CreatedBy = User.Id;
                    x.UpdatedBy = User.Id;                                            
                });
                db.Add(obj);
                Message.Add(ref objMsg, (await db.SaveChangesAsync()), "");
                if (objMsg.status == Message.Type.success) 
                {
                    //Reload Object & Assing Id
                    await db.Entry(obj).ReloadAsync();
                    obj.ListId.Add(obj.Id);                    
                    var Complaint = (await ListAsync(obj, User)).FirstOrDefault();
                    if (Complaint != null) 
                    {
                        Complaint.ComplaintAssign = obj.ComplaintAssign;
                        Message msg = await NotifyComplaintLogByEmail(Complaint, true);
                        objMsg.status = msg.status != Message.Type.success ? msg.status : objMsg.status;
                        objMsg.statusText += msg.statusText;
                    }
                    objMsg.obj = Complaint;                    
                    objMsg.data = (await GetViewOptionAsync()).data;
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
                obj = (await ListAsync(obj, User)).FirstOrDefault();                
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
                UpdateComplaint.ContactPerson = obj.ContactPerson;
                UpdateComplaint.ContactNo = obj.ContactNo;
                UpdateComplaint.Email = obj.Email;
                UpdateComplaint.Department = obj.Department;                
                UpdateComplaint.Priority = obj.Priority;                
                UpdateComplaint.Problem = obj.Problem;                
                UpdateComplaint.UpdatedBy = User.Id;
                UpdateComplaint.UpdatedAt = DateTime.Now;
                db.Update(UpdateComplaint);
                //Update Complaint Assing
                var UpdateComplaintAssign = await db.ComplaintAssign.Where(x => x.ComplaintId == obj.Id).ToListAsync();
                UpdateComplaintAssign.ForEach(x =>
                {
                    var ComplaintAssign = obj.ComplaintAssign.FirstOrDefault(ca => ca.Id == x.Id);
                    x.Status = ComplaintAssign != null ? UpdateComplaint.Status : App.Status.Cancel;
                    x.UpdatedBy = User.Id;
                    x.UpdatedAt = DateTime.Now;                    
                });
                db.UpdateRange(UpdateComplaintAssign);
                //Add Complaint Assing
                var AddComplaintAssign = obj.ComplaintAssign.Where(x => x.Id == 0 && !UpdateComplaintAssign.Any(x1 => x1.ComplaintId == x.ComplaintId && x1.UserId == x.UserId)).ToList();
                AddComplaintAssign.ForEach(x =>
                {
                    x.ComplaintId = obj.Id;
                    x.Status = UpdateComplaint.Status;
                    x.CreatedBy = User.Id;
                    x.UpdatedBy = User.Id;                    
                });
                db.AddRange(AddComplaintAssign);
                Message.Update(ref objMsg, (await db.SaveChangesAsync()), "");
                if (objMsg.status == Message.Type.success)
                {                    
                    obj.ListId.Add(obj.Id);
                    var Complaint = (await ListAsync(obj, User)).FirstOrDefault();
                    if (Complaint != null)
                    {
                        Complaint.ComplaintAssign = obj.ComplaintAssign;
                        Message msg = await NotifyComplaintLogByEmail(Complaint, false);
                        objMsg.status = msg.status != Message.Type.success ? msg.status : objMsg.status;
                        objMsg.statusText += msg.statusText;
                    }
                    objMsg.obj = Complaint;                    
                    objMsg.data = (await GetViewOptionAsync()).data;
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
                        item.Status = DeleteComplaint.Status;
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
                        item.Status = DeleteComplaint.Status;
                        item.UpdatedBy = User.Id;
                        item.UpdatedAt = DateTime.Now;
                        db.Update(item);
                    }
                }                
                Message.Delete(ref objMsg, (await db.SaveChangesAsync()), "");
                if (objMsg.status == Message.Type.success)
                {
                    DeleteComplaint.ListId.Add(Id);
                    DeleteComplaint.ListStatus.Add(App.Status.Delete);
                    objMsg.obj = (await ListAsync(DeleteComplaint, User)).FirstOrDefault();
                    objMsg.data = (await GetViewOptionAsync()).data;
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> NotifyComplaintLogByEmail(Complaint obj, bool IsReg)
        {
            Message objMsg = new Message();
            try 
            {
                //Sent Email To Customer
                if (IsReg)
                {
                    string CustomerMailBody = $"Your complaint No. {obj.Code} has been registered. ";
                    var CustomerMail = new MailMessage();
                    CustomerMail.To.Add(obj.Email);
                    Util.SentMail(db, CustomerMail, $"Complaint Registered: {obj.Code}", CustomerMailBody, "info", ref objMsg);
                    objMsg.statusText = objMsg.status == Message.Type.success ? "Email has been sent to the customer." : "Failed to send email to the customer.";
                }
                else 
                {
                    objMsg.status = Message.Type.success;
                }
                    //Sent Email To Serice Eng
                var ListId = obj.ComplaintAssign.Select(ca => ca.UserId).ToList();
                var ListUserEmail = await db.User.Where(x => App.ActiveStatus.Contains(x.Status) && ListId.Contains(x.Id)).Select(x => x.Email).ToListAsync();
                var UserMail = new MailMessage();
                ListUserEmail.ForEach(Email => { UserMail.To.Add(Email); });
                if (ListUserEmail.Any()) 
                {
                    var UserMailBody = new StringBuilder();
                    UserMailBody.AppendLine("<p>");
                    UserMailBody.AppendLine($"You have been assigned Complaint No. {obj.Code}.<br/>");
                    UserMailBody.AppendLine($"Please resolve it at the earliest possible.");
                    UserMailBody.AppendLine("</p><hr/>");
                    UserMailBody.AppendLine($@"
                        <table style='width:90%; border-collapse: collapse;' border='0'>
                            <tbody>
                                <tr><th colspan='2' style='text-align:left;'>Complaint Info :</th></tr>
                                <tr><th style='text-align:left;'>Customer</th><td><b>:</b> {obj.CustomerDesc}</td></tr>
                                <tr><th style='text-align:left;'>Location</th><td><b>:</b> {obj.CustomerLocation}</td></tr>
                                <tr><th style='text-align:left;'>Department</th><td><b>:</b> {obj.Department}</td></tr>
                                <tr><th style='text-align:left;'>Contact Person</th><td><b>:</b> {obj.ContactPerson}</td></tr>
                                <tr><th style='text-align:left;'>Contact No.</th><td><b>:</b> {obj.ContactNo}</td></tr>
                                <tr><th style='text-align:left;'>Email</th><td><b>:</b> {obj.Email}</td></tr>
                                <tr><th style='text-align:left;'>Address</th><td><b>:</b> {obj.CustomerAddress}</td></tr>
                                <tr><td colspan='2' style='text-align:left;'><b>Problem :</b><div>{obj.Problem.Replace("\r\n", "<br/>")}</div></td></tr>
                            </tbody>
                        </table>
                        <hr/>
                    ");                    
                    Thread.Sleep(1000);
                    Message objMsgEng = new Message();
                    Util.SentMail(db, UserMail, $"Complaint Assigned: {obj.Code}", UserMailBody.ToString(), "info", ref objMsgEng);
                    if (objMsg.status != Message.Type.success || objMsgEng.status != Message.Type.success)
                    {
                        objMsg.status = objMsg.status != Message.Type.success && objMsgEng.status != Message.Type.success ? Message.Type.error : Message.Type.warning;
                    }
                    else
                    {
                        objMsg.status = Message.Type.success;
                    }
                    objMsg.statusText += $"<br>{(objMsgEng.status == Message.Type.success ? "Email sent to assigned engineer(s)." : "Failed to notify assigned engineer(s).")}";
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
