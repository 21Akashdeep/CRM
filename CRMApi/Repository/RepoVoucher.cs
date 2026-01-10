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
            //if (obj.FromDate != DateTime.MinValue)
            //    dbVoucherQuery = dbVoucherQuery.Where(grn => grn.Date.Date >= obj.FromDate.Date);
            //if (obj.ToDate != DateTime.MinValue)
            //    dbVoucherQuery = dbVoucherQuery.Where(grn => grn.Date.Date <= obj.ToDate.Date);
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
        public async Task<Message> AddAsync (Voucher? obj,User User)
        {
            Message objMsg = new Message();

            try
            {
                
                    try
                    {
                        obj.CreatedBy = User.Id;
                        obj.CreatedAt = DateTime.Now;
                        obj.UpdatedBy = User.Id;
                        obj.UpdatedAt = DateTime.Now;
                        obj.VoucherItem.ForEach(vi =>
                        {
                            vi.CreatedBy = User.Id;
                            vi.CreatedAt = DateTime.Now;
                            vi.UpdatedBy = User.Id;
                            vi.UpdatedAt = DateTime.Now;
                        });
                        db.Add(obj);
                        int isSaved = await db.SaveChangesAsync();
                        if (isSaved <= 0)
                        {
                            Message.Error(ref objMsg, "Voucher was not saved");

                           return objMsg;

                        }
                        db.Entry(obj).Reload();
                        
                       
                    }
                    catch (Exception ex)
                    {
                        Message.Exception(ref objMsg, ex);
                        
                    }
                
                if (objMsg.status == Message.Type.success)
                {
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

            try
            {
                var voucher = await db.Voucher
                    .Include(v => v.VoucherItem)
                    .FirstOrDefaultAsync(x =>
                        App.ActiveStatus.Contains(x.Status) &&
                        x.Id == obj.Id);

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
                voucher.UpdatedBy = User.Id;
                voucher.UpdatedAt = DateTime.Now;

                
                foreach (var vi in voucher.VoucherItem)
                {
                    var reqItem = obj.VoucherItem.FirstOrDefault(x => x.Id == vi.Id);

                    if (reqItem == null)
                    {
                        vi.Status = App.Status.ItemDelete; 
                    }
                    else
                    {
                        vi.ItemId = reqItem.ItemId;
                        vi.Qty = reqItem.Qty;
                        vi.Rate = reqItem.Rate;
                        vi.Amount = reqItem.Amount;
                        vi.Remarks = reqItem.Remarks;
                    }

                    vi.UpdatedBy = User.Id;
                    vi.UpdatedAt = DateTime.Now;
                }

                
                var newItems = obj.VoucherItem.Where(x => x.Id == 0).ToList();
                foreach (var vi in newItems)
                {
                    vi.Status = voucher.Status;
                    vi.CreatedBy = User.Id;
                    vi.CreatedAt = DateTime.Now;
                    vi.UpdatedBy = User.Id;
                    vi.UpdatedAt = DateTime.Now;

                    voucher.VoucherItem.Add(vi);
                }

                await db.SaveChangesAsync();
                Message.Success(ref objMsg, "Voucher updated successfully");

                objMsg.obj = (await ListAsync(
                    new Voucher { ListId = new List<int> { obj.Id } },
                    User)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }

            return objMsg;
        }


    }
}
