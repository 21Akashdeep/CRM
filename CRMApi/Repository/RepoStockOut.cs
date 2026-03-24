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
        public async Task<List<Voucher>> ListAsync(Voucher? obj, User User)
        {
            obj ??= new Voucher();
            obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : App.ActiveStatus;

            var vc1 = await db.Voucher.Where(x => App.VoucherType.StockOut == x.Type).ToListAsync();

            var dbVoucher = (
                from vc in db.Voucher
                join pr in db.Party on vc.PartyId equals pr.Id  into prj
                from pr in prj.DefaultIfEmpty()
                join st in db.Store on vc.StoreId equals st.Id
                join sts in db.Setting on new { Category = App.SettingName.Status, Value = vc.Status.ToString() } equals new { sts.Category, sts.Value }
                join cby in db.User on vc.CreatedBy equals cby.Id
                join uby in db.User on vc.UpdatedBy equals uby.Id
                where
                       vc.Type == App.VoucherType.StockOut &&
                       (obj.FromDate == DateTime.MinValue || vc.Date.Date >= obj.FromDate.Date) &&
                       (obj.ToDate == DateTime.MinValue || vc.Date.Date <= obj.ToDate.Date) &&
                       (!obj.ListPartyId.Any() || obj.ListPartyId.Contains(vc.PartyId ?? 0)) &&
                        obj.ListStatus.Contains(vc.Status) &&
                       (!obj.ListId.Any() || obj.ListId.Contains(vc.Id))
                select new Voucher
                {
                    Id = vc.Id,
                    No = vc.No,
                    Type = vc.Type,
                    Date = vc.Date,
                    StoreId = vc.StoreId,
                    StoreDesc = st.Description,
                    PartyId = vc.PartyId,
                    PartyDesc = pr.Description,                  
                    Remarks = vc.Remarks,
                    Status = vc.Status,
                    StatusDesc = sts.Description,
                    StatusCss = sts.StatusCss,
                    CreatedBy = vc.CreatedBy,
                    CreatedByName = cby.CreatedByName,
                    CreatedAt = vc.CreatedAt,
                    UpdatedBy = vc.UpdatedBy,
                    UpdatedByName = uby.UpdatedByName,
                    UpdatedAt = vc.UpdatedAt,
                    IsEdit = true,
                    IsPrint = true,
                    IsDelete = vc.Status == App.Status.Enable ? true : false,
                }
            ).ToList();
            var listVoucherId = dbVoucher.Select(x => x.Id).ToList();
            var dbVoucherItem = await (
                from vci in db.VoucherItem
                join vc in db.Voucher on vci.VoucherId equals vc.Id
                join itm in db.Item on vci.ItemId equals itm.Id
                join un in db.Unit on itm.UnitId equals un.Id
                join st in db.Store on vci.StoreId equals st.Id
                join sts in db.Setting on new { Category = App.SettingName.Status, Value = vci.Status.ToString() } equals new { sts.Category, sts.Value }
                join cby in db.User on vci.CreatedBy equals cby.Id
                join uby in db.User on vci.UpdatedBy equals uby.Id
                where
                    listVoucherId.Contains(vci.VoucherId) &&
                    obj.ListStatus.Contains(vci.Status)
                select new VoucherItem
                {
                    Id = vci.Id,
                    VoucherId = vci.VoucherId,
                    StoreId = vci.StoreId,
                    StoreDesc = st.Description,
                    ItemId = vci.ItemId,
                    ItemDesc = itm.Description,
                    VoucherDesc = vc.Type,
                    Remarks = vci.Remarks,
                    SerialNo = vci.SerialNo,
                    BatchNo = vci.BatchNo,
                    ExpiryOn = vci.ExpiryOn,
                    Qty = vci.Qty,
                    UnitDesc = un.Description,
                    Status = vci.Status,
                    StatusDesc = sts.Description,
                    StatusCss = sts.CssClass ?? "",
                    CreatedBy = vci.CreatedBy,
                    CreatedByName = cby.Name,
                    CreatedAt = vci.CreatedAt,
                    UpdatedBy = vci.UpdatedBy,
                    UpdatedByName = uby.Name,
                    UpdatedAt = uby.UpdatedAt
                }
            ).ToListAsync();
            var voucherItemLookup = dbVoucherItem.ToLookup(x => x.VoucherId);
            foreach (var row in dbVoucher)
            {
                row.VoucherItem = voucherItemLookup[row.Id].ToList();
            }
            return dbVoucher;
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
        public async Task<Message> EditAsync(int Id, User User)
        {
            Message objMsg = new Message();
            try
            {
                var voucher = (await ListAsync(new Voucher
                {
                    ListId = new List<int> { Id },
                    ListStatus = new List<int>(App.ActiveStatus) { App.Status.Delete }
                }, User)).FirstOrDefault();
                if (voucher == null)
                {
                    Message.Error(ref objMsg, "Gdn was not found for edit.");
                    return objMsg;
                }
                var listVoucherItemId = voucher.VoucherItem.Select(x => x.Id).ToList();
                var dbStockItem = await repoVoucher.StockItemAsync(new StockItemFltrDto
                {
                    ListNotContainId = listVoucherItemId,
                    ListStoreId = new List<int> { voucher.StoreId },
                    ToDate = voucher.Date
                }, User);
                var stockItem = dbStockItem.Select(x => new GdnItemDto
                {
                    ItemId = x.ItemId,
                    ItemDesc = x.ItemDesc,
                    UnitDesc = x.UnitDesc,
                    SerialNo = x.SerialNo,
                    ExpiryOn = x.ExpiryOn,
                    Qty = x.Qty,
                }).ToList();

                objMsg.data = new
                {
                    Voucher = voucher,
                    StockItem = stockItem,
                    AddOption = (await GetAddOptionAsync()).data
                };
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

        public async Task<Message> GetStockItemAsync(StockItemFltrDto obj, User User)
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
        public async Task<Message> GetStockItemWithSerialNoAsync(StockItemFltrDto obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                objMsg.data = await repoVoucher.StockItemWithSerialNoAsync(obj, User);
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
