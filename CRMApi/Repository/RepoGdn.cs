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
    public class RepoGdn
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoGdn(DBCRM _db)
        {
            db = _db;
        }
        public async Task<Message> GetViewOptionAsync()
        {
            Message objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();

                var ListStatus = await db.Voucher
                    .GroupBy(x => new { x.Status })
                    .Select(x => x.Key.Status.ToString())
                    .ToListAsync();

                Option.Status = await db.Setting
                    .Where(st => App.ActiveStatus.Contains(st.Status) &&
                                 st.Name == App.SettingName.Status &&
                                 ListStatus.Contains(st.Value))
                    .Select(st => new
                    {
                        st.Value,
                        st.Description
                    })
                    .ToListAsync();

                Option.Customer = await db.Party
                    .Where(x => App.ActiveStatus.Contains(x.Status))
                    .Select(x => new
                    {
                        x.Id,
                        x.Name
                    })
                    .ToListAsync();

                Option.ConName = await db.Voucher
                    .Where(x => App.ActiveStatus.Contains(x.Status))
                    .Select(x => new
                    {
                        x.Id,
                        x.ConName,
                    })
                    .ToListAsync();

                objMsg.data = Option;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
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
        public async Task<Message> ExportAsync(Voucher obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                // Get filtered voucher list
                var dbVoucher = await ListAsync(obj, User);

                if (dbVoucher == null || !dbVoucher.Any())
                {
                    Message.Error(ref objMsg, "Record not found");
                    return objMsg;
                }

                // Select ONLY required columns for export
                var voucherExport = dbVoucher.Select(vo => new
                {
                    VoucherType = vo.Type,
                    VoucherNo = vo.No,
                    VoucherDate = vo.Date.ToString("dd-MMM-yyyy"),
                    Party= vo.PartyDesc ?? "-",
                    ConsigneeName = vo.ConName,
                    Address = string.Join(", ",
                                new[] { vo.ConAdd1, vo.ConAdd2, vo.ConPincode, vo.ConPostOffice, vo.ConStateName }
                                .Where(x => !string.IsNullOrWhiteSpace(x))),
                    RefNo = vo.RefNo,
                    RefDate = vo.RefDate.HasValue ? vo.RefDate.Value.ToString("dd-MMM-yyyy") : "",
                    EwayNo = vo.EwayNo,
                    EwayDate = vo.EwayDate.HasValue ? vo.EwayDate.Value.ToString("dd-MMM-yyyy") : "",
                    Remarks = vo.Remarks,
                    Status = vo.StatusDesc
                }).ToList();

                DataTable objDataTable = Util.ListToDataTable(voucherExport);

                var objCompany = await db.Company
                    .FirstOrDefaultAsync(pt => App.ActiveStatus.Contains(pt.Status));

                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Company Info not found");
                    return objMsg;
                }

                objCompany.SheetName = "Gdn List";
                objCompany.ReportDesc =
                    $"Gdn List Generated On - {DateTime.Now:dd-MMM-yyyy HH:mm}";

                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);

                if (string.IsNullOrEmpty(objMsg.base64))
                    Message.Error(ref objMsg, "Record not found");
                else
                    Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }

            return objMsg;
        }
       
    }
}
