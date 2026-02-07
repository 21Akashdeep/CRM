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
                .Where(x => x.TotalQty > 0)
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
    }
}
