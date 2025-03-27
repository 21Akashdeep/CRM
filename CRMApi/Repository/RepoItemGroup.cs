using DocumentFormat.OpenXml.InkML;
using CRMApi.Models;
using CRMApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoItemGroup
    {
        private readonly DbCRM db;
        private readonly AppSetting App = Util.AppSetting;
        
        public RepoItemGroup(DbCRM _db)
        {
            db = _db;
        }
        public Message GetViewOption()
        {
            Message objMsg = new Message();            
            try 
            {
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
                Option.ItemGroup = db.ItemGroup.Where(ig => App.ActiveStatus.Contains(ig.Status)).Select(ig => new
                {
                    ig.Id,
                    ig.Code,
                    ig.Name,
                    ig.Description
                }).ToList();
                objMsg.data = Option;
                Message.Success(ref objMsg, "Data found");
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
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public List<ItemGroup> List(ItemGroup? obj, User User)
        {
            obj ??= new ItemGroup();
            obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : App.ActiveStatus;
            var dbItemGroup = db.ItemGroup.Where(ig => obj.ListStatus.Contains(ig.Status)).ToList();
            dbItemGroup = obj.ListId.Any() ? dbItemGroup.Where(loc => obj.ListId.Contains(loc.Id)).ToList() : dbItemGroup;

            return (
                from itg in dbItemGroup                
                join sts in db.Setting on itg.Status.ToString() equals sts.Value
                join cby in db.User on itg.CreatedBy equals cby.Id
                join uby in db.User on itg.UpdatedBy equals uby.Id
                where sts.Name == App.SettingName.Status
                select new ItemGroup
                {
                    Id = itg.Id,
                    Code = itg.Code,
                    Name = itg.Name,
                    Description = itg.Description,                    
                    Status = itg.Status,
                    StatusDesc = sts.Description,
                    StatusCss = sts.CssClass,
                    CreatedBy = itg.CreatedBy,
                    CreatedByName = cby.Name,
                    CreatedAt = itg.CreatedAt,
                    UpdatedBy = itg.UpdatedBy,
                    UpdatedByName = uby.Name,
                    UpdatedAt = itg.UpdatedAt,
                    IsEdit = itg.Status == App.Status.Enable ? true : false,
                    IsDelete = itg.Status == App.Status.Enable ? true : false,
                    IsEnable = itg.Status == App.Status.Delete ? true : false,
                    IsDuplicate = true,
                }
            ).ToList();            
        }
        public Message Get(ItemGroup obj, User User)
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
        public Message Print(ItemGroup obj, User User) 
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
        public Message Export(ItemGroup obj, User User)
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
                //Get Item group
                var ItemGroup = List(obj, User);
                //Convert List To DataTable
                DataTable objDataTable = Util.ListToDataTable(ItemGroup);
                //Remove Un-wanted column                
                objDataTable.Columns.Remove("IsEdit");
                objDataTable.Columns.Remove("IsDelete");
                objDataTable.Columns.Remove("IsEnable");
                objDataTable.Columns.Remove("IsDuplicate");
                objDataTable.Columns.Remove("ListId");                
                objDataTable.Columns.Remove("ListStatus");
                //Convert Datatable to base64               
                objCompany.SheetName = "Item Group List";
                objCompany.ReportDesc = $"Item Group - {DateTime.Now.ToString("dd-MMM-yyyy")}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Add(ItemGroup obj, User User)
        {
            Message objMsg = new Message();
            obj.Code = Util.SanitizeInput(obj.Code ?? "", App.Regexp.AlphaLgNum);            
            obj.CreatedBy = User.Id;
            obj.UpdatedBy = User.Id;
            //Validate Duplicate
            var ItemGroup = List(obj, User);
            if (ItemGroup.Where(ig => ig.Code == obj.Code).Count() > 0)
            {
                Message.Duplicate(ref objMsg, $"Item Group Code : {obj.Code} already exists.");
                return objMsg;
            }
            if (ItemGroup.Where(ig => ig.Name == obj.Name).Count() > 0)
            {
                Message.Duplicate(ref objMsg, $"Item Group Name : {obj.Name} already exists.");
                return objMsg;
            }
            if (ItemGroup.Where(ig => ig.Description == obj.Description).Count() > 0)
            {
                Message.Duplicate(ref objMsg, $"Item Group Description : {obj.Description} already exists.");
                return objMsg;
            }
            //Add Value
            db.Add(obj);
            Message.Add(ref objMsg, db.SaveChanges(), "");            
            if (objMsg.status == Message.Type.success)
            {
                db.Entry(obj).CurrentValues.SetValues(
                    db.Database.SqlQueryRaw<ItemGroup>("SELECT * FROM ITEMGROUP WHERE ID = {0} AND ROWNUM = 1", obj.Id).AsEnumerable().FirstOrDefault() ?? obj
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
            ItemGroup obj = new ItemGroup();
            obj.ListId.Add(Id);
            var ItemGroup = List(obj, User).FirstOrDefault();
            if (ItemGroup == null)
            {
                Message.Error(ref objMsg, "Item Group did find for edit.");
                return objMsg;
            }
            objMsg.obj = ItemGroup;
            Message.Success(ref objMsg, "Item Group found");
            return objMsg;
        }
        public Message Update(ItemGroup obj, User User)
        {
            Message objMsg = new Message();
            obj.Code = Util.SanitizeInput(obj.Code ?? "", App.Regexp.AlphaLgNum);
            var ItemGroup = db.ItemGroup.ToList();
            if (ItemGroup.Where(ig => ig.Code == obj.Code && ig.Id != obj.Id).Count() > 0)
            {
                Message.Duplicate(ref objMsg, $"Item Group Code : {obj.Code} already exists.");
                return objMsg;
            }
            if (ItemGroup.Where(ig => ig.Name == obj.Name && ig.Id != obj.Id).Count() > 0)
            {
                Message.Duplicate(ref objMsg, $"Item Group Name : {obj.Name} already exists.");
                return objMsg;
            }
            if (ItemGroup.Where(ig => ig.Description == obj.Description && ig.Id != obj.Id).Count() > 0)
            {
                Message.Duplicate(ref objMsg, $"Item Group Description : {obj.Description} already exists.");
                return objMsg;
            }
            var UpdateItemGroup = ItemGroup.FirstOrDefault(ig => ig.Id == obj.Id);
            if (UpdateItemGroup == null)
            {
                Message.Duplicate(ref objMsg, $"Item Group Name : {obj.Name} did not find for update.");
                return objMsg;
            }
            UpdateItemGroup.Code = String.IsNullOrEmpty(obj.Code) ? UpdateItemGroup.Code : obj.Code;
            UpdateItemGroup.Name = obj.Name;
            UpdateItemGroup.Description = obj.Description;
            UpdateItemGroup.UpdatedBy = User.Id;
            UpdateItemGroup.UpdatedAt = obj.UpdatedAt;
            db.Update(UpdateItemGroup);
            Message.Update(ref objMsg, db.SaveChanges(), "");
            if (objMsg.status == Message.Type.success)
            {
                obj.ListId.Add(UpdateItemGroup.Id);
                objMsg.obj = List(obj, User).FirstOrDefault();
                objMsg.data = GetViewOption().data;
            }
            return objMsg;
        }
        public Message Delete(int Id, User User)
        {
            Message objMsg = new Message();
            var DeleteItemGroup = db.ItemGroup.Where(x => x.Id == Id).AsEnumerable().FirstOrDefault();
            if (DeleteItemGroup == null)
            {
                Message.Error(ref objMsg, "Item Group did not find for delete");
                return objMsg;
            }
            DeleteItemGroup.Status = App.Status.Delete;
            DeleteItemGroup.UpdatedBy = User.Id;
            DeleteItemGroup.UpdatedAt = DateTime.Now;
            db.Update(DeleteItemGroup);
            Message.Delete(ref objMsg, db.SaveChanges(), "");
            if (objMsg.status == Message.Type.success)
            {                
                DeleteItemGroup.ListStatus.Add(App.Status.Delete);
                DeleteItemGroup.ListId.Add(Id);
                objMsg.obj = List(DeleteItemGroup, User).FirstOrDefault();
                objMsg.data = GetViewOption().data;
            }
            return objMsg;
        }
        public Message Enable(int Id, User User)
        {
            Message objMsg = new Message();
            var EnableItemGroup = db.ItemGroup.Where(loc => loc.Id == Id).AsEnumerable().FirstOrDefault();
            if (EnableItemGroup == null)
            {
                Message.Error(ref objMsg, "Item Group did not find for enable");
                return objMsg;
            }
            EnableItemGroup.Status = App.Status.Enable;
            EnableItemGroup.UpdatedBy = User.Id;
            EnableItemGroup.UpdatedAt = DateTime.Now;
            db.Update(EnableItemGroup);
            Message.Enable(ref objMsg, db.SaveChanges(), "");
            if (objMsg.status == Message.Type.success)
            {                
                EnableItemGroup.ListId.Add(Id);
                objMsg.obj = List(EnableItemGroup, User).FirstOrDefault();
                objMsg.data = GetViewOption().data;
            }
            return objMsg;
        }
    }
}
