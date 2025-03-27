using CRMApi.Models;
using CRMApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoItemSubGroup
    {
        private readonly DbCRM db;
        private AppSetting App = Util.AppSetting;
        public RepoItemSubGroup(DbCRM _db) 
        {
            db = _db;
        }
        public Message GetViewOption()
        {
            Message objMsg = new Message();
            try 
            {                
                //Expando Object
                dynamic Option = new ExpandoObject();
                Option.Status = (
                        from ac in db.ItemGroup
                        join st in db.Setting on new { Value = ac.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                        group st by new { st.Value, st.Description } into st
                        select new
                        {
                            st.Key.Value,
                            st.Key.Description
                        }
                    ).ToList();
                Option.ItemSubGroup = db.ItemSubGroup.Where(isg => App.ActiveStatus.Contains(isg.Status)).Select(isg => new
                {
                    isg.Id,
                    isg.Code,
                    isg.Name,
                    isg.Description
                }).ToList();
                Option.ItemGroup = db.ItemGroup.Where(ig => App.ActiveStatus.Contains(ig.Status)).Select(ig => new
                {
                    ig.Id,
                    ig.Code,
                    ig.Name,
                    ig.Description
                }).ToList();
                objMsg.data = Option;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message GetAddOption()
        {
            Message objMsg = new Message();
            try
            {
                //Expando Object
                dynamic Option = new ExpandoObject();                                
                Option.ItemGroup = db.ItemGroup.Where(ig => App.ActiveStatus.Contains(ig.Status)).Select(ig => new
                {
                    ig.Id,
                    ig.Code,
                    ig.Name,
                    ig.Description
                }).ToList();
                objMsg.data = Option;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public List<ItemSubGroup> List(ItemSubGroup? obj, User User)
        {
            obj ??= new ItemSubGroup();
            obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : App.ActiveStatus;

            var dbItemSubGroup = db.ItemSubGroup.Where(isg => obj.ListStatus.Contains(isg.Status)).ToList();
            dbItemSubGroup = obj.ListId.Any() ? dbItemSubGroup.Where(isg => obj.ListId.Contains(isg.Id)).ToList() : dbItemSubGroup;
            dbItemSubGroup = obj.ListItemGroupId.Any() ? dbItemSubGroup.Where(isg => obj.ListItemGroupId.Contains(isg.ItemGroupId)).ToList() : dbItemSubGroup;

            return (
                from isg in dbItemSubGroup
                join grp in db.ItemGroup on isg.ItemGroupId equals grp.Id                
                join sts in db.Setting on isg.Status.ToString() equals sts.Value
                join cby in db.User on isg.CreatedBy equals cby.Id
                join uby in db.User on isg.UpdatedBy equals uby.Id
                where sts.Name == App.SettingName.Status
                select new ItemSubGroup
                {
                    Id = isg.Id,
                    Code = isg.Code,
                    Name = isg.Name,
                    Description = isg.Description,                    
                    ItemGroupId = isg.ItemGroupId,
                    ItemGroupDesc = grp.Description,                    
                    Status = isg.Status,
                    StatusName = sts.Description,
                    StatusCss = sts.CssClass,
                    CreatedBy = isg.CreatedBy,
                    CreatedByName = cby.Name,
                    CreatedAt = isg.CreatedAt,
                    UpdatedBy = isg.UpdatedBy,
                    UpdatedByName = uby.Name,
                    UpdatedAt = isg.UpdatedAt,
                    IsEdit = isg.Status == App.Status.Enable ? true : false,
                    IsDelete = isg.Status == App.Status.Enable ? true : false,
                    IsEnable = isg.Status == App.Status.Delete ? true : false,
                    IsDuplicate = true,
                }
            ).ToList();            
        }
        public Message Get(ItemSubGroup obj, User User) 
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
        public Message Print(ItemSubGroup obj, User User)
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
        public Message Export(ItemSubGroup obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                //Get Party
                var objCompany = db.Company.Where(pt => App.ActiveStatus.Contains(pt.Status)).AsEnumerable().FirstOrDefault();
                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Company Info did found");
                    return objMsg;
                }
                //Get Item Sub Group
                var ItemSubGroup = List(obj, User);
                //Convert List To DataTable
                DataTable objDataTable = Util.ListToDataTable(ItemSubGroup);
                //Remove Un-wanted column                
                objDataTable.Columns.Remove("IsEdit");
                objDataTable.Columns.Remove("IsDelete");
                objDataTable.Columns.Remove("IsEnable");
                objDataTable.Columns.Remove("IsDuplicate");
                objDataTable.Columns.Remove("ListId");
                objDataTable.Columns.Remove("ListStatus");
                //Convert Datatable to base64               
                objCompany.SheetName = "Item Sub Group List";
                objCompany.ReportDesc = $"Item Sub Group - {DateTime.Now.ToString("dd-MMM-yyyy")}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Add(ItemSubGroup obj, User User)
        {
            Message objMsg = new Message();
            obj.Code = Util.SanitizeInput(obj.Code ?? "", App.Regexp.AlphaLgNum);            
            obj.CreatedBy = User.Id;
            obj.UpdatedBy = User.Id;
            //Validate Duplicate
            var ItemSubGroup = List(obj, User);
            if (ItemSubGroup.Where(isg => isg.Code == obj.Code).Any())
            {
                Message.Duplicate(ref objMsg, $"Item Sub Group Code : {obj.Code} already exists.");
                return objMsg;
            }
            if (ItemSubGroup.Where(isg => isg.Name == obj.Name).Any())
            {
                Message.Duplicate(ref objMsg, $"Item Sub Group Name : {obj.Name} already exists.");
                return objMsg;
            }
            if (ItemSubGroup.Where(isg => isg.Description == obj.Description).Any())
            {
                Message.Duplicate(ref objMsg, $"Item Sub Group Description : {obj.Description} already exists.");
                return objMsg;
            }
            //Add Value
            db.Add(obj);
            Message.Add(ref objMsg, db.SaveChanges(), "");
            
            if (objMsg.status == Message.Type.success)
            {
                db.Entry(obj).CurrentValues.SetValues(
                    db.Database.SqlQueryRaw<ItemSubGroup>("SELECT * FROM ITEMSUBGROUP WHERE ID = {0} AND ROWNUM = 1", obj.Id).AsEnumerable().FirstOrDefault() ?? obj
                );
                obj.ListId.Add(obj.Id);
                objMsg.obj = List(obj, User).FirstOrDefault();
                objMsg.data = GetViewOption().data;
            }
            return objMsg;
        }
        public Message Edit(int Id, User User)
        {
            Message objMsg = new Message();
            try 
            {
                ItemSubGroup obj = new ItemSubGroup();
                obj.ListId.Add(Id);
                var ItemSubGroup = List(obj, User).FirstOrDefault();
                if (ItemSubGroup == null)
                {
                    Message.Error(ref objMsg, "Item Sub Group did find for edit.");
                    return objMsg;
                }
                objMsg.obj = ItemSubGroup;
                objMsg.data = GetAddOption().data;
                Message.Success(ref objMsg, "Item Sub Group found.");
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Update(ItemSubGroup obj, User User)
        {
            Message objMsg = new Message();
            obj.Code = Util.SanitizeInput(obj.Code ?? "", App.Regexp.AlphaLgNum);
            var ItemSubGroup = db.ItemSubGroup.ToList();
            if (ItemSubGroup.Where(isg => isg.Code == obj.Code && isg.Id != obj.Id).Count() > 0)
            {
                Message.Duplicate(ref objMsg, $"Item Sub Group Code : {obj.Code} already exists.");
                return objMsg;
            }
            if (ItemSubGroup.Where(isg => isg.Name == obj.Name && isg.Id != obj.Id).Count() > 0)
            {
                Message.Duplicate(ref objMsg, $"Item Sub Group Name : {obj.Name} already exists.");
                return objMsg;
            }
            if (ItemSubGroup.Where(isg => isg.Description == obj.Description && isg.Id != obj.Id).Count() > 0)
            {
                Message.Duplicate(ref objMsg, $"Item Sub Group Description : {obj.Description} already exists.");
                return objMsg;
            }
            var UpdateItemSubGroup = ItemSubGroup.FirstOrDefault(lo => lo.Id == obj.Id);
            if (UpdateItemSubGroup == null)
            {
                Message.Duplicate(ref objMsg, $"Item Sub Group Name : {obj.Name} did not find for update.");
                return objMsg;
            }
            UpdateItemSubGroup.Code = String.IsNullOrEmpty(obj.Code) ? UpdateItemSubGroup.Code : obj.Code;
            UpdateItemSubGroup.Name = obj.Name;
            UpdateItemSubGroup.Description = obj.Description;
            UpdateItemSubGroup.ItemGroupId = obj.ItemGroupId;
            UpdateItemSubGroup.UpdatedBy = User.Id;
            UpdateItemSubGroup.UpdatedAt = obj.UpdatedAt;
            db.Update(UpdateItemSubGroup);
            Message.Update(ref objMsg, db.SaveChanges(), "");
            if (objMsg.status == Message.Type.success)
            {
                obj.ListId.Add(UpdateItemSubGroup.Id);
                objMsg.obj = List(obj, User).FirstOrDefault();
                objMsg.data = GetViewOption().data;
            }
            return objMsg;
        }
        public Message Delete(int Id, User User)
        {
            Message objMsg = new Message();
            var DeleteItemSubGroup = db.ItemSubGroup.Where(x => x.Id == Id).AsEnumerable().FirstOrDefault();
            if (DeleteItemSubGroup == null)
            {
                Message.Error(ref objMsg, "Item Sub Group did not find for delete");
                return objMsg;
            }
            DeleteItemSubGroup.Status = App.Status.Delete;
            DeleteItemSubGroup.UpdatedBy = User.Id;
            DeleteItemSubGroup.UpdatedAt = DateTime.Now;
            db.Update(DeleteItemSubGroup);
            Message.Delete(ref objMsg, db.SaveChanges(), "");
            if (objMsg.status == Message.Type.success)
            {                
                DeleteItemSubGroup.ListStatus.Add(App.Status.Delete);
                DeleteItemSubGroup.ListId.Add(Id);
                objMsg.obj = List(DeleteItemSubGroup, User).FirstOrDefault();
                objMsg.data = GetAddOption().data;
            }
            return objMsg;
        }
        public Message Enable(int Id, User User)
        {
            Message objMsg = new Message();
            var EnableItemSubGroup = db.ItemSubGroup.Where(loc => loc.Id == Id).AsEnumerable().FirstOrDefault();
            if (EnableItemSubGroup == null)
            {
                Message.Error(ref objMsg, "Item Sub Group did not find for enable");
                return objMsg;
            }
            EnableItemSubGroup.Status = App.Status.Enable;
            EnableItemSubGroup.UpdatedBy = User.Id;
            EnableItemSubGroup.UpdatedAt = DateTime.Now;
            db.Update(EnableItemSubGroup);
            Message.Enable(ref objMsg, db.SaveChanges(), "");
            if (objMsg.status == Message.Type.success)
            {
                EnableItemSubGroup.ListId.Add(Id);
                objMsg.obj = List(EnableItemSubGroup, User).FirstOrDefault();
                objMsg.data = GetViewOption().data;
            }
            return objMsg;
        }
    }
}
