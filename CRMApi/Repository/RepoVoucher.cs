using CRMApi.Models;
using CRMApi.Services;
using Microsoft.EntityFrameworkCore;

namespace CRMApi.Repository
{
    public class RepoVoucher
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoVoucher(DBCRM _db)
        {
            db = _db;
        }
        public async Task<List<Voucher>> ListAsync(Voucher? obj, User User)
        {
            obj ??= new Voucher();
            obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : App.ActiveStatus;
            var dbVoucherQuery = db.Voucher.Where(grn => obj.ListStatus.Contains(grn.Status)).AsQueryable();
            //Add Filter
            if (obj.ListId.Any())
                dbVoucherQuery = dbVoucherQuery.Where(grn => obj.ListId.Contains(grn.Id));
            if (obj.FromDate != DateTime.MinValue)
                dbVoucherQuery = dbVoucherQuery.Where(grn => grn.Date.Date >= obj.FromDate.Date);
            if (obj.ToDate != DateTime.MinValue)
                dbVoucherQuery = dbVoucherQuery.Where(grn => grn.Date.Date <= obj.ToDate.Date);
            if (obj.ListPartyId.Any())
                dbVoucherQuery = dbVoucherQuery.Where(grn => obj.ListPartyId.Contains(grn.PartyId));
            

            var dbVoucher = await (
                from vc in dbVoucherQuery
                join par in db.Party on vc.PartyId equals par.Id            
                join sts in db.Setting on new { Category = App.SettingName.Status, Value = vc.Status.ToString() } equals new { sts.Category, sts.Value }
                join cby in db.User on vc.CreatedBy equals cby.Id
                join uby in db.User on vc.UpdatedBy equals uby.Id
                select new
                {
                    Voucher = vc,
                    PartyDesc = par.Description,                  
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
                    ItemId = vci.ItemId,
                    ItemDesc = itm.Description,
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
                return new Voucher
                {
                    Id = voucher.Id,
                    No = voucher.No,
                    Type = voucher.Type,
                    Date = voucher.Date,
                    PartyId = voucher.PartyId,
                    PartyDesc = x.PartyDesc,
                    ConName = voucher.ConName,
                    ConAdd1 = voucher.ConAdd1,
                    ConAdd2 = voucher.ConAdd2,
                    ConPincode = voucher.ConPincode,
                    ConPostOffice = voucher.ConPostOffice,
                    ConStateCode = voucher.ConStateCode,
                    ConStateName = voucher.ConStateName,
                    RefDate = voucher.RefDate,
                    RefNo = voucher.RefNo,
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
        public async Task<List<VoucherItem>> ListItemAsync(Voucher? obj,User User)  
        {
            obj ??= new Voucher();
            obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : App.ActiveStatus;
            var dbVoucherItemQuery = db.VoucherItem.Where(vci => obj.ListStatus.Contains(vci.Status)).AsQueryable();

            var voucherItem = await (
                from vci in dbVoucherItemQuery
                join vc in db.Voucher on vci.VoucherId equals vc.Id
                join it in db.Item on vci.ItemId equals it.Id
                join st in db.Store on vci.StoreId equals st.Id
                join sts in db.Setting on new { Category = App.SettingName.Status, Value = vc.Status.ToString() } equals new { sts.Category, sts.Value }
                join cby in db.User on vc.CreatedBy equals cby.Id
                join uby in db.User on vc.UpdatedBy equals uby.Id
                where vci.VoucherId == obj.Id
                select new VoucherItem
                {


                    Id = vci.Id,
                    VoucherId = vci.VoucherId,
                    ItemId = vci.ItemId,
                    StoreId = vci.StoreId,
                    ItemDesc = it.Description,
                    StoreDesc = st.Description,
                    SerialNo = vci.SerialNo,
                    BatchNo = vci.BatchNo,
                    ExpiryOn = vci.ExpiryOn,
                    Qty = vci.Qty,
                    Rate = vci.Rate,
                    Amount = vci.Amount,
                    DiscountRate = vci.DiscountRate,
                    DiscountAmount = vci.DiscountAmount,
                    TotalAmount = vci.TotalAmount,
                    ListTax = vci.ListTax,
                    TaxRate = vci.TaxRate,
                    TaxAmount = vci.TaxAmount,
                    GrossAmount = vci.GrossAmount,
                    ImageUrl = vci.ImageUrl,
                    ReasonCode = vci.ReasonCode,
                    Remarks = vci.ReasonCode,
                    Status = vci.Status,
                    StatusDesc = st.Description,
                    StatusCss = sts.CssClass ?? "",
                    CreatedBy = vci.CreatedBy,
                    CreatedByName = cby.Name,
                    CreatedAt = vci.CreatedAt,
                    UpdatedBy = vci.UpdatedBy,
                    UpdatedByName = uby.Name,
                    UpdatedAt = vci.UpdatedAt,
                    IsEdit = vci.Status == App.Status.Enable ? true : false,                 
                    IsDelete = vci.Status == App.Status.Enable ? true : false,
                    IsEnable = vci.Status == App.Status.Delete ? true : false,

                  

                }).ToListAsync();

            return voucherItem;
        }
        //public async Task<Message> AddAsync (Voucher obj,User User)
        //{
        //    Message objMsg = new Message();
        //    try
        //    {
        //        obj.CreatedBy = User.Id;
        //        obj.CreatedAt = DateTime.Now;
        //        obj.UpdatedBy = User.Id;
        //        obj.UpdatedAt = DateTime.Now;
        //        obj.VoucherItem.ForEach(vi =>
        //        {
        //            vi.StoreId = obj.StoreId;
        //            vi.CreatedBy = User.Id;
        //            vi.CreatedAt = DateTime.Now;
        //            vi.UpdatedBy = User.Id;
        //            vi.UpdatedAt = DateTime.Now;
        //        });
        //        db.Add(obj);
        //        Message.Add(ref objMsg, (await db.SaveChangesAsync()));

        //        db.Entry(obj).Reload();

        //        foreach (var vi in obj.VoucherItem)
        //        {
        //            vi.VoucherId = obj.Id;
        //        }
        //        Message.Add(ref objMsg, (await db.SaveChangesAsync()));

        //        if (objMsg.status == Message.Type.success)
        //        {
        //            objMsg.obj = (await ListAsync(new Voucher
        //            {
        //                ListId = new List<int> { obj.Id }
        //            }, User)).FirstOrDefault();                   
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Message.Exception(ref objMsg, ex);
        //    }
        //    return objMsg;
        //}
        public async Task<Message> AddAsync(Voucher obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                obj.CreatedBy = User.Id;
                obj.CreatedAt = DateTime.Now;
                obj.UpdatedBy = User.Id;
                obj.UpdatedAt = DateTime.Now;

                obj.VoucherItem.ForEach(vi =>
                {
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

                    objMsg.obj = (await ListAsync(new Voucher
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
                voucher.No = obj.No;
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
                voucher.RefNo = obj.RefNo;
                voucher.RefDate = obj.RefDate;
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
                        vi.StoreId = item.StoreId;
                        vi.ItemId = item.ItemId;
                        vi.SerialNo = item.SerialNo;
                        vi.BatchNo = item.BatchNo;
                        vi.Qty = item.Qty;
                        vi.Rate = item.Rate;
                        vi.Amount =item.Amount;
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
                    new Voucher { ListId = new List<int> { obj.Id } },
                    User)).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }

            return objMsg;
        }
        public async Task<Message> DeleteAsync(int Id,User User)
        {
            Message objMsg = new Message();
            try
            {
                var Voucher = await db.Voucher
                    .Include(vc => vc.VoucherItem.Where(vi=> App.ActiveStatus.Contains(vi.Status)))
                    .FirstOrDefaultAsync(vc => App.ActiveStatus.Contains(vc.Status) && vc.Id == Id);
                if (Voucher == null)
                {
                    Message.Error(ref objMsg, "Voucher was not found for Delete");
                    return objMsg;
                }
                Voucher.Status = App.Status.Delete;
                Voucher.UpdatedBy = User.Id;
                Voucher.UpdatedAt = DateTime.Now;
                Voucher.VoucherItem.ForEach(vi =>
                {
                    vi.Status = Voucher.Status;
                    vi.UpdatedBy = User.Id;
                    vi.UpdatedAt = DateTime.Now;
                });
                db.Update(Voucher);                
                Message.Delete(ref objMsg, (await db.SaveChangesAsync()));
                if (objMsg.status == Message.Type.success)
                {
                    objMsg.obj = (await ListAsync(new Voucher
                    {
                        ListId = new List<int> { Voucher.Id },
                        ListStatus = new List<int> { App.Status.Delete }
                    }, User)).FirstOrDefault();                   
                }
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
                voucherItem.Status = App.Status.Delete;
                voucherItem.UpdatedBy = user.Id;
                voucherItem.UpdatedAt = DateTime.Now;
                db.Update(voucherItem);
                Message.Delete(ref objMsg, (await db.SaveChangesAsync()));                
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> EnableAsync(int id, User user)
        {
            Message objMsg = new Message();

            try
            {
                var voucher = await db.Voucher
                    .Include(v => v.VoucherItem.Where(vi=> vi.Status == App.Status.Delete))
                    .FirstOrDefaultAsync(v => v.Id == id && v.Status == App.Status.Delete);
                if (voucher == null)
                {
                    Message.Error(ref objMsg, "Voucher was not found for enable");
                    return objMsg;
                }
                voucher.Status = App.Status.Enable;
                voucher.UpdatedBy = user.Id;
                voucher.UpdatedAt = DateTime.Now;
                foreach (var vi in voucher.VoucherItem)
                {
                    vi.Status = App.Status.Enable;
                    vi.UpdatedBy = user.Id;
                    vi.UpdatedAt = DateTime.Now;
                }                
                Message.Enable(ref objMsg, (await db.SaveChangesAsync()));
                if (objMsg.status == Message.Type.success) 
                {
                    //Assing Voucher in objMsg.obj
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
