
using CRMApi.Models;
using CRMApi.Services;
using SixLabors.Fonts;
using System.Data;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoApprovalConfig
    {
        public readonly DbCRM db;
        public AppSetting App = Util.AppSetting;
        public RepoApprovalRole RepoApprovalRole;
        public RepoApprovalConfig(DbCRM _db)
        {
            db = _db;
            RepoApprovalRole = new RepoApprovalRole(db);
        }
        public Message GetViewOption()
        {
            Message objMsg = new Message();
            try 
            {
                dynamic Option = new ExpandoObject();
                Option.Status = (
                    from ac in db.ApprovalConfig
                    join st in db.Setting on new { Value = ac.Status.ToString(), Name = App.SettingName.Status} equals new { st.Value, st.Name }
                    group st by new { st.Value, st.Description } into st
                    select new
                    {
                        st.Key.Value,
                        st.Key.Description
                    }
                ).ToList();
                Option.ApprovalRole = db.ApprovalRole.Where(ar => App.ActiveStatus.Contains(ar.Status)).Select(ar => new
                {
                    ar.Id,
                    ar.Code,
                    ar.Name,
                    ar.Description
                }).ToList();
                Option.Api = db.Api.Where(ap => App.ActiveStatus.Contains(ap.Status) && ap.IsApprovalRequired).Select(ap => new
                {
                    ap.Id,
                    ap.Code,
                    ap.Name,
                    ap.Description
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
        public Message GetAddOption()
        {
            Message objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();
                Option.ApprovalRole = db.ApprovalRole.Where(ar => App.ActiveStatus.Contains(ar.Status)).Select(ar => new
                {
                    ar.Id,
                    ar.Code,
                    ar.Name,
                    ar.Description
                }).ToList();
                Option.Api = db.Api.Where(ap => App.ActiveStatus.Contains(ap.Status) && ap.IsApprovalRequired).Select(ap => new
                {
                    ap.Id,
                    ap.Code,
                    ap.Name,
                    ap.Description
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
        public List<ApprovalConfig> List(ApprovalConfig? obj, User User)
        {
            obj ??= new ApprovalConfig();
            obj.ListStatus = obj.ListStatus.Count == 0 ? App.ActiveStatus : obj.ListStatus;

            var dbApprovalConfig = db.ApprovalConfig.Where(ac => obj.ListStatus.Contains(ac.Status)).ToList();
            dbApprovalConfig = obj.ListApiId.Any() ? dbApprovalConfig.Where(ac => obj.ListApiId.Contains(ac.ApiId)).ToList() : dbApprovalConfig;
            dbApprovalConfig = obj.ListId.Any() ? dbApprovalConfig.Where(ac => obj.ListId.Contains(ac.Id)).ToList() : dbApprovalConfig;

            var ApprovalSeq = (
                from ac in dbApprovalConfig
                from asq in ac.ApprovalSeq
                join ar in db.ApprovalRole on asq.ApprovalRoleId equals ar.Id
                select new 
                {
                    ac.Id,
                    asq.ApprovalRoleId,
                    ApprovalRoleDesc = ar.Description,
                    asq.SeqNo
                }
            ).ToList();

            var ApprovalConfig = (
                from ac in dbApprovalConfig
                join ap in db.Api on ac.ApiId equals ap.Id                
                join st in db.Setting on new { Value = ac.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                join cb in db.User on ac.CreatedBy equals cb.Id
                join ub in db.User on ac.UpdatedBy equals ub.Id
                select new ApprovalConfig
                {
                    Id = ac.Id,
                    ApiId = ac.ApiId,
                    ApiDesc = ap.Description,                    
                    HtmlString = ac.HtmlString,
                    ApprovalSeq = ac.ApprovalSeq,                    
                    ApprovalSeqText = String.Join("\n", ApprovalSeq.Where(x=> x.Id == ac.Id).Select(x=> $"{x.ApprovalRoleDesc} (Level : {x.SeqNo})").ToList()),
                    Status = ac.Status,
                    StatusDesc = st.Description,
                    StatusCss = st.CssClass,
                    StatusIcon = st.Icon,
                    CreatedBy = ac.CreatedBy,
                    CreatedByName = cb.Name,
                    CreatedAt = ac.CreatedAt,
                    UpdatedBy = ac.UpdatedBy,
                    UpdatedByName = ub.Name,
                    UpdatedAt = ac.UpdatedAt,
                    IsDelete = ac.Status == App.Status.Enable ? true : false,
                    IsEnable  = ac.Status == App.Status.Delete ? true : false
                }
            ).ToList();
            return ApprovalConfig;
        }        
        public Message Get(ApprovalConfig obj, User User)
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
        public Message Print(ApprovalConfig obj, User User)
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
        public Message Export(ApprovalConfig obj, User User)
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
                //Get Api Group
                var ApprovalConfig = List(obj, User);
                //Convert List To DataTable
                DataTable objDataTable = Util.ListToDataTable(ApprovalConfig);
                //Remove Un-wanted column                                
                objDataTable.Columns.Remove("HtmlString");
                objDataTable.Columns.Remove("ApprovalSeq");                               
                objDataTable.Columns.Remove("IsDelete");
                objDataTable.Columns.Remove("IsEnable");                
                objDataTable.Columns.Remove("ListId");
                objDataTable.Columns.Remove("ListApiId");
                objDataTable.Columns.Remove("ListStatus");
                //Convert Datatable to base64               
                objCompany.SheetName = "Approval Config List";
                objCompany.ReportDesc = $"Approval Config - {DateTime.Now.ToString("dd-MMM-yyyy")}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }        
        public Message Add(ApprovalConfig obj, User User)
        {
            Message objMsg = new Message();
            try
            {                
                var dbApprovalConfig = db.ApprovalConfig.Where(ac => App.ActiveStatus.Contains(ac.Status)).ToList();
                if (dbApprovalConfig.Where(ac=> ac.ApiId == obj.ApiId).Any())
                {
                    Message.Duplicate(ref objMsg, $"Api {obj.ApiDesc} is already added in approval config.");
                    return objMsg;
                }
                obj.CreatedBy = User.Id;
                obj.UpdatedBy = User.Id;                
                db.AddRange(obj);
                Message.Add(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {
                    var ApprovalConfig = new ApprovalConfig();
                    ApprovalConfig.ListId.Add(obj.Id);
                    objMsg.obj = List(ApprovalConfig, User).FirstOrDefault();
                    objMsg.data = GetViewOption().data;
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Delete(ApprovalConfig obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var DeleteApprovalConfig = db.ApprovalConfig.Where(ac => ac.Status == App.Status.Enable && ac.Id == obj.Id).AsEnumerable().FirstOrDefault();
                if (DeleteApprovalConfig == null) 
                {
                    Message.Error(ref objMsg, "Approval Config did not found for delete.");
                    return objMsg;
                }
                DeleteApprovalConfig.Status = App.Status.Delete;
                DeleteApprovalConfig.UpdatedBy = User.Id;
                DeleteApprovalConfig.UpdatedAt = DateTime.Now;

                db.Update(DeleteApprovalConfig);
                Message.Delete(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {
                    var ApprovalConfig = new ApprovalConfig();
                    ApprovalConfig.ListId.Add(obj.Id);
                    ApprovalConfig.ListStatus.Add(App.Status.Delete);
                    objMsg.obj = List(ApprovalConfig, User).FirstOrDefault();
                    objMsg.data = GetViewOption().data;
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Enable(ApprovalConfig obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var dbApprovalConfig = db.ApprovalConfig.ToList();
                var ApprovalConfig = dbApprovalConfig.Where(ac => ac.Status == App.Status.Delete && ac.Id == obj.Id).FirstOrDefault();
                if (ApprovalConfig == null)
                {
                    Message.Error(ref objMsg, "Approval Config did not found for enable.");
                    return objMsg;
                }
                var ApiDesc = db.Api.Where(ap => ap.Id == ApprovalConfig.ApiId).AsEnumerable().FirstOrDefault()?.Description ?? "";
                if (dbApprovalConfig.Where(ac => ac.Status == App.Status.Enable && ac.ApiId == ApprovalConfig.ApiId).Any()) 
                {
                    Message.Error(ref objMsg, $"Api ({ApiDesc}) already available in Approval Config.<br>So, You cant enable this.");
                    return objMsg;
                }
                ApprovalConfig.Status = App.Status.Enable;
                ApprovalConfig.UpdatedBy = User.Id;
                ApprovalConfig.UpdatedAt = DateTime.Now;

                db.Update(ApprovalConfig);
                Message.Enable(ref objMsg, db.SaveChanges(), "");
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
