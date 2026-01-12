using CRMApi.Models;
using CRMApi.Services;
using System.Data;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoStore
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;

        public RepoStore(DBCRM _db)
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
                    from s in db.Store
                    join st in db.Setting
                        on new { Value = s.Status.ToString(), Name = App.SettingName.Status }
                        equals new { st.Value, st.Name }
                    group st by new { st.Value, st.Description } into st
                    select new
                    {
                        st.Key.Value,
                        st.Key.Description
                    }
                ).ToList();

                Option.Store = db.Store
                    .Where(s => App.ActiveStatus.Contains(s.Status))
                    .Select(s => new
                    {
                        s.Id,
                        s.Code,
                        s.Name,
                        s.Description
                    })
                    .ToList();

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
                objMsg.data = new { };
                Message.Success(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }

        public List<Store> List(Store? obj, User User)
        {
            obj ??= new Store();
            obj.ListStatus = obj.ListStatus.Count == 0 ? App.ActiveStatus : obj.ListStatus;

            var dbStore = db.Store
                .Where(s => obj.ListStatus.Contains(s.Status))
                .AsQueryable();

            dbStore = obj.ListId.Count == 0
                ? dbStore
                : dbStore.Where(s => obj.ListId.Contains(s.Id));

            var list = (
                from s in dbStore
                join st in db.Setting
                    on new { Value = s.Status.ToString(), Name = App.SettingName.Status }
                    equals new { st.Value, st.Name }
                join cb in db.User on s.CreatedBy equals cb.Id
                join ub in db.User on s.UpdatedBy equals ub.Id
                select new Store
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.Name,
                    Description = s.Description,
                    Status = s.Status,
                    StatusName = st.Description,
                    StatusCss = st.CssClass ?? "",
                    CreatedBy = s.CreatedBy,
                    CreatedByName = cb.Name,
                    CreatedAt = s.CreatedAt,
                    UpdatedBy = s.UpdatedBy,
                    UpdatedByName = ub.Name,
                    UpdatedAt = s.UpdatedAt,
                    IsEdit = s.Status == App.Status.Enable,
                    IsDuplicate = true,
                    IsDelete = s.Status == App.Status.Enable,
                    IsEnable = s.Status == App.Status.Delete
                }
            ).ToList();

            return list;
        }

        public Message Print(Store obj, User User)
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

        public Message Export(Store obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var objCompany = db.Company.FirstOrDefault(c => App.ActiveStatus.Contains(c.Status));
                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Company did not find for export.");
                    return objMsg;
                }

                var list = List(obj, User).Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.Name,
                    x.Description,
                    x.Status,
                    x.StatusName,
                    x.CreatedByName,
                    x.CreatedAt,
                    x.UpdatedByName,
                    x.UpdatedAt
                }).ToList();

                DataTable dt = Util.ListToDataTable(list);

                objCompany.SheetName = "Store List";
                objCompany.ReportDesc = $"Store - {DateTime.Now:dd-MMM-yyyy}";

                objMsg.base64 = Util.DataTableToBase64(dt, objCompany);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }

        public Message Add(Store obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                obj.Code = Util.SanitizeInput(obj.Code, null) ?? "";
                obj.Name = Util.SanitizeInput(obj.Name, null) ?? "";
                obj.Description = Util.SanitizeInput(obj.Description, null) ?? "";

                var duplicate = db.Store.FirstOrDefault(s =>
                    App.ActiveStatus.Contains(s.Status) &&
                    (s.Code == obj.Code || s.Name == obj.Name || s.Description == obj.Description));

                if (duplicate != null)
                {
                    if (duplicate.Code == obj.Code)
                        Message.Duplicate(ref objMsg, $"Store Code : {obj.Code} already exists.");
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"Store Name : {obj.Name} already exists.");
                    if (duplicate.Description == obj.Description)
                        Message.Duplicate(ref objMsg, $"Store Description : {obj.Description} already exists.");
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
                    objMsg.data = GetViewOption().data;
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }

        public Message Edit(Store obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var store = List(obj, User).FirstOrDefault();
                if (store == null)
                {
                    Message.Error(ref objMsg, "Store did not find for edit.");
                    return objMsg;
                }

                objMsg.obj = store;
                Message.Success(ref objMsg, "Record found.");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }

        public Message Update(Store obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                obj.Name = Util.SanitizeInput(obj.Name, App.Regexp.AlphaNum) ?? "";

                var duplicate = db.Store.FirstOrDefault(s =>
                    App.ActiveStatus.Contains(s.Status) &&
                    (s.Code == obj.Code || s.Name == obj.Name || s.Description == obj.Description) &&
                    s.Id != obj.Id);

                if (duplicate != null)
                {
                    if (duplicate.Code == obj.Code)
                        Message.Duplicate(ref objMsg, $"Store Code : {obj.Code} already exists.");
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"Store Name : {obj.Name} already exists.");
                    if (duplicate.Description == obj.Description)
                        Message.Duplicate(ref objMsg, $"Store Description : {obj.Description} already exists.");
                    return objMsg;
                }

                var update = db.Store.FirstOrDefault(s => s.Id == obj.Id);
                if (update == null)
                {
                    Message.Error(ref objMsg, "Store did not find for update.");
                    return objMsg;
                }

                update.Code = obj.Code;
                update.Name = obj.Name;
                update.Description = obj.Description;
                update.UpdatedBy = User.Id;
                update.UpdatedAt = DateTime.Now;

                db.Update(update);
                Message.Update(ref objMsg, db.SaveChanges(), "");

                if (objMsg.status == Message.Type.success)
                {
                    obj.ListId.Add(update.Id);
                    objMsg.obj = List(obj, User).FirstOrDefault();
                    objMsg.data = GetViewOption().data;
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }

        public Message Delete(Store obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var delete = db.Store.FirstOrDefault(s =>
                    App.ActiveStatus.Contains(s.Status) && s.Id == obj.Id);

                if (delete == null)
                {
                    Message.Error(ref objMsg, "Store did not find for delete.");
                    return objMsg;
                }

                delete.Status = App.Status.Delete;
                delete.UpdatedBy = User.Id;
                delete.UpdatedAt = DateTime.Now;

                db.Update(delete);
                Message.Delete(ref objMsg, db.SaveChanges(), "");

                if (objMsg.status == Message.Type.success)
                {
                    obj.ListId.Add(obj.Id);
                    obj.ListStatus.Add(App.Status.Delete);
                    objMsg.obj = List(obj, User).FirstOrDefault();
                    objMsg.data = GetViewOption().data;
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }

        public Message Enable(Store obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var enable = db.Store.FirstOrDefault(s =>
                    s.Status == App.Status.Delete && s.Id == obj.Id);

                if (enable == null)
                {
                    Message.Error(ref objMsg, "Store did not find for enable.");
                    return objMsg;
                }

                enable.Status = App.Status.Enable;
                enable.UpdatedBy = User.Id;
                enable.UpdatedAt = DateTime.Now;

                db.Update(enable);
                Message.Update(ref objMsg, db.SaveChanges(), "");

                if (objMsg.status == Message.Type.success)
                {
                    obj.ListId.Add(obj.Id);
                    objMsg.obj = List(obj, User).FirstOrDefault();
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
