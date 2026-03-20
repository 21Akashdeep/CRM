using CRMApi.Dto;
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
    public class RepoStockOut
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        private readonly VoucherRepo repoVoucher;
        public RepoStockOut(DBCRM _db)
        {
            db = _db;
            repoVoucher = new VoucherRepo(db);
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
                Option.StockOutNo = await db.Voucher.Where(x => App.ActiveStatus.Contains(x.Status) && x.Type == "StockOut").Select(x => new
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

                    Item,
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
        public async Task<Message> AddAsync(Voucher obj, User User)
        {
            Message objMsg = new Message();

            try
            {
                obj.PartyId = obj.PartyId == 0 ? null : obj.PartyId;
                obj.Type = "StockOut";
                foreach (var item in obj.VoucherItem)
                {
                    item.Qty = -(item.Qty);
                }
                objMsg = await new VoucherRepo(db).AddAsync(obj, User);
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
            obj.ListType = new List<string> { "StockOut" };
            var voucher = await new VoucherRepo(db).ListAsync(obj, User);
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
                    Message.Error(ref objMsg, "StockIn was not found for edit.");
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
        public async Task<Message> UpdateAsync(Voucher obj, User User)
        {
            Message objMsg = new Message();

            try
            {
                obj.Type = "StockOut";
                foreach (var item in obj.VoucherItem)
                {
                    if(item.Qty >= 0)
                    item.Qty = -(item.Qty);
                }
                objMsg = objMsg = await new VoucherRepo(db).UpdateAsync(obj, User);

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
        public async Task<Message> ExportAsync(Voucher obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                // 1️⃣ Get Company
                var objCompany = await db.Company
                    .FirstOrDefaultAsync(pt => App.ActiveStatus.Contains(pt.Status));

                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "StockIn info not found");
                    return objMsg;
                }

                // 2️⃣ Get GDN List using same pattern
                var Gdn = (await ListAsync(obj, User)).Select(co => new
                {
                    co.Id,
                    co.No,
                    co.Date,
                    Party = co.PartyDesc,
                    co.ConName,
                    Status = co.StatusDesc,
                    co.CreatedByName,
                    co.CreatedAt,
                    co.UpdatedByName,
                    co.UpdatedAt
                }).ToList();


                DataTable objDataTable = Util.ListToDataTable(Gdn);

                objCompany.SheetName = "StockIn List";
                objCompany.ReportDesc = $"StockIn - {DateTime.Now:dd-MMM-yyyy}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);

                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }

            return objMsg;
        }

        public async Task<Message> GetItemDetailsAsync(StockItemFltrDto obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                objMsg.data = await repoVoucher.StockItemAsync(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
    }
}
