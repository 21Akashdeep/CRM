using CRMApi.Dto;
using CRMApi.Models;
using CRMApi.Services;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.EntityFrameworkCore;
using System.Buffers.Text;
using System.Data;
using System.Dynamic;
using System.Net.Mail;
using System.Text;
using static CRMApi.Dto.DtoDocument;
using static CRMApi.Dto.DtoTask;
using Task = CRMApi.Models.Task;



namespace CRMApi.Repository
{
    public class RepoTask
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public (string Name, string MimeType, string Base64, Message Message) DtoDocAdd { get; private set; }
        public RepoTask(DBCRM db)
        {
            this.db = db;
        }
        public Message GetViewOption()
        {
            Message objMsg = new Message();
            try
            {
                dynamic option = new ExpandoObject();
                var ListStatus = db.Task.Select(x => x.Status.ToString()).ToList();
                option.Status = (
                    from st in db.Setting
                    where App.ActiveStatus.Contains(st.Status) && st.Name == App.SettingName.Status && ListStatus.Contains(st.Value)
                    select new
                    {
                        st.Value,
                        st.Description,
                    }
                ).ToList();

                option.ListId = db.Task
               .Where(x => App.AllActiveStatus.Contains(x.Status))
               .Select(x => new
               {
                 x.Id,
                 x.Description
               }).ToList();

                option.Party = db.Party.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new {
                    x.Id,
                    x.Name,
                    x.Description,
                    x.Code,

                }).ToList();

                var debugTaskCategory = App.SettingName.TaskCategory;
                option.Category = db.Setting.Where(x => App.ActiveStatus.Contains(x.Status) &&
                  x.Category == App.SettingName.TaskCategory
                 ).Select(x => new
                 {
                     x.Value,
                     x.Description

                 }).ToList();


                objMsg.data = option;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message GetAddOption()
        {
            Message objMsg = new Message();
            try
            {
                //Expando Object
                dynamic Option = new ExpandoObject();
                Option.Party = db.Party.Where(ig => App.ActiveStatus.Contains(ig.Status)).Select(ig => new
                {
                    ig.Id,
                    ig.Code,
                    ig.Name,
                    ig.Description
                }).ToList();

                Option.Category = db.Setting.Where(x => x.Category == App.SettingName.TaskCategory).
                   Select(x => new
                   {
                       x.Value,
                       x.Description

                   })
                   .ToList();

                Option.Task = db.Task.Where(ts => App.ActiveStatus.Contains(ts.Status)).Select(ts => new
                {
                    TaskId = ts.Id,
                    TaskDesc =  ts.Description



                }).ToList();

                Option.Employee = db.Employee.Where(em => App.ActiveStatus.Contains(em.Status)).Select(em => new
                {
                    em.Id,
                    EmployeeName = em.Name

                }).ToList();

                Option.User = (
                     from usr in db.User
                     join ulo in db.UserLocation on usr.Id equals ulo.UserId
                     join cus in db.Party on ulo.LocationId equals cus.LocationId

                     where App.ActiveStatus.Contains(usr.Status)
                     select new
                     {
                         Id = usr.Id,
                         UserName = usr.Name,


                     }
                 ).ToList();

                objMsg.data = Option;

                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<List<DtoTaskList>> ListAsync(DtoTaskFltr? obj, User User)
        {

            obj ??= new DtoTaskFltr();
            obj.ListStatus = obj.ListStatus.Count == 0 ? App.AllActiveStatus : obj.ListStatus;            
            var taskList = await (
                from dpt in db.Task
                join sts in db.Setting on new { Value = dpt.Status.ToString(), Name = App.SettingName.Status } equals new { sts.Value, sts.Name }
                join cus in db.Party on dpt.PartyId equals cus.Id              
                join cby in db.User on dpt.CreatedBy equals cby.Id
                join uby in db.User on dpt.UpdatedBy equals uby.Id
                where
                obj.ListStatus.Contains(dpt.Status) &&
                (!obj.ListId.Any() || obj.ListId.Contains(dpt.Id)) &&
                (!obj.ListPartyId.Any() || obj.ListPartyId.Contains(dpt.PartyId))
                let file = Util.GetFile(dpt.TechnicalDoc ?? "")
                select new DtoTaskList
                {
                    Id = dpt.Id,
                    Code = dpt.Code,
                    Name = dpt.Name,
                    Description = dpt.Description,
                    PartyId = dpt.PartyId,
                    PartyDesc = cus.Description,
                    Category = dpt.Category,                  
                    PoNo = dpt.PoNo,
                    PoDate = dpt.PoDate,
                    StartDate = dpt.StartDate,
                    EndDate = dpt.EndDate,
                    Remarks = dpt.Remarks,
                    DocName = file.Name,                  
                    DocMimeType = file.MimeType,
                    DocBase64 = file.Base64,
                    CreatedByName = cby.Name,
                    CreatedAt = dpt.CreatedAt,
                    UpdatedByName = uby.Name,
                    UpdatedBy = dpt.UpdatedBy,
                    UpdatedAt = dpt.UpdatedAt,
                    Status = dpt.Status,
                    StatusDesc = sts.Description,
                    StatusCss = sts.CssClass ?? "",
                    IsEdit = dpt.Status == App.Status.Enable || dpt.CreatedBy == User.Id || User.UserType == App.UserType.SysAdmin ? true : false,
                    IsDuplicate = true,
                    IsDelete = dpt.Status == App.Status.Enable || dpt.CreatedBy == User.Id || User.UserType == App.UserType.SysAdmin ? true : false,
                    IsEnable = dpt.Status == App.Status.Delete ? true : false,
                    IsAddStatus = dpt.Status == App.Status.Pending || dpt.Status == App.Status.Processing ? true : false,
                }
            ).ToListAsync();
            return taskList;
        }
        public async Task<List<DtoTaskItemList>> ListItem(List<int> ListTaskId, List<int> ListSatus)
        {
            var taskItemList = await (
                from tsi in db.TaskItem
                join tsk in db.Task on tsi.TaskId equals tsk.Id
                join sts in db.Setting on new { Value = tsi.Status.ToString(), Name = App.SettingName.Status } equals new { sts.Value, sts.Name }
                join cby in db.User on tsi.CreatedBy equals cby.Id
                join uby in db.User on tsi.UpdatedBy equals uby.Id
                where ListTaskId.Contains(tsi.TaskId) && ListSatus.Contains(tsi.Status)
                let file = Util.GetFile(tsi.TechnicalDoc ?? "")
                select new DtoTaskItemList
                {
                    Id = tsi.Id,
                    TaskId = tsi.TaskId,
                    TaskDesc = tsk.Description,
                    Description = tsi.Description,
                    EstimatedDays = tsi.EstimatedDays,
                    StartDateTime = tsi.StartDateTime,
                    EndDateTime = tsi.EndDateTime,
                    DocName = file.Name,
                    DocMimeType = file.MimeType,
                    DocBase64 = file.Base64,
                    AssignToList = tsi.AssignToList,
                    Remarks = tsi.Remarks,
                    CreatedByName = cby.Name,
                    CreatedAt = tsi.CreatedAt,
                    UpdatedByName = uby.Name,
                    UpdatedBy = tsi.UpdatedBy,
                    UpdatedAt = tsi.UpdatedAt,
                    Status = tsi.Status,
                    StatusDesc = sts.Description,
                    StatusCss = sts.CssClass ?? ""
                }
                ).ToListAsync();
            return taskItemList;
        }

        public async Task<List<DtoTaskItemList>> ListItemAsync(DtoTaskItemFltr? obj, User User)
        {

            obj ??= new DtoTaskItemFltr();
            obj.ListStatus = obj.ListStatus.Count == 0 ? App.AllActiveStatus : obj.ListStatus;
            var taskItemList = await (
                from dpt in db.TaskItem
                join sts in db.Setting on new { Value = dpt.Status.ToString(), Name = App.SettingName.Status } equals new { sts.Value, sts.Name }  
                join ts in db.Task on dpt.TaskId equals ts.Id
                join cby in db.User on dpt.CreatedBy equals cby.Id
                join uby in db.User on dpt.UpdatedBy equals uby.Id
                where
                obj.ListStatus.Contains(dpt.Status) &&
                (!obj.ListId.Any() || obj.ListId.Contains(dpt.Id)) &&
                (!obj.ListTaskId.Any() || obj.ListTaskId.Contains(dpt.TaskId)) &&
                (
                User.UserType == App.UserType.SysAdmin
                || dpt.CreatedBy == User.Id
                 || EF.Functions.JsonContains(
                  dpt.AssignToList,
                  $"{{\"Id\": {User.Id}}}"
                 ) )
                let file = Util.GetFile(dpt.TechnicalDoc ?? "")
                select new DtoTaskItemList
                {
                    Id = dpt.Id, 
                    TaskId = dpt.TaskId,
                    Description = dpt.Description,                  
                    EstimatedDays = dpt.EstimatedDays,                
                    StartDateTime = dpt.StartDateTime,
                    EndDateTime = dpt.EndDateTime,
                    AssignToList = dpt.AssignToList,
                    Remarks = dpt.Remarks,
                    DocName = file.Name,
                    DocMimeType = file.MimeType,
                    DocBase64 = file.Base64,
                    CreatedByName = cby.Name,
                    CreatedAt = dpt.CreatedAt,
                    UpdatedByName = uby.Name,
                    UpdatedBy = dpt.UpdatedBy,
                    UpdatedAt = dpt.UpdatedAt,
                    Status = dpt.Status,
                    StatusDesc = sts.Description,
                    StatusCss = sts.CssClass ?? "",
                    IsEdit = dpt.Status == App.Status.Enable || dpt.CreatedBy == User.Id ? true : false,
                    IsDuplicate = true,
                    IsDelete = dpt.Status == App.Status.Enable ? true : false,
                    IsEnable = dpt.Status == App.Status.Delete ? true : false,
                    IsAddStatus = dpt.Status == App.Status.Pending || dpt.Status == App.Status.Processing ? true : false,
                }
            ).ToListAsync();
            return taskItemList;
        }
        public async  Task<Message> Print(DtoTaskFltr obj, User User)
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
        public async Task<Message> Export(DtoTaskFltr obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                //Get Company
                var objCompany = await db.Company.FirstOrDefaultAsync(pt => App.ActiveStatus.Contains(pt.Status));
                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Company not found.");
                    return objMsg;
                }
                //Get Task Group
                var result = await ListAsync(obj, User);
                var Deparment = result.Select(x => new
                {
                   
                    x.Code,
                    x.Name,                                    
                    x.Description,
                    x.PoNo,
                    x.PoDate,
                    x.StartDate,
                    x.EndDate,
                    x.Remarks,                  
                    Party = x.PartyDesc,                 
                    x.CreatedByName,
                    x.CreatedAt,
                    x.UpdatedBy,
                    x.UpdatedByName,
                    x.UpdatedAt
                }).ToList();
                //Convert List To DataTable
                DataTable objDataTable = Util.ListToDataTable(Deparment);
                //Convert Datatable to base64                
                objCompany.SheetName = "Task List";
                objCompany.ReportDesc = $"Task - {DateTime.Now.ToString("dd-MMM-yyyy")}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> Add(DtoTaskAdd obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var Task = db.Task.Where(op => op.Status == App.Status.Approved && op.Code == obj.Code).AsEnumerable().FirstOrDefault();
                obj.Name = Util.SanitizeInput(obj.Name, null) ?? "";
                obj.Description = Util.SanitizeInput(obj.Description, null) ?? "";
                obj.PoNo = Util.SanitizeInput(obj.PoNo, null) ?? "";
                //obj.TechnicalDoc = Util.SanitizeInput(obj.TechnicalDoc, null) ?? "";

                // Checking Duplicate
                var duplicate = await db.Task
                    .Where(ap => ap.Description == obj.Description)
                    .Select(ap => new { ap.Description })
                    .FirstOrDefaultAsync();

                if (duplicate != null)
                {
                    if (duplicate.Description.ToLower() == obj.Description.ToLower())
                        Message.Duplicate(ref objMsg, $"Task Description : {obj.Name} already exists.");
                    return objMsg;
                }

                var duplicateItem = obj.TaskItem
               .Where(x => !string.IsNullOrWhiteSpace(x.Description))
               .Select(x => x.Description.Trim().ToLower())
               .GroupBy(d => d)
               .FirstOrDefault(g => g.Count() > 1);

                if (duplicateItem != null)
                {
                    Message.Duplicate(
                        ref objMsg,
                        $"Duplicate TaskItem description not allowed: {duplicateItem.Key}"
                    );
                    return objMsg;
                }


                // Add Task Document
                if (!string.IsNullOrWhiteSpace(obj.DocMimeType) && !string.IsNullOrEmpty(obj.DocBase64))
                {
                    var addDoc = Util.AddFile(new DtoDocument.DtoDocAdd
                    {
                        Base64 = Convert.FromBase64String(obj.DocBase64 ?? ""),
                        MimeType = obj.DocMimeType,
                        FileName = obj.DocName,
                        FilePath = $"{App.DocPath.TaskTechDoc}/{obj.DocName}"
                    });
                    if(addDoc.Message.status == Message.Type.success)
                    {
                        obj.DocName = addDoc.FilePath;
                    }
                }
                //Add Task Item Document                
                foreach (var item in obj.TaskItem)
                {
                    if (!string.IsNullOrWhiteSpace(item.DocMimeType) && !string.IsNullOrEmpty(item.DocBase64))
                    {
                        var addDoc = Util.AddFile(new DtoDocument.DtoDocAdd
                        {
                            Base64 = Convert.FromBase64String(item.DocBase64 ?? ""),
                            MimeType = item.DocMimeType,
                            FileName = null,
                            FilePath = $"{App.DocPath.TaskTechDoc}"
                        });
                        if (addDoc.Message.status == Message.Type.success)
                        {
                            item.DocName = addDoc.FilePath;
                        }
                        else 
                        {                            
                            objMsg.status = Message.Type.error;
                            objMsg.statusText += $"<br>Task Item Document '{item.DocName}' upload failed: {addDoc.Message.statusText}";
                            break;
                        }
                    }
                }
                if(objMsg.status == Message.Type.error)
                    return objMsg;
                // Insert Task                
                var task = new Models.Task
                {                   
                    Name = obj.Name,
                    Description = obj.Description,
                    PartyId = obj.PartyId,
                    Category = obj.Category,
                    PoNo = obj.PoNo,
                    TechnicalDoc = obj.DocName,
                    PoDate = obj.PoDate,
                    StartDate = obj.StartDate,
                    EndDate = obj.EndDate,
                    Remarks = obj.Remarks,
                    Status = App.Status.Pending,
                    CreatedBy = User.Id,
                    UpdatedBy = User.Id,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    TaskItem = obj.TaskItem.Select(x => new TaskItem
                    {
                        Description = x.Description,
                        TechnicalDoc = x.DocName,
                        EstimatedDays = x.EstimatedDays,
                        StartDateTime = x.StartDateTime,
                        EndDateTime = x.EndDateTime,
                        AssignToList = x.AssignToList,                        
                        Remarks = x.Remarks,
                        Status = App.Status.Pending,
                        CreatedBy = User.Id,
                        UpdatedBy = User.Id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    }).ToList()
                };
                db.Add(task);

                Message.Add(ref objMsg, (await db.SaveChangesAsync()), "");
                var DtoTaskList = await ListAsync(new DtoTaskFltr { ListId = new List<int> { task.Id } }, User);
                if (task!= null)
                {
                   
                    foreach (var item in task.TaskItem)
                    {

                        Message msg = await NotifyComplaintLogByEmail(item, DtoTaskList, true);
                        objMsg.status = msg.status != Message.Type.success ? msg.status : objMsg.status;
                        objMsg.statusText += msg.statusText;

                    }
                }             
                if (objMsg.status == Message.Type.success)
                {
                    objMsg.obj = DtoTaskList;
                    objMsg.data = GetViewOption().data;
                }                
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message>Edit(int Id, User User)
        {

            Message objMsg = new Message();
            try
            {                                
                var task = (await ListAsync(new DtoTaskFltr
                {
                    ListId = new List<int> { Id }
                }, User)).FirstOrDefault();

                if (task == null)
                {
                    Message.Error(ref objMsg, "Task did find for edit.");
                    return objMsg;
                }                

                task.TaskItem = await ListItem(
                    new List<int> { task.Id },
                    new List<int> { task.Status,App.Status.Processing}
                );

                objMsg.obj = task;
                objMsg.data = GetAddOption().data;

                Message.Success(ref objMsg, "Task found.");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> Update(DtoTaskAdd obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var Task = db.Task.Where(op => op.Status == App.Status.Approved && op.Code == obj.Code).AsEnumerable().FirstOrDefault();
                obj.Name = Util.SanitizeInput(obj.Name, null) ?? "";
                obj.Description = Util.SanitizeInput(obj.Description, null) ?? "";
                obj.PoNo = Util.SanitizeInput(obj.PoNo, null) ?? "";

                      var duplicate = await db.Task
                     .Where(t => t.Id != obj.Id &&
                     (t.Name == obj.Name || t.Description == obj.Description))
                     .FirstOrDefaultAsync();

                if (duplicate != null)
                {
                    Message.Duplicate(ref objMsg, $"Task Name : {obj.Name} already exists.");
                    return objMsg;
                }

                var UpdateTask = await db.Task.FirstOrDefaultAsync(t => t.Id == obj.Id  && (
                User.UserType == App.UserType.SysAdmin
                || t.CreatedBy == User.Id) && t.Status != App.Status.Delete);

                if (UpdateTask == null)
                {
                    Message.Error(ref objMsg, "Task not found or you are not authorized to update it.");
                    return objMsg;
                }

                if ( !string.IsNullOrWhiteSpace(obj.DocMimeType) && !string.IsNullOrEmpty(obj.DocBase64))
                {
                    var addDoc = Util.AddFile(new DtoDocument.DtoDocAdd
                    {
                        Base64 = Convert.FromBase64String(obj.DocBase64 ?? ""),
                        MimeType = obj.DocMimeType,
                        FileName = obj.DocName,
                        FilePath = $"{App.DocPath.TaskTechDoc}/{obj.DocName}"
                    });
                    if (addDoc.Message.status == Message.Type.success)
                    {
                        obj.DocName = addDoc.FilePath;
                        UpdateTask.TechnicalDoc = obj.DocName;
                    }
                    
                }

                
                UpdateTask.Name = obj.Name;
                UpdateTask.Description = obj.Description;
                UpdateTask.PartyId = obj.PartyId;
                UpdateTask.Category = obj.Category;
                UpdateTask.PoNo = obj.PoNo;
               
                UpdateTask.PoDate = obj.PoDate;
                UpdateTask.StartDate = obj.StartDate;
                UpdateTask.EndDate = obj.EndDate;
                UpdateTask.Remarks = obj.Remarks;                            
                UpdateTask.UpdatedBy = User.Id;
                UpdateTask.UpdatedAt = DateTime.Now;

               
                db.Update(UpdateTask);

                //Update Task Item

                //Update Task Item Document
                //
                var DtoTaskList = await ListAsync(new DtoTaskFltr { ListId = new List<int> { obj.Id } }, User);
                foreach (var item in obj.TaskItem)
                {
                    TaskItem taskitem;

                    if (item.Id == 0)
                    {


                        taskitem = new TaskItem
                        {
                            TaskId = obj.Id,
                            Description = item.Description,
                            EstimatedDays = item.EstimatedDays,
                            StartDateTime = item.StartDateTime,
                            EndDateTime = item.EndDateTime,
                            Remarks = item.Remarks,
                            AssignToList = item.AssignToList,
                            Status = App.Status.Pending,

                            CreatedBy = User.Id,
                            CreatedAt = DateTime.Now,

                            UpdatedBy = User.Id,          
                            UpdatedAt = DateTime.Now      
                        };
                        if (!string.IsNullOrWhiteSpace(item.DocMimeType) && !string.IsNullOrEmpty(item.DocBase64))
                        {
                            var addDoc = Util.AddFile(new DtoDocument.DtoDocAdd
                            {
                                Base64 = Convert.FromBase64String(item.DocBase64 ?? ""),
                                MimeType = item.DocMimeType,
                                FileName = null,
                                FilePath = $"{App.DocPath.TaskTechDoc}"
                            });
                            if (addDoc.Message.status == Message.Type.success)
                            {
                                item.DocName = addDoc.FilePath;
                            }
                            else
                            {

                                objMsg.status = Message.Type.error;
                                objMsg.statusText += $"<br>Task Item Document '{item.DocName}' upload failed: {addDoc.Message.statusText}";
                                break;

                            }
                        }

                        db.TaskItem.Add(taskitem);
                        UpdateTask.Status = App.Status.Pending;
                        db.Update(UpdateTask);
                    }
                    else
                    {
                        var taskItem = await db.TaskItem
                            .FirstOrDefaultAsync(ti => ti.Id == item.Id && ti.TaskId == obj.Id && ti.Status == App.Status.Pending);

                        if (taskItem == null) continue;

                        if (!string.IsNullOrWhiteSpace(item.DocBase64) &&
                              !string.IsNullOrWhiteSpace(item.DocMimeType))
                        {
                            var uniqueFileName =
                                $"TaskItem_{taskItem.Id}_{Path.GetFileNameWithoutExtension(item.DocName)}";

                            var addDoc = Util.AddFile(new DtoDocument.DtoDocAdd
                            {
                                Base64 = Convert.FromBase64String(item.DocBase64),
                                MimeType = item.DocMimeType,
                                FileName = uniqueFileName,
                                FilePath = App.DocPath.TaskTechDoc
                            });

                            if (addDoc.Message.status == Message.Type.success)
                            {
                                taskItem.TechnicalDoc = addDoc.FilePath;
                            }
                        }

                        taskItem.Description = item.Description;
                        taskItem.EstimatedDays = item.EstimatedDays;
                        taskItem.StartDateTime = item.StartDateTime;
                        taskItem.EndDateTime = item.EndDateTime;
                        taskItem.Remarks = item.Remarks;
                        taskItem.AssignToList = item.AssignToList;
                        taskItem.UpdatedBy = User.Id;
                        taskItem.UpdatedAt = DateTime.Now;

                        db.Update(taskItem);

                        Message msg = await NotifyComplaintLogByEmail(taskItem, DtoTaskList, true);
                        objMsg.status = msg.status != Message.Type.success ? msg.status : objMsg.status;
                        objMsg.statusText += msg.statusText;

                    }
                }
                Message.Update(ref objMsg, await db.SaveChangesAsync(), "");

                return objMsg;                           
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> NotifyComplaintLogByEmail(TaskItem obj, List<DtoTaskList> tskobj,bool IsReg)
        {
            Message objMsg = new Message();
            try
            {               
                var ListId = obj.AssignToList.Select(ca => ca.Id).ToList();
                var ListUserEmail = await db.User.Where(x => App.ActiveStatus.Contains(x.Status) && ListId.Contains(x.Id)).Select(x => x.Email).ToListAsync();
                var AssignedPer = await db.User.Where(x =>App.ActiveStatus.Contains(x.Status) && x.Id == obj.UpdatedBy)
               .Select(x => new
                 {
                   x.Id,
                   x.ContactNo,
                   x.Email,
                   x.Name
                  })
               .FirstOrDefaultAsync();
                string assignedName = AssignedPer?.Name ?? "N/A";
                string assignedContact = AssignedPer?.ContactNo ?? "N/A";
                var UserMail = new MailMessage();
                ListUserEmail.ForEach(Email => { UserMail.To.Add(Email); });
                if (ListUserEmail.Any())
                {
                    var UserMailBody = new StringBuilder();
                    UserMailBody.AppendLine("<p>");
                    UserMailBody.AppendLine($"You have been assigned Task Id. {tskobj[0].Code}.<br/>");
                    UserMailBody.AppendLine($"Please Complete the Task As Per Estemated Time.");
                    UserMailBody.AppendLine("</p><hr/>");
                    UserMailBody.AppendLine($@"
                        <table style='width:90%; border-collapse: collapse;' border='0'>
                            <tbody>                              
                                  <tr><th style='text-align:left;'>Task Description</th><td><b>:</b> {obj.Description}</td></tr>
                                 <tr><th style='text-align:left;'>TaskItem Id</th><td><b>:</b> {obj.Id}</td></tr>
                                <tr><th style='text-align:left;'>Project</th><td><b>:</b> {tskobj[0].PartyDesc}</td></tr>
                               <tr><th style='text-align:left;'>Assigned By</th><td><b>:</b> {assignedName}</td></tr>
                                <tr><th style='text-align:left;'>Contac No</th><td><b>:</b> {assignedContact}</td></tr>
                                <tr><th style='text-align:left;'>Start Date</th><td><b>:</b> {obj.StartDateTime}</td></tr>
                                <tr><th style='text-align:left;'>End Date</th><td><b>:</b> {obj.EndDateTime}</td></tr>
                                 <tr><th style='text-align:left;'>Complete Task WithIn</th><td><b>:</b> {obj.EstimatedDays} Days</td></tr>
                               
                            </tbody>
                        </table>
                        <hr/>
                    ");
                    Thread.Sleep(1000);
                    Message objMsgEng = new Message();
                    Util.SentMail(db, UserMail, $"Assigned Task: {tskobj[0].Code}", UserMailBody.ToString(), "info", ref objMsgEng);
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
        public async Task<Message> Delete(DtoTaskFltr obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var task = await db.Task
                    .FirstOrDefaultAsync(t =>                      
                        t.Id == obj.ListId.First() && t.Status
                        == App.Status.Pending);

                if (task == null)
                {
                    Message.Error(ref objMsg, "Task not found for delete.");
                    return objMsg;
                }

                bool isCreator = task.CreatedBy == User.Id;
                bool isSysAdmin = User.Name.ToLower() == "sysadmin";

                if (!isCreator && !isSysAdmin)
                {
                    Message.Error(ref objMsg, "You are not authorized to delete this task.");
                    return objMsg;
                }

                Util.DeleteFile(task.TechnicalDoc ?? "");
                task.Status = App.Status.Delete;
                task.UpdatedBy = User.Id;
                task.UpdatedAt = DateTime.Now;

                var taskItems = await db.TaskItem
                    .Where(ti => ti.TaskId == task.Id && App.AllActiveStatus.Contains(ti.Status) && ti.Status == App.Status.Pending)
                    .ToListAsync();

                foreach (var item in taskItems)
                {
                    item.Status = App.Status.Delete;
                    item.UpdatedBy = User.Id;
                    item.UpdatedAt = DateTime.Now;


                }

                await db.SaveChangesAsync();

                Util.DeleteFile(task.TechnicalDoc ?? "");
                foreach (var item in taskItems)
                {
                    Util.DeleteFile(item.TechnicalDoc ?? "");
                }

                Message.Delete(ref objMsg, 1, "");

                obj.ListId.Add(obj.ListId.First());
                obj.ListStatus.Add(App.Status.Delete);
                objMsg.obj = await ListAsync(obj, User);
                objMsg.data = GetViewOption().data;
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> Enable(DtoTaskFltr obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var task = await db.Task
                    .FirstOrDefaultAsync(t =>
                        t.Status == App.Status.Delete &&
                        t.Id == obj.ListId.First());

                if (task == null)
                {
                    Message.Error(ref objMsg, "Task not found or already enabled.");
                    return objMsg;
                }

                task.Status = App.Status.Enable;
                task.UpdatedBy = User.Id;
                task.UpdatedAt = DateTime.Now;

                await db.SaveChangesAsync();

                Message.Enable(ref objMsg, 1, "");

                if (objMsg.status == Message.Type.success)
                {
                    obj.ListId.Add(obj.Id);
                    objMsg.obj = await ListAsync(obj, User);
                    objMsg.data = GetViewOption().data;
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
