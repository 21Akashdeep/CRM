using CRMApi.Models;
using CRMApi.Services;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;
using System.Net.Mail;
using System.Text;

namespace CRMApi.Repository
{
    public class RepoEmployee
    {

        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoEmployee(DBCRM _db)
        {
            db = _db;
        }
        public async Task<Message> GetViewOptionAsync()
        {
            Message objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();
                var ListStatus = await db.Employee.GroupBy(x => new { x.Status }).Select(x => x.Key.Status.ToString()).ToListAsync();
                Option.Status = await db.Setting.Where(st => App.ActiveStatus.Contains(st.Status) && st.Name == App.SettingName.Status && ListStatus.Contains(st.Value)).Select(st => new
                {
                    st.Value,
                    st.Description
                }).ToListAsync();
                Option.Employee = await db.Employee.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.Name
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
        public async Task<Message> GetAddOptionAsync(Employee obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();
                Option.Qualification = await db.Setting.Where(x => App.ActiveStatus.Contains(x.Status) && x.Name == App.SettingName.JntvtiCateGory).Select(x => new
                {
                    Id = x.Value,
                    x.Description
                }).ToListAsync();



                //var dbEmployeeAssign = db.EmployeeAssign.Where(x => App.AllActiveStatus.Contains(x.Status)).AsQueryable();
                //Option.EmployeeAssing = await (
                //    from usr in db.User
                //    join ulo in db.UserLocation on usr.Id equals ulo.UserId
                //    join loc in db.Location on ulo.LocationId equals loc.Id
                //    join cus in db.Customer on ulo.LocationId equals cus.LocationId
                //    join coa in dbEmployeeAssign on new { UserId = usr.Id, EmployeeId = obj.Id } equals new { coa.UserId, coa.EmployeeId } into EmployeeAssign
                //    from coa in EmployeeAssign.DefaultIfEmpty()
                //    where App.ActiveStatus.Contains(usr.Status) && cus.Id == obj.CustomerId
                //    select new
                //    {
                //        Id = coa != null ? coa.Id : 0,
                //        EmployeeId = coa != null ? coa.EmployeeId : 0,
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
        public async Task<List<Employee>> ListAsync(Employee? obj, User User)
        {
            obj = obj == null ? new Employee() : obj;
            obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : App.AllActiveStatus;
            var dbEmployeeQuery = db.Employee.Where(co => obj.ListStatus.Contains(co.Status)).AsQueryable();

            if (obj.ListId.Any())
                dbEmployeeQuery = dbEmployeeQuery.Where(co => obj.ListId.Contains(co.Id));
            if (obj.ListGender.Any())
                dbEmployeeQuery = dbEmployeeQuery.Where(co => obj.ListGender.Contains(co.Gender));


            //Get Employee List
            var Employee = (
                from em in dbEmployeeQuery
                join sm in db.Setting on new { Name = App.SettingName.JntvtiCateGory, Value = em.JntvtiCategory } equals new { sm.Name, sm.Value }
                join qu in db.Qualification on em.QualificationId equals qu.Id       
                join st in db.Setting on new { Name = App.SettingName.Status, Value = em.Status.ToString() } equals new { st.Name, st.Value }
                join cb in db.User on em.CreatedBy equals cb.Id
                join ub in db.User on em.UpdatedBy equals ub.Id
                select new Employee
                {
                    Id = em.Id,
                    Name = em.Name,
                    DOB = em.DOB,
                    Gender = em.Gender,
                    Email=em.Email,
                    JntvtiCategory=sm.Value,
                    ContactNo = em.ContactNo,
                    GuardianName = em.GuardianName,
                    IsEpfDeduct = em.IsEpfDeduct,
                    IsEsiDeduct = em.IsEsiDeduct,
                    QualificationDesc = qu.Description,
                    Status = em.Status,
                    StatusDesc = st.Description,
                    StatusCss = st.CssClass,
                    CreatedBy = em.CreatedBy,
                    CreatedByName = cb.Name,
                    CreatedAt = em.CreatedAt,
                    UpdatedBy = em.UpdatedBy,
                    UpdatedByName = ub.Name,
                    UpdatedAt = em.UpdatedAt,
                    IsEdit = em.Status == App.Status.Enable ? true : false,
                    IsDuplicate = true,
                    IsDelete = em.Status == App.Status.Enable ? true : false,
                    IsEnable = em.Status == App.Status.Delete ? true : false

                }
            ).ToList();
            return Employee;
        }
        public async Task<Message> GetAsync(Employee obj, User User)
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
        public async Task<Message> PrintAsync(Employee obj, User User)
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
        public async Task<Message> ExportAsync(Employee obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var dbEmployee = await ListAsync(obj, User);
                var Employee = db.Employee.Select(em => new
                {
                    em.Name,
                    Gender = em.Gender,
                    em.DOB,
                    Qualification = em.QualificationDesc,
                   
                    Status = em.StatusDesc,
               
                   
                }).ToList();

                DataTable objDataTable = Util.ListToDataTable(Employee);

                var objCompany = await db.Company.FirstOrDefaultAsync(pt => App.ActiveStatus.Contains(pt.Status));
                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Company Info did found");
                    return objMsg;
                }

                objCompany.SheetName = "Employee Item Wise List";
                objCompany.ReportDesc = $"Employee Item Wise List Generated On - {DateTime.Now.ToString("dd-MMM-yyyy HH:mm")}";
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
        public async Task<Message> AddAsync(Employee obj, User User)
        {
            
            Message objMsg = new Message();
            try
            {
                //Sanitize Input
                //obj.Code = Util.SanitizeInput(obj.Code, null) ?? "";
                obj.Name = Util.SanitizeInput(obj.Name, null) ?? "";
                var duplicate = db.Employee.FirstOrDefault(ap => App.ActiveStatus.Contains(ap.Status) && (ap.Name == obj.Name));

                if (duplicate != null)
                {
                    //if (duplicate.Code == obj.Code)
                    //    Message.Duplicate(ref objMsg, $"Employee Code : {obj.Code} already exists.");
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"Employee Name : {obj.Name} already exists.");                
                    return objMsg;
                }
                obj.CreatedBy = User.Id;
                obj.UpdatedBy = User.Id;
                db.Add(obj);
                Message.Add(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {
                    db.Entry(obj).Reload();
                    obj.ListId.Add(obj.Id);
                    objMsg.obj = await ListAsync(obj, User);
                    objMsg.data = await GetViewOptionAsync();
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        //public async Task<Message> EditAsync(int Id, User User)
        //{
        //    Message objMsg = new Message();
        //    try
        //    {
        //        Employee? obj = new Employee();
        //        obj.ListId.Add(Id);
        //        obj.ListStatus.AddRange(App.AllActiveStatus);
        //        obj = (await ListAsync(obj, User)).FirstOrDefault();
        //        if (obj == null)
        //        {
        //            Message.Error(ref objMsg, "Employee did not find for edit.");
        //            return objMsg;
        //        }
        //        var AddOption = await GetAddOptionAsync(obj, User);
        //        objMsg.obj = obj;
        //        objMsg.data = AddOption.data;
        //        Message.Success(ref objMsg, "Record found");
        //    }
        //    catch (Exception ex)
        //    {
        //        Message.Exception(ref objMsg, ex);
        //    }
        //    return objMsg;
        //}
        //public async Task<Message> UpdateAsync(Employee obj, User User)
        //{
        //    Message objMsg = new Message();
        //    try
        //    {
        //        var UpdateEmployee = await db.Employee.FindAsync(obj.Id);
        //        if (UpdateEmployee == null)
        //        {
        //            Message.Error(ref objMsg, "Employee did not find for update.");
        //            return objMsg;
        //        }
        //        UpdateEmployee.Date = obj.Date;
        //        UpdateEmployee.SupportMode = obj.SupportMode;
        //        UpdateEmployee.CustomerId = obj.CustomerId;
        //        UpdateEmployee.Address1 = obj.Address1;
        //        UpdateEmployee.Address2 = obj.Address2;
        //        UpdateEmployee.PinCode = obj.PinCode;
        //        UpdateEmployee.PostOffice = obj.PostOffice;
        //        UpdateEmployee.District = obj.District;
        //        UpdateEmployee.AdminDivId = obj.AdminDivId;
        //        UpdateEmployee.EmployeeId = obj.EmployeeId;
        //        UpdateEmployee.ContactPerson = obj.ContactPerson;
        //        UpdateEmployee.ContactNo = obj.ContactNo;
        //        UpdateEmployee.Email = obj.Email;
        //        UpdateEmployee.Department = obj.Department;
        //        UpdateEmployee.Priority = obj.Priority;
        //        UpdateEmployee.Problem = obj.Problem;
        //        UpdateEmployee.UpdatedBy = User.Id;
        //        UpdateEmployee.UpdatedAt = DateTime.Now;
        //        db.Update(UpdateEmployee);
        //        //Update Employee Assing
        //        var UpdateEmployeeAssign = await db.EmployeeAssign.Where(x => x.EmployeeId == obj.Id).ToListAsync();
        //        UpdateEmployeeAssign.ForEach(x =>
        //        {
        //            var EmployeeAssign = obj.EmployeeAssign.FirstOrDefault(ca => ca.Id == x.Id);
        //            x.Status = EmployeeAssign != null ? UpdateEmployee.Status : App.Status.Cancel;
        //            x.UpdatedBy = User.Id;
        //            x.UpdatedAt = DateTime.Now;
        //        });
        //        db.UpdateRange(UpdateEmployeeAssign);
        //        //Add Employee Assing
        //        var AddEmployeeAssign = obj.EmployeeAssign.Where(x => x.Id == 0 && !UpdateEmployeeAssign.Any(x1 => x1.EmployeeId == x.EmployeeId && x1.UserId == x.UserId)).ToList();
        //        AddEmployeeAssign.ForEach(x =>
        //        {
        //            x.EmployeeId = obj.Id;
        //            x.Status = UpdateEmployee.Status;
        //            x.CreatedBy = User.Id;
        //            x.UpdatedBy = User.Id;
        //        });
        //        db.AddRange(AddEmployeeAssign);
        //        Message.Update(ref objMsg, (await db.SaveChangesAsync()), "");
        //        if (objMsg.status == Message.Type.success)
        //        {
        //            obj.ListId.Add(obj.Id);
        //            var Employee = (await ListAsync(obj, User)).FirstOrDefault();
        //            if (Employee != null)
        //            {
        //                Employee.EmployeeAssign = obj.EmployeeAssign;
        //                Message msg = await NotifyEmployeeLogByEmail(Employee, false);
        //                objMsg.status = msg.status != Message.Type.success ? msg.status : objMsg.status;
        //                objMsg.statusText += msg.statusText;
        //            }
        //            objMsg.obj = Employee;
        //            objMsg.data = (await GetViewOptionAsync()).data;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Message.Exception(ref objMsg, ex);
        //    }
        //    return objMsg;
        //}
        //public async Task<Message> DeleteAsync(int Id, User User)
        //{
        //    Message objMsg = new Message();
        //    try
        //    {
        //        var DeleteEmployee = await db.Employee.FindAsync(Id);
        //        if (DeleteEmployee == null)
        //        {
        //            Message.Error(ref objMsg, "Employee did not find for update.");
        //            return objMsg;
        //        }
        //        DeleteEmployee.Status = App.Status.Delete;
        //        DeleteEmployee.UpdatedBy = User.Id;
        //        DeleteEmployee.UpdatedAt = DateTime.Now;
        //        db.Update(DeleteEmployee);
        //        //Delete Employee Assign
        //        var DeleteEmployeeAssign = await db.EmployeeAssign.Where(x => x.EmployeeId == Id).ToListAsync();
        //        if (DeleteEmployeeAssign.Any())
        //        {
        //            foreach (var item in DeleteEmployeeAssign)
        //            {
        //                item.Status = DeleteEmployee.Status;
        //                item.UpdatedBy = User.Id;
        //                item.UpdatedAt = DateTime.Now;
        //                db.Update(item);
        //            }
        //        }
        //        //Delete Employee Status
        //        var DeleteEmployeeStatus = await db.EmployeeStatus.Where(x => x.EmployeeId == Id).ToListAsync();
        //        if (DeleteEmployeeStatus.Any())
        //        {
        //            foreach (var item in DeleteEmployeeStatus)
        //            {
        //                item.Status = DeleteEmployee.Status;
        //                item.UpdatedBy = User.Id;
        //                item.UpdatedAt = DateTime.Now;
        //                db.Update(item);
        //            }
        //        }
        //        Message.Delete(ref objMsg, (await db.SaveChangesAsync()), "");
        //        if (objMsg.status == Message.Type.success)
        //        {
        //            DeleteEmployee.ListId.Add(Id);
        //            DeleteEmployee.ListStatus.Add(App.Status.Delete);
        //            objMsg.obj = (await ListAsync(DeleteEmployee, User)).FirstOrDefault();
        //            objMsg.data = (await GetViewOptionAsync()).data;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Message.Exception(ref objMsg, ex);
        //    }
        //    return objMsg;
        //}
        //public async Task<Message> NotifyEmployeeLogByEmail(Employee obj, bool IsReg)
        //{
        //    Message objMsg = new Message();
        //    try
        //    {
        //        //Sent Email To Customer
        //        if (IsReg)
        //        {
        //            string CustomerMailBody = $"Your Employee No. {obj.Code} has been registered. ";
        //            var CustomerMail = new MailMessage();
        //            CustomerMail.To.Add(obj.Email);
        //            Util.SentMail(db, CustomerMail, $"Employee Registered: {obj.Code}", CustomerMailBody, "info", ref objMsg);
        //            objMsg.statusText = objMsg.status == Message.Type.success ? "Email has been sent to the customer." : "Failed to send email to the customer.";
        //        }
        //        else
        //        {
        //            objMsg.status = Message.Type.success;
        //        }
        //        //Sent Email To Serice Eng
        //        var ListId = obj.EmployeeAssign.Select(ca => ca.UserId).ToList();
        //        var ListUserEmail = await db.User.Where(x => App.ActiveStatus.Contains(x.Status) && ListId.Contains(x.Id)).Select(x => x.Email).ToListAsync();
        //        var UserMail = new MailMessage();
        //        ListUserEmail.ForEach(Email => { UserMail.To.Add(Email); });
        //        if (ListUserEmail.Any())
        //        {
        //            var UserMailBody = new StringBuilder();
        //            UserMailBody.AppendLine("<p>");
        //            UserMailBody.AppendLine($"You have been assigned Employee No. {obj.Code}.<br/>");
        //            UserMailBody.AppendLine($"Please resolve it at the earliest possible.");
        //            UserMailBody.AppendLine("</p><hr/>");
        //            UserMailBody.AppendLine($@"
        //                <table style='width:90%; border-collapse: collapse;' border='0'>
        //                    <tbody>
        //                        <tr><th colspan='2' style='text-align:left;'>Employee Info :</th></tr>
        //                        <tr><th style='text-align:left;'>Customer</th><td><b>:</b> {obj.CustomerDesc}</td></tr>
        //                        <tr><th style='text-align:left;'>Location</th><td><b>:</b> {obj.CustomerLocation}</td></tr>
        //                        <tr><th style='text-align:left;'>Department</th><td><b>:</b> {obj.Department}</td></tr>
        //                        <tr><th style='text-align:left;'>Contact Person</th><td><b>:</b> {obj.ContactPerson}</td></tr>
        //                        <tr><th style='text-align:left;'>Contact No.</th><td><b>:</b> {obj.ContactNo}</td></tr>
        //                        <tr><th style='text-align:left;'>Email</th><td><b>:</b> {obj.Email}</td></tr>
        //                        <tr><th style='text-align:left;'>Address</th><td><b>:</b> {obj.CustomerAddress}</td></tr>
        //                        <tr><th style='text-align:left;'>priority</th><td><b>:</b> {obj.PriorityDesc}</td></tr>
        //                        <tr><td colspan='2' style='text-align:left;'><b>Problem :</b><div>{obj.Problem.Replace("\r\n", "<br/>")}</div></td></tr>
        //                    </tbody>
        //                </table>
        //                <hr/>
        //            ");
        //            Thread.Sleep(1000);
        //            Message objMsgEng = new Message();
        //            Util.SentMail(db, UserMail, $"Employee Assigned: {obj.Code}", UserMailBody.ToString(), "info", ref objMsgEng);
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
        //public async Task<Message> Close(int Id, User User)
        //{
        //    Message objMsg = new Message();
        //    try
        //    {
        //        var dbEmployeeQuery = db.Employee.Where(co => co.Id == Id).AsQueryable();
        //        if (User.UserType != App.UserType.SysAdmin)
        //        {
        //            dbEmployeeQuery = dbEmployeeQuery.Where(co => co.Status == App.Status.RequestForClose);
        //        }
        //        var Employee = await dbEmployeeQuery.FirstOrDefaultAsync();
        //        if (Employee == null)
        //        {
        //            Message.Error(ref objMsg, "Employee did not for close");
        //            return objMsg;
        //        }
        //        Employee.Status = App.Status.Completed;
        //        Employee.UpdatedBy = User.Id;
        //        Employee.UpdatedAt = DateTime.Now;
        //        db.Update(Employee);
        //        Message.Close(ref objMsg, (await db.SaveChangesAsync()), "");
        //        if (objMsg.status == Message.Type.success)
        //        {
        //            Employee.ListId.Add(Id);
        //            objMsg.obj = (await ListAsync(Employee, User)).FirstOrDefault();
        //            objMsg.data = (await GetViewOptionAsync()).data;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Message.Exception(ref objMsg, ex);
        //    }
        //    return objMsg;
        //}

    }
}
