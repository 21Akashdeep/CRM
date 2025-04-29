using DocumentFormat.OpenXml.Spreadsheet;
using CRMApi.Models;
using CRMApi.Services;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoSetting
    {
        private readonly DbCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoSetting(DbCRM _db) 
        {
            db = _db;            
        }
        public Message GetViewOption() 
        {
            Message objMsg = new Message();
            try 
            {
                var dbSetting = db.Setting.Where(x => App.ActiveStatus.Contains(x.Status)).ToList();
                dynamic Option = new ExpandoObject();
                Option.Status = dbSetting.Where(x => x.Name == App.SettingName.Status).ToList();
                Option.SettingCategory = dbSetting.GroupBy(x => new { x.Category }).Select(x => new { x.Key.Category }).ToList();
                Option.SettingName = dbSetting.GroupBy(x => new { x.Name }).Select(x => new { x.Key.Name }).ToList();
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
                var dbSetting = db.Setting.Where(x => App.ActiveStatus.Contains(x.Status)).ToList();
                dynamic Option = new ExpandoObject();                
                Option.SettingCategory = dbSetting.GroupBy(x => new { x.Category }).Select(x => new { x.Key.Category }).ToList();
                Option.SettingName = dbSetting.GroupBy(x => new { x.Name }).Select(x => new { x.Key.Name }).ToList();
                objMsg.data = Option;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public List<Setting> List(Setting? obj) 
        {            
            obj = obj == null ? new Setting() : obj;
            obj.ListStatus = obj.ListStatus.Count == 0 ? App.ActiveStatus : obj.ListStatus;
            var dbSetting = db.Setting.Where(st => obj.ListStatus.Contains(st.Status)).ToList();
            dbSetting = obj.ListStatus.Count > 0 ? dbSetting.Where(x => obj.ListStatus.Contains(x.Status)).ToList() : dbSetting;
            dbSetting = obj.ListCategory.Count > 0 ? dbSetting.Where(x => obj.ListCategory.Contains(x.Category)).ToList() : dbSetting;
            dbSetting = obj.ListName.Count > 0 ? dbSetting.Where(x => obj.ListName.Contains(x.Name)).ToList() : dbSetting;
            dbSetting = obj.ListId.Count > 0 ? dbSetting.Where(x => obj.ListId.Contains(x.Id)).ToList() : dbSetting;
            var Setting = (
                from st in dbSetting
                join se in db.Setting on st.Status.ToString() equals se.Value
                join cu in db.User on st.CreatedBy equals cu.Id
                join uu in db.User on st.UpdatedBy equals uu.Id
                select new Setting 
                {
                    Id = st.Id,
                    Category = st.Category,
                    Name = st.Name,
                    Description = st.Description,
                    Value = st.Value,
                    CssClass = st.CssClass,
                    Icon = st.Icon,
                    Status = st.Status,
                    StatusText = se.Description,
                    StatusCss = se.CssClass,
                    CreatedBy = st.CreatedBy,
                    CreatedByName = cu.Name,
                    CreatedAt = st.CreatedAt,
                    UpdatedBy = st.UpdatedBy,
                    UpdatedByName = uu.Name,
                    UpdatedAt = st.UpdatedAt,
                    IsEdit = st.Status > 0 ? true : false,
                    IsDuplicate = st.Status > 0 ? true : false,
                    IsDelete = st.Status > 0 ? true : false,
                    IsEnable = st.Status == 0 ? true : false,
                }
            ).ToList();
            return Setting;
        }
        public Message Add(Setting obj)
        {
            Message objMsg = new Message();
            try
            {
                //Sanitize Input
                obj.Category = Util.SanitizeInput(obj.Category, App.Regexp.AlphaNum) ?? "";
                obj.Name = Util.SanitizeInput(obj.Name, App.Regexp.AlphaNum) ?? "";
                obj.Description = Util.SanitizeInput(obj.Description, null) ?? "";
                obj.Value = Util.SanitizeInput(obj.Value, App.Regexp.AlphaNum) ?? "";
                //Check Duplicate
                var dbSetting = db.Setting.FirstOrDefault(x => x.Status == 1 && x.Name == obj.Name && x.Value == obj.Value);
                if (dbSetting != null)
                {
                    Message.Duplicate(ref objMsg, "Setting Name & Value : " + obj.Name + " & " + obj.Value);
                    return objMsg;
                }
                //Add                 
                obj.CreatedBy = obj.User.Id;
                obj.UpdatedBy = obj.User.Id;
                db.Add(obj);
                Message.Add(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success) 
                {
                    obj.ListId.Add(obj.Id);
                    objMsg.obj = List(obj).FirstOrDefault();
                    objMsg.data = GetAddOption().data;
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Edit(int Id)
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
        public Message Update(Setting obj)
        {
            Message objMsg = new Message();
            try
            {
                //Senitise Input
                obj.Category = Util.SanitizeInput(obj.Category, App.Regexp.AlphaNum) ?? "";
                obj.Name = Util.SanitizeInput(obj.Name, App.Regexp.AlphaNum) ?? "";
                obj.Description = Util.SanitizeInput(obj.Description, null) ?? "";
                obj.Value = Util.SanitizeInput(obj.Value, App.Regexp.AlphaNum) ?? "";
                //Prevent Duplicate
                var dbSetting = db.Setting.FirstOrDefault(x => x.Status == 1 && x.Name == obj.Name && x.Value == obj.Value && x.Id != obj.Id);
                if (dbSetting != null)
                {
                    Message.Duplicate(ref objMsg, "Name & Value : " + obj.Name + " & " + obj.Value);
                    return objMsg;
                }
                //Get Setting For Update
                var UpdateSetting = db.Setting.FirstOrDefault(x => x.Status == 1 && x.Id == obj.Id);
                if (UpdateSetting == null)
                {
                    Message.Error(ref objMsg, "Setting did not find.");
                    return objMsg;
                }
                //Update Setting
                UpdateSetting.Category = obj.Category;
                UpdateSetting.Name = obj.Name;
                UpdateSetting.Description = obj.Description;
                UpdateSetting.Value = obj.Value;
                UpdateSetting.CssClass = obj.CssClass;
                UpdateSetting.Icon = obj.Icon;
                UpdateSetting.UpdatedBy = obj.User.Id;
                db.Update(UpdateSetting);
                Message.Update(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success) 
                {
                    obj.ListId.Add(obj.Id);
                    objMsg.obj = List(obj).FirstOrDefault();
                    objMsg.data = GetViewOption().data;
                }                
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Delete(Setting obj)
        {
            Message objMsg = new Message();
            try
            {
                var DeleteSetting = db.Setting.Where(x => x.Status == App.Status.Enable && x.Id == obj.Id).AsEnumerable().FirstOrDefault();
                if (DeleteSetting == null)
                {
                    Message.Error(ref objMsg, "Setting did not find for delete this record...!!!");
                    return objMsg;
                }
                DeleteSetting.Status = 0;
                DeleteSetting.CreatedBy = obj.User.Id;
                DeleteSetting.UpdatedBy = obj.User.Id;
                db.Update(DeleteSetting);
                Message.Delete(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success) 
                {
                    obj.ListId.Add(DeleteSetting.Id);
                    obj.ListStatus.Add(0);
                    obj.ListStatus.AddRange(App.ActiveStatus);                    
                    objMsg.obj = List(obj).FirstOrDefault();
                    objMsg.data = GetViewOption().data;
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Enable(Setting obj)
        {
            Message objMsg = new Message();
            try
            {
                var EnableSetting = db.Setting.Where(x => x.Status == App.Status.Delete && x.Id == obj.Id).AsEnumerable().FirstOrDefault();
                if (EnableSetting == null)
                {
                    Message.Error(ref objMsg, "Setting did not for enable this record...!!!");
                    return objMsg;
                }
                EnableSetting.Status = 1;
                EnableSetting.CreatedBy = obj.User.Id;
                EnableSetting.UpdatedBy = obj.User.Id;
                db.Update(EnableSetting);
                Message.Enable(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success) 
                {
                    obj.ListId.Add(EnableSetting.Id);
                    objMsg.obj = List(obj).FirstOrDefault();
                    objMsg.data = GetViewOption().data;
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
