using CRMApi.Dto;
using CRMApi.Models;
using CRMApi.Repository;
using CRMApi.Services;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Net.Mail;
using static CRMApi.Dto.DtoTask;

namespace CRMApi.Repository
{
    public class RepoGdn
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        private readonly VoucherRepo repoVoucher;

        public RepoGdn(DBCRM _db)
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
                var ListStatus = await db.Voucher
                    .GroupBy(x => new { x.Status })
                    .Select(x => x.Key.Status.ToString())
                    .ToListAsync();

                Option.Status = await db.Setting
                .Where(st => App.ActiveStatus.Contains(st.Status)
                            && st.Name == App.SettingName.Status
                            && ListStatus.Contains(st.Value))
                .Select(st => new
                {
                    st.Value,
                    st.Description
                }).ToListAsync();
                Option.Party = await db.Party
                .Where(x => App.ActiveStatus.Contains(x.Status))
                .Select(x => new
                {
                    x.Id,
                    x.Description
                }).ToListAsync();

                Option.Number = await db.Voucher
                .Where(x => App.ActiveStatus.Contains(x.Status) && x.Type == App.VoucherType.DeliveryNote)
                .Select(x => new
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
                var Party = await db.Party
                    .Where(par => App.ActiveStatus.Contains(par.Status))
                    .Select(par => new
                    {
                        par.Id,
                        par.Code,
                        par.Name,
                        par.Description
                    }).ToListAsync();

                var StockType = await db.Setting
                    .Where(x => App.ActiveStatus.Contains(x.Status) && x.Name == App.SettingName.StockType)
                    .Select(x => new
                    {
                        x.Value,
                        x.Description
                    }).ToListAsync();

                var State = await db.AdminDiv
                    .Where(x => App.ActiveStatus.Contains(x.Status))
                    .Select(x => new
                    {
                        x.Id,
                        x.Code,
                        x.Name
                    }).ToListAsync();

                var Store = await db.Store
                    .Where(x => App.ActiveStatus.Contains(x.Status))
                    .Select(x => new
                    {
                        x.Id,
                        x.Description
                    }).ToListAsync();
                var Unit = await db.Unit
                    .Where(x => App.ActiveStatus.Contains(x.Status))
                    .Select(x => new
                    {
                        x.Id,
                        x.Description
                    }).ToListAsync();

                objMsg.data = new
                {
                    Store,
                    StockType,
                    Party,
                    State,
                    Unit
                };
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<List<GdnDto>> ListAsync(GdnFltrDto? obj, User User)
        {
            obj ??= new GdnFltrDto();
            obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : App.ActiveStatus;                        

            var dbGdn = (
                from vc in db.Voucher
                join pr in db.Party on vc.PartyId equals pr.Id
                join st in db.Store on vc.StoreId equals st.Id
                join sts in db.Setting on new { Category = App.SettingName.Status, Value = vc.Status.ToString() } equals new { sts.Category, sts.Value }
                join cby in db.User on vc.CreatedBy equals cby.Id
                join uby in db.User on vc.UpdatedBy equals uby.Id
                where
                       vc.Type == App.VoucherType.DeliveryNote &&
                       (obj.FromDate == DateTime.MinValue || vc.Date.Date >= obj.FromDate.Date) &&
                       (obj.ToDate == DateTime.MinValue || vc.Date.Date <= obj.ToDate.Date) &&
                       (!obj.ListPartyId.Any() || obj.ListPartyId.Contains(vc.PartyId ?? 0)) &&
                        obj.ListStatus.Contains(vc.Status) &&
                       (!obj.ListId.Any() || obj.ListId.Contains(vc.Id))
                select new GdnDto
                {
                    Id = vc.Id,
                    No = vc.No,
                    Type = vc.Type,
                    Date = vc.Date,
                    StoreId = vc.StoreId,
                    StoreDesc = st.Description,
                    PartyId = vc.PartyId,
                    PartyDesc = pr.Description,
                    ConName = vc.ConName,
                    ConAdd1 = vc.ConAdd1,
                    ConAdd2 = vc.ConAdd2,
                    ConPincode = vc.ConPincode,
                    ConPostOffice = vc.ConPostOffice,
                    ConStateCode = vc.ConStateCode,
                    ConStateName = vc.ConStateName,
                    PoDate = vc.PoDate,
                    PoNo = vc.PoNo,
                    ListVoucherId = vc.ListVoucherId,
                    EwayNo = vc.EwayNo,
                    EwayDate = vc.EwayDate,
                    Remarks = vc.Remarks,
                    Status = vc.Status,
                    StatusDesc = sts.Description,
                    StatusCss = sts.CssClass,
                    CreatedBy = vc.CreatedBy,
                    CreatedByName = cby.Name,
                    CreatedAt = vc.CreatedAt,
                    UpdatedBy = vc.UpdatedBy,
                    UpdatedByName = uby.Name,
                    UpdatedAt = vc.UpdatedAt,                    
                    IsEdit = true,
                    IsPrint = true,
                    IsDelete = vc.Status == App.Status.Enable ? true : false,
                }
            ).ToList();

            var listVoucherId = dbGdn.Select(x => x.Id).ToList();
            var dbGdnItem = await (
                from vci in db.VoucherItem
                join vc in db.Voucher on vci.VoucherId equals vc.Id
                join itm in db.Item on vci.ItemId equals itm.Id
                join un in db.Unit on vci.UnitId equals un.Id
                join st in db.Store on vci.StoreId equals st.Id
                join sts in db.Setting on new { Category = App.SettingName.Status, Value = vci.Status.ToString() } equals new { sts.Category, sts.Value }
                join cby in db.User on vci.CreatedBy equals cby.Id
                join uby in db.User on vci.UpdatedBy equals uby.Id
                where 
                    listVoucherId.Contains(vci.VoucherId) &&
                    obj.ListStatus.Contains(vci.Status)
                select new GdnItemDto
                {
                    Id = vci.Id,
                    VoucherId = vci.VoucherId,
                    StoreId = vci.StoreId,
                    StoreDesc = st.Description,
                    StockType = vci.StockType,
                    ItemId = vci.ItemId,
                    UnitId = vci.UnitId,
                    ItemDesc = itm.Description,
                    VoucherDesc = vc.Type,
                    Remarks = vci.Remarks,
                    SerialNo = vci.SerialNo,
                    BatchNo = vci.BatchNo,
                    ExpiryOn = vci.ExpiryOn,
                    Qty = vci.Qty,
                    ConversionFactor = vci.ConversionFactor,
                    BaseQty = vci.BaseQty,
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
            var gdnItemLookup = dbGdnItem.ToLookup(x => x.VoucherId);
            foreach (var row in dbGdn)
            {
                  row.GdnItem =   gdnItemLookup[row.Id].ToList();
            }
            return dbGdn;
        }
        public async Task<Message> AddAsync(Voucher obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var stockBalance = await repoVoucher.StockItemBalanceAsync1(new StockItemFltrDto
                {
                    ToDate = obj.Date,
                    ListItemId = obj.VoucherItem.Select(x => x.ItemId).ToList(),
                    ListSerialNo = obj.VoucherItem.Select(x => x.SerialNo).ToList(),
                    ListStoreId = new List<int> { obj.StoreId },
                }, User);

                foreach (var item in obj.VoucherItem)
                {
                    var availableQty = stockBalance
                        .Where(x => x.ItemId == item.ItemId && x.SerialNo == item.SerialNo && x.StoreId == obj.StoreId)
                        .Sum(x => x.BaseQty);

                    if (item.BaseQty > availableQty)
                    {
                        objMsg.status = Message.Type.error;
                        objMsg.statusText = $"Qty for Serial No {item.SerialNo} cannot exceed available qty ({availableQty}).";
                        return objMsg;
                    }
                }

                obj.CreatedBy = User.Id;
                obj.CreatedAt = DateTime.Now;
                obj.UpdatedBy = User.Id;
                obj.UpdatedAt = DateTime.Now;

                obj.VoucherItem.ForEach(vi =>
                {
                    vi.Qty = -(vi.Qty);
                    vi.BaseQty = -(vi.BaseQty);
                    vi.StoreId = obj.StoreId;
                    vi.CreatedBy = User.Id;
                    vi.CreatedAt = DateTime.Now;
                    vi.UpdatedBy = User.Id;
                    vi.UpdatedAt = DateTime.Now;
                });

                db.Add(obj);

                int result = await db.SaveChangesAsync();
                Message.Add(ref objMsg, result);

                if (objMsg.status == Message.Type.success)
                {
                    db.Entry(obj).Reload();

                    foreach (var vi in obj.VoucherItem)
                    {
                        vi.VoucherId = obj.Id;
                    }

                    await db.SaveChangesAsync();

                    objMsg.obj = (await ListAsync(new GdnFltrDto
                    {
                        ListId = new List<int> { obj.Id }
                    }, User)).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }

            return objMsg;
        }
        public async Task<Message> getUnit(int Id,User user)
        {
            Message objMsg = new Message();

            try
            {
                var unit = await (
                from itu in db.ItemUnit
                join un in db.Unit on itu.UnitId equals un.Id
                where App.ActiveStatus.Contains(itu.Status)
                && itu.ItemId == Id
                select new
                {
                  itu.ItemId,
                 UnitId = itu.UnitId,
                 UnitDesc = un.Description,   
                 itu.ValuePerUnit,
                 itu.ConversionFactor
                }).ToListAsync();

                objMsg.data = unit;              
            }
            catch(Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }

            return objMsg;
        }
        public async Task<Message> EditAsync(int Id, User user)
        {
            Message objMsg = new Message();
            try
            {
                var gdn = (await ListAsync(new GdnFltrDto
                {
                    ListId = new List<int> { Id },
                    ListStatus = new List<int>(App.ActiveStatus) { App.Status.Delete }
                }, user)).FirstOrDefault();
                if (gdn == null) 
                {
                    Message.Error(ref objMsg, "Gdn was not found for edit.");
                    return objMsg;
                }
                var listGdnItemId = gdn.GdnItem.Select(x => x.Id).ToList();
                var dbStockItem = await repoVoucher.StockItemAsync(new StockItemFltrDto
                {
                    ListExcludeViId = listGdnItemId,
                    ListStoreId = new List<int> { gdn.StoreId },                     
                    ToDate = gdn.Date
                }, user);
                var stockItem = dbStockItem.Select(x => new GdnItemDto
                {
                    ItemId = x.ItemId,
                    ItemDesc = x.ItemDesc,
                    UnitDesc = x.UnitDesc,                    
                    SerialNo = x.SerialNo,
                    ExpiryOn = x.ExpiryOn,
                    Qty = x.BaseQty,
                    BaseQty = x.BaseQty
                }).ToList();
                var unit = await (
                from itu in db.ItemUnit
                join un in db.Unit on itu.UnitId equals un.Id
                where App.ActiveStatus.Contains(itu.Status)
                select new
                {
                    itu.ItemId,
                    UnitId = itu.UnitId,
                    UnitDesc = un.Description,
                    itu.ValuePerUnit,
                    itu.ConversionFactor
                }).ToListAsync();

                objMsg.data = new
                {
                    Gdn = gdn,
                    StockItem = stockItem,
                    AddOption = (await GetAddOptionAsync()).data,
                    Unit = unit
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
            var storeid = obj.StoreId;
            try
            {
                var voucher = await db.Voucher.Include(v => v.VoucherItem).FirstOrDefaultAsync(x => App.ActiveStatus.Contains(x.Status) && x.Id == obj.Id);

                if (voucher == null)
                {
                    Message.Error(ref objMsg, "Voucher was not found for update");
                    return objMsg;
                }
                voucher.Type = obj.Type;
                voucher.PartyId = obj.PartyId;
                voucher.Date = obj.Date;
                voucher.Remarks = obj.Remarks;
                voucher.ConAdd1 = obj.ConAdd1;
                voucher.ConAdd2 = obj.ConAdd2;
                voucher.ConName = obj.ConName;
                voucher.ConPincode = obj.ConPincode;
                voucher.ConPostOffice = obj.ConPostOffice;
                voucher.ConStateCode = obj.ConStateCode;
                voucher.ConStateName = obj.ConStateName;
                voucher.PoDate = obj.PoDate;
                voucher.PoNo = obj.PoNo;
                voucher.EwayNo = obj.EwayNo;
                voucher.EwayDate = obj.EwayDate;
                voucher.UpdatedBy = User.Id;
                voucher.UpdatedAt = DateTime.Now;
                foreach (var vi in voucher.VoucherItem)
                {
                    var item = obj.VoucherItem.FirstOrDefault(x => x.Id == vi.Id);
                    if (item == null)
                    {
                        vi.Status = App.Status.ItemDelete;
                    }
                    else
                    {
                        vi.ExpiryOn = item.ExpiryOn;
                        vi.StoreId = storeid;
                        vi.ItemId = item.ItemId;
                        vi.SerialNo = item.SerialNo;
                        vi.BatchNo = item.BatchNo;
                        vi.Qty = item.Qty;
                        vi.BaseQty = item.BaseQty;
                        vi.ConversionFactor = item.ConversionFactor;
                        vi.Rate = item.Rate;
                        vi.Amount = item.Amount;
                        vi.Remarks = item.Remarks;
                        vi.Status = voucher.Status;
                    }
                    vi.UpdatedBy = User.Id;
                    vi.UpdatedAt = DateTime.Now;
                }
                db.Update(voucher);

                var newItem = obj.VoucherItem.Where(x => x.Id == 0).ToList();
                foreach (var vi in newItem)
                {
                    vi.StoreId = storeid;
                    vi.Qty = -(vi.Qty);
                    vi.VoucherId = voucher.Id;
                    vi.Status = voucher.Status;
                    vi.CreatedBy = User.Id;
                    vi.CreatedAt = DateTime.Now;
                    vi.UpdatedBy = User.Id;
                    vi.UpdatedAt = DateTime.Now;
                }
                db.AddRange(newItem);
                // await db.SaveChangesAsync();
                Message.Update(ref objMsg, (await db.SaveChangesAsync()));

                if (objMsg.status == Message.Type.success)
                {
                    objMsg.obj = (await ListAsync(
                    new GdnFltrDto { ListId = new List<int> { obj.Id } },
                    User)).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }

            return objMsg;
        }
        public async Task<Message> PrintAsync(GdnFltrDto obj, User User)
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
        public async Task<Message> ExportAsync(GdnFltrDto obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                // 1️⃣ Get Company
                var objCompany = await db.Company
                    .FirstOrDefaultAsync(pt => App.ActiveStatus.Contains(pt.Status));

                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "GDN info not found");
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

                objCompany.SheetName = "GDN List";
                objCompany.ReportDesc = $"GDN - {DateTime.Now:dd-MMM-yyyy}";
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
                obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : App.ActiveStatus;
                obj.ToDate = obj.ToDate == DateTime.MinValue ? DateTime.Now.Date : obj.ToDate.Date;
                var voucherItem = await (
                        from vci in db.VoucherItem
                        join vc in db.Voucher on vci.VoucherId equals vc.Id
                        join it in db.Item on vci.ItemId equals it.Id
                        join ut in db.Unit on it.UnitId equals ut.Id
                        where
                            (!obj.ListExcludeViId.Any() || !obj.ListExcludeViId.Contains(vci.Id)) &&
                            vc.Date.Date <= obj.ToDate.Date &&
                            obj.ListStoreId.Contains(vci.StoreId) &&
                            (!obj.ListStockType.Any() || obj.ListStockType.Contains(vci.StockType)) &&
                            obj.ListStatus.Contains(vci.Status) &&
                            obj.ListStatus.Contains(vc.Status)
                        group new { vci, it, ut } by new { vci.ItemId, ItemDesc = it.Description, UnitDesc = ut.Description, vci.StockType, vci.IsReturnable, vci.IsReturned } into g
                        where g.Sum(x => x.vci.BaseQty) > 0
                        select new StockItemDto
                        {
                            ItemId = g.Key.ItemId,
                            ItemDesc = $"{g.Key.ItemDesc} (Bal. Qty : {g.Sum(x => x.vci.BaseQty).ToString("#.000")} {g.Key.UnitDesc})",
                            UnitDesc = g.Key.UnitDesc,
                            Qty = g.Sum(x => x.vci.BaseQty),
                            StockType = g.Key.StockType,
                            IsReturnable = g.Key.IsReturnable,
                            IsReturned = g.Key.IsReturned
                        }
                    ).ToListAsync();
                objMsg.data = voucherItem;
            }
            catch(Exception ex)
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
                obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : App.ActiveStatus;
                obj.ToDate = obj.ToDate == DateTime.MinValue ? DateTime.Now.Date : obj.ToDate.Date;
                var voucherItem = await (
                        from vci in db.VoucherItem
                        join vc in db.Voucher on vci.VoucherId equals vc.Id
                        join it in db.Item on vci.ItemId equals it.Id
                        join ut in db.Unit on it.UnitId equals ut.Id
                        join isg in db.ItemSubGroup on it.ItemSubGroupId equals isg.Id
                        join ig in db.ItemGroup on it.ItemGroupId equals ig.Id
                        where
                            vc.Date.Date <= obj.ToDate.Date &&
                            (!obj.ListExcludeViId.Any() || !obj.ListExcludeViId.Contains(vci.Id)) &&
                            (!obj.ListItemId.Any() || obj.ListItemId.Contains(vci.ItemId)) &&
                            (!obj.ListSerialNo.Any() || obj.ListSerialNo.Contains(vci.SerialNo)) &&
                            obj.ListStoreId.Contains(vci.StoreId) &&
                            (!obj.ListStockType.Any() || obj.ListStockType.Contains(vci.StockType)) &&
                            obj.ListStatus.Contains(vci.Status) &&
                            obj.ListStatus.Contains(vc.Status)
                        group new { vci, it, ut } by new { vci.ItemId, ItemDesc = it.Description, UnitDesc = ut.Description, SerialNo = vci.SerialNo, vci.StoreId, vci.StockType } into g
                        where g.Sum(x => x.vci.BaseQty) > 0
                        select new StockItemDto
                        {
                            ItemId = g.Key.ItemId,
                            ItemDesc = g.Key.ItemDesc,
                            UnitDesc = g.Key.UnitDesc,
                            SerialNo = g.Key.SerialNo,
                            BaseQty = g.Sum(x => x.vci.BaseQty),
                            StockType = g.Key.StockType,
                            StoreId = g.Key.StoreId
                        }
                    ).ToListAsync();
                objMsg.data = voucherItem;
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
    }
}

