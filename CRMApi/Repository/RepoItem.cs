using CRMApi.Models;
using CRMApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;
using System.Threading.Tasks;

namespace CRMApi.Repository
{
    public class RepoItem
    {
        private readonly DBCRM db;
        private AppSetting App = Util.AppSetting;
        private RepoItemGroup RepoItemGroup;
        private RepoItemSubGroup RepoItemSubGroup;
        private RepoUnit RepoUnit;
        public RepoItem(DBCRM _db)
        {
            db = _db;
            RepoItemGroup = new RepoItemGroup(db);
            RepoItemSubGroup = new RepoItemSubGroup(db);
            RepoUnit = new RepoUnit(db);
        }
        public async Task<Message> GetViewOptionAsync(User user)
        {
            Message objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();
                var ListStatus = await db.Item.GroupBy(x => new { x.Status }).Select(x => x.Key.Status.ToString()).ToListAsync();
                Option.Status = await db.Setting.Where(st => App.ActiveStatus.Contains(st.Status) && st.Name == App.SettingName.Status && ListStatus.Contains(st.Value)).Select(st => new
                {
                    st.Value,
                    st.Description
                }).ToListAsync();
                Option.Status = await(
               from ac in db.Item
               join st in db.Setting on new { Value = ac.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                group st by new { st.Value, st.Description } into st
                 select new
               {
              st.Key.Value,
              st.Key.Description
             }).ToListAsync();
                
                Option.ItemGroup = await db.ItemGroup.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Code,
                    x.Description,
                }).ToListAsync();
                Option.ItemGroup = await db.ItemGroup.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Code,
                    x.Description,
                }).ToListAsync();
                Option.ItemGroup = await db.ItemGroup.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Code,
                    x.Description,
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
        public async Task<Message> GetAddOptionAsync(Item? obj = null)
        {
            Message objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();

                obj ??= new Item();

                var dbItemSubgroup = await (
                    from itsg in db.ItemSubGroup
                    join itg in db.ItemGroup on itsg.ItemGroupId equals itg.Id
                    where itsg.ItemGroupId == obj.ItemGroupId && App.ActiveStatus.Contains(itsg.Status)
                     && App.ActiveStatus.Contains(itsg.Status)
                    select new
                    {
                        itsg.Id,
                        itsg.Description

                    }
                ).ToListAsync();


                Option.Unit = await db.Unit
                .Where(x => App.ActiveStatus.Contains(x.Status))
                .Select(x => new
                {
                    UnitId = x.Id,
                    UnitCode = x.Code,
                    UnitName = x.Name,
                    UnitDesc = x.Description,
                }).Distinct().ToListAsync();


                Option.ItemDescription = await db.Item
                    .Where(und => App.ActiveStatus.Contains(und.Status))
                    .Select(und => new
                    {
                        und.Id,
                        und.Description
                    }).ToListAsync();

                // ItemGroup
                Option.ItemGroup = await db.ItemGroup
                    .Where(x => App.ActiveStatus.Contains(x.Status))
                    .Select(x => new {
                        x.Id,
                        x.Description
                    }).Distinct().ToListAsync();

                // ItemSubGroup
                Option.ItemSubGroup = dbItemSubgroup

                    .Select(x => new
                    {
                        x.Id,
                        x.Description
                    }).Distinct().ToList();

                objMsg.data = Option;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> GetAsync(Item obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                objMsg.data = await List(obj, User);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }

        public async Task<List<Item>> List(Item? obj, User User)
        {
            obj ??= new Item();
            obj.ListStatus = App.ActiveStatus;

            var dbItem = (
                from itm in db.Item
                join igp in db.ItemGroup on itm.ItemGroupId equals igp.Id
                join isg in db.ItemSubGroup on itm.ItemSubGroupId equals isg.Id
                join unt in db.Unit on itm.UnitId equals unt.Id
                join sts in db.Setting on new { Value = itm.Status.ToString(), Name = App.SettingName.Status } equals new { sts.Value, sts.Name }
                join cby in db.User on itm.CreatedBy equals cby.Id
                join uby in db.User on itm.UpdatedBy equals uby.Id
                where
                           (!obj.ListItemGroupId.Any() || obj.ListItemGroupId.Contains(itm.Id)) &&
                           (!obj.ListItemSubGroupId.Any() || obj.ListItemSubGroupId.Contains(itm.ItemSubGroupId)) &&
                           obj.ListStatus.Contains(itm.Status) &&
                           (!obj.ListId.Any() || obj.ListId.Contains(itm.Id))
                select new Item
                {
                    Id = itm.Id,
                    Code = itm.Code,
                    Make = itm.Make,
                    Name = itm.Name,
                    Model = itm.Model,
                    Description = itm.Description,
                    ItemGroupId = itm.ItemGroupId,
                    ItemGroupDesc = igp.Description,
                    ItemSubGroupId = itm.ItemSubGroupId,
                    ItemSubGroupDesc = isg.Description,
                    HsnCode = itm.HsnCode,
                    UnitId = itm.UnitId,
                    UnitDesc = unt.Description,
                    IsImport = itm.IsImport,
                    Status = itm.Status,
                    StatusDesc = sts.Description,
                    StatusCss = sts.CssClass,
                    CreatedBy = itm.CreatedBy,
                    CreatedByName = cby.Name,
                    CreatedAt = itm.CreatedAt,
                    UpdatedBy = itm.UpdatedBy,
                    UpdatedByName = uby.Name,
                    UpdatedAt = itm.UpdatedAt,
                    IsEdit = itm.Status == App.Status.Enable ? true : false,
                    IsDuplicate = true,
                    IsDelete = itm.Status == App.Status.Enable ? true : false,
                    IsEnable = itm.Status == App.Status.Delete ? true : false,
                }
            ).ToList();
            var ListItemId = dbItem.Select(x => x.Id).ToList();
            var dbItemUnit = await (
                from itu in db.ItemUnit
                join itm in db.Item on itu.ItemId equals itm.Id
                join un in db.Unit on itu.UnitId equals un.Id
                join sts in db.Setting on new { Category = App.SettingName.Status, Value = itu.Status.ToString() } equals new { sts.Category, sts.Value }
                join cby in db.User on itu.CreatedBy equals cby.Id
                join uby in db.User on itu.UpdatedBy equals uby.Id
                where
                    ListItemId.Contains(itu.ItemId) &&
                    obj.ListStatus.Contains(itu.Status)
                select new ItemUnit
                {
                    Id = itu.Id,
                    ItemId = itm.Id,
                    UnitId = un.Id,
                    UnitDesc = un.Description,
                    ValuePerUnit = itu.ValuePerUnit,
                    ConversionFactor = itu.ConversionFactor,
                    IsBaseUnit = itu.IsBaseUnit,
                    IsSmallest = itu.IsSmallest,
                    Status = itu.Status,
                    StatusDesc = sts.Description,
                    StatusCss = sts.CssClass ?? "",
                    CreatedBy = itu.CreatedBy,
                    CreatedByName = cby.Name,
                    CreatedAt = itu.CreatedAt,
                    UpdatedBy = itu.UpdatedBy,
                    UpdatedByName = uby.Name,
                    UpdatedAt = uby.UpdatedAt
                }
                ).ToListAsync();
            var itemUnitLookUp = dbItemUnit.ToLookup(x => x.ItemId);
            foreach (var row in dbItem)
            {
                row.ItemUnit = itemUnitLookUp[row.Id].ToList();
            }
            return dbItem;

        }
        public async Task<Message> AddAsync(Item obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                obj.Name = Util.SanitizeInput(obj.Name, "")!;
                obj.Description = Util.SanitizeInput(obj.Description, "")!;
                var dbItem = await List(obj, User);
                if (!String.IsNullOrEmpty(obj.Code) && dbItem.Where(it => it.Code == obj.Code).Any())
                {
                    Message.Duplicate(ref objMsg, $"Item Code ({obj.Code}).");
                    return objMsg;
                }
                if (dbItem.Any(it => it.Name == obj.Name))
                {
                    Message.Duplicate(ref objMsg, $"Item Name ({obj.Name}).");
                    return objMsg;
                }
                if (dbItem.Any(it => it.Name == obj.Name && it.Make == obj.Make && it.Model == obj.Model))
                {
                    Message.Duplicate(ref objMsg, $"Item Name ({obj.Name}) with Make ({obj.Make}) & Model ({obj.Model}).");
                    return objMsg;
                }
                if (dbItem.Any(it => it.Description == obj.Description))
                {
                    Message.Duplicate(ref objMsg, $"Item Description ({obj.Description}).");
                    return objMsg;
                }

                obj.CreatedBy = User.Id;
                obj.UpdatedBy = User.Id;
                obj.CreatedAt = DateTime.Now;
                obj.UpdatedAt = DateTime.Now;
                obj.ItemUnit.ForEach(x =>
                {
                    x.CreatedBy = User.Id;
                    x.UpdatedBy = User.Id;
                    x.CreatedAt = DateTime.Now;
                    x.UpdatedAt = DateTime.Now;
                });
                db.Add(obj);
                int result = await db.SaveChangesAsync();
                Message.Add(ref objMsg, result);
                if (objMsg.status == Message.Type.success)
                {
                    db.Entry(obj).Reload();
                    objMsg.obj = await List(obj, User);
                    objMsg.data = await GetViewOptionAsync(User);
                }
               
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> Update(Item obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                obj.Code = Util.SanitizeInput(obj.Code ?? "", App.Regexp.AlphaLgNum) ?? "";
                var dbItem = db.Item.ToList();
                //if (!String.IsNullOrEmpty(obj.Code) && dbItem.Where(x => x.Code == obj.Code && x.Id != obj.Id).Any())
                //{
                //    Message.Duplicate(ref objMsg, $"Item Code {obj.Code}");
                //    return objMsg;
                //}
                if (dbItem.FirstOrDefault(x => x.Name == obj.Name && x.Id != obj.Id) != null)
                {
                    Message.Duplicate(ref objMsg, $"Item Name {obj.Name}");
                    return objMsg;
                }
                if (dbItem.FirstOrDefault(x => x.Description == obj.Description && x.Id != obj.Id) != null)
                {
                    Message.Duplicate(ref objMsg, $"Item Description {obj.Description}");
                    return objMsg;
                }
                var UpdateItem = dbItem.FirstOrDefault(x => x.Id == obj.Id);
                if (UpdateItem == null)
                {
                    Message.Error(ref objMsg, $"Item did not find for update.");
                    return objMsg;
                }                
                UpdateItem.Name = obj.Name;
                UpdateItem.Make = obj.Make;
                UpdateItem.Model = obj.Model;
                UpdateItem.Description = obj.Description;
                UpdateItem.ItemGroupId = obj.ItemGroupId;
                UpdateItem.ItemSubGroupId = obj.ItemSubGroupId;
                UpdateItem.HsnCode = obj.HsnCode;
                UpdateItem.UnitId = obj.UnitId;
                UpdateItem.UpdatedBy = User.Id;
                UpdateItem.UpdatedAt = DateTime.Now;
                db.Update(UpdateItem);
                Message.Update(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {
                    obj.ListId.Add(obj.Id);
                    objMsg.obj = await List(obj,User);
                    objMsg.data =  await GetViewOptionAsync(User);
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        //public async Task<Message> UpdateAsync(Item obj, User User)
        //{
        //    Message objMsg = new Message();

        //    try
        //    {
        //        var Item = await db.Item.Include(v => v.ItemUnit).FirstOrDefaultAsync(x => App.ActiveStatus.Contains(x.Status) && x.Id == obj.Id);
        //        var  itemUnit  = await db.ItemUnit.Where(x => x.ItemId == obj.Id && App.ActiveStatus.Contains(x.Status)).ToListAsync();
        //        itemUnit.ForEach(vi =>
        //        {
        //              vi.Status = App.Status.Delete;
        //        });
        //        db.UpdateRange(itemUnit);

        //        obj.ItemUnit.ForEach( x =>
        //        {
        //            x.Id = 0;
        //        });


        //        if (Item == null)
        //        {
        //            Message.Error(ref objMsg, "Item was not found for update");
        //            return objMsg;
        //        }
        //        Item.Name = obj.Name;
        //        Item.Description = obj.Description;
        //        Item.ItemGroupId = obj.ItemGroupId;
        //        Item.ItemSubGroupId = obj.ItemSubGroupId;
        //        Item.HsnCode = obj.HsnCode;
        //        Item.UnitId = obj.UnitId;
        //        Item.UpdatedBy = User.Id;
        //        Item.UpdatedAt = DateTime.Now;
        //        foreach (var vi in Item.ItemUnit)
        //        {
        //            var Unit = obj.ItemUnit.FirstOrDefault(x => x.Id == vi.Id);
        //            if (Unit == null)
        //            {
        //                vi.Status = App.Status.Delete;
        //            }
        //            else
        //            {
        //                vi.ItemId = Unit.ItemId;
        //                vi.UnitId = Unit.UnitId;
        //                vi.ValuePerUnit = Unit.ValuePerUnit;
        //                vi.ConversionFactor = Unit.ConversionFactor;
        //                vi.IsBaseUnit = Unit.IsBaseUnit;
        //                vi.IsSmallest = Unit.IsSmallest;
        //                vi.Status = Item.Status;
        //            }
        //            vi.UpdatedBy = User.Id;
        //            vi.UpdatedAt = DateTime.Now;
        //        }
        //        db.Update(Item);

        //        var newUnit = obj.ItemUnit.Where(x => x.Id == 0).ToList();
        //        foreach (var vi in newUnit)
        //        {
        //            vi.ItemId = Item.Id;
        //            vi.Status = Item.Status;
        //            vi.CreatedBy = User.Id;
        //            vi.CreatedAt = DateTime.Now;
        //            vi.UpdatedBy = User.Id;
        //            vi.UpdatedAt = DateTime.Now;
        //        }
        //        db.AddRange(newUnit);
        //        // await db.SaveChangesAsync();
        //        Message.Update(ref objMsg, (await db.SaveChangesAsync()));

        //        if (objMsg.status == Message.Type.success)
        //        {
        //            objMsg.obj = (await List(
        //            new Item { ListId = new List<int> { obj.Id } },
        //            User)).FirstOrDefault();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Message.Exception(ref objMsg, ex);
        //    }

        //    return objMsg;
        //}
        public async Task<Message> UpdateAsync(Item obj, User User)
        {
            Message objMsg = new Message();

            try
            {
                var Item = await db.Item
                    .FirstOrDefaultAsync(x => App.ActiveStatus.Contains(x.Status) && x.Id == obj.Id);

                if (Item == null)
                {
                    Message.Error(ref objMsg, "Item was not found for update");
                    return objMsg;
                }
                Item.Name = obj.Name;
                Item.Description = obj.Description;
                Item.ItemGroupId = obj.ItemGroupId;
                Item.ItemSubGroupId = obj.ItemSubGroupId;
                Item.HsnCode = obj.HsnCode;
                Item.UnitId = obj.UnitId;
                Item.UpdatedBy = User.Id;
                Item.UpdatedAt = DateTime.Now;

                var itemUnits = await db.ItemUnit
                    .Where(x => x.ItemId == obj.Id)
                    .ToListAsync();

                itemUnits.ForEach(x =>
                {
                    x.Status = App.Status.Delete;
                    x.UpdatedBy = User.Id;
                    x.UpdatedAt = DateTime.Now;
                });

                if (obj.ItemUnit != null && obj.ItemUnit.Count > 0)
                {
                    foreach (var vi in obj.ItemUnit)
                    {
                        vi.Id = 0;
                        vi.ItemId = Item.Id;
                        vi.Status = Item.Status;
                        vi.CreatedBy = User.Id;
                        vi.CreatedAt = DateTime.Now;
                        vi.UpdatedBy = User.Id;
                        vi.UpdatedAt = DateTime.Now;
                    }

                    await db.ItemUnit.AddRangeAsync(obj.ItemUnit);
                }
               
                Message.Update(ref objMsg, await db.SaveChangesAsync());

                if (objMsg.status == Message.Type.success)
                {
                    objMsg.obj = (await List(
                        new Item { ListId = new List<int> { obj.Id } },
                        User)).FirstOrDefault();
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
                objMsg.obj = (await List(new Item
                {
                    ListId = new List<int> { Id },
                    ListStatus = new List<int>(App.ActiveStatus) { App.Status.Delete }
                }, User)).FirstOrDefault();
                if (objMsg.obj == null)
                {
                    Message.Error(ref objMsg, "Item was not found for edit.");
                    return objMsg;
                }
                objMsg.data = (await GetAddOptionAsync(objMsg.obj)).data;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> PrintAsync(Item obj, User User)
        {

            Message objMsg = new Message();
            try
            {
                objMsg.data = await List(obj, User);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> ExportAsync(Item obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                // 1️⃣ Get Company
                var objCompany = await db.Company
                    .FirstOrDefaultAsync(pt => App.ActiveStatus.Contains(pt.Status));

                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Item info not found");
                    return objMsg;
                }

                // 2️⃣ Get GDN List using same pattern
                var Gdn = (await List(obj, User)).Select(co => new
                {
                    co.Id,
                    co.Name,
                    co.ItemGroupDesc,                  
                    co.ItemSubGroupDesc,
                    co.UnitDesc,
                    Status = co.StatusDesc,
                    co.CreatedByName,
                    co.CreatedAt,
                    co.UpdatedByName,
                    co.UpdatedAt
                }).ToList();


                DataTable objDataTable = Util.ListToDataTable(Gdn);

                objCompany.SheetName = "Item List";
                objCompany.ReportDesc = $"Item - {DateTime.Now:dd-MMM-yyyy}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);

                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }

            return objMsg;
        }
        public async Task<Message> Enable(int Id, User User)
        {
            Message objMsg = new Message();
            try
            {
                var EnableItem = db.Item.Where(it => it.Id == Id).AsEnumerable().FirstOrDefault();
                if (EnableItem == null)
                {
                    Message.Error(ref objMsg, "Item did not find for delete.");
                    return objMsg;
                }
                EnableItem.Status = App.Status.Enable;
                EnableItem.UpdatedBy = User.Id;
                EnableItem.UpdatedAt = DateTime.Now;
                db.Update(EnableItem);

                Message.Enable(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {
                    EnableItem.ListId.Add(Id);
                    objMsg.obj = List(EnableItem, User);
                    objMsg.data = await  GetViewOptionAsync(User);
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
