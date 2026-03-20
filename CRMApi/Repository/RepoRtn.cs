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
    public class RepoRtn
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoRtn(DBCRM _db)
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
                    .Where(x => x.Type == "ReturnNote")
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
                        x.Name
                    }).ToListAsync();

                Option.ConName = await db.Voucher
                    .Where(x => App.ActiveStatus.Contains(x.Status)
                             && !string.IsNullOrEmpty(x.ConName)
                             && x.Type == "ReturnNote")
                    .Select(x => new
                    {
                        x.Id,
                        x.ConName,
                    }).ToListAsync();

                Option.Number = await db.Voucher
                    .Where(x => App.ActiveStatus.Contains(x.Status)
                        && !string.IsNullOrEmpty(x.No)
                        && x.Type == "ReturnNote")
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

                //var Type = await db.Setting
                //    .Where(x => App.ActiveStatus.Contains(x.Status) && x.Name == App.SettingName.VoucherType)
                //    .Select(x => new
                //    {
                //        x.Value,
                //        x.Description
                //    }).ToListAsync();

                var State = await db.AdminDiv
                    .Where(x => App.ActiveStatus.Contains(x.Status))
                    .Select(x => new
                    {
                        x.Id,
                        x.Code,
                        x.Name
                    }).ToListAsync();

                //var ReasonCode = await db.Setting
                //    .Where(x => App.ActiveStatus.Contains(x.Status) && x.Name == App.SettingName.ReasonCode)
                //    .Select(x => new
                //    {
                //        x.Value,
                //        x.Description
                //    }).ToListAsync();

                var Store = await db.Store
                    .Where(x => App.ActiveStatus.Contains(x.Status))
                    .Select(x => new
                    {
                        x.Id,
                        x.Description
                    }).ToListAsync();

                var GdnNo = await db.Voucher
                    .Where(x => App.ActiveStatus.Contains(x.Status)
                             && x.Type == "DeliveryNote")
                    .Select(x => new
                    {
                        x.Id,
                        x.No
                    })
                    .ToListAsync();

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
                    GdnNo,
                    //Type,
                    Item,
                    //ReasonCode,
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
            obj.ListType = new List<string> {"ReturnNote"};
            var voucher = await new VoucherRepo(db).ListAsync(obj, User);
            return voucher;
        }
        public async Task<Message> GetGdnItemAsync(List<int> ListGdnId, User User)
        {
            Message objMsg = new Message();
            try
            {
                if (ListGdnId == null || !ListGdnId.Any())
                {
                    Message.Error(ref objMsg, "GDN not selected");
                    return objMsg;
                }

                var gdnItem = await (
                    from vci in db.VoucherItem
                    join vc in db.Voucher on vci.VoucherId equals vc.Id
                    join itm in db.Item on vci.ItemId equals itm.Id
                    join st in db.Store on vci.StoreId equals st.Id
                    where ListGdnId.Contains(vc.Id)
                       && vc.Type == "DeliveryNote"
                       && App.ActiveStatus.Contains(vc.Status)
                       && App.ActiveStatus.Contains(vci.Status)
                    select new VoucherItem
                    {
                        Id = vci.Id,
                        VoucherId = vci.VoucherId,

                        ItemId = vci.ItemId,
                        ItemDesc = itm.Description,
                        StoreId = vci.StoreId,
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

                        Remarks = vci.Remarks,
                        Status = App.Status.Enable
                    }
                ).ToListAsync();

                objMsg.data = gdnItem;
                Message.Success(ref objMsg, "GDN items loaded");
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
        public async Task<Message> EditAsync(int Id, User User)
        {
            Message objMsg = new Message();
            try
            {
                var obj = (await ListAsync(new Voucher
                {
                    ListId = new List<int> { Id },
                    ListStatus = new List<int>(App.ActiveStatus) { App.Status.Delete }
                }, User)).FirstOrDefault();

                if (obj == null)
                {
                    Message.Error(ref objMsg, "RTN not found.");
                    return objMsg;
                }

                objMsg.obj = obj;
                objMsg.data = (await GetAddOptionAsync()).data;

                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> DeleteItemAsync(Voucher obj, User user)
        {
            Message objMsg = new Message();
            try
            {

                objMsg = await  new VoucherRepo(db).DeleteItemAsync(obj.DeleteItemId, user);
                objMsg.data = await ListAsync(obj, user);

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
                    Message.Error(ref objMsg, "RTN info not found");
                    return objMsg;
                }

                // 2️⃣ Get RTN List
                var Rtn = (await ListAsync(obj, User)).Select(co => new
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

                DataTable objDataTable = Util.ListToDataTable(Rtn);

                objCompany.SheetName = "RTN List";
                objCompany.ReportDesc = $"RTN - {DateTime.Now:dd-MMM-yyyy}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);

                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }

            return objMsg;
        }

    }
}
