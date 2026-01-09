using CRMApi.Models;
using CRMApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;
using System.Net.Mail;
using System.Text;

namespace CRMApi.Repository
{
    public class RepoGrn
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoGrn(DBCRM _db)
        {
            db = _db;
        }
        public async Task<Message> GetViewOptionAsync()
        {
            Message objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();
                var ListStatus = await db.Voucher.GroupBy(x => new { x.Status }).Select(x => x.Key.Status.ToString()).ToListAsync();
                Option.Status = await db.Setting.Where(st => App.ActiveStatus.Contains(st.Status) && st.Name == App.SettingName.Status && ListStatus.Contains(st.Value)).Select(st => new
                {
                    st.Value,
                    st.Description
                }).ToListAsync();
                Option.Grn = await db.Voucher.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.No
                }).ToListAsync();
                Option.Party = await db.Party.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
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
        public async Task<Message> GetAddOptionAsync(Grn obj, User User)
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
                var dbParty = await new RepoParty(db).ListAsync(null, User);
                Option.Party = dbParty.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
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
                    x.LocationDesc,
                    SubText = Util.AddressDesc(new Composite.AddressDesc { Add1 = x.Address1, Add2 = x.Address2, PinCode = x.PinCode, PostOffice = x.PostOffice, OtherText = x.GstNo })
                }).ToList();
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
                Option.Department = await db.Voucher.Where(x => App.AllActiveStatus.Contains(x.Status)).GroupBy(x => new { x.Department }).Select(x => new
                {
                    Value = x.Key.Department,
                    Description = x.Key.Department
                }).ToListAsync();
                Option.Priority = await db.Setting.Where(x => App.ActiveStatus.Contains(x.Status) && x.Name == App.SettingName.Priority).Select(x => new
                {
                    Id = x.Value,
                    x.Description
                }).ToListAsync();
                //var dbGrnAssign = db.Voucher.Where(x => App.AllActiveStatus.Contains(x.Status)).AsQueryable();
                //Option.GrnAssing = await (
                //    from usr in db.User
                //    join ulo in db.UserLocation on usr.Id equals ulo.UserId
                //    join loc in db.Location on ulo.LocationId equals loc.Id
                //    join cus in db.Party on ulo.LocationId equals cus.LocationId
                //    join coa in dbGrnAssign on new { UserId = usr.Id, GrnId = obj.Id } equals new { coa.UserId, coa.GrnId } into GrnAssign
                //    from coa in GrnAssign.DefaultIfEmpty()
                //    where App.ActiveStatus.Contains(usr.Status) && cus.Id == obj.PartyId
                //    select new
                //    {
                //        Id = coa != null ? coa.Id : 0,
                //        GrnId = coa != null ? coa.GrnId : 0,
                //        UserId = usr.Id,
                //        UserName = usr.Name,
                //        LocationDesc = loc.Description,
                //        IsAdded = coa != null,
                //    }
                //).ToListAsync();
                objMsg.data = Option;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<List<Grn>> ListAsync(Grn? obj, User User)
        {
            //filter Grn data
            obj = obj == null ? new Grn() : obj;
            obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : App.AllActiveStatus;
            var dbGrnQuery = db.Voucher.Where(co => obj.ListStatus.Contains(co.Status)).AsQueryable();

            if (obj.FromDate != default(DateTime))
                dbGrnQuery = dbGrnQuery.Where(co => co.Date.Date >= obj.FromDate.Date);
            if (obj.ToDate != default(DateTime))
                dbGrnQuery = dbGrnQuery.Where(co => co.Date.Date <= obj.ToDate.Date);
            if (obj.ListId.Any())
                dbGrnQuery = dbGrnQuery.Where(co => obj.ListId.Contains(co.Id));
            if (obj.ListSupportMode.Any())
                dbGrnQuery = dbGrnQuery.Where(co => obj.ListSupportMode.Contains(co.No));
            if (obj.ListPartyId.Any())
                dbGrnQuery = dbGrnQuery.Where(co => obj.ListPartyId.Contains(co.PartyId));
            if (obj.ListPriority.Any())
                dbGrnQuery = dbGrnQuery.Where(co => obj.ListPriority.Contains(co.Priority));
            //if (obj.ListAssignTo.Any())
            //{
            //    var ListAssignedGrnId = await db.Voucher.Where(ca => App.AllActiveStatus.Contains(ca.Status) && obj.ListAssignTo.Contains(ca.No)).Select(ca => ca.GrnId).ToListAsync();
            //    dbGrnQuery = dbGrnQuery.Where(co => ListAssignedGrnId.Contains(co.Id));
            //}
            var dbGrn = await dbGrnQuery.ToListAsync();
            //Get Grn List
            var Grn = (
                from co in dbGrn
                join sm in db.Setting on new { Name = App.SettingName.SupportMode, Value = co.No } equals new { sm.Name, sm.Value }
                join cu in db.Party on co.PartyId equals cu.Id
                join lo in db.Location on cu.LocationId equals lo.Id
                join ad in db.AdminDiv on co.AdminDivId equals ad.Id
                join cn in db.Country on co.CountryId equals cn.Id
                join pr in db.Setting on new { Name = App.SettingName.Priority, Value = co.Priority.ToString() } equals new { pr.Name, pr.Value }
                join st in db.Setting on new { Name = App.SettingName.Status, Value = co.Status.ToString() } equals new { st.Name, st.Value }
                join cb in db.User on co.CreatedBy equals cb.Id
                join ub in db.User on co.UpdatedBy equals ub.Id
                select new Grn
                {
                    Id = co.Id,
                    No = co.No,
                    Date = co.Date,
                    //SupportMode = co.SupportMode,
                    SupportModeDesc = sm.Description,
                    PartyId = co.PartyId,
                    //PartyDesc = cu.Description,
                    Address1 = co.Address1,
                    Address2 = co.Address2,
                    PinCode = co.PinCode,
                    PostOffice = co.PostOffice,
                    District = co.District,
                    AdminDivId = co.AdminDivId,
                    AdminDivDesc = ad.Description,
                    CountryId = co.CountryId,
                    CountryDesc = cn.Description,
                    PartyAddress = Util.AddressDesc(new Composite.AddressDesc
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
                    PartyLocation = lo.Description,
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
                    IsClose = App.ByPassUserType.Contains(User.UserType) || co.Status == App.Status.RequestForClose ? true : false,
                    IsAddStatus = co.Status == App.Status.Pending || co.Status == App.Status.Processing ? true : false,
                    IsDetailedView = true
                }
            ).ToList();
            return Grn;
        }
        private List<Grn> ListSample(Grn? obj, User User, int page = 1, int pageSize = 50, string sortBy = "Date", bool sortDesc = true, bool exportAll = false)
        {
            obj ??= new Grn();
            obj.ListStatus = obj.ListStatus.Count == 0 ? App.ActiveStatus : obj.ListStatus;

            var query = db.Voucher.Where(co => obj.ListStatus.Contains(co.Status));

            if (obj.FromDate != default)
                query = query.Where(co => co.Date.Date >= obj.FromDate.Date);

            if (obj.ToDate != default)
                query = query.Where(co => co.Date.Date <= obj.ToDate.Date);

            if (obj.ListId.Any())
                query = query.Where(co => obj.ListId.Contains(co.Id));

            if (obj.ListSupportMode.Any())
                query = query.Where(co => obj.ListSupportMode.Contains(co.No));

            if (obj.ListPartyId.Any())
                query = query.Where(co => obj.ListPartyId.Contains(co.PartyId));

            if (obj.ListPriority.Any())
                query = query.Where(co => obj.ListPriority.Contains(co.Priority));

            var dbGrn = query.ToList();


            var Grn = (
                from co in dbGrn
                join sm in db.Setting on new { Name = App.SettingName.SupportMode, Value = co.No.ToString() } equals new { sm.Name, sm.Value }
                join cu in db.Party on co.PartyId equals cu.Id
                join ad in db.AdminDiv on co.AdminDivId equals ad.Id
                join cn in db.Country on co.CountryId equals cn.Id
                join pr in db.Setting on new { Name = App.SettingName.Priority, Value = co.Priority.ToString() } equals new { pr.Name, pr.Value }
                join st in db.Setting on new { Name = App.SettingName.Status, Value = co.Status.ToString() } equals new { st.Name, st.Value }
                join cb in db.User on co.CreatedBy equals cb.Id
                join ub in db.User on co.UpdatedBy equals ub.Id
                select new Grn
                {
                    Id = co.Id,
                    No = co.No,
                    Date = co.Date,
                    //SupportMode = co.SupportMode,
                    SupportModeDesc = sm.Description,
                    PartyId = co.PartyId,
                    //PartyDesc = cu.Description,
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
            Grn = sortDesc
                ? Grn.OrderByDescending(x => EF.Property<object>(x, sortBy))
                : Grn.OrderBy(x => EF.Property<object>(x, sortBy));

            // Pagination
            if (!exportAll)
                Grn = Grn.Skip((page - 1) * pageSize).Take(pageSize);

            return Grn.ToList();
        }
        public async Task<Message> GetAsync(Grn obj, User User)
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
        public async Task<Message> PrintAsync(Grn obj, User User)
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
        public async Task<Message> ExportAsync(Grn obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var dbGrn = await ListAsync(obj, User);
                var Grn = dbGrn.Select(co => new
                {
                    co.Date,
                    GrnNo = co.No,
                    SupportMode = co.SupportModeDesc,
                    //Party = co.PartyDesc,
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

                DataTable objDataTable = Util.ListToDataTable(Grn);

                var objCompany = await db.Company.FirstOrDefaultAsync(pt => App.ActiveStatus.Contains(pt.Status));
                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Company Info did found");
                    return objMsg;
                }

                objCompany.SheetName = "Grn Item Wise List";
                objCompany.ReportDesc = $"Grn Item Wise List Generated On - {DateTime.Now.ToString("dd-MMM-yyyy HH:mm")}";
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
        //public async Task<Message> DetailedViewAsync(int Id, User User)
        //{
        //    Message objMsg = new Message();
        //    try
        //    {
        //        var dbGrn = await ListAsync(new Grn { ListId = new List<int> { Id }, ListStatus = App.AllActiveStatus }, User);
        //        var dbGrnStatus = await new RepoGrnStatus(db).ListAsync(new GrnStatus { ListGrnId = new List<int> { Id } }, User);
        //        var dbGrnItem = await new RepoGrnItem(db).ListAsync(new GrnItem { ListGrnId = new List<int> { Id } }, User);
        //        var Grn = dbGrn.Select(Grn => new
        //        {
        //            Grn,
        //            GrnStatus = dbGrnStatus,
        //            GrnItem = dbGrnItem
        //        }).FirstOrDefault();
        //        objMsg.obj = Grn;
        //        if (Grn != null)
        //        {
        //            Message.Success(ref objMsg, "Record found.");
        //        }
        //        else
        //        {
        //            Message.Error(ref objMsg, "Record did not find.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Message.Exception(ref objMsg, ex);
        //    }
        //    return objMsg;
        //}
        public async Task<Message> AddAsync(Grn obj, User User)
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
                //obj.GrnAssign.ForEach(x =>
                //{
                //    x.Status = obj.Status;
                //    x.CreatedBy = User.Id;
                //    x.UpdatedBy = User.Id;
                //});
                db.Add(obj);
                Message.Add(ref objMsg, (await db.SaveChangesAsync()), "");
                if (objMsg.status == Message.Type.success)
                {
                    //Reload Object & Assing Id
                    await db.Entry(obj).ReloadAsync();
                    obj.ListId.Add(obj.Id);
                    var Grn = (await ListAsync(obj, User)).FirstOrDefault();
                    //if (Grn != null)
                    //{
                    //    Grn.GrnAssign = obj.GrnAssign;
                    //    Message msg = await NotifyGrnLogByEmail(Grn, true);
                    //    objMsg.status = msg.status != Message.Type.success ? msg.status : objMsg.status;
                    //    objMsg.statusText += msg.statusText;
                    //}
                    objMsg.obj = Grn;
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
                Grn? obj = new Grn();
                obj.ListId.Add(Id);
                obj.ListStatus.AddRange(App.AllActiveStatus);
                obj = (await ListAsync(obj, User)).FirstOrDefault();
                if (obj == null)
                {
                    Message.Error(ref objMsg, "Grn did not find for edit.");
                    return objMsg;
                }
                var AddOption = await GetAddOptionAsync(obj, User);
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
        public async Task<Message> UpdateAsync(Grn obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var UpdateGrn = await db.Voucher.FindAsync(obj.Id);
                if (UpdateGrn == null)
                {
                    Message.Error(ref objMsg, "Grn did not find for update.");
                    return objMsg;
                }
                UpdateGrn.Date = obj.Date;
                UpdateGrn.No = obj.No;
                UpdateGrn.PartyId = obj.PartyId;
                UpdateGrn.Address1 = obj.Address1;
                UpdateGrn.Address2 = obj.Address2;
                UpdateGrn.PinCode = obj.PinCode;
                UpdateGrn.PostOffice = obj.PostOffice;
                UpdateGrn.District = obj.District;
                UpdateGrn.AdminDivId = obj.AdminDivId;
                UpdateGrn.CountryId = obj.CountryId;
                UpdateGrn.ContactPerson = obj.ContactPerson;
                UpdateGrn.ContactNo = obj.ContactNo;
                UpdateGrn.Email = obj.Email;
                UpdateGrn.Department = obj.Department;
                UpdateGrn.Priority = obj.Priority;
                UpdateGrn.Problem = obj.Problem;
                UpdateGrn.UpdatedBy = User.Id;
                UpdateGrn.UpdatedAt = DateTime.Now;
                db.Update(UpdateGrn);
                //Update Grn Assing
                //var UpdateGrnAssign = await db.Voucher.Where(x => x.Id == obj.Id).ToListAsync();
                //UpdateGrnAssign.ForEach(x =>
                //{
                //    var GrnAssign = obj.GrnAssign.FirstOrDefault(ca => ca.Id == x.Id);
                //    x.Status = GrnAssign != null ? UpdateGrn.Status : App.Status.Cancel;
                //    x.UpdatedBy = User.Id;
                //    x.UpdatedAt = DateTime.Now;
                //});
                //db.UpdateRange(UpdateGrnAssign);
                //Add Grn Assing
                //var AddGrnAssign = obj.GrnAssign.Where(x => x.Id == 0 && !UpdateGrnAssign.Any(x1 => x1.GrnId == x.GrnId && x1.UserId == x.UserId)).ToList();
                //AddGrnAssign.ForEach(x =>
                //{
                //    x.GrnId = obj.Id;
                //    x.Status = UpdateGrn.Status;
                //    x.CreatedBy = User.Id;
                //    x.UpdatedBy = User.Id;
                //});
                //db.AddRange(AddGrnAssign);
                Message.Update(ref objMsg, (await db.SaveChangesAsync()), "");
                //if (objMsg.status == Message.Type.success)
                //{
                //    obj.ListId.Add(obj.Id);
                //    var Grn = (await ListAsync(obj, User)).FirstOrDefault();
                //    if (Grn != null)
                //    {
                //        Grn.GrnAssign = obj.GrnAssign;
                //        Message msg = await NotifyGrnLogByEmail(Grn, false);
                //        objMsg.status = msg.status != Message.Type.success ? msg.status : objMsg.status;
                //        objMsg.statusText += msg.statusText;
                //    }
                //    objMsg.obj = Grn;
                //    objMsg.data = (await GetViewOptionAsync()).data;
                //}
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
                var DeleteGrn = await db.Voucher.FindAsync(Id);
                if (DeleteGrn == null)
                {
                    Message.Error(ref objMsg, "Grn did not find for update.");
                    return objMsg;
                }
                DeleteGrn.Status = App.Status.Delete;
                DeleteGrn.UpdatedBy = User.Id;
                DeleteGrn.UpdatedAt = DateTime.Now;
                db.Update(DeleteGrn);
                //Delete Grn Assign
                var DeleteGrnAssign = await db.Voucher.Where(x => x.Id == Id).ToListAsync();
                if (DeleteGrnAssign.Any())
                {
                    foreach (var item in DeleteGrnAssign)
                    {
                        item.Status = DeleteGrn.Status;
                        item.UpdatedBy = User.Id;
                        item.UpdatedAt = DateTime.Now;
                        db.Update(item);
                    }
                }
                //Delete Grn Status
                var DeleteGrnStatus = await db.Voucher.Where(x => x.Id == Id).ToListAsync();
                if (DeleteGrnStatus.Any())
                {
                    foreach (var item in DeleteGrnStatus)
                    {
                        item.Status = DeleteGrn.Status;
                        item.UpdatedBy = User.Id;
                        item.UpdatedAt = DateTime.Now;
                        db.Update(item);
                    }
                }
                Message.Delete(ref objMsg, (await db.SaveChangesAsync()), "");
                if (objMsg.status == Message.Type.success)
                {
                    DeleteGrn.ListId.Add(Id);
                    DeleteGrn.ListStatus.Add(App.Status.Delete);
                    objMsg.obj = (await ListAsync(DeleteGrn, User)).FirstOrDefault();
                    objMsg.data = (await GetViewOptionAsync()).data;
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        //public async Task<Message> NotifyGrnLogByEmail(Grn obj, bool IsReg)
        //{
        //    Message objMsg = new Message();
        //    try
        //    {
        //        //Sent Email To Party
        //        if (IsReg)
        //        {
        //            string PartyMailBody = $"Your Grn No. {obj.Code} has been registered. ";
        //            var PartyMail = new MailMessage();
        //            PartyMail.To.Add(obj.Email);
        //            Util.SentMail(db, PartyMail, $"Grn Registered: {obj.Code}", PartyMailBody, "info", ref objMsg);
        //            objMsg.statusText = objMsg.status == Message.Type.success ? "Email has been sent to the Party." : "Failed to send email to the Party.";
        //        }
        //        else
        //        {
        //            objMsg.status = Message.Type.success;
        //        }
        //        //Sent Email To Serice Eng
        //        var ListId = obj.GrnAssign.Select(ca => ca.UserId).ToList();
        //        var ListUserEmail = await db.User.Where(x => App.ActiveStatus.Contains(x.Status) && ListId.Contains(x.Id)).Select(x => x.Email).ToListAsync();
        //        var UserMail = new MailMessage();
        //        ListUserEmail.ForEach(Email => { UserMail.To.Add(Email); });
        //        if (ListUserEmail.Any())
        //        {
        //            var UserMailBody = new StringBuilder();
        //            UserMailBody.AppendLine("<p>");
        //            UserMailBody.AppendLine($"You have been assigned Grn No. {obj.Code}.<br/>");
        //            UserMailBody.AppendLine($"Please resolve it at the earliest possible.");
        //            UserMailBody.AppendLine("</p><hr/>");
        //            UserMailBody.AppendLine($@"
        //                <table style='width:90%; border-collapse: collapse;' border='0'>
        //                    <tbody>
        //                        <tr><th colspan='2' style='text-align:left;'>Grn Info :</th></tr>
        //                        <tr><th style='text-align:left;'>Party</th><td><b>:</b> {obj.PartyDesc}</td></tr>
        //                        <tr><th style='text-align:left;'>Location</th><td><b>:</b> {obj.PartyLocation}</td></tr>
        //                        <tr><th style='text-align:left;'>Department</th><td><b>:</b> {obj.Department}</td></tr>
        //                        <tr><th style='text-align:left;'>Contact Person</th><td><b>:</b> {obj.ContactPerson}</td></tr>
        //                        <tr><th style='text-align:left;'>Contact No.</th><td><b>:</b> {obj.ContactNo}</td></tr>
        //                        <tr><th style='text-align:left;'>Email</th><td><b>:</b> {obj.Email}</td></tr>
        //                        <tr><th style='text-align:left;'>Address</th><td><b>:</b> {obj.PartyAddress}</td></tr>
        //                        <tr><th style='text-align:left;'>priority</th><td><b>:</b> {obj.PriorityDesc}</td></tr>
        //                        <tr><td colspan='2' style='text-align:left;'><b>Problem :</b><div>{obj.Problem.Replace("\r\n", "<br/>")}</div></td></tr>
        //                    </tbody>
        //                </table>
        //                <hr/>
        //            ");
        //            Thread.Sleep(1000);
        //            Message objMsgEng = new Message();
        //            Util.SentMail(db, UserMail, $"Grn Assigned: {obj.Code}", UserMailBody.ToString(), "info", ref objMsgEng);
        //            if (objMsg.status != Message.Type.success || objMsgEng.status != Message.Type.success)
        //            {
        //                objMsg.status = objMsg.status != Message.Type.success && objMsgEng.status != Message.Type.success ? Message.Type.error : Message.Type.warning;
        //            }
        //            else
        //            {
        //                objMsg.status = Message.Type.success;
        //            }
        //            objMsg.statusText += $"<br>{(objMsgEng.status == Message.Type.success ? "Email sent to assigned engineer(s)." : "Failed to notify assigned engineer(s).")}";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Message.Exception(ref objMsg, ex);
        //    }
        //    return objMsg;
        //}
        public async Task<Message> Close(int Id, User User)
        {
            Message objMsg = new Message();
            try
            {
                var dbGrnQuery = db.Voucher.Where(co => co.Id == Id).AsQueryable();
                if (User.UserType != App.UserType.SysAdmin)
                {
                    dbGrnQuery = dbGrnQuery.Where(co => co.Status == App.Status.RequestForClose);
                }
                var Grn = await dbGrnQuery.FirstOrDefaultAsync();
                if (Grn == null)
                {
                    Message.Error(ref objMsg, "Grn did not for close");
                    return objMsg;
                }
                Grn.Status = App.Status.Completed;
                Grn.UpdatedBy = User.Id;
                Grn.UpdatedAt = DateTime.Now;
                db.Update(Grn);
                Message.Close(ref objMsg, (await db.SaveChangesAsync()), "");
                if (objMsg.status == Message.Type.success)
                {
                    Grn.ListId.Add(Id);
                    objMsg.obj = (await ListAsync(Grn, User)).FirstOrDefault();
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
