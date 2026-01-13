using CRMApi.Models;
using CRMApi.Repository;
using CRMApi.Services;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Net.Mail;
using static CRMApi.Dto.DtoTask;

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
                Option.GrnNo = await db.Voucher.Where(x => App.ActiveStatus.Contains(x.Status) && x.Type == "ReceiptNote").Select(x => new
                {
                    x.Id,
                    x.No,
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
                var Party = await db.Party.Where(par => App.ActiveStatus.Contains(par.Status) ).Select(par => new
                {
                    par.Id,
                    par.Code,
                    par.Name,
                    par.Description
                }).ToListAsync();
                var Type = await db.Setting.Where(x => App.ActiveStatus.Contains(x.Status) && x.Name == App.SettingName.VoucherType).Select(x => new
                {
                    x.Value,
                    x.Description
                }).ToListAsync();

                var State = await db.AdminDiv.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.Name

                }).ToListAsync();

                var ReasonCode = await db.Setting.Where(x => App.ActiveStatus.Contains(x.Status) && x.Name == App.SettingName.ReasonCode).Select(x => new
                {
                    x.Value,
                    x.Description

                }).ToListAsync();
                var Store = await db.Store.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {

                    x.Id,
                    x.Description

                }).ToListAsync();
                var Item = await (
                    from itm in db.Item
                    join unt in db.Unit on itm.UnitId equals unt.Id                   
                    select new
                    {
                        itm.Id,
                        itm.Code,
                        itm.Name,
                        itm.Description,
                        itm.UnitId,
                        UnitDesc = unt.Description,
                        
                    }
                ).ToListAsync();
                objMsg.data = new
                {
                    State,
                    Party,
                    Type,
                    Item,
                    ReasonCode,
                    Store,
                };
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
            obj.ListType = new List<string> {"ReceiptNote"};
            var voucher = await new RepoVoucher(db).ListAsync(obj, User);
            return voucher;
           
           
        }
        public async Task<Message> EditAsync(int Id, User User)
        {
            Message objMsg = new Message();
            try
            {
                objMsg.obj = (await ListAsync(new Voucher
                {
                    ListId = new List<int> { Id },
                    ListStatus = new List<int>(App.ActiveStatus) { App.Status.Delete }
                }, User)).FirstOrDefault();
                if (objMsg.obj == null)
                {
                    Message.Error(ref objMsg, "Voucher was not found for edit.");
                    return objMsg;
                }
                objMsg.data = (await GetAddOptionAsync()).data;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        //public async Task<Message> AddAsync(Voucher obj, User User)
        //{

        //}
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
