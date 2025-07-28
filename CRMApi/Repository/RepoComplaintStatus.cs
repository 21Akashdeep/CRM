using CRMApi.Models;
using CRMApi.Services;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2016.Presentation.Command;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using System.Net.Mail;

namespace CRMApi.Repository
{
    public class RepoComplaintStatus
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoComplaintStatus(DBCRM _db)
        {
            db = _db;
        }
        public async Task<Message> GetViewOptionAsync()
        {
            var objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();
                var ListStatus = new List<int>() { App.Status.Scheduled, App.Status.Processing, App.Status.Completed };
                Option.Status = await db.Setting.Where(x => x.Name == App.SettingName.Status && ListStatus.Select(value => value.ToString()).Contains(x.Value)).Select(x => new
                {
                    x.Value,
                    x.Description
                }).ToListAsync();

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
        public async Task<Message> GetAddOptionAsync(ComplaintStatus obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();
                var ListStatus = new List<int>() { App.Status.Processing, App.Status.Completed };
                Option.Status = await db.Setting.Where(x => x.Name == App.SettingName.Status && ListStatus.Select(value => value.ToString()).Contains(x.Value)).Select(x => new
                {
                    x.Value,
                    x.Description
                }).ToListAsync();
                obj.ListComplaintId.Add(obj.ComplaintId);
                Option.ComplaintStatus = await ListAsync(obj, User);
                objMsg.data = Option;
                Message.Success(ref objMsg, "record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<List<ComplaintStatus>> ListAsync(ComplaintStatus? obj, User User)
        {
            obj ??= new ComplaintStatus();
            obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : new List<int> { App.Status.Processing, App.Status.Completed };
            var dbComplaintStatusQuery = db.ComplaintStatus.Where(x => obj.ListStatus.Contains(x.Status)).AsQueryable();
            dbComplaintStatusQuery = dbComplaintStatusQuery.Where(x => !obj.ListId.Any() || obj.ListId.Contains(x.Id));
            dbComplaintStatusQuery = dbComplaintStatusQuery.Where(x => !obj.ListComplaintId.Any() || obj.ListComplaintId.Contains(x.ComplaintId));

            var dbComplaintStatus = await (
                from cs in dbComplaintStatusQuery
                join co in db.Complaint on cs.ComplaintId equals co.Id
                join st in db.Setting on new { Name = App.SettingName.Status, Value = cs.Status.ToString() } equals new { st.Name, st.Value }
                join cb in db.User on cs.CreatedBy equals cb.Id
                join ub in db.User on cs.UpdatedBy equals ub.Id
                where (obj.FromDate == DateTime.MinValue || co.Date.Date >= obj.FromDate.Date) &&
                      (obj.ToDate == DateTime.MinValue || co.Date.Date <= obj.ToDate.Date)
                select new ComplaintStatus
                {
                    Id = cs.Id,
                    ComplaintId = cs.ComplaintId,
                    ComplaintNo = co.Code,
                    Remarks = cs.Remarks,
                    Status = cs.Status,
                    StatusDesc = st.Description,
                    StatusCss = st.CssClass,
                    CreatedBy = cs.CreatedBy,
                    CreatedByName = cb.Name,
                    CreatedAt = cs.CreatedAt,
                    UpdatedBy = cs.UpdatedBy,
                    UpdatedByName = ub.Name,
                    UpdatedAt = cs.UpdatedAt,
                    IsDelete = co.Status == App.Status.Processing || User.UserType == App.UserType.SysAdmin || User.UserType == App.UserType.Admin ? true : false
                }
            ).ToListAsync();
            return dbComplaintStatus;
        }
        public async Task<Message> GetComplaintAsync(ComplaintStatus obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var Complaint = await new RepoComplaint(db).ListAsync(new Complaint
                {
                    ListId = obj.ListComplaintId,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    ListStatus = obj.ListStatus.Any() ? obj.ListStatus : new List<int> { App.Status.Scheduled, App.Status.Processing }
                }, User);
                List<int> ListAssignTo = App.ByPassUserType.Contains(User.UserType) ? new List<int>() : new List<int> { User.Id };
                objMsg.data = ListAssignTo.Any() ? Complaint.Where(x => x.AssignTo.Any(x1 => ListAssignTo.Contains(x1.AssignTo))).ToList() : Complaint;
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> AddAsync(ComplaintStatus obj, User User)
        {
            Message objMsg = new Message();
            try
            {                                
                var dbComplaint = await db.Complaint.FirstOrDefaultAsync(x => App.UnderProcess.Contains(x.Status) && x.Id == obj.ComplaintId);
                if (dbComplaint == null) 
                {
                    Message.Error(ref objMsg, "Complaint did not find.");
                    return objMsg;
                }
                //Verify Otp
                if (obj.Status == App.Status.Completed) 
                {
                    if (String.IsNullOrEmpty(obj.OtpNo)) 
                    {
                        Message.Error(ref objMsg, "With Complete status OTP is manadatory.");
                        return objMsg;
                    }
                    var dbOtp = await db.Otp.Where(o => o.RefNo == dbComplaint.Code && o.Status == App.Status.UnVerified).OrderByDescending(o => o.Id).ToListAsync();
                    var Otp = dbOtp.FirstOrDefault(o => o.OtpNo == obj.OtpNo);
                    if(Otp == null)
                    {
                        Message.Error(ref objMsg, "Invalid OTP.");
                        return objMsg;
                    }
                    if (Otp.Expiry < DateTime.Now) 
                    {
                        Message.Error(ref objMsg, "OTP has been expired.");
                        return objMsg;
                    }
                    Otp.Status = App.Status.Verified;
                    Otp.UpdatedBy = User.Id;
                    Otp.UpdatedAt = DateTime.Now;
                    db.Update(Otp);
                    //Delete Unwanted Otp
                    var DeleteOtp = dbOtp.Where(o => o.Id != Otp.Id).ToList();
                    DeleteOtp.ForEach(x => { 
                        x.Status = App.Status.Delete;
                        x.UpdatedBy = User.Id;
                        x.UpdatedAt = DateTime.Now;
                    });
                    db.UpdateRange(DeleteOtp);
                }
                //Update Complaint
                dbComplaint.Status = obj.Status;
                dbComplaint.UpdatedBy = User.Id;
                dbComplaint.UpdatedAt = DateTime.Now;
                db.Update(dbComplaint);

                //Update Complaint Status                
                obj.CreatedBy = User.Id;
                obj.CreatedAt = DateTime.Now;
                obj.UpdatedBy = User.Id;
                obj.UpdatedAt = DateTime.Now;
                db.Add(obj);
                
                int isSaved = await db.SaveChangesAsync();
                
                Message.Add(ref objMsg, isSaved, "");
                
                if (objMsg.status == Message.Type.success)
                {
                    obj.ListId.Add(obj.Id);
                    var ComplaintStatus = await ListAsync(obj, User);                    
                    var Complaint = await new RepoComplaint(db).ListAsync(new Complaint
                    {
                        ListId = new List<int> { obj.ComplaintId },
                        ListStatus = new List<int> { App.Status.Scheduled, App.Status.Processing, App.Status.Completed }
                    }, User);
                    dynamic data = new ExpandoObject();
                    data.ComplaintStatus = ComplaintStatus;
                    data.Complaint = Complaint.FirstOrDefault();
                    objMsg.data = data;
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> SendOtpAsync(ComplaintStatus obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                var Complaint = await db.Complaint.FirstOrDefaultAsync(x => x.Id == obj.ComplaintId);
                if (Complaint == null) 
                {
                    Message.Error(ref objMsg, "Complaint did not find for send OPT.");
                    return objMsg;
                }
                if (String.IsNullOrEmpty(Complaint.Email)) 
                {
                    Message.Error(ref objMsg, "Complaint Email did not find for send OPT.");
                    return objMsg;
                }
                Otp Otp = new Otp
                {
                    RefType = App.RefType.Complaint,
                    RefId = Complaint.Id,
                    RefNo = Complaint.Code,                    
                    OtpNo = Util.RandomNum(),
                    Expiry = DateTime.Now.AddMinutes(15),
                    Status = App.Status.UnVerified,
                    CreatedBy = User.Id,
                    CreatedAt = DateTime.Now,
                    UpdatedBy = User.Id,
                    UpdatedAt = DateTime.Now
                };
                db.Add(Otp);
                int isSaved = await db.SaveChangesAsync();
                if (isSaved > 0)
                {                    
                    MailMessage objMail = new MailMessage();
                    objMail.To.Add(Complaint.Email);                    
                    string EmailBody = $@"                        
                        <p>OTP <strong>{Otp.OtpNo}</strong> has been generated for closing Complaint No. <strong>{Complaint.Code}</strong>.</p>                        
                        <p>Kindly ensure the complaint is fully resolved before sharing this OTP.</p>                        
                    ";
                    Util.SentMail(db, objMail, "Complaint Closing OTP", EmailBody, "success", ref objMsg);
                    objMsg.statusText = objMsg.status == Message.Type.success ? "OTP has been sent." : "OTP has not been sent.";
                }
                else {
                    Message.Error(ref objMsg, $"OTP did not generate.");
                }
                objMsg.obj = Otp;
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
                var dbComplaintStatus = await db.ComplaintStatus.FirstOrDefaultAsync(x => x.Id == Id);
                if (dbComplaintStatus == null)
                {
                    Message.Error(ref objMsg, "Complaint Status did find for delete");
                    return objMsg;
                }
                db.Remove(dbComplaintStatus);
                int isSaved = await db.SaveChangesAsync();
                Message.Delete(ref objMsg, isSaved, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
    }
}
