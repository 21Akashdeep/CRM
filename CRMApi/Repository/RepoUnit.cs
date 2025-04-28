using CRMApi.Models;
using CRMApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;
using System.Linq;

namespace CRMApi.Repository
{
    public class RepoUnit
    {
        private readonly DbCRM db;
        private AppSetting App = Util.AppSetting;
        public RepoUnit(DbCRM _db) 
        {
            db = _db;
        }
        public Message GetViewOption() 
        {
            Message objMsg = new Message();
            try 
            {
                dynamic Option = new ExpandoObject();
                var dbSetting = db.Setting.ToList();
                Option.Status = (
                    from ac in db.Unit.AsEnumerable()
                    join st in dbSetting on new { Value = ac.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                    group st by new { st.Value, st.Description } into st
                    select new
                    {
                        st.Key.Value,
                        st.Key.Description
                    }
                ).ToList();
                Option.Unit = db.Unit.Where(un => App.ActiveStatus.Contains(un.Status)).Select(un => new
                {
                    un.Id,
                    un.Code,
                    un.Name,
                    un.Description
                }).ToList();
                Option.UQCG = dbSetting.Where(st => st.Name == App.SettingName.UQCG).OrderBy(st => st.Description).Select(st => new
                {
                    st.Value,
                    st.Description
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
                var dbSetting = db.Setting.ToList();                
                Option.UQCG = dbSetting.Where(st => st.Name == App.SettingName.UQCG).OrderBy(st => st.Description).Select(st => new
                {
                    st.Value,
                    st.Description
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
        public List<Unit> List(Unit? obj, User User)
        {                        
            obj ??= new Unit();
            obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : App.ActiveStatus;
            var dbUnit = db.Unit.Where(un => obj.ListStatus.Contains(un.Status)).ToList();
            dbUnit = obj.ListId.Any() ? dbUnit.Where(un => obj.ListId.Contains(un.Id)).ToList() : dbUnit;
            dbUnit = obj.ListUQCG.Any() ? dbUnit.Where(un => obj.ListUQCG.Contains(un.UQCG)).ToList() : dbUnit;

            return (
                from un in dbUnit
                join st in db.Setting on new { Value = un.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name}
                join cb in db.User on un.CreatedBy equals cb.Id
                join ub in db.User on un.UpdatedBy equals ub.Id
                select new Unit 
                {
                    Id = un.Id,
                    Code = un.Code,
                    Name = un.Name,
                    Description = un.Description,
                    UQCG = un.UQCG,
                    DecimalPlace = un.DecimalPlace,
                    Status = un.Status,
                    StatusDesc = st.Description,
                    StatusCss = st.CssClass,
                    CreatedBy = un.CreatedBy,
                    CreatedByName = cb.Name,
                    CreatedAt = un.CreatedAt,
                    UpdatedBy = un.CreatedBy,
                    UpdatedByName = cb.Name,
                    UpdatedAt = un.CreatedAt,
                    IsEdit = un.Status == App.Status.Enable ? true : false,
                    IsDuplicate = true,
                    IsDelete = un.Status == App.Status.Enable ? true : false,
                    IsEnable = un.Status == App.Status.Delete ? true : false,
                }
            ).ToList();
        }
        public Message Get(Unit obj, User User)
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
        public Message Print(Unit obj, User User)
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
        public Message Export(Unit obj, User User)
        {
            Message objMsg = new Message();
            try
            {                
                //Get Company
                var objCompany = db.Company.FirstOrDefault(pt => App.ActiveStatus.Contains(pt.Status) && pt.Id == User.CompanyId);
                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Company Info did found");
                    return objMsg;
                }
                //Get Unit
                var Unit = List(obj, User);
                //Convert List To DataTable
                DataTable objDataTable = Util.ListToDataTable(Unit);
                //Remove Un-wanted column                
                objDataTable.Columns.Remove("IsEdit");
                objDataTable.Columns.Remove("IsDelete");
                objDataTable.Columns.Remove("IsEnable");
                objDataTable.Columns.Remove("IsDuplicate");
                objDataTable.Columns.Remove("ListId");
                objDataTable.Columns.Remove("ListUQCG");
                objDataTable.Columns.Remove("ListStatus");
                //Convert Datatable to base64               
                objCompany.SheetName = "Unit List";
                objCompany.ReportDesc = $"Unit - {DateTime.Now.ToString("dd-MMM-yyyy")}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Add(Unit obj, User User)
        {
            Message objMsg = new Message();
            obj.Code = String.IsNullOrEmpty(obj.Code) ? "" : Util.SanitizeInput(obj.Code, App.Regexp.AlphaLgNum);            
            obj.CreatedBy = User.Id;
            obj.UpdatedBy = User.Id;
            //Validate Duplicate
            var Unit = List(obj, User);
            if (Unit.Where(un => un.Code == obj.Code).Count() > 0)
            {
                Message.Duplicate(ref objMsg, $"Unit Code : {obj.Code} already exists.");
                return objMsg;
            }
            if (Unit.Where(un => un.Name == obj.Name).Count() > 0)
            {
                Message.Duplicate(ref objMsg, $"Unit Name : {obj.Name} already exists.");
                return objMsg;
            }
            if (Unit.Where(un => un.Description == obj.Description).Count() > 0)
            {
                Message.Duplicate(ref objMsg, $"Unit Description : {obj.Description} already exists.");
                return objMsg;
            }
            //Add Value
            db.Add(obj);
            Message.Add(ref objMsg, db.SaveChanges(), "");            
            if (objMsg.status == Message.Type.success)
            {
                db.Entry(obj).CurrentValues.SetValues(
                    db.Database.SqlQueryRaw<Unit>("SELECT * FROM UNIT WHERE ID = {0} AND ROWNUM = 1", obj.Id).AsEnumerable().FirstOrDefault() ?? obj
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
            Unit obj = new Unit();
            obj.ListId.Add(Id);
            var Unit = List(obj, User).FirstOrDefault();
            if (Unit == null)
            {
                Message.Error(ref objMsg, "Unit did find for edit.");
                return objMsg;
            }
            objMsg.obj = Unit;
            objMsg.data = GetAddOption().data;
            Message.Success(ref objMsg, "Unit found");
            return objMsg;
        }
        public Message Update(Unit obj, User User)
        {
            Message objMsg = new Message();
            obj.Code = Util.SanitizeInput(obj.Code ?? "", App.Regexp.AlphaLgNum);
            var Unit = db.Unit.ToList();
            if (Unit.Where(lo => lo.Code == obj.Code && lo.Id != obj.Id).Any())
            {
                Message.Duplicate(ref objMsg, $"Unit Code : {obj.Code} already exists.");
                return objMsg;
            }
            if (Unit.Where(lo => lo.Name == obj.Name && lo.Id != obj.Id).Any())
            {
                Message.Duplicate(ref objMsg, $"Unit Name : {obj.Name} already exists.");
                return objMsg;
            }
            if (Unit.Where(lo => lo.Description == obj.Description && lo.Id != obj.Id).Any())
            {
                Message.Duplicate(ref objMsg, $"Unit Description : {obj.Description} already exists.");
                return objMsg;
            }
            var UpdateUnit = Unit.FirstOrDefault(lo => lo.Id == obj.Id);
            if (UpdateUnit == null)
            {
                Message.Duplicate(ref objMsg, $"Unit Name : {obj.Name} did not find for update.");
                return objMsg;
            }
            UpdateUnit.Code = String.IsNullOrEmpty(obj.Code) ? UpdateUnit.Code : obj.Code;
            UpdateUnit.Name = obj.Name;
            UpdateUnit.Description = obj.Description;
            UpdateUnit.UQCG = obj.UQCG;
            UpdateUnit.DecimalPlace = obj.DecimalPlace;
            UpdateUnit.UpdatedBy = User.Id;
            UpdateUnit.UpdatedAt = obj.UpdatedAt;
            db.Update(UpdateUnit);
            Message.Update(ref objMsg, db.SaveChanges(), "");
            if (objMsg.status == Message.Type.success)
            {
                obj.ListId.Add(UpdateUnit.Id);
                objMsg.obj = List(obj, User).FirstOrDefault();
                objMsg.data = GetViewOption().data;
            }
            return objMsg;
        }
        public Message Delete(int Id, User User)
        {
            Message objMsg = new Message();
            var DeleteUnit = db.Unit.Where(x => x.Id == Id).AsEnumerable().FirstOrDefault();
            if (DeleteUnit == null)
            {
                Message.Error(ref objMsg, "Unit did not find for delete");
                return objMsg;
            }
            DeleteUnit.Status = App.Status.Delete;
            DeleteUnit.UpdatedBy = User.Id;
            DeleteUnit.UpdatedAt = DateTime.Now;
            db.Update(DeleteUnit);
            Message.Delete(ref objMsg, db.SaveChanges(), "");
            if (objMsg.status == Message.Type.success)
            {
                DeleteUnit.ListId.Add(Id);
                DeleteUnit.ListStatus.Add(App.Status.Delete);
                objMsg.obj = List(DeleteUnit, User).FirstOrDefault();
                objMsg.data = GetViewOption().data;
            }
            return objMsg;
        }
        public Message Enable(int Id, User User)
        {
            Message objMsg = new Message();
            var EnableUnit = db.Unit.Where(loc => loc.Id == Id).AsEnumerable().FirstOrDefault();
            if (EnableUnit == null)
            {
                Message.Error(ref objMsg, "Unit did not find for enable");
                return objMsg;
            }
            EnableUnit.Status = 1;
            EnableUnit.UpdatedBy = User.Id;
            EnableUnit.UpdatedAt = DateTime.Now;
            db.Update(EnableUnit);
            Message.Enable(ref objMsg, db.SaveChanges(), "");
            if (objMsg.status == Message.Type.success)
            {
                EnableUnit.ListId.Add(Id);
                objMsg.obj = List(EnableUnit, User).FirstOrDefault();
                objMsg.data = GetViewOption().data;
            }
            return objMsg;
        }
    }
}
