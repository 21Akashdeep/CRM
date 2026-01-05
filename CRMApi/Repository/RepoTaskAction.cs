using CRMApi.Models;
using CRMApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using System.Net.Mail;

namespace CRMApi.Repository
{
    public class RepoTaskAction
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        private RepoTask repoTask;
        public RepoTaskAction(DBCRM _db)
        {
            db = _db;
            repoTask = new RepoTask(db);
        }
        public async Task<Message> GetViewOptionAsync()
        {
            var objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();
                var ListStatus = new List<int>() { App.Status.Pending, App.Status.Processing, App.Status.RequestForClose, App.Status.Completed };
                Option.Status = await db.Setting.Where(x => x.Name == App.SettingName.Status && ListStatus.Select(value => value.ToString()).Contains(x.Value)).Select(x => new
                {
                    x.Value,
                    x.Description
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
        public async Task<Message> GetAddOptionAsync(TaskAction obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();
                var ListStatus = new List<int>() { App.Status.Processing, App.Status.RequestForClose, App.Status.Completed };
                Option.Status = await db.Setting.Where(x => x.Name == App.SettingName.Status && ListStatus.Select(value => value.ToString()).Contains(x.Value)).Select(x => new
                {
                    x.Value,
                    x.Description
                }).ToListAsync();
              


                Option.TaskItem = await db.TaskItem.Where(x => App.ActiveStatus.Contains(x.Status) && x.TaskId == obj.TaskId).Select(x => new
                {
                    x.Id,
                    x.Description

                }).ToListAsync();               
                objMsg.data = Option;
                Message.Success(ref objMsg, "record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<List<TaskAction>> ListAsync(TaskAction? obj, User User)
        {
            obj ??= new TaskAction();
            obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : new List<int> { App.Status.Processing, App.Status.Completed };
            var dbTaskActionQuery = db.TaskAction.Where(x => obj.ListStatus.Contains(x.Status)).AsQueryable();
            

            var dbTaskAction = await (
                from cs in dbTaskActionQuery
                join ts in db.Task on cs.TaskId equals ts.Id
                join tsi in db.TaskItem on cs.TaskItemId equals tsi.Id 
                join st in db.Setting on new { Name = App.SettingName.Status, Value = cs.Status.ToString() } equals new { st.Name, st.Value }
                join cb in db.User on cs.CreatedBy equals cb.Id
                join ub in db.User on cs.UpdatedBy equals ub.Id
                where cs.TaskItemId == obj.TaskItemId
                
               
                select new TaskAction
                {
                    Id = cs.Id,
                    TaskId = cs.TaskId,
                    TaskItemId = cs.TaskItemId,                 
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
                    IsDelete = ts.Status == App.Status.Processing || User.UserType == App.UserType.SysAdmin || User.UserType == App.UserType.Admin ? true : false
                }
            ).ToListAsync();
            return dbTaskAction;
        }
        public async Task<Message> GetTaskAsync(TaskAction obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var task = await repoTask.ListAsync(new Dto.DtoTask.DtoTaskFltr
                {
                    ListId = obj.ListTaskId,                    
                    ListStatus = obj.ListStatus.Any() ? obj.ListStatus : new List<int> { App.Status.Pending, App.Status.Processing, App.Status.RequestForClose }
                   
                }, User);
                objMsg.data = task;
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> AddAsync(TaskAction obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var dbTask = await db.Task.FirstOrDefaultAsync(x => App.UnderProcess.Contains(x.Status) && x.Id == obj.TaskId);
                var dbUser = await db.User.Where(x => App.ActiveStatus.Contains(x.Status) && x.Id ==obj.CreatedBy ).Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Email,
                    x.ContactNo,

                }).FirstOrDefaultAsync();
                if (dbTask == null)
                {
                    Message.Error(ref objMsg, "Complaint did not find.");
                    return objMsg;
                }

               
                //Update Complaint
                dbTask.Status = obj.Status;
                dbTask.UpdatedBy = User.Id;
                dbTask.UpdatedAt = DateTime.Now;
                db.Update(dbTask);

                //Add Complaint Status                
                obj.CreatedBy = User.Id;
                obj.CreatedAt = DateTime.Now;
                obj.UpdatedBy = User.Id;
                obj.UpdatedAt = DateTime.Now;
                obj.Date = DateOnly.MinValue;
                db.Add(obj);

                Message.Add(ref objMsg, (await db.SaveChangesAsync()), "");

                if (objMsg.status == Message.Type.success && obj.NotifyToOwner ==1)
                {
                    obj.ListId.Add(obj.Id);
                    objMsg.data = new
                    {
                        TaskAction = (await ListAsync(obj, User)),
                        Task = (await GetTaskAsync(obj, User))
                    };
                    obj.StatusDesc = (await db.Setting.FirstOrDefaultAsync(x => App.ActiveStatus.Contains(x.Status) && x.Name == App.SettingName.Status && x.Value == obj.Status.ToString()))?.Description ?? "";
                    Message objEmailMsg = await NotifyStatusViaEmail(obj);
                    objMsg.status = objEmailMsg.status == Message.Type.success ? objMsg.status : Message.Type.warning;
                    objMsg.statusText += $"<br>{objEmailMsg.statusText}";
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> NotifyStatusViaEmail(TaskAction obj)
        {
            Message objMsg = new Message();
            try
            {
                var Task = await db.Task.FirstOrDefaultAsync(x => x.Id == obj.TaskId);
                if (Task == null)
                {
                    Message.Error(ref objMsg, "TaskItem did not find for notify.");
                    return objMsg;
                }
               
                var User = await db.User.FirstOrDefaultAsync(x => x.Id == Task.CreatedBy);
                
                if (String.IsNullOrEmpty(User.Email))
                {
                    Message.Error(ref objMsg, "Service Eng. email did not find for notify.");
                    return objMsg;
                }
                MailMessage objMail = new MailMessage();
               
                objMail.To.Add(User.Email);
                string EmailBody = $@"                        
                    <p>Complaint No. <strong>{Task.Code}</strong> has been updated with status <strong>{obj.StatusDesc}</strong>.</p>
                    <p>Remarks: {obj.Remarks}</p>
                ";
                string ColorType = obj.Status == App.Status.Completed ? "success" : "warning";
                Util.SentMail(db, objMail, "Task Status Update", EmailBody, ColorType, ref objMsg);
                Message.Success(ref objMsg, "Email notification has been sent.");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }

        //public async Task<Message> SendOtpAsync(TaskAction obj, User User)
        //{
        //    Message objMsg = new Message();
        //    try
        //    {
        //        var Complaint = await db.Complaint.FirstOrDefaultAsync(x => x.Id == obj.ComplaintId);
        //        if (Complaint == null)
        //        {
        //            Message.Error(ref objMsg, "Complaint did not find for send OPT.");
        //            return objMsg;
        //        }
        //        if (String.IsNullOrEmpty(Complaint.Email))
        //        {
        //            Message.Error(ref objMsg, "Complaint Email did not find for send OPT.");
        //            return objMsg;
        //        }
        //        Otp Otp = new Otp
        //        {
        //            RefType = App.RefType.Complaint,
        //            RefId = Complaint.Id,
        //            RefNo = Complaint.Code,
        //            OtpNo = Util.RandomNum(),
        //            Expiry = DateTime.Now.AddMinutes(15),
        //            Status = App.Status.UnVerified,
        //            CreatedBy = User.Id,
        //            CreatedAt = DateTime.Now,
        //            UpdatedBy = User.Id,
        //            UpdatedAt = DateTime.Now
        //        };
        //        db.Add(Otp);
        //        int isSaved = await db.SaveChangesAsync();
        //        if (isSaved > 0)
        //        {
        //            MailMessage objMail = new MailMessage();
        //            objMail.To.Add(Complaint.Email);
        //            string EmailBody = $@"                        
        //                <p>OTP <strong>{Otp.OtpNo}</strong> has been generated for closing Complaint No. <strong>{Complaint.Code}</strong>.</p>                        
        //                <p>Kindly ensure the complaint is fully resolved before sharing this OTP.</p>                        
        //            ";
        //            Util.SentMail(db, objMail, "Complaint Closing OTP", EmailBody, "success", ref objMsg);
        //            objMsg.statusText = objMsg.status == Message.Type.success ? "OTP has been sent." : "OTP has not been sent.";
        //        }
        //        else
        //        {
        //            Message.Error(ref objMsg, $"OTP did not generate.");
        //        }
        //        objMsg.obj = Otp;
        //    }
        //    catch (Exception ex)
        //    {
        //        Message.Exception(ref objMsg, ex);
        //    }
        //    return objMsg;
        //}
        public async Task<Message> DeleteAsync(int Id, User User)
        {
            Message objMsg = new Message();
            try
            {
                var dbTaskAction = await db.TaskAction.FirstOrDefaultAsync(x => x.Id == Id);
                if (dbTaskAction == null)
                {
                    Message.Error(ref objMsg, "Task Status did find for delete");
                    return objMsg;
                }
                db.Remove(dbTaskAction);
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
