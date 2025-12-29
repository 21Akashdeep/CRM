using CRMApi.Models;
using CRMApi.Services;
using System.Data;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoQualification
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoQualification(DBCRM _db)
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
                    from q in db.Qualification
                    join st in db.Setting
                        on new { Value = q.Status.ToString(), Name = App.SettingName.Status }
                        equals new { st.Value, st.Name }
                    group st by new { st.Value, st.Description } into st
                    select new
                    {
                        st.Key.Value,
                        st.Key.Description
                    }
                ).ToList();

                Option.Qualification = db.Qualification
                    .Where(q => App.ActiveStatus.Contains(q.Status))
                    .Select(q => new
                    {
                        q.Id,
                        q.Code,
                        q.Name,
                        q.Description
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
        public List<Qualification> List(Qualification? obj, User User)
        {
            obj ??= new Qualification();
            obj.ListStatus = obj.ListStatus.Count == 0 ? App.ActiveStatus : obj.ListStatus;

            var dbQualification = db.Qualification
                .Where(q => obj.ListStatus.Contains(q.Status))
                .AsQueryable();

            dbQualification = obj.ListId.Count == 0
                ? dbQualification
                : dbQualification.Where(q => obj.ListId.Contains(q.Id));

            var list = (
                from q in dbQualification
                join st in db.Setting
                    on new { Value = q.Status.ToString(), Name = App.SettingName.Status }
                    equals new { st.Value, st.Name }
                join cb in db.User on q.CreatedBy equals cb.Id
                join ub in db.User on q.UpdatedBy equals ub.Id
                select new Qualification
                {
                    Id = q.Id,
                    Code = q.Code,
                    Name = q.Name,
                    Description = q.Description,
                    Status = q.Status,
                    StatusName = st.Description,
                    StatusCss = st.CssClass ?? "",
                    CreatedBy = q.CreatedBy,
                    CreatedByName = cb.Name,
                    CreatedAt = q.CreatedAt,
                    UpdatedBy = q.UpdatedBy,
                    UpdatedByName = ub.Name,
                    UpdatedAt = q.UpdatedAt,
                    IsEdit = q.Status == App.Status.Enable,
                    IsDuplicate = true,
                    IsDelete = q.Status == App.Status.Enable,
                    IsEnable = q.Status == App.Status.Delete
                }
            ).ToList();

            return list;
        }
        public Message Print(Qualification obj, User User)
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
        public Message Export(Qualification obj, User User)
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

                objCompany.SheetName = "Qualification List";
                objCompany.ReportDesc = $"Qualification - {DateTime.Now:dd-MMM-yyyy}";

                objMsg.base64 = Util.DataTableToBase64(dt, objCompany);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Add(Qualification obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                obj.Code = Util.SanitizeInput(obj.Code, null) ?? "";
                obj.Name = Util.SanitizeInput(obj.Name, null) ?? "";
                obj.Description = Util.SanitizeInput(obj.Description, null) ?? "";

                var duplicate = db.Qualification.FirstOrDefault(q =>
                    App.ActiveStatus.Contains(q.Status) &&
                    (q.Code == obj.Code || q.Name == obj.Name || q.Description == obj.Description));

                if (duplicate != null)
                {
                    if (duplicate.Code == obj.Code)
                        Message.Duplicate(ref objMsg, $"Qualification Code : {obj.Code} already exists.");
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"Qualification Name : {obj.Name} already exists.");
                    if (duplicate.Description == obj.Description)
                        Message.Duplicate(ref objMsg, $"Qualification Description : {obj.Description} already exists.");
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
        public Message Edit(Qualification obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var qualification = List(obj, User).FirstOrDefault();
                if (qualification == null)
                {
                    Message.Error(ref objMsg, "Qualification did not find for edit.");
                    return objMsg;
                }

                objMsg.obj = qualification;
                Message.Success(ref objMsg, "Record found.");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Update(Qualification obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                obj.Name = Util.SanitizeInput(obj.Name, App.Regexp.AlphaNum) ?? "";

                var duplicate = db.Qualification.FirstOrDefault(q =>
                    App.ActiveStatus.Contains(q.Status) &&
                    (q.Code == obj.Code || q.Name == obj.Name || q.Description == obj.Description) &&
                    q.Id != obj.Id);

                if (duplicate != null)
                {
                    if (duplicate.Code == obj.Code)
                        Message.Duplicate(ref objMsg, $"Qualification Code : {obj.Code} already exists.");
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"Qualification Name : {obj.Name} already exists.");
                    if (duplicate.Description == obj.Description)
                        Message.Duplicate(ref objMsg, $"Qualification Description : {obj.Description} already exists.");
                    return objMsg;
                }

                var update = db.Qualification.FirstOrDefault(q => q.Id == obj.Id);
                if (update == null)
                {
                    Message.Error(ref objMsg, "Qualification did not find for update.");
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
        public Message Delete(Qualification obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var delete = db.Qualification.FirstOrDefault(q =>
                    App.ActiveStatus.Contains(q.Status) && q.Id == obj.Id);

                if (delete == null)
                {
                    Message.Error(ref objMsg, "Qualification did not find for delete.");
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
        public Message Enable(Qualification obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var enable = db.Qualification.FirstOrDefault(q =>
                    q.Status == App.Status.Delete && q.Id == obj.Id);

                if (enable == null)
                {
                    Message.Error(ref objMsg, "Qualification did not find for enable.");
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

