using CRMApi.Models;
using CRMApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;

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
        public Message GetViewOption(User User)
        {
            Message objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();
                Option.Status = (
                    from ac in db.Item
                    join st in db.Setting on new { Value = ac.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                    group st by new { st.Value, st.Description } into st
                    select new
                    {
                        st.Key.Value,
                        st.Key.Description
                    }
                ).ToList();
                Option.ItemGroup = RepoItemGroup.List(null, User).Select(ig => new
                {
                    ig.Id,
                    ig.Code,
                    ig.Name,
                    ig.Description
                }).ToList();
                Option.ItemSubGroup = RepoItemSubGroup.List(null, User).Select(ig => new
                {
                    ig.Id,
                    ig.Code,
                    ig.Name,
                    ig.Description
                }).ToList();
                Option.Item = List(null, User).Where(ig => App.ActiveStatus.Contains(ig.Status)).Select(ig => new
                {
                    ig.Id,
                    ig.Code,
                    ig.Name,
                    ig.Description
                }).ToList();
                objMsg.data = Option;
                Message.Success(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message GetAddOption(Item obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();
                Option.ItemGroup = RepoItemGroup.List(null, User).Select(ig => new
                {
                    ig.Id,
                    ig.Code,
                    ig.Name,
                    ig.Description
                }).ToList();

                ItemSubGroup ItemSubGroup = new ItemSubGroup();
                if (obj.ItemGroupId > 0)
                    ItemSubGroup.ListItemGroupId.Add(obj.ItemGroupId);
                Option.ItemSubGroup = RepoItemSubGroup.List(ItemSubGroup, User).Select(ig => new
                {
                    ig.Id,
                    ig.Code,
                    ig.Name,
                    ig.Description,
                    ig.ItemGroupId
                }).ToList();
                Option.Unit = RepoUnit.List(null, User).Select(un => new
                {
                    un.Id,
                    un.Code,
                    un.Name,
                    un.Description,
                }).ToList();
                objMsg.data = Option;
                Message.Success(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public List<Item> List(Item? obj, User User)
        {
            obj ??= new Item();
            obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : App.ActiveStatus;
            var dbItem = db.Item.Where(it => obj.ListStatus.Contains(it.Status)).ToList();
            dbItem = obj.ListId.Any() ? dbItem.Where(it => obj.ListId.Contains(it.Id)).ToList() : dbItem;
            dbItem = obj.ListItemGroupId.Any() ? dbItem.Where(it => obj.ListItemGroupId.Contains(it.ItemGroupId)).ToList() : dbItem;
            dbItem = obj.ListItemSubGroupId.Any() ? dbItem.Where(it => obj.ListItemSubGroupId.Contains(it.ItemSubGroupId)).ToList() : dbItem;

            return (
                from itm in dbItem
                join igp in db.ItemGroup on itm.ItemGroupId equals igp.Id
                join isg in db.ItemSubGroup on itm.ItemSubGroupId equals isg.Id
                join unt in db.Unit on itm.UnitId equals unt.Id                
                join sts in db.Setting on new { Value = itm.Status.ToString(), Name = App.SettingName.Status } equals new { sts.Value, sts.Name }
                join cby in db.User on itm.CreatedBy equals cby.Id
                join uby in db.User on itm.UpdatedBy equals uby.Id
                select new Item
                {
                    Id = itm.Id,
                    Code = itm.Code,
                    Name = itm.Name,
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

        }
        public Message Get(Item obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                objMsg.data = List(obj, User);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Print(Item obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                objMsg.data = List(obj, User);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Export(Item obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                //Get Party
                var objCompany = db.Company.FirstOrDefault(pt => App.ActiveStatus.Contains(pt.Status));
                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Company Info did found");
                    return objMsg;
                }
                //Get Item
                var Item = List(obj, User);
                //Convert List To DataTable
                DataTable objDataTable = Util.ListToDataTable(Item);
                //Remove Un-wanted column                
                objDataTable.Columns.Remove("IsEdit");
                objDataTable.Columns.Remove("IsDelete");
                objDataTable.Columns.Remove("IsEnable");
                objDataTable.Columns.Remove("IsDuplicate");
                objDataTable.Columns.Remove("ListId");
                objDataTable.Columns.Remove("ListItemGroupId");
                objDataTable.Columns.Remove("ListItemSubGroupId");
                objDataTable.Columns.Remove("ListStatus");
                //Convert Datatable to base64               
                objCompany.SheetName = "Item List";
                objCompany.ReportDesc = $"Item - {DateTime.Now.ToString("dd-MMM-yyyy")}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Add(Item obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                obj.Name = Util.SanitizeInput(obj.Name, "")!;
                obj.Description = Util.SanitizeInput(obj.Description, "")!;
                var dbItem = List(obj, User);
                if (!String.IsNullOrEmpty(obj.Code) && dbItem.Where(it => it.Code == obj.Code).Any())
                {
                    Message.Duplicate(ref objMsg, $"Item Code ({obj.Code}).");
                    return objMsg;
                }
                if (dbItem.Where(it => it.Name == obj.Name).Any())
                {
                    Message.Duplicate(ref objMsg, $"Item Name ({obj.Name}).");
                    return objMsg;
                }
                if (dbItem.Where(it => it.Description == obj.Description).Any())
                {
                    Message.Duplicate(ref objMsg, $"Item Description ({obj.Description}).");
                    return objMsg;
                }                
                obj.CreatedBy = User.Id;
                obj.UpdatedBy = User.Id;
                db.Add(obj);
                Message.Add(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {
                    db.Entry(obj).Reload();                    
                    obj.ListId.Add(obj.Id);
                    objMsg.obj = List(obj, User).FirstOrDefault();
                    objMsg.data = GetViewOption(User).data;
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Edit(int Id, User User)
        {
            Message objMsg = new Message();
            try
            {
                var Item = List(new Item { ListId = new List<int>() { Id } }, User).FirstOrDefault();
                if (Item == null)
                {
                    Message.Error(ref objMsg, "Item did not find for edit.");
                    return objMsg;
                }                
                objMsg.obj = Item;                
                objMsg.data = GetAddOption(Item, User).data;
                Message.Success(ref objMsg, "Item found");
            }
            catch (Exception ex) { Message.Exception(ref objMsg, ex); }
            return objMsg;
        }
        public Message Update(Item obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                obj.Code = Util.SanitizeInput(obj.Code ?? "", App.Regexp.AlphaLgNum) ?? "";
                var dbItem = db.Item.ToList();
                if (!String.IsNullOrEmpty(obj.Code) && dbItem.Where(x => x.Code == obj.Code && x.Id != obj.Id).Any())
                {
                    Message.Duplicate(ref objMsg, $"Item Code {obj.Code}");
                    return objMsg;
                }
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
                UpdateItem.Code = String.IsNullOrEmpty(obj.Code) ? UpdateItem.Code : obj.Code;
                UpdateItem.Name = obj.Name;
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
                    objMsg.obj = List(obj, User).FirstOrDefault();
                    objMsg.data = GetViewOption(User).data;
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Delete(int Id, User User)
        {
            Message objMsg = new Message();
            try
            {
                var DeleteItem = db.Item.Where(it => it.Id == Id).AsEnumerable().FirstOrDefault();
                if (DeleteItem == null)
                {
                    Message.Error(ref objMsg, "Item did not find for delete.");
                    return objMsg;
                }
                DeleteItem.Status = App.Status.Delete;
                DeleteItem.UpdatedBy = User.Id;
                DeleteItem.UpdatedAt = DateTime.Now;
                db.Update(DeleteItem);


                Message.Delete(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {
                    DeleteItem.ListStatus.Add(App.Status.Delete);
                    DeleteItem.ListId.Add(Id);
                    objMsg.obj = List(DeleteItem, User).FirstOrDefault();
                    objMsg.data = GetViewOption(User).data;
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Enable(int Id, User User)
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
                    objMsg.obj = List(EnableItem, User).FirstOrDefault();
                    objMsg.data = GetViewOption(User).data;
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
