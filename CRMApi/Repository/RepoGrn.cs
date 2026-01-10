using CRMApi.Models;
using CRMApi.Services;
using CRMApi.Repository;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Net.Mail;

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
                Option.Customer = await db.Party.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.Name
                }).ToListAsync();
                Option.ConName = await db.Voucher.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.ConName,
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
        //public async Task<Message> GetAddOptionAsync()
        //{
        //    Message objMsg = new Message();
        //    try
        //    {

        //         dynamic Option = new ExpandoObject();
        //        //Option.SupportMode = await db.Setting.Where(x => App.ActiveStatus.Contains(x.Status) && x.Name == App.SettingName.SupportMode).Select(x => new
        //        //{
        //        //    Id = x.Value,
        //        //    x.Description
        //        //}).ToListAsync();
        //        //var dbCustomer = await new RepoCustomer(db).ListAsync(null, User);
        //        //Option.Customer = dbCustomer.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
        //        //{
        //        //    x.Id,
        //        //    x.Description,
        //        //    x.Address1,
        //        //    x.Address2,
        //        //    x.PinCode,
        //        //    x.PostOffice,
        //        //    x.District,
        //        //    x.AdminDivId,
        //        //    x.CountryId,
        //        //    x.ContactNo,
        //        //    x.Email,                    
        //        //    x.LocationDesc,
        //        //    SubText = Util.AddressDesc(new Composite.AddressDesc { Add1 = x.Address1, Add2 = x.Address2, PinCode = x.PinCode, PostOffice = x.PostOffice, OtherText = x.GstNo })
        //        //}).ToList();
        //        //Option.AdminDiv = await db.AdminDiv.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
        //        //{
        //        //    x.Id,
        //        //    x.Description
        //        //}).ToListAsync();
        //        //Option.Country = await db.Country.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
        //        //{
        //        //    x.Id,
        //        //    x.Description
        //        //}).ToListAsync();                
        //        //Option.Department = await db.Complaint.Where(x => App.AllActiveStatus.Contains(x.Status)).GroupBy(x => new { x.Department }).Select(x => new
        //        //{
        //        //    Value = x.Key.Department,
        //        //    Description = x.Key.Department
        //        //}).ToListAsync();
        //        //Option.Priority = await db.Setting.Where(x => App.ActiveStatus.Contains(x.Status) && x.Name == App.SettingName.Priority).Select(x => new
        //        //{
        //        //    Id = x.Value,
        //        //    x.Description
        //        //}).ToListAsync();
        //        //var dbComplaintAssign = db.ComplaintAssign.Where(x => App.AllActiveStatus.Contains(x.Status)).AsQueryable();
        //        //Option.ComplaintAssing = await (
        //        //    from usr in db.User
        //        //    join ulo in db.UserLocation on usr.Id equals ulo.UserId
        //        //    join loc in db.Location on ulo.LocationId equals loc.Id
        //        //    join cus in db.Customer on ulo.LocationId equals cus.LocationId
        //        //    join coa in dbComplaintAssign on new {UserId = usr.Id, ComplaintId = obj.Id} equals new { coa.UserId, coa.ComplaintId } into ComplaintAssign
        //        //    from coa in ComplaintAssign.DefaultIfEmpty()
        //        //    where App.ActiveStatus.Contains(usr.Status) && cus.Id == obj.CustomerId
        //        //    select new
        //        //    {
        //        //        Id = coa != null ? coa.Id : 0,
        //        //        ComplaintId = coa != null ? coa.ComplaintId : 0,
        //        //        UserId = usr.Id,
        //        //        UserName = usr.Name,
        //        //        LocationDesc = loc.Description,
        //        //        IsAdded = coa != null,
        //        //    }
        //        //).ToListAsync();                
        //        objMsg.data = Option;
        //        Message.Success(ref objMsg, "Record found");

        //    }
        //    catch (Exception ex)
        //    {
        //        Message.Exception(ref objMsg, ex);
        //    }
        //    return objMsg;
        //}
        public async Task<List<Voucher>> ListAsync(Voucher? obj, User User)
        {

            obj ??= new Voucher();

            var voucher = await new RepoVoucher(db).ListAsync(obj, User);

            return voucher;
           
           
        }
        public async Task<Message> PrintAsync(Voucher obj, User User)
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
    //    public async Task<Message> ExportAsync(Voucher obj, User User)
    //    {
    //        Message objMsg = new Message();
    //        try
    //        {
    //            // Get filtered voucher list
    //            var dbVoucher = await ListAsync(obj, User);

    //            if (dbVoucher == null || !dbVoucher.Any())
    //            {
    //                Message.Error(ref objMsg, "Record not found");
    //                return objMsg;
    //            }

    //            // Select ONLY required columns for export
    //            var voucherExport = dbVoucher.Select(vo => new
    //            {
    //                VoucherType = vo.Type,
    //                VoucherNo = vo.No,
    //                VoucherDate = vo.Date.ToString("dd-MMM-yyyy"),
    //                Customer = vo.CustomerDesc ?? "-",
    //                ConsigneeName = vo.ConName,
    //                Address = string.Join(", ",
    //                            new[] { vo.ConAdd1, vo.ConAdd2, vo.ConPincode, vo.ConPostOffice, vo.ConStateName }
    //                            .Where(x => !string.IsNullOrWhiteSpace(x))),
    //                RefNo = vo.RefNo,
    //                RefDate = vo.RefDate.HasValue ? vo.RefDate.Value.ToString("dd-MMM-yyyy") : "",
    //                EwayNo = vo.EwayNo,
    //                EwayDate = vo.EwayDate.HasValue ? vo.EwayDate.Value.ToString("dd-MMM-yyyy") : "",
    //                Remarks = vo.Remarks,
    //                Status = vo.StatusDesc
    //            }).ToList();

    //            DataTable objDataTable = Util.ListToDataTable(voucherExport);

    //            // Company Info
    //            var objCompany = await db.Company
    //                .FirstOrDefaultAsync(pt => App.ActiveStatus.Contains(pt.Status));

    //            if (objCompany == null)
    //            {
    //                Message.Error(ref objMsg, "Company Info not found");
    //                return objMsg;
    //            }

    //            objCompany.SheetName = "Grn List";
    //            objCompany.ReportDesc =
    //                $"Grn List Generated On - {DateTime.Now:dd-MMM-yyyy HH:mm}";

    //            objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);

    //            if (string.IsNullOrEmpty(objMsg.base64))
    //                Message.Error(ref objMsg, "Record not found");
    //            else
    //                Message.Success(ref objMsg, "Record found");
    //        }
    //        catch (Exception ex)
    //        {
    //            Message.Exception(ref objMsg, ex);
    //        }

    //        return objMsg;
    //    }
    }
}
