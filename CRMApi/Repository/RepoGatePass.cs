using CRMApi.Models;
using CRMApi.Services;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;
using System.Net.Mail;
using System.Runtime.Intrinsics.Arm;
using System.Text;

namespace CRMApi.Repository
{
    public class RepoGatePass
    {

        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoGatePass(DBCRM _db)
        {
            db = _db;
        }
        public async Task<Message> GetViewOptionAsync()
        {
            Message objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();
                var ListStatus = await db.GatePass.GroupBy(x => new { x.Status }).Select(x => x.Key.Status.ToString()).ToListAsync();
                Option.Status = await db.Setting.Where(st => App.ActiveStatus.Contains(st.Status) && st.Name == App.SettingName.Status && ListStatus.Contains(st.Value)).Select(st => new
                {
                    st.Value,
                    st.Description
                }).ToListAsync();
                Option.Location = await db.GatePass.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.Location
                }).ToListAsync();
                Option.Department = await db.GatePass.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.Department
                }).ToListAsync();

                Option.PassType =  await db.Setting.Where(x => App.ActiveStatus.Contains(x.Status) && x.Category == "PassType").Select (x => new
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
        public async Task<Message> GetAddOptionAsync()
        {
            Message objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();

                Option.Location = await db.GatePass.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.Location
                }).ToListAsync();
                Option.Department = await db.GatePass.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.Department
                }).ToListAsync();

                Option.PassType = await db.Setting.Where(x => App.ActiveStatus.Contains(x.Status) && x.Category == "PassType").Select(x => new
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
        public async Task<List<GatePass>> ListAsync(GatePass? obj, User User)
        {
            obj = obj == null ? new GatePass() : obj;
            obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : App.AllActiveStatus;
            var dbGatePassQuery = db.GatePass.Where(co => obj.ListStatus.Contains(co.Status)).AsQueryable();

            if (obj.ListId.Any()) dbGatePassQuery = dbGatePassQuery.Where(co => obj.ListId.Contains(co.Id));
            if (obj.ListLocation.Any()) dbGatePassQuery = dbGatePassQuery.Where(co => obj.ListLocation.Contains(co.Location));
            if (obj.ListDepartment.Any()) dbGatePassQuery = dbGatePassQuery.Where(co => obj.ListDepartment.Contains(co.Department));
            if (obj.ListGatePassType.Any()) dbGatePassQuery = dbGatePassQuery.Where(co => obj.ListGatePassType.Contains(co.Type));

            var GatePassList = await (
                from gp in dbGatePassQuery
                join em in db.Employee on gp.EmployeeId equals em.Id into emJoin
                from em in emJoin.DefaultIfEmpty()
                join st1 in db.Setting on new { Category = "PassType", Value = gp.Type } equals new { Category = st1.Category, Value = st1.Value } into st1Join
                from st1 in st1Join.DefaultIfEmpty()
                join st in db.Setting on new { Name = App.SettingName.Status, Value = gp.Status.ToString() }equals new { st.Name, st.Value } into stJoin
                from st in stJoin.DefaultIfEmpty()
                join cb in db.User on gp.CreatedBy equals cb.Id
                join ub in db.User on gp.UpdatedBy equals ub.Id

                select new GatePass
                {
                    Id = gp.Id,
                    GatePassNo = gp.GatePassNo,
                    Type = gp.Type,
                    TypeDesc = st1 != null ? st1.Description : gp.Type,
                    EmployeeId = gp.EmployeeId,
                    EmployeeDesc = em != null ? em.Name : "-",
                    ExpiryOn = gp.ExpiryOn,
                    TrainingExpiryOn = gp.TrainingExpiryOn,
                    MedicalExpiryOn = gp.MedicalExpiryOn,
                    LabourLicenseExpiryOn = gp.LabourLicenseExpiryOn,
                    Status = gp.Status,
                    StatusDesc = st != null ? st.Description : "",
                    StatusCss = st != null ? st.CssClass : "",
                    CreatedBy = gp.CreatedBy,
                    CreatedByName = cb.Name,
                    CreatedAt = gp.CreatedAt,
                    UpdatedBy = gp.UpdatedBy,
                    UpdatedByName = ub.Name,
                    UpdatedAt = gp.UpdatedAt,
                    IsEdit = gp.Status == App.Status.Enable,
                    IsDuplicate = true,
                    IsDelete = gp.Status == App.Status.Enable,
                    IsEnable = gp.Status == App.Status.Delete
                }
            ).ToListAsync();

            return GatePassList;
        }
        public async Task<Message> GetAsync(GatePass obj, User User)
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
        public async Task<Message> PrintAsync(GatePass obj, User User)
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
        //public async Task<Message> ExportAsync(GatePass obj, User User)
        //{
        //    Message objMsg = new Message();
        //    try
        //    {
        //        var dbGatePass = await ListAsync(obj, User);
        //        var GatePass = db.GatePass.Select(em => new
        //        {
        //            em.Name,
        //            Gender = em.Gender,
        //            em.DOB,
        //            Qualification = em.QualificationDesc,

        //            Status = em.StatusDesc,


        //        }).ToList();

        //        DataTable objDataTable = Util.ListToDataTable(GatePass);

        //        var objCompany = await db.Company.FirstOrDefaultAsync(pt => App.ActiveStatus.Contains(pt.Status));
        //        if (objCompany == null)
        //        {
        //            Message.Error(ref objMsg, "Company Info did found");
        //            return objMsg;
        //        }

        //        objCompany.SheetName = "GatePass Item Wise List";
        //        objCompany.ReportDesc = $"GatePass Item Wise List Generated On - {DateTime.Now.ToString("dd-MMM-yyyy HH:mm")}";
        //        objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);

        //        if (String.IsNullOrEmpty(objMsg.base64))
        //        {
        //            Message.Error(ref objMsg, "Record did not find");
        //        }
        //        else
        //        {
        //            Message.Success(ref objMsg, "Record found");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Message.Exception(ref objMsg, ex);
        //    }
        //    return objMsg;
        //}
        //public async Task<Message> AddAsync(GatePass obj, User User)
        //{
        //    Message objMsg = new Message();
        //    try
        //    {

        //        obj.Name = Util.SanitizeInput(obj.Name, null) ?? "";
        //        obj.Gender = Util.SanitizeInput(obj.Gender, null) ?? "";
        //        obj.GuardianRelation = Util.SanitizeInput(obj.GuardianRelation, null) ?? "";
        //        obj.GuardianName = Util.SanitizeInput(obj.GuardianName, null) ?? "";
        //        obj.MaritalStatus = Util.SanitizeInput(obj.MaritalStatus, null) ?? "";
        //        obj.JntvtiCategory = Util.SanitizeInput(obj.JntvtiCategory, null) ?? "";
        //        obj.ContactNo = Util.SanitizeInput(obj.ContactNo, null) ?? "";
        //        obj.Email = Util.SanitizeInput(obj.Email, null) ?? "";
        //        obj.UanNo = Util.SanitizeInput(obj.UanNo, null) ?? "";
        //        obj.EsiNo = Util.SanitizeInput(obj.EsiNo, null) ?? "";

        //        var duplicate = await db.GatePass.FirstOrDefaultAsync(ap =>
        //            App.ActiveStatus.Contains(ap.Status) &&
        //            (
        //                ap.GatePassNo == obj.GatePassNo ||
        //                (obj.Email != "" && ap.Email == obj.Email) ||
        //                (obj.ContactNo != "" && ap.ContactNo == obj.ContactNo)
        //            )
        //        );

        //        if (duplicate != null)
        //        {
        //            if (duplicate.Name == obj.Name)
        //                Message.Duplicate(ref objMsg, $"GatePass Name : {obj.Name} already exists.");

        //            else if (obj.Email != "" && duplicate.Email == obj.Email)
        //                Message.Duplicate(ref objMsg, $"GatePass Email : {obj.Email} already exists.");

        //            else if (obj.ContactNo != "" && duplicate.ContactNo == obj.ContactNo)
        //                Message.Duplicate(ref objMsg, $"GatePass Contact No : {obj.ContactNo} already exists.");

        //            return objMsg;
        //        }


        //        obj.CreatedBy = User.Id;
        //        obj.UpdatedBy = User.Id;

        //        db.GatePass.Add(obj);
        //        Message.Add(ref objMsg, await db.SaveChangesAsync(), "");

        //        if (objMsg.status == Message.Type.success)
        //        {
        //            await db.Entry(obj).ReloadAsync();

        //            obj.ListId.Add(obj.Id);

        //            objMsg.obj = (await ListAsync(obj, User)).FirstOrDefault();
        //            objMsg.data = (await GetViewOptionAsync()).data;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Message.Exception(ref objMsg, ex);
        //    }
        //    return objMsg;
        //}

        //public async Task<Message> EditAsync(int Id, User User)
        //{
        //    Message objMsg = new Message();
        //    try
        //    {
        //        GatePass? obj = new GatePass();
        //        obj.ListId.Add(Id);
        //        obj.ListStatus.AddRange(App.AllActiveStatus);
        //        obj = (await ListAsync(obj, User)).FirstOrDefault();
        //        if (obj == null)
        //        {
        //            Message.Error(ref objMsg, "GatePass did not find for edit.");
        //            return objMsg;
        //        }
        //        var AddOption = await GetAddOptionAsync();
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

        //private async Task GetAddOptionAsync(GatePass obj, User user)
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<Message> UpdateAsync(GatePass obj, User User)
        //{
        //    Message objMsg = new Message();
        //    try
        //    {
        //        // 🔹 Sanitize inputs

        //        var dbGatePass = db.GatePass.ToList();

        //        // 🔹 Duplicate Name check
        //        if (dbGatePass.Any(x => x.Name == obj.Name && x.Id != obj.Id))
        //        {
        //            Message.Duplicate(ref objMsg, $"GatePass Name {obj.Name}");
        //            return objMsg;
        //        }

        //        // 🔹 Duplicate Contact check
        //        if (!string.IsNullOrEmpty(obj.ContactNo) &&
        //            dbGatePass.Any(x => x.ContactNo == obj.ContactNo && x.Id != obj.Id))
        //        {
        //            Message.Duplicate(ref objMsg, $"Contact No {obj.ContactNo}");
        //            return objMsg;
        //        }

        //        // 🔹 Fetch existing record
        //        var UpdateGatePass = dbGatePass.FirstOrDefault(x => x.Id == obj.Id);
        //        if (UpdateGatePass == null)
        //        {
        //            Message.Error(ref objMsg, "GatePass not found for update.");
        //            return objMsg;
        //        }

        //        // 🔹 Update fields
        //        UpdateGatePass.Name = obj.Name;
        //        UpdateGatePass.DOB = obj.DOB;
        //        UpdateGatePass.Gender = obj.Gender;
        //        UpdateGatePass.GuardianRelation = obj.GuardianRelation;
        //        UpdateGatePass.GuardianName = obj.GuardianName;
        //        UpdateGatePass.MaritalStatus = obj.MaritalStatus;
        //        UpdateGatePass.JntvtiCategory = obj.JntvtiCategory;
        //        UpdateGatePass.QualificationId = obj.QualificationId;
        //        UpdateGatePass.ContactNo = obj.ContactNo;
        //        UpdateGatePass.Email = obj.Email;
        //        UpdateGatePass.IsEpfDeduct = obj.IsEpfDeduct;
        //        UpdateGatePass.UanNo = obj.UanNo;
        //        UpdateGatePass.IsEsiDeduct = obj.IsEsiDeduct;
        //        UpdateGatePass.EsiNo = obj.EsiNo;
        //        UpdateGatePass.Status = obj.Status;



        //        // 🔹 Audit
        //        UpdateGatePass.UpdatedBy = User.Id;
        //        UpdateGatePass.UpdatedAt = DateTime.Now;

        //        db.Update(UpdateGatePass);

        //        Message.Update(ref objMsg, await db.SaveChangesAsync(), "");

        //        // 🔹 Return updated data
        //        if (objMsg.status == Message.Type.success)
        //        {
        //            obj.ListId.Add(obj.Id);
        //            objMsg.obj = ListAsync(obj, User);
        //            objMsg.data = GetViewOptionAsync();
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
        //        var DeleteGatePass = db.GatePass.Where(it => it.Id == Id).AsEnumerable().FirstOrDefault();
        //        if (DeleteGatePass == null)
        //        {
        //            Message.Error(ref objMsg, "Item did not find for delete.");
        //            return objMsg;
        //        }
        //        DeleteGatePass.Status = App.Status.Delete;
        //        DeleteGatePass.UpdatedBy = User.Id;
        //        DeleteGatePass.UpdatedAt = DateTime.Now;
        //        db.Update(DeleteGatePass);


        //        Message.Delete(ref objMsg, db.SaveChanges(), "");
        //        if (objMsg.status == Message.Type.success)
        //        {
        //            DeleteGatePass.ListStatus.Add(App.Status.Delete);
        //            DeleteGatePass.ListId.Add(Id);
        //            objMsg.obj = await ListAsync(DeleteGatePass, User);
        //            objMsg.data = await GetViewOptionAsync();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Message.Exception(ref objMsg, ex);
        //    }
        //    return objMsg;
        //}
        //public Message Enable(int Id, User User)
        //{
        //    Message objMsg = new Message();
        //    try
        //    {
        //        var EnableGatePass = db.GatePass.Where(it => it.Id == Id).AsEnumerable().FirstOrDefault();
        //        if (EnableGatePass == null)
        //        {
        //            Message.Error(ref objMsg, "Item did not find for delete.");
        //            return objMsg;
        //        }
        //        EnableGatePass.Status = App.Status.Enable;
        //        EnableGatePass.UpdatedBy = User.Id;
        //        EnableGatePass.UpdatedAt = DateTime.Now;
        //        db.Update(EnableGatePass);

        //        Message.Enable(ref objMsg, db.SaveChanges(), "");
        //        if (objMsg.status == Message.Type.success)
        //        {
        //            EnableGatePass.ListId.Add(Id);
        //            objMsg.obj = ListAsync(EnableGatePass, User);
        //            objMsg.data = GetViewOptionAsync();
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
