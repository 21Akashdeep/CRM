using CRMApi.Models;
using CRMApi.Services;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoComplaintStatus
    {
        private readonly DbCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoComplaintStatus(DbCRM _db)
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
                Option.Status = await db.Setting.Where(x => x.Name == App.SettingName.Status && ListStatus.Select(value=> value.ToString()).Contains(x.Value)).Select(x => new
                {
                    x.Value,
                    x.Description
                }).ToListAsync();
                
                Option.Complaint = await db.Complaint.Where(x=> ListStatus.Contains(x.Status)).Select(x => new
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
            var dbComplaintStatusQuery = db.ComplaintStatus.Where(x => obj.ListStatus.Contains(x.Status) && x.CompanyId == User.CompanyId).AsQueryable();
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
                    UpdatedAt = cs.UpdatedAt
                }
            ).ToListAsync();
            return dbComplaintStatus;
        }        
        public async Task<Message> GetComplaintScheduleAsync(ComplaintSchedule obj, User User) 
        {
            Message objMsg = new Message();            
            try
            {
                obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : new List<int> { App.Status.Scheduled, App.Status.Processing };
                obj.ListAssignToId = User.UserType == App.UserType.SysAdmin || User.UserType == App.UserType.Admin ? new List<int>() : new List<int> { User.Id };
                var dbComplaintSchedule = await new RepoComplaintSchedule(db).ListAsync(obj, User);
                objMsg.data = dbComplaintSchedule;
                
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
    }
}
