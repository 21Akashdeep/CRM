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
                Option.JntvtiGrade = await db.Setting.Where(x =>App.ActiveStatus.Contains(x.Status) && x.Name == "JntvtiCategory").Select(x => new
                 {
                    x.Id,
                    x.Description
                 }).ToListAsync();
                Option.Qualification = await db.Qualification.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
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
        public async Task<Message> GetAddOptionAsync()
        {
            Message objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();
                Option.JntvtiCategory = await db.Setting.Where(x => App.ActiveStatus.Contains(x.Status) && x.Name == App.SettingName.JntvtiCateGory).Select(x => new
                {
                    Id = x.Value,
                    x.Description
                }).ToListAsync();

                Option.qualification = await db.Qualification.Where(x => App.ActiveStatus.Contains(x.Status)).Select
                    (x => new
                    {
                        x.Id,
                        x.Description
                    }
                    ).ToListAsync();



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
            if (obj.ListJntvtiCategory.Any())
                dbEmployeeQuery = dbEmployeeQuery.Where(co => obj.ListJntvtiCategory.Contains(co.JntvtiCategory));
            if (obj.ListQualification.Any())
                dbEmployeeQuery = dbEmployeeQuery.Where(co => obj.ListQualification.Contains(co.QualificationId));


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
                    Email =em.Email,
                    JntvtiCategory = em.JntvtiCategory,
                    JntvtiCategoryDesc = sm.Description,
                    QualificationId = em.QualificationId,
                    ContactNo = em.ContactNo,
                    GuardianName = em.GuardianName,
                    IsEpfDeduct = em.IsEpfDeduct,
                    IsEsiDeduct = em.IsEsiDeduct,
                    EsiNo  = em.EsiNo,
                    GuardianRelation = em.GuardianRelation,
                    MaritalStatus = em.MaritalStatus,
                    UanNo = em.UanNo,
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
          
                obj.Name = Util.SanitizeInput(obj.Name, null) ?? "";
                obj.Gender = Util.SanitizeInput(obj.Gender, null) ?? "";
                obj.GuardianRelation = Util.SanitizeInput(obj.GuardianRelation, null) ?? "";
                obj.GuardianName = Util.SanitizeInput(obj.GuardianName, null) ?? "";
                obj.MaritalStatus = Util.SanitizeInput(obj.MaritalStatus, null) ?? "";
                obj.JntvtiCategory = Util.SanitizeInput(obj.JntvtiCategory, null) ?? "";
                obj.ContactNo = Util.SanitizeInput(obj.ContactNo, null) ?? "";
                obj.Email = Util.SanitizeInput(obj.Email, null) ?? "";
                obj.UanNo = Util.SanitizeInput(obj.UanNo, null) ?? "";
                obj.EsiNo = Util.SanitizeInput(obj.EsiNo, null) ?? "";

                var duplicate = await db.Employee.FirstOrDefaultAsync(ap =>
                    App.ActiveStatus.Contains(ap.Status) &&
                    (
                        ap.Name == obj.Name ||
                        (obj.Email != "" && ap.Email == obj.Email) ||
                        (obj.ContactNo != "" && ap.ContactNo == obj.ContactNo)
                    )
                );

                if (duplicate != null)
                {
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"Employee Name : {obj.Name} already exists.");

                    else if (obj.Email != "" && duplicate.Email == obj.Email)
                        Message.Duplicate(ref objMsg, $"Employee Email : {obj.Email} already exists.");

                    else if (obj.ContactNo != "" && duplicate.ContactNo == obj.ContactNo)
                        Message.Duplicate(ref objMsg, $"Employee Contact No : {obj.ContactNo} already exists.");

                    return objMsg;
                }


                obj.CreatedBy = User.Id;
                obj.UpdatedBy = User.Id;

                db.Employee.Add(obj);
                Message.Add(ref objMsg, await db.SaveChangesAsync(), "");

                if (objMsg.status == Message.Type.success)
                {
                    await db.Entry(obj).ReloadAsync();

                    obj.ListId.Add(obj.Id);

                    objMsg.obj = (await ListAsync(obj, User)).FirstOrDefault();
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
                Employee? obj = new Employee();
                obj.ListId.Add(Id);
                obj.ListStatus.AddRange(App.AllActiveStatus);
                obj = (await ListAsync(obj, User)).FirstOrDefault();
                if (obj == null)
                {
                    Message.Error(ref objMsg, "Employee did not find for edit.");
                    return objMsg;
                }
                var AddOption = await GetAddOptionAsync();
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

        private async Task GetAddOptionAsync(Employee obj, User user)
        {
            throw new NotImplementedException();
        }

        public async Task<Message> UpdateAsync(Employee obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                // 🔹 Sanitize inputs
               
                var dbEmployee = db.Employee.ToList();

                // 🔹 Duplicate Name check
                if (dbEmployee.Any(x => x.Name == obj.Name && x.Id != obj.Id))
                {
                    Message.Duplicate(ref objMsg, $"Employee Name {obj.Name}");
                    return objMsg;
                }

                // 🔹 Duplicate Contact check
                if (!string.IsNullOrEmpty(obj.ContactNo) &&
                    dbEmployee.Any(x => x.ContactNo == obj.ContactNo && x.Id != obj.Id))
                {
                    Message.Duplicate(ref objMsg, $"Contact No {obj.ContactNo}");
                    return objMsg;
                }

                // 🔹 Fetch existing record
                var UpdateEmployee = dbEmployee.FirstOrDefault(x => x.Id == obj.Id);
                if (UpdateEmployee == null)
                {
                    Message.Error(ref objMsg, "Employee not found for update.");
                    return objMsg;
                }

                // 🔹 Update fields
                UpdateEmployee.Name = obj.Name;
                UpdateEmployee.DOB = obj.DOB;
                UpdateEmployee.Gender = obj.Gender;
                UpdateEmployee.GuardianRelation = obj.GuardianRelation;
                UpdateEmployee.GuardianName = obj.GuardianName;
                UpdateEmployee.MaritalStatus = obj.MaritalStatus;
                UpdateEmployee.JntvtiCategory = obj.JntvtiCategory;
                UpdateEmployee.QualificationId = obj.QualificationId;
                UpdateEmployee.ContactNo = obj.ContactNo;
                UpdateEmployee.Email = obj.Email;
                UpdateEmployee.IsEpfDeduct = obj.IsEpfDeduct;
                UpdateEmployee.UanNo = obj.UanNo;
                UpdateEmployee.IsEsiDeduct = obj.IsEsiDeduct;
                UpdateEmployee.EsiNo = obj.EsiNo;
                UpdateEmployee.Status = obj.Status;

                

                // 🔹 Audit
                UpdateEmployee.UpdatedBy = User.Id;
                UpdateEmployee.UpdatedAt = DateTime.Now;

                db.Update(UpdateEmployee);

                Message.Update(ref objMsg, await db.SaveChangesAsync(), "");

                // 🔹 Return updated data
                if (objMsg.status == Message.Type.success)
                {
                    obj.ListId.Add(obj.Id);
                    objMsg.obj = ListAsync(obj, User);
                    objMsg.data = GetViewOptionAsync();
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
                var DeleteEmployee = db.Employee.Where(it => it.Id == Id).AsEnumerable().FirstOrDefault();
                if (DeleteEmployee == null)
                {
                    Message.Error(ref objMsg, "Item did not find for delete.");
                    return objMsg;
                }
                DeleteEmployee.Status = App.Status.Delete;
                DeleteEmployee.UpdatedBy = User.Id;
                DeleteEmployee.UpdatedAt = DateTime.Now;
                db.Update(DeleteEmployee);


                Message.Delete(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {
                    DeleteEmployee.ListStatus.Add(App.Status.Delete);
                    DeleteEmployee.ListId.Add(Id);
                    objMsg.obj = await ListAsync(DeleteEmployee, User);
                    objMsg.data = await GetViewOptionAsync();
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Enable(int Id, User User)
        {
            Message objMsg = new Message();
            try
            {
                var EnableEmployee = db.Employee.Where(it => it.Id == Id).AsEnumerable().FirstOrDefault();
                if (EnableEmployee == null)
                {
                    Message.Error(ref objMsg, "Item did not find for delete.");
                    return objMsg;
                }
                EnableEmployee.Status = App.Status.Enable;
                EnableEmployee.UpdatedBy = User.Id;
                EnableEmployee.UpdatedAt = DateTime.Now;
                db.Update(EnableEmployee);

                Message.Enable(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {
                    EnableEmployee.ListId.Add(Id);
                    objMsg.obj =  ListAsync(EnableEmployee, User);
                    objMsg.data =  GetViewOptionAsync();
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
