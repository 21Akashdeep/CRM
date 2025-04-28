using CRMApi.Models;
using CRMApi.Services;
using System.Data;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoZone
    {
        private readonly DbCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoZone(DbCRM _db) 
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
                    from ac in db.Zone
                    join st in db.Setting on new { Value = ac.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                    group st by new { st.Value, st.Description } into st
                    select new
                    {
                        st.Key.Value,
                        st.Key.Description
                    }
                ).ToList();
                Option.Zone = db.Zone.Where(ag => App.ActiveStatus.Contains(ag.Status)).Select(ag => new
                {
                    ag.Id,
                    ag.Code,
                    ag.Name,
                    ag.Description,                    
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
                dynamic Option = new ExpandoObject();                
                Option.Country = db.Country.Where(ag => App.ActiveStatus.Contains(ag.Status)).Select(ag => new
                {
                    ag.Id,
                    ag.Code,
                    ag.Name,
                    ag.Description,
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
        public List<Zone> List(Zone? obj, User User) 
        {
            obj = obj == null ? new Zone() : obj;
            obj.ListStatus = obj.ListStatus.Count == 0 ? App.ActiveStatus : obj.ListStatus;
            //Get Zone List
            var dbZone = db.Zone.Where(ap => obj.ListStatus.Contains(ap.Status)).AsQueryable();
            dbZone = obj.ListId.Count == 0 ? dbZone : dbZone.Where(ap => obj.ListId.Contains(ap.Id));
            
            //Zone List
            var Zone = (
                from zn in dbZone
                join cn in db.Country on zn.CountryId equals cn.Id
                join st in db.Setting on new { Value = zn.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                join cb in db.User on zn.CreatedBy equals cb.Id
                join ub in db.User on zn.UpdatedBy equals ub.Id
                select new Zone
                {
                    Id = zn.Id,
                    Code = zn.Code,
                    Name = zn.Name,
                    Description = zn.Description,
                    CountryId = zn.CountryId,
                    CountryDesc = cn.Description,
                    Status = zn.Status,
                    StatusName = st.Description,
                    StatusCss = st.CssClass ?? "",
                    CreatedBy = zn.CreatedBy,
                    CreatedByName = cb.Name,
                    CreatedAt = zn.CreatedAt,
                    UpdatedBy = zn.UpdatedBy,
                    UpdatedByName = ub.Name,
                    UpdatedAt = zn.UpdatedAt,
                    IsEdit = zn.Status == App.Status.Enable ? true : false,
                    IsDuplicate = true,
                    IsDelete = zn.Status == App.Status.Enable ? true : false,
                    IsEnable = zn.Status == App.Status.Delete ? true : false,
                }
            ).ToList();
            return Zone;
        }
        public Message Print(Zone obj, User User) 
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
        public Message Export(Zone obj, User User)
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
                //Get Zone Group
                var Zone = List(obj, User).Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.Name,
                    x.Description,
                    x.CountryDesc,                    
                    Status = x.StatusName,
                    x.CreatedBy,
                    x.CreatedByName,
                    x.CreatedAt,
                    x.UpdatedBy,
                    x.UpdatedByName,
                    x.UpdatedAt
                }).ToList();
                //Convert List To DataTable
                DataTable objDataTable = Util.ListToDataTable(Zone);                
                //Convert Datatable to base64               
                objCompany.SheetName = "Zone List";
                objCompany.ReportDesc = $"Zone - {DateTime.Now.ToString("dd-MMM-yyyy")}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Add(Zone obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                //Sanitize Input
                obj.Code = Util.SanitizeInput(obj.Code, null) ?? "";
                obj.Name = Util.SanitizeInput(obj.Name, null) ?? "";
                obj.Description = Util.SanitizeInput(obj.Description, null) ?? "";
                var duplicate = db.Zone.FirstOrDefault(ap => App.ActiveStatus.Contains(ap.Status) && (ap.Code == obj.Code || ap.Name == obj.Name || ap.Description == obj.Description));
                if(duplicate != null)
                {
                    if(duplicate.Code == obj.Code)
                        Message.Duplicate(ref objMsg, $"Zone Code : {obj.Code} already exists.");
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"Zone Name : {obj.Name} already exists.");
                    if (duplicate.Description == obj.Description)
                        Message.Duplicate(ref objMsg, $"Zone Description : {obj.Description} already exists.");
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
        public Message Edit(Zone obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {                                
                var Zone = List(obj, User).FirstOrDefault();
                if (Zone == null) 
                {
                    Message.Error(ref objMsg, "Zone did not find for edit.");
                    return objMsg;
                }
                objMsg.obj = Zone;                
                Message.Success(ref objMsg, "Record found.");
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Update(Zone obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                obj.Name = Util.SanitizeInput(obj.Name, App.Regexp.AlphaNum) ?? "";
                var dbZone = db.Zone.Where(ap => App.ActiveStatus.Contains(ap.Status)).ToList();
                var duplicate = db.Zone.FirstOrDefault(ap => App.ActiveStatus.Contains(ap.Status) && (ap.Code == obj.Code || ap.Name == obj.Name || ap.Description == obj.Description) && ap.Id != obj.Id);
                if (duplicate != null)
                {
                    if (duplicate.Code == obj.Code)
                        Message.Duplicate(ref objMsg, $"Zone Code : {obj.Code} already exists.");
                    if (duplicate.Name == obj.Name)
                        Message.Duplicate(ref objMsg, $"Zone Name : {obj.Name} already exists.");
                    if (duplicate.Description == obj.Description)
                        Message.Duplicate(ref objMsg, $"Zone Description : {obj.Description} already exists.");
                    return objMsg;
                }
                
                var UpdateZone = dbZone.Where(fm => fm.Id == obj.Id).FirstOrDefault();
                if (UpdateZone == null)
                {
                    Message.Error(ref objMsg, "Zone did not find for update.");
                    return objMsg;
                }
                UpdateZone.Code = obj.Code;
                UpdateZone.Name = obj.Name;
                UpdateZone.Description = obj.Description;
                UpdateZone.CountryId = obj.CountryId;
                UpdateZone.UpdatedBy = User.Id;
                UpdateZone.UpdatedAt = DateTime.Now;
                db.Update(UpdateZone);
                Message.Update(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success) 
                {
                    obj.ListId.Add(UpdateZone.Id);
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
        public Message Delete(Zone obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                var DeleteZone = db.Zone.FirstOrDefault(ap => App.ActiveStatus.Contains(ap.Status) && ap.Id == obj.Id);
                if (DeleteZone == null)
                {
                    Message.Error(ref objMsg, "Zone did not find for delete.");
                    return objMsg;
                }
                DeleteZone.Status = App.Status.Delete;
                DeleteZone.UpdatedBy = User.Id;
                DeleteZone.UpdatedAt = DateTime.Now;
                db.Update(DeleteZone);
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
        public Message Enable(Zone obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var EnableZone = db.Zone.FirstOrDefault(ap => App.Status.Delete == ap.Status && ap.Id == obj.Id);
                if (EnableZone == null)
                {
                    Message.Error(ref objMsg, "Zone did not find for enable.");
                    return objMsg;
                }
                EnableZone.Status = App.Status.Enable;
                EnableZone.UpdatedBy = User.Id;
                EnableZone.UpdatedAt = DateTime.Now;
                db.Update(EnableZone);
                Message.Delete(ref objMsg, db.SaveChanges(), "");
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
