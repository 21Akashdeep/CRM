using CRMApi.Dto;
using CRMApi.Models;
using CRMApi.Repository;
using CRMApi.Services;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;

namespace CRMApi.Repository
{
    public class RepoStockTransfer
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        private readonly VoucherRepo RepoVoucher;
        public RepoStockTransfer(DBCRM _db)
        {
            db = _db;

            RepoVoucher = new VoucherRepo(_db);
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
                Option.StockInNo = await db.Voucher.Where(x => App.ActiveStatus.Contains(x.Status) && x.Type == "StockIn").Select(x => new
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
                       ItemId = itm.Id,
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
            obj.ListType = new List<string> { "StockTransfer" };
            obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : App.ActiveStatus;
            var dbVoucherQuery = db.Voucher.Where(grn => obj.ListStatus.Contains(grn.Status)).AsQueryable();
            //Add Filter
            if (obj.Id > 0)
            {
                dbVoucherQuery = dbVoucherQuery.Where(x => x.Id == obj.Id);
            }
            if (obj.ListType.Any())
                dbVoucherQuery = dbVoucherQuery.Where(vc => obj.ListType.Contains(vc.Type));
            if (obj.ListId.Any())
                dbVoucherQuery = dbVoucherQuery.Where(grn => obj.ListId.Contains(grn.Id));
            if (obj.ListNo.Any())
                dbVoucherQuery = dbVoucherQuery.Where(vc => obj.ListNo.Contains(vc.Id));
            if (obj.FromDate != DateTime.MinValue)
                dbVoucherQuery = dbVoucherQuery.Where(grn => grn.Date.Date >= obj.FromDate.Date);
            if (obj.ToDate != DateTime.MinValue)
                dbVoucherQuery = dbVoucherQuery.Where(grn => grn.Date.Date <= obj.ToDate.Date);
            if (obj.ListPartyId.Any())
            {
                dbVoucherQuery = dbVoucherQuery.Where(grn => grn.PartyId.HasValue && obj.ListPartyId.Contains(grn.PartyId.Value)
                  );
            }


            var dbVoucher = await (
                from vc in dbVoucherQuery
                join par in db.Party on vc.PartyId equals par.Id into partyJoin
                from par in partyJoin.DefaultIfEmpty()
                join sts in db.Setting on new { Category = App.SettingName.Status, Value = vc.Status.ToString() } equals new { sts.Category, sts.Value }
                join cby in db.User on vc.CreatedBy equals cby.Id
                join uby in db.User on vc.UpdatedBy equals uby.Id
                select new
                {
                    Voucher = vc,
                    PartyDesc = par != null ? par.Description : null,
                    StatusDesc = sts.Description,
                    StatusCss = sts.CssClass,
                    CreatedByName = cby.Name,
                    UpdatedByName = uby.Name
                }
            ).ToListAsync();

            var dbVoucherItem = await (
                from vci in db.VoucherItem
                join vc in db.Voucher on vci.VoucherId equals vc.Id
                join itm in db.Item on vci.ItemId equals itm.Id
                join un in db.Unit on itm.UnitId equals un.Id
                join st in db.Store on vci.StoreId equals st.Id
                join sts in db.Setting on new { Category = App.SettingName.Status, Value = vci.Status.ToString() } equals new { sts.Category, sts.Value }
                join cby in db.User on vci.CreatedBy equals cby.Id
                join uby in db.User on vci.UpdatedBy equals uby.Id
                where vci.Status == vc.Status
                select new VoucherItem
                {
                    Id = vci.Id,
                    VoucherId = vci.VoucherId,
                    StoreId = vci.StoreId,
                    StoreDesc = st.Description,
                    ItemId = vci.ItemId,
                    ItemDesc = itm.Description,
                    UnitDesc = un.Description,
                    VoucherDesc = vc.Type,
                    Remarks = vci.Remarks,
                    SerialNo = vci.SerialNo,
                    BatchNo = vci.BatchNo,
                    ExpiryOn = vci.ExpiryOn,
                    Qty = vci.Qty,
                    Rate = vci.Rate,
                    Amount = vci.Amount,
                    DiscountAmount = vci.DiscountAmount,
                    DiscountRate = vci.DiscountRate,
                    TotalAmount = vci.TotalAmount,
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
            var VoucherItemLookup = dbVoucherItem.ToLookup(x => x.VoucherId);
            var Voucher = dbVoucher.Select(x =>
            {
                var voucher = x.Voucher;
                var item = VoucherItemLookup[voucher.Id];
                return new Voucher
                {
                    Id = voucher.Id,
                    No = voucher.No,
                    Type = voucher.Type,
                    Date = voucher.Date,
                    PartyId = voucher.PartyId,
                    PartyDesc = x.PartyDesc,
                    //StoreDesc = dbVoucherItem[0].StoreDesc,
                    StoreDesc = VoucherItemLookup[voucher.Id].Select(x => x.StoreDesc).FirstOrDefault(),
                    TotalQty = item.Where(i => !string.IsNullOrEmpty(i.ItemDesc) && i.Qty >= 0).Count(),
                    ItemName = item.Where(i => !string.IsNullOrEmpty(i.ItemDesc) && i.Qty >=0).Select(i => i.ItemDesc!)
                    .Distinct().ToList(),
                    ConName = voucher.ConName,
                    ConAdd1 = voucher.ConAdd1,
                    ConAdd2 = voucher.ConAdd2,
                    ConPincode = voucher.ConPincode,
                    ConPostOffice = voucher.ConPostOffice,
                    ConStateCode = voucher.ConStateCode,
                    ConStateName = voucher.ConStateName,
                    ChallanDate = voucher.ChallanDate,
                    ChallanNo = voucher.ChallanNo,
                    InvoiceDate = voucher.InvoiceDate,
                    InvoiceNo = voucher.InvoiceNo,
                    ListVoucherId = voucher.ListVoucherId,
                    EwayNo = voucher.EwayNo,
                    EwayDate = voucher.EwayDate,
                    Remarks = voucher.Remarks,
                    Status = voucher.Status,
                    StatusDesc = x.StatusDesc,
                    StatusCss = x.StatusCss,
                    CreatedBy = voucher.CreatedBy,
                    CreatedByName = x.CreatedByName,
                    CreatedAt = voucher.CreatedAt,
                    UpdatedBy = voucher.UpdatedBy,
                    UpdatedByName = x.UpdatedByName,
                    UpdatedAt = voucher.UpdatedAt,
                    VoucherItem = VoucherItemLookup[voucher.Id].ToList(),
                    IsEdit = true,
                    IsPrint = true,
                    IsDelete = voucher.Status == App.Status.Enable ? true : false,
                };
            }
            ).ToList();
            return Voucher;


        }
        public async Task<Message> AddAsync(Voucher obj, User user)
        {
            Message objMsg = new Message();
            obj.Type = "StockTransfer";
            try
            {
                var strategy = db.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await db.Database.BeginTransactionAsync();

                    try
                    {
                        obj.CreatedBy = user.Id;
                        obj.CreatedAt = DateTime.Now;
                        obj.UpdatedBy = user.Id;
                        obj.UpdatedAt = DateTime.Now;
                        foreach (var vi in obj.VoucherItem)
                        {
                            vi.StoreId = obj.FromStoreId;
                            vi.Qty = -(vi.Qty);
                            vi.CreatedBy = user.Id;
                            vi.CreatedAt = DateTime.Now;
                            vi.UpdatedBy = user.Id;
                            vi.UpdatedAt = DateTime.Now;
                        }
                        await db.Voucher.AddAsync(obj);

                        int result = await db.SaveChangesAsync();
                        Message.Add(ref objMsg, result);

                        if (objMsg.status != Message.Type.success)
                        {
                            await transaction.RollbackAsync();
                            return;
                        }
                        var dbItems = await db.VoucherItem.Where(x =>x.Status == App.Status.Enable && x.StoreId == obj.FromStoreId).Select(x => new VoucherItem
                         {
                           SerialNo = x.SerialNo,
                           Qty = x.Qty,
                           VoucherId = x.VoucherId,
                           ItemId = x.ItemId,
                           StoreId = x.StoreId,
                           Remarks = x.Remarks,
                           ExpiryOn = x.ExpiryOn,
                           Status = x.Status
                            }).ToListAsync();

                        var stockTran = dbItems.Where(x => obj.VoucherItem.Any(vi => vi.SerialNo == x.SerialNo && vi.ItemId == x.ItemId)).ToList();


                        var stockMsg = await UpdateStockAsync(stockTran, obj.FromStoreId,obj.ToStoreId, user);
                        if (stockMsg.status == Message.Type.error)
                        {
                            Message.Error(ref objMsg,
                                "Stock transaction was not saved. The entire transaction has been rolled back.");
                            await transaction.RollbackAsync();
                            return;
                        }

                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Message.Exception(ref objMsg, ex);
                    }
                });

               
                objMsg.obj = (await ListAsync(new Voucher
                {
                    ListId = new List<int> { obj.Id }
                }, user)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }

            return objMsg;
        }
        public async Task<Message> UpdateStockAsync(List<VoucherItem> stockTran,int FromStoreId,int ToStoreId,User user)
        {
            Message objMsg = new Message();
            try
            {
                var groupedItems = stockTran
                    .GroupBy(x => new { x.SerialNo, x.ItemId, x.StoreId })
                    .Select(g => new
                    {
                        g.Key.SerialNo,
                        g.Key.ItemId,
                        g.Key.StoreId,
                        TotalQty = g.Sum(x => x.Qty)
                    })
                    .ToList();

                var invalid = groupedItems.FirstOrDefault(x => x.TotalQty < 0);
                if (invalid != null)
                {
                    Message.Error(
                        ref objMsg,
                        $"Stock validation failed. SerialNo: {invalid.SerialNo}, " +
                        $"ItemId: {invalid.ItemId}, StoreId: {invalid.StoreId}, TotalQty: {invalid.TotalQty}");

                    return objMsg; 
                }
                var finalStockTransferItem = stockTran
                    .Where(x => x.Status == App.Status.Enable && x.Qty < 0)
                    .ToList();

                foreach (var vi in finalStockTransferItem)
                {
                    var newItem = new VoucherItem
                    {
                        VoucherId = vi.VoucherId,
                        ItemId = vi.ItemId,
                        SerialNo = vi.SerialNo,
                        StoreId = ToStoreId,
                        Qty = Math.Abs(vi.Qty),
                        ExpiryOn = vi.ExpiryOn,
                        Remarks = vi.Remarks,
                        Status = App.Status.Enable,
                        CreatedBy = user.Id,
                        CreatedAt = DateTime.Now,
                        UpdatedBy = user.Id,
                        UpdatedAt = DateTime.Now
                    };

                    db.VoucherItem.Add(newItem);
                }

                Message.Update(ref objMsg, await db.SaveChangesAsync());
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        //public async Task<List<VoucherItem>> GetListItemAsync(Voucher? obj, User User)
        //{
        //    Message objMsg = new Message();

           
        //        obj ??= new Voucher();
        //        obj.ListType = new List<String> { "ReceiptNote","ReturnNote","StockIn" };

        //        var listVoucher = await new RepoVoucher(db).ListAsync(obj, User);

        //        var vId = listVoucher.Where(x => x.Status == App.Status.Enable).Select(x => new
        //        {
        //            x.Id
        //        }).ToList();
                
        //        var listVoucherItem = await new RepoVoucher(db).ListItemAsync(obj, User);


        //        //var ListVoucherItem = await 



            
            
        //    return listVoucherItem;

        //}
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
                    Message.Error(ref objMsg, "StockOut was not found for edit.");
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
        public async Task<Message> UpdateAsync1(Voucher obj, User User)
        {
            Message objMsg = new Message();
            var VoucherItem = obj.VoucherItem;

            var voucherIdList = VoucherItem.Select(x =>  x.VoucherId).ToList();
            var serialNoList = VoucherItem.Select(x => x.SerialNo).ToList();
            var itemIdList = VoucherItem.Select(x =>  x.ItemId).ToList();
            try
            {
                var AddedStockItem = await db.VoucherItem.Where(x => App.ActiveStatus.Contains(x.Status) && voucherIdList.Contains(x.VoucherId) && serialNoList.Contains(x.SerialNo) && itemIdList.Contains(x.ItemId)).ToListAsync();
                var groupedItems = AddedStockItem
                    .GroupBy(x => new { x.SerialNo, x.ItemId, x.StoreId })
                    .Select(g => new
                    {
                        g.Key.SerialNo,
                        g.Key.ItemId,
                        g.Key.StoreId,
                        TotalQty = g.Sum(x => x.Qty)
                    })
                    .ToList();

                var invalid = groupedItems.FirstOrDefault(x => x.TotalQty > 0);
                if (invalid != null)
                {
                    Message.Error(
                        ref objMsg,
                        $"Stock validation failed. SerialNo: {invalid.SerialNo}, " +
                        $"ItemId: {invalid.ItemId}, StoreId: {invalid.StoreId}, TotalQty: {invalid.TotalQty}");

                    return objMsg;
                }
                var newAddedVoucherItem = obj.VoucherItem.Where(x => x.Id == 0).ToList();

                foreach( var vi in newAddedVoucherItem)
                {

                    var newItem = new VoucherItem
                    {
                        VoucherId = obj.Id,
                        ItemId = vi.ItemId,
                        SerialNo = vi.SerialNo,
                        StoreId = obj.ToStoreId,
                        Qty = Math.Abs(vi.Qty),
                        ExpiryOn = vi.ExpiryOn,
                        Remarks = vi.Remarks,
                        Status = App.Status.Enable,
                        CreatedBy = User.Id,
                        CreatedAt = DateTime.Now,
                        UpdatedBy = User.Id,
                        UpdatedAt = DateTime.Now
                    };

                    db.VoucherItem.Add(newItem);

                }
                obj.Type = "StockIn";
                objMsg = await  RepoVoucher.UpdateAsync(obj, User);
                
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
            var newAdddedItem = obj.VoucherItem.Where(x => x.Id == 0);

            try
            {
                var strategy = db.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await db.Database.BeginTransactionAsync();

                    try
                    {

                        foreach (var vi in newAdddedItem)
                        {
                            var newItem = new VoucherItem
                            {
                                VoucherId = obj.Id,
                                ItemId = vi.ItemId,
                                SerialNo = vi.SerialNo,
                                StoreId = obj.FromStoreId,
                                Qty = -(vi.Qty),
                                ExpiryOn = vi.ExpiryOn,
                                Remarks = vi.Remarks,
                                Status = App.Status.Enable,
                                CreatedBy = User.Id,
                                CreatedAt = DateTime.Now,
                                UpdatedBy = User.Id,
                                UpdatedAt = DateTime.Now
                            };

                            db.VoucherItem.Add(newItem);


                        }
                        int result = await db.SaveChangesAsync();
                        Message.Add(ref objMsg, result);

                        if (objMsg.status != Message.Type.success)
                        {
                            await transaction.RollbackAsync();
                            return;
                        }

                        var dbItems = await db.VoucherItem.Where(x => x.Status == App.Status.Enable && x.StoreId == obj.FromStoreId).Select(x => new VoucherItem
                        {
                            SerialNo = x.SerialNo,
                            Qty = x.Qty,
                            VoucherId = x.VoucherId,
                            ItemId = x.ItemId,
                            StoreId = x.StoreId,
                            Remarks = x.Remarks,
                            ExpiryOn = x.ExpiryOn,
                            Status = x.Status
                        }).ToListAsync();

                        var stockTran = dbItems.Where(x => obj.VoucherItem.Any(vi => vi.SerialNo == x.SerialNo && vi.ItemId == x.ItemId)).ToList();
                        var groupedItems = stockTran.GroupBy(x => new { x.SerialNo, x.ItemId, x.StoreId }).Select(g => new
                        {
                        g.Key.SerialNo,
                        g.Key.ItemId,
                        g.Key.StoreId,
                        TotalQty = g.Sum(x => x.Qty)
                        }).ToList();

                        var invalid = groupedItems.FirstOrDefault(x => x.TotalQty < 0);
                        if (invalid != null)
                        {
                            Message.Error(
                                ref objMsg,
                                $"Stock validation failed. SerialNo: {invalid.SerialNo}, " +
                                $"ItemId: {invalid.ItemId}, StoreId: {invalid.StoreId}, TotalQty: {invalid.TotalQty}");

                            return ;
                        }
                       

                        foreach (var vi in newAdddedItem)
                        {
                            var newItem = new VoucherItem
                            {
                                VoucherId = obj.Id,
                                ItemId = vi.ItemId,
                                SerialNo = vi.SerialNo,
                                StoreId = obj.ToStoreId,
                                Qty = vi.Qty,
                                ExpiryOn = vi.ExpiryOn,
                                Remarks = vi.Remarks,
                                Status = App.Status.Enable,
                                CreatedBy = User.Id,
                                CreatedAt = DateTime.Now,
                                UpdatedBy = User.Id,
                                UpdatedAt = DateTime.Now
                            };

                            db.VoucherItem.Add(newItem);
                        }

                        Message.Update(ref objMsg, await db.SaveChangesAsync());
                        if (objMsg.status == Message.Type.error)
                        {
                            Message.Error(ref objMsg,
                                "Stock transaction was not saved. The entire transaction has been rolled back.");
                            await transaction.RollbackAsync();
                            return;
                        }
                        await transaction.CommitAsync();

                        objMsg.obj = await ListAsync(obj, User);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Message.Exception(ref objMsg, ex);
                    }

                });
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }

            return objMsg;
        }

        public async Task<Message> DeleteItemAsync(int id, User user)
        {
            Message objMsg = new Message();
            try
            {
                var voucherItem = await db.VoucherItem.FirstOrDefaultAsync(vci => App.ActiveStatus.Contains(vci.Status) && vci.Id == id);
                if (voucherItem == null)
                {
                    Message.Error(ref objMsg, "Voucher item was not found for delete");
                    return objMsg;
                }
                voucherItem.Status = App.Status.Cancel;
                voucherItem.UpdatedBy = user.Id;
                voucherItem.UpdatedAt = DateTime.Now;
                db.Update(voucherItem);

                var voucherItemStock = await db.VoucherItem.FirstOrDefaultAsync(vis => App.ActiveStatus.Contains(vis.Status) && vis.VoucherId == voucherItem.VoucherId && vis.SerialNo == voucherItem.SerialNo && vis.ItemId == voucherItem.ItemId && vis.StoreId != voucherItem.StoreId && vis.Qty <0);

                if (voucherItemStock != null)
                {
                    voucherItemStock.Status = App.Status.Cancel;
                    voucherItemStock.UpdatedBy = user.Id;
                    voucherItemStock.UpdatedAt = DateTime.Now;
                    db.Update(voucherItemStock);
                }
                Message.Delete(ref objMsg, (await db.SaveChangesAsync()));

                Voucher voucher = new Voucher();
                voucher.Id = voucherItem.VoucherId;

                objMsg.obj = await ListAsync(voucher, user);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);

            }
            return objMsg;
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
                objMsg.data = await RepoVoucher.StockItemAsync(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
    }
}

