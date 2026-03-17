using CRMApi.Models;
using CRMApi.Services;
using System.Dynamic;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Net.Mail;
using static CRMApi.Dto.DtoTask;

namespace CRMApi.Repository
{
    public class RepoStoreDashboard
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoStoreDashboard(DBCRM _db)
        {
            db = _db;
        }
        public async Task<Message> GetViewOptionAsync()
        {
            Message objMsg = new Message();
            try
            { 
                dynamic Option = new ExpandoObject();
                var ListStatus = await db.VoucherItem.GroupBy(x => new { x.Status }).Select(x => x.Key.Status.ToString()).ToListAsync();

                



                Option.Store = await db.Store.Where(x => x.Status == App.Status.Enable).ToListAsync();

                Option.ItemGroup = await db.ItemGroup.Where(x => x.Status == App.Status.Enable).ToListAsync();
                Option.ItemSubGroup = await db.ItemSubGroup.Where(x => x.Status == App.Status.Enable).ToListAsync();
                Option.Item = await db.Item.Where(x => x.Status == App.Status.Enable).ToListAsync();




                objMsg.data = Option;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }      
        public async Task<Message> ListAsync(Voucher? obj, User User)
        {

            obj ??= new Voucher();

            Message objMsg = new Message();

            var VoucherItem = await db.VoucherItem.Where(x => x.Status == App.Status.Enable).ToListAsync();


            dynamic Option = new ExpandoObject();

            var itemInJamshedpur =
                from vi in VoucherItem
                join itm in db.Item on vi.ItemId equals itm.Id
                join ig in db.ItemGroup on itm.ItemGroupId equals ig.Id
                join isg in db.ItemSubGroup on itm.ItemSubGroupId equals isg.Id

                where vi.StoreId == obj.StoreId

                group new { vi, itm, ig, isg } by vi.ItemId into g

                select new
                {
                    ItemId = g.Key,

                    ItemDesc = g.Select(x => x.itm.Description).FirstOrDefault(),

                    ItemGroupDesc = g.Select(x => x.ig.Description).FirstOrDefault(),

                    ItemSubGroupDesc = g.Select(x => x.isg.Description).FirstOrDefault(),

                    TotalQty = g.Sum(x => x.vi.Qty)
                };

            Option.ItemInJamshedpur = itemInJamshedpur
                .Where(x => x.TotalQty >=0 )
                .ToList();

            objMsg.data = Option;
            Message.Success(ref objMsg, "Record found");

            return objMsg;
        }
        public async Task<Message> GetDataByGroup(Voucher obj, User User)
        {
            Message objMsg = new Message();

            dynamic Option = new ExpandoObject();

            var VoucherItem = await db.VoucherItem.Where(x => x.Status == App.Status.Enable).ToListAsync();

            var itemIds = await db.Item
           .Where(x => x.Status == App.Status.Enable
           && obj.ListGroupId.Contains(x.ItemGroupId))
          .Select(x => 
           x.Id)
         .ToListAsync();

            Option.itemSubGroup = await db.ItemSubGroup.Where(x => x.Status == App.Status.Enable && obj.ListGroupId.Contains(x.ItemGroupId)).ToListAsync();

            

            var itemInJamshedpur =
                from vi in VoucherItem
                join itm in db.Item on vi.ItemId equals itm.Id
                join ig in db.ItemGroup on itm.ItemGroupId equals ig.Id
                join isg in db.ItemSubGroup on itm.ItemSubGroupId equals isg.Id

                where vi.StoreId == obj.StoreId 

                group new { vi, itm, ig, isg } by vi.ItemId into g

                select new
                {
                    ItemId = g.Key,

                    ItemDesc = g.Select(x => x.itm.Description).FirstOrDefault(),

                    ItemGroupDesc = g.Select(x => x.ig.Description).FirstOrDefault(),

                    ItemSubGroupDesc = g.Select(x => x.isg.Description).FirstOrDefault(),

                    TotalQty = g.Sum(x => x.vi.Qty)
                };

           var  ItemData = itemInJamshedpur
                .Where(x => x.TotalQty > 0)
                .ToList();

            Option.ItemInJamshedpur = ItemData
           .Where(x => itemIds.Contains(x.ItemId))
           .ToList();

            objMsg.data = Option;

            return objMsg;
        }
        public async Task<Message> GetDataByGroupAndSubGroup(Voucher obj, User User)
        {
            Message objMsg = new Message();
            dynamic Option = new ExpandoObject();
            var VoucherItem = await db.VoucherItem.Where(x => x.Status == App.Status.Enable).ToListAsync();

            var itemIds = await db.Item
           .Where(x => x.Status == App.Status.Enable
            && (obj.ListGroupId.Contains(x.ItemGroupId) && obj.ListSubGroupId.Contains(x.ItemSubGroupId)))
           .Select(x =>
           x.Id)
            .ToListAsync();

            Option.item = await db.Item.Where(x => x.Status == App.Status.Enable && itemIds.Contains(x.Id)).ToListAsync();

            var itemInJamshedpur =
                from vi in VoucherItem
                join itm in db.Item on vi.ItemId equals itm.Id
                join ig in db.ItemGroup on itm.ItemGroupId equals ig.Id
                join isg in db.ItemSubGroup on itm.ItemSubGroupId equals isg.Id

                where vi.StoreId == obj.StoreId

                group new { vi, itm, ig, isg } by vi.ItemId into g

                select new
                {
                    ItemId = g.Key,

                    ItemDesc = g.Select(x => x.itm.Description).FirstOrDefault(),

                    ItemGroupDesc = g.Select(x => x.ig.Description).FirstOrDefault(),

                    ItemSubGroupDesc = g.Select(x => x.isg.Description).FirstOrDefault(),

                    TotalQty = g.Sum(x => x.vi.Qty)
                };

            var ItemData = itemInJamshedpur
                 .Where(x => x.TotalQty > 0)
                 .ToList();

            Option.ItemInJamshedpur = ItemData
           .Where(x => itemIds.Contains(x.ItemId))
           .ToList();

            objMsg.data = Option;

            return objMsg;
        }
        public async Task<Message> GetDataByItemId(Voucher obj, User User)
        {
            Message objMsg = new Message();
            dynamic Option = new ExpandoObject();
            var VoucherItem = await db.VoucherItem.Where(x => x.Status == App.Status.Enable).ToListAsync();

            var itemIds = await db.Item
           .Where(x => x.Status == App.Status.Enable
            && (obj.ListGroupId.Contains(x.ItemGroupId) && obj.ListSubGroupId.Contains(x.ItemSubGroupId) && obj.ListItemId.Contains(x.Id)))
           .Select(x =>
           x.Id)
            .ToListAsync();

            //Option.item = await db.Item.Where(x => x.Status == App.Status.Enable && itemIds.Contains(x.Id)).ToListAsync();

            var itemInJamshedpur =
                from vi in VoucherItem
                join itm in db.Item on vi.ItemId equals itm.Id
                join ig in db.ItemGroup on itm.ItemGroupId equals ig.Id
                join isg in db.ItemSubGroup on itm.ItemSubGroupId equals isg.Id

                where vi.StoreId == obj.StoreId

                group new { vi, itm, ig, isg } by vi.ItemId into g

                select new
                {
                    ItemId = g.Key,

                    ItemDesc = g.Select(x => x.itm.Description).FirstOrDefault(),

                    ItemGroupDesc = g.Select(x => x.ig.Description).FirstOrDefault(),

                    ItemSubGroupDesc = g.Select(x => x.isg.Description).FirstOrDefault(),

                    TotalQty = g.Sum(x => x.vi.Qty)
                };

            var ItemData = itemInJamshedpur
                 .Where(x => x.TotalQty > 0)
                 .ToList();

            Option.ItemInJamshedpur = ItemData
          .Where(x => itemIds.Contains(x.ItemId))
          .ToList();

           
            objMsg.data = Option;

            return objMsg;
        }
        public async Task<Message> PrintAsync(Voucher obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                //objMsg.data = await ListAsync(obj, User);
                //Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> Export(Voucher obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                //Get Company
                var objCompany = await db.Company.FirstOrDefaultAsync(pt => App.ActiveStatus.Contains(pt.Status));
                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Company not found.");
                    return objMsg;
                }
                //Get Task Group
                var result = await ListAsync(obj, User);
                //var Deparment = result.Select(x => new
                //{

                //    x.Code,
                //    x.Name,
                //    x.Description,
                //    x.PoNo,
                //    x.PoDate,
                //    x.StartDate,
                //    x.EndDate,
                //    x.Remarks,
                //    Party = x.PartyDesc,
                //    x.CreatedByName,
                //    x.CreatedAt,
                //    x.UpdatedBy,
                //    x.UpdatedByName,
                //    x.UpdatedAt
                //}).ToList();
                //Convert List To DataTable
               // DataTable objDataTable = Util.ListToDataTable(Deparment);
                //Convert Datatable to base64                
                objCompany.SheetName = "Task List";
                objCompany.ReportDesc = $"Task - {DateTime.Now.ToString("dd-MMM-yyyy")}";
               // objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> GetItemFullHistoryAsync(int itemId, int storeId)
        {
            Message objMsg = new Message();
            try
            {
                var history = await
                (
                    from vi in db.VoucherItem
                    join v in db.Voucher on vi.VoucherId equals v.Id
                    join s in db.Store on vi.StoreId equals s.Id
                    join u in db.User on v.CreatedBy equals u.Id
                    join itm in db.Item on vi.ItemId equals itm.Id
                    join ig in db.ItemGroup on itm.ItemGroupId equals ig.Id
                    join isg in db.ItemSubGroup on itm.ItemSubGroupId equals isg.Id
                    join unit in db.Unit on itm.UnitId equals unit.Id
                    join p in db.Party on v.PartyId equals p.Id into party
                    from p in party.DefaultIfEmpty()

                    where vi.Status == App.Status.Enable && v.Status == App.Status.Enable && vi.ItemId == itemId && vi.StoreId == storeId
                    group new{vi, v,s,u,itm,ig,isg,
                        PartyName = p != null ? p.Name : null,
                        UnitName = unit.Description
                    }
                    by new{v.Id,v.Type,v.No,v.Date,StoreDesc = s.Description,CreatedBy = u.Name,v.CreatedAt} into g
                    orderby g.Key.Date, g.Key.Id
                    select new
                    {
                        VoucherId = g.Key.Id,
                        g.Key.Type,
                        g.Key.No,
                        g.Key.Date,
                        g.Key.StoreDesc,
                        g.Key.CreatedBy,
                        g.Key.CreatedAt,
                        ItemGroup = g.Select(x => x.ig.Description).FirstOrDefault(),
                        ItemSubGroup = g.Select(x => x.isg.Description).FirstOrDefault(),
                        ItemName = g.Select(x => x.itm.Description).FirstOrDefault(),
                        Party = g.Select(x => x.PartyName).FirstOrDefault() ?? "",
                        Unit = g.Select(x => x.UnitName).FirstOrDefault(),
                        SerialList = g.Select(x => x.vi.SerialNo),
                        QtyIn = (g.Key.Type == "ReceiptNote"|| g.Key.Type == "StockIn"|| g.Key.Type == "Return")? g.Sum(x => x.vi.Qty): 0,
                        QtyOut =(g.Key.Type == "DeliveryNote"|| g.Key.Type == "StockOut"|| g.Key.Type == "Transfer"|| g.Key.Type == "Adjustment")? Math.Abs(g.Sum(x => x.vi.Qty)): 0
                    }
                    ).ToListAsync();
                    decimal balance = 0;
                    var ledger = history.Select(x =>
                    {
                    balance += x.QtyIn;
                    balance -= x.QtyOut;
                    return new
                    {
                        x.VoucherId,
                        x.Type,
                        x.No,
                        x.Date,
                        x.ItemGroup,
                        x.ItemSubGroup,
                        x.ItemName,
                        x.StoreDesc,
                        x.Party,
                        x.Unit,
                        SerialNo = string.Join(",", x.SerialList.Where(s => !string.IsNullOrEmpty(s)).Distinct()),
                        x.QtyIn,
                        x.QtyOut,
                        Balance = balance,
                        x.CreatedBy,
                        x.CreatedAt
                    };
                    }).ToList();

                    objMsg.data = ledger;

                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> GetHistoryViewOptionAsync()
        {
            Message objMsg = new Message();

            try
            {
                dynamic Option = new ExpandoObject();

                Option.VoucherType = await db.Setting
                    .Where(x => App.ActiveStatus.Contains(x.Status)
                        && x.Name == App.SettingName.VoucherType)
                    .Select(x => new
                    {
                        x.Value,
                        x.Description
                    }).ToListAsync();

                Option.User = await db.User
                    .Where(x => App.ActiveStatus.Contains(x.Status))
                    .Select(x => new
                    {
                        x.Id,
                        x.Name
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
        public async Task<Message> PrintItemHistoryAsync(int itemId, int storeId)
        {
            Message objMsg = new Message();

            try
            {
                var history = await GetItemFullHistoryAsync(itemId, storeId);

                objMsg.data = history.data;

                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }

            return objMsg;
        }
        public async Task<Message> ExportItemHistoryAsync(int itemId, int storeId)
        {
            Message objMsg = new Message();

            try
            {
                var objCompany = await db.Company
                    .FirstOrDefaultAsync(pt => App.ActiveStatus.Contains(pt.Status));

                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Company info not found");
                    return objMsg;
                }

                var history = (await GetItemFullHistoryAsync(itemId, storeId)).data;

                var list = ((IEnumerable<dynamic>)history).Select(x => new
                {
                    x.Date,
                    x.Type,
                    x.No,
                    x.StoreDesc,
                    x.Party,
                    x.Unit,
                    x.SerialNo,
                    x.QtyIn,
                    x.QtyOut,
                    x.Balance,
                    x.CreatedBy,
                    x.CreatedAt
                }).ToList();

                DataTable objDataTable = Util.ListToDataTable(list);

                objCompany.SheetName = "Item History";
                objCompany.ReportDesc = $"Item History - {DateTime.Now:dd-MMM-yyyy}";

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
