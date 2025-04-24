using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using DocumentFormat.OpenXml.Presentation;
using CRMApi.Models;
using CRMApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;
using System.Net.Mail;
using System.Security.Cryptography.Xml;

namespace CRMApi.Repository
{
    public class RepoUser
    {
        private readonly DbCRM db;
        private AppSetting App = Util.AppSetting;
        //private RepoApi RepoApi;
        private RepoApprovalRole RepoApprovalRole;
        public RepoUser(DbCRM _db) 
        {
            db = _db;            
            RepoApprovalRole = new RepoApprovalRole(db);
        }        
        
        public Message GetViewOption() 
        {
            Message objMsg = new Message();
            try 
            {
                var dbUser = db.User.ToList();
                var dbSetting = db.Setting.Where(st => App.ActiveStatus.Contains(st.Status)).ToList();
                
                dynamic Options = new ExpandoObject();

                Options.Status = (
                    from ur in dbUser
                    join st in dbSetting on new { Value = ur.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                    group ur by new { ur.Status, st.Description } into ur
                    select new
                    {
                        Value = ur.Key.Status,
                        ur.Key.Description
                    }
                ).ToList();
                
                Options.User = dbUser.Where(ur => App.ActiveStatus.Contains(ur.Status)).Select(ur => new
                {
                    ur.Id,
                    ur.UserId,
                    ur.Name
                }).ToList();
                
                Options.UserType = dbSetting.Where(st => st.Name == App.SettingName.UserType).Select(st => new
                {
                    st.Value,
                    st.Description
                }).ToList();
                
                objMsg.data = Options;
                Message.Success(ref objMsg, "");
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message GetAddOption(int Id) 
        {
            Message objMsg = new Message();
            try 
            {
                dynamic Options = new ExpandoObject();
                
                //Get Setting Option
                var dbSetting = db.Setting.Where(st => App.ActiveStatus.Contains(st.Status)).ToList();
                Options.UserType = dbSetting.Where(st => st.Name == App.SettingName.UserType).Select(st => new { st.Value, st.Description }).ToList();

                //Get User
                var dbUser = db.User.Where(ur => App.ActiveStatus.Contains(ur.Status)).ToList();                                                
                Options.User = dbUser.Select(ur => new
                {
                    ur.Id,
                    ur.UserId,
                    ur.Name
                }).ToList();

                //Get Company Permission                
                var dbCompany = db.Company.Where(x => App.ActiveStatus.Contains(x.Status)).ToList();
                var dbUserCompany = dbUser.Where(x => x.Id == Id).SelectMany(x => x.Company).ToList();
                Options.Company = (
                    from com in dbCompany
                    join uco in dbUserCompany on com.Id equals uco.CompanyId into UserCompany
                    from uco in UserCompany.DefaultIfEmpty()
                    select new 
                    {                        
                        CompanyId = com.Id,
                        CompanyName = com.Name,
                        CompanyDesc = com.Description,
                        IsDefault = uco?.IsDefault ?? false,
                        IsAdded = uco != null,
                    }
                ).ToList();

                //Get Api Permission                
                var dbApi = db.Api.Where(ap => App.ActiveStatus.Contains(ap.Status)).ToList();
                var dbUserApi = dbUser.Where(x => x.Id == Id).SelectMany(x => x.Api).ToList();
                Options.Api = (
                    from api in dbApi
                    join agp in db.ApiGroup on api.ApiGroupId equals agp.Id
                    join set in dbSetting on new { Value = api.ApiType, Name = App.SettingName.ApiType } equals new { set.Value, set.Name }
                    join ape in dbUserApi on api.Id equals ape.ApiId into ApiPer
                    from ape in ApiPer.DefaultIfEmpty()
                    orderby agp.SeqNo, api.SeqNo
                    select new
                    {
                        ApiId = api.Id,                        
                        ApiName = api.Name,
                        ApiDesc = api.Description,
                        ApiGroupDesc = agp.Description,
                        api.ApiType,
                        ApiTypeDesc = set.Description,
                        View = ape?.View ?? false,
                        Add = ape?.Add ?? false,
                        Update = ape?.Update ?? false,
                        Delete = ape?.Delete ?? false,
                        Enable = ape?.Enable ?? false,                        
                        Print = ape?.Print ?? false,
                        Import = ape?.Import ?? false,
                        Export = ape?.Export ?? false,
                        IsAdded = ape != null
                    }
                ).ToList();

                //Get Approval Role
                var dbApprovalRole = db.ApprovalRole.Where(ar => App.ActiveStatus.Contains(ar.Status)).ToList();
                var dbUserAprRole = dbUser.Where(x => x.Id == Id).SelectMany(x => x.ApprovalRole).ToList();
                Options.AprRole = (
                    from arl in dbApprovalRole
                    join arp in dbUserAprRole on arl.Id equals arp.ApprovalRoleId into ApRo
                    from arp in ApRo.DefaultIfEmpty()
                    select new 
                    {
                        ApprovalRoleId = arl.Id,
                        ApprovalRoleDesc = arl.Description,
                        IsAdded = arp != null
                    }
                ).ToList();

                objMsg.data = Options;
                Message.Success(ref objMsg, "");
            } 
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public List<User> List(User? obj, User objLogger)
        {            
            obj ??= new User();
            obj.ListStatus = obj.ListStatus.Count == 0 ? App.ActiveStatus : obj.ListStatus;
            
            var dbUsers = db.User.ToList();

            var dbUser = dbUsers.Where(x => obj.ListStatus.Contains(x.Status)).ToList();            
            dbUser = obj.ListUserType.Any() ? dbUser.Where(x => obj.ListUserType.Contains(x.UserType)).ToList() : dbUser;
            dbUser = obj.ListId.Any() ? dbUser.Where(x => obj.ListId.Contains(x.Id)).ToList() : dbUser;
           
            var dbSetting = db.Setting.Where(st => App.ActiveStatus.Contains(st.Status)).ToList();
            
            var User = (
                from ur in dbUser                
                join ut in dbSetting on new { Value = ur.UserType, Name = App.SettingName.UserType } equals new { ut.Value, ut.Name }
                join st in dbSetting on new { Value = ur.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                join cb in dbUsers on ur.CreatedBy equals cb.Id
                join ub in dbUsers on ur.UpdatedBy equals ub.Id
                select new User
                {
                    Id = ur.Id,
                    UserId = ur.UserId,
                    Password = ur.Password,                    
                    Code = ur.Code,
                    Name = ur.Name,
                    DateOfBirth = ur.DateOfBirth,
                    Gender = ur.Gender,
                    FatherName = ur.FatherName,
                    ContactNo = ur.ContactNo,
                    Email = ur.Email,
                    UserType = ur.UserType,
                    UserTypeDesc = ut.Description,                    
                    PasswordExpiredAt = ur.PasswordExpiredAt,
                    Company = ur.Company,
                    Api = ur.Api,
                    ApprovalRole = ur.ApprovalRole,
                    Theme = ur.Theme,
                    Status = ur.Status,
                    StatusDesc = st.Description,
                    StatusCss = st.CssClass,
                    CreatedBy = ur.CreatedBy,
                    CreatedByName = cb.Name,
                    CreatedAt = ur.CreatedAt,
                    UpdatedBy = ur.UpdatedBy,
                    UpdatedByName = ub.Name,
                    UpdatedAt = ur.UpdatedAt,
                    IsReSentPassword = ur.Status == App.Status.Enable ? true : false,
                    IsEdit = ur.Status == App.Status.Enable ? true : false,
                    IsDuplicate = true,
                    IsDelete = ur.Status == App.Status.Enable ? true : false,
                    IsEnable = ur.Status == App.Status.Delete ? true : false,
                }
            ).ToList();
            return User;
        }        
        public Message Print(User obj, User objLogger)
        {
            Message objMsg = new Message();
            try
            {
                objMsg.data = List(obj, objLogger);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Export(User obj, User objLooger)
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
                //Get User
                var dbUser = List(obj, objLooger);
                var User = dbUser.Select(u => new
                {
                    u.Id,
                    u.UserId,
                    UserType = u.UserTypeDesc,
                    u.Name,
                    u.Code,
                    u.DateOfBirth,
                    u.Gender,
                    u.FatherName,
                    u.ContactNo,
                    u.Email,
                    u.PasswordExpiredAt,
                    Company = String.Join(", ", u.Company.Select(c=> c.CompanyDesc).ToList()),
                    Api = String.Join(", ", u.Api.Select(c => c.ApiDesc).ToList()),
                    ApprovalRole = String.Join(", ", u.ApprovalRole.Select(c => c.ApprovalRoleDesc).ToList()),
                    u.Theme,
                    Status = u.StatusDesc,
                    CreatedBy = u.CreatedByName,
                    u.CreatedAt,
                    UpdatedBy = u.UpdatedByName,
                    u.UpdatedAt
                }).ToList();
                //Convert List To DataTable
                DataTable objDataTable = Util.ListToDataTable(User);                
                //Convert Datatable to base64               
                objCompany.SheetName = "User List";
                objCompany.ReportDesc = $"User List - {DateTime.Now.ToString("dd-MMM-yyyy")}";
                objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);
                Message.Success(ref objMsg, "Record found.");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Add(User obj, User objLogger) 
        {
            Message objMsg = new Message();
            try 
            {
                obj.UserId = Util.SanitizeInput(obj.UserId, $"{App.Regexp.AlphaNum}_");
                var dbUser = db.User.Where(ur => App.ActiveStatus.Contains(ur.Status)).ToList();
                if (dbUser.Where(ur => ur.UserId == obj.UserId).Count() > 0)
                {
                    Message.Duplicate(ref objMsg, $"User Id : {obj.UserId}");
                    return objMsg;
                }
                if (dbUser.Where(ur => ur.ContactNo == obj.ContactNo).Count() > 0)
                {
                    Message.Duplicate(ref objMsg, $"Contact No. : {obj.ContactNo}");
                    return objMsg;
                }
                if (dbUser.Where(ur => ur.Email == obj.Email).Count() > 0)
                {
                    Message.Duplicate(ref objMsg, $"Email : {obj.Email}");
                    return objMsg;
                }                
                obj.Password = Util.Encrypt(Util.RandomAlphaNum());
                obj.PasswordExpiredAt = DateTime.Now.AddMonths(24);
                obj.CreatedBy = objLogger.Id;
                obj.UpdatedBy = objLogger.Id;
                
                db.Add(obj);
                Message.Add(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {
                    obj.ListId.Add(obj.Id);
                    objMsg.obj = List(obj, objLogger);
                    Message msg = new Message();
                    SentPassword(obj.Id, ref msg);
                    if (msg.status != Message.Type.success)
                    {
                        objMsg.status = Message.Type.warning;
                        objMsg.statusText += $"Sent Password Error Via Email : {msg.statusText}";
                    }
                }
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public void SentPassword(int Id, ref Message objMsg)
        {
            var obj = db.User.Where(ur => App.ActiveStatus.Contains(ur.Status) && ur.Id == Id).AsEnumerable().FirstOrDefault();
            if (obj == null) 
            {
                Message.Error(ref objMsg, "User did not find for Re-Sent Password");
                return;
            }
            if (obj.Email == "") 
            {
                Message.Error(ref objMsg, "Email is empty.");
                return;
            }

            string matter = @$"
                <table style='width:30%;' cellpadding='4'>
                    <tr>
                        <th>User Id</th>
                        <th>&nbsp;:&nbsp;</th>
                        <td>{obj.UserId}</td>
                    </tr>
                    <tr>
                        <th>Password</th>
                        <th>&nbsp;:&nbsp;</th>
                        <td>{Util.Decrypt(obj.Password)}</td>
                    </tr>
                </table>
            ";
            MailMessage objMail = new MailMessage();
            objMail.To.Add(obj.Email);
            Util.SentMail(db, objMail, $"{App.AppInfo.Name} User Login Credential", matter, "info", ref objMsg);
        }
        public Message Update(User obj, User objLogger) 
        {
            Message objMsg = new Message();
            try 
            {
                obj.UserId = Util.SanitizeInput(obj.UserId, $"{App.Regexp.AlphaNum}_");                
                var dbUser = db.User.Where(x => App.ActiveStatus.Contains(x.Status)).ToList();
                if (dbUser.Where(ur => ur.UserId == obj.UserId && ur.Id != obj.Id).Count() > 0)
                {
                    Message.Duplicate(ref objMsg, $"User Id : {obj.UserId}");
                    return objMsg;
                }
                var UpdateUser = dbUser.Where(ur => ur.Id == obj.Id).FirstOrDefault();
                if (UpdateUser == null)
                {
                    Message.Error(ref objMsg, "User did not find for update");
                    return objMsg;
                }                
                UpdateUser.UserId = UpdateUser.UserId == "SysAdmin" ? UpdateUser.UserId : obj.UserId;
                UpdateUser.UserType = obj.UserType;                
                UpdateUser.Code = obj.Code;
                UpdateUser.Name = obj.Name;
                UpdateUser.DateOfBirth = obj.DateOfBirth;
                UpdateUser.FatherName = obj.FatherName;
                UpdateUser.ContactNo = obj.ContactNo;
                UpdateUser.Email = obj.Email;                
                UpdateUser.PasswordExpiredAt = obj.PasswordExpiredAt == null || obj.PasswordExpiredAt == default(DateTime) ? UpdateUser.CreatedAt.AddMonths(24) : obj.PasswordExpiredAt;
                UpdateUser.Company = obj.Company;
                UpdateUser.Api = obj.Api;
                UpdateUser.ApprovalRole = obj.ApprovalRole;
                UpdateUser.UpdatedBy = objLogger.Id;
                UpdateUser.UpdatedAt = DateTime.Now;
                db.Update(UpdateUser);
                Message.Update(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success) 
                {
                    obj.ListId.Add(UpdateUser.Id);
                    objMsg.obj = List(obj, objLogger).FirstOrDefault();
                    objMsg.data = GetViewOption().data;
                }
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        //This Method Only use by self User
        public Message UpdateProfile(User obj)
        {
            Message objMsg = new Message();
            try
            {
                var dbUser = db.User.Where(usr => App.ActiveStatus.Contains(usr.Status) && usr.Id == obj.Id).AsEnumerable().FirstOrDefault();
                if (dbUser == null)
                {
                    Message.Error(ref objMsg, "");
                    return objMsg;
                }
                dbUser.Theme = obj.Theme;
                dbUser.UpdatedBy = obj.Id;
                dbUser.UpdatedAt = DateTime.Now;
                dbUser.ApiType = obj.ApiType;
                db.Update(dbUser);
                Message.Update(ref objMsg, db.SaveChanges(), "");
                objMsg.data = UserInfo(dbUser).data;
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        //This Method only use by self user
        public Message UpdatePassword(User obj)
        {
            Message objMsg = new Message();
            try
            {                
                var UpdateUser = db.User.Where(x => App.ActiveStatus.Contains(x.Status) && x.Id == obj.Id && x.Password == Util.Encrypt(obj.Password)).AsEnumerable().FirstOrDefault();
                if (UpdateUser == null)
                {
                    Message.Error(ref objMsg, "User did not find for update.");
                    return objMsg;
                }
                if (obj.NewPassword != obj.ConfirmPassword) 
                {
                    Message.Error(ref objMsg, "New Password and Comfirm Pasword did not match.");
                    return objMsg;
                }
                UpdateUser.Password = Util.Encrypt(obj.NewPassword ?? obj.Password);
                UpdateUser.UpdatedBy = obj.Id;
                UpdateUser.UpdatedAt = DateTime.Now;
                db.Update(UpdateUser);
                Message.Update(ref objMsg, db.SaveChanges(), "");                
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }        
        public Message Delete(User obj, User objLogger) 
        {
            Message objMsg = new Message();
            try 
            {
                var DeleteUser = db.User.Where(ap => App.ActiveStatus.Contains(ap.Status) && ap.Id == obj.Id).AsEnumerable().FirstOrDefault();
                if (DeleteUser == null)
                {
                    Message.Error(ref objMsg, "User did not find for delete.");
                    return objMsg;
                }
                if (DeleteUser.UserId == "SysAdmin")
                {
                    Message.Error(ref objMsg, "Opps, You can not delete SysAdmin.");
                    return objMsg;
                }
                DeleteUser.Status = App.Status.Delete;
                DeleteUser.UpdatedBy = objLogger.Id;
                DeleteUser.UpdatedAt = DateTime.Now;
                db.Update(DeleteUser);
                Message.Delete(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {
                    obj.ListId.Add(obj.Id);
                    obj.ListStatus.Add(App.Status.Delete);
                    objMsg.obj = List(obj, objLogger).FirstOrDefault();
                    objMsg.data = GetViewOption().data;
                }
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Enable(User obj, User objLogger)
        {
            Message objMsg = new Message();
            try
            {
                var EnableUser = db.User.Where(ap => App.Status.Delete == ap.Status && ap.Id == obj.Id).AsEnumerable().FirstOrDefault();
                if (EnableUser == null)
                {
                    Message.Error(ref objMsg, "User did not find for enable.");
                    return objMsg;
                }
                EnableUser.Status = App.Status.Enable;
                EnableUser.UpdatedBy = objLogger.Id;
                EnableUser.UpdatedAt = DateTime.Now;
                db.Update(EnableUser);
                Message.Delete(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {
                    obj.ListId.Add(obj.Id);
                    objMsg.obj = List(obj, objLogger).FirstOrDefault();
                    objMsg.data = GetViewOption().data;
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message AppMenu(User obj) 
        {
            Message objMsg = new Message();
            try 
            {                
                
                var dbUserApi = (
                    from ape in obj.Api
                    join api in db.Api on ape.ApiId equals api.Id
                    join apg in db.ApiGroup on api.ApiGroupId equals apg.Id
                    where api.ApiType == obj.ApiType
                    select new
                    {
                        ape.ApiId,
                        ApiName = api.Name,
                        ApiDesc = api.Description,
                        ApiIcon = api.Icon,
                        ApiSeqNo = api.SeqNo,
                        api.ApiType,
                        api.ApiGroupId,
                        ApiGroupName = apg.Name,
                        ApiGroupDesc = apg.Description,
                        ApiGroupIcon = apg.Icon,
                        ApiGroupSeqNo = apg.SeqNo,
                        ApiGroupParentId = apg.ParentId,
                        ape.View,
                        ape.Add,
                        ape.Update,
                        ape.Delete,
                        ape.Print,
                        ape.Import,
                        ape.Export,
                    }
                ).ToList();
                
                objMsg.data = dbUserApi.OrderBy(ape => ape.ApiGroupSeqNo).Where(ape => ape.ApiGroupParentId == 0)
                    .GroupBy(ape => new
                    {
                        ape.ApiGroupId,
                        ape.ApiGroupIcon,
                        ape.ApiGroupDesc,
                    })
                    .Select(ape => new
                    {
                        ape.Key.ApiGroupId,
                        ape.Key.ApiGroupIcon,
                        ape.Key.ApiGroupDesc,
                        Menu = dbUserApi.OrderBy(me => me.ApiSeqNo).Where(me => me.ApiGroupId == ape.Key.ApiGroupId && me.ApiGroupParentId == 0)
                        .Select(me => new
                        {
                            me.ApiId,
                            me.ApiIcon,
                            me.ApiName,
                            me.ApiDesc
                        }).ToList(),
                        SubGroup = dbUserApi.OrderBy(sm => sm.ApiGroupSeqNo).Where(sm => sm.ApiGroupParentId > 0 && sm.ApiGroupParentId == ape.Key.ApiGroupId)
                        .GroupBy(sm => new
                        {
                            sm.ApiGroupId,
                            sm.ApiGroupIcon,
                            sm.ApiGroupDesc
                        })
                        .Select(sm => new
                        {
                            sm.Key.ApiGroupId,
                            sm.Key.ApiGroupIcon,
                            sm.Key.ApiGroupDesc,
                            Menu = dbUserApi.OrderBy(me1 => me1.ApiSeqNo).Where(me1 => me1.ApiGroupParentId > 0 && me1.ApiGroupId == sm.Key.ApiGroupId).Select(me1 => new
                            {
                                me1.ApiId,
                                me1.ApiIcon,
                                me1.ApiName,
                                me1.ApiDesc
                            }).ToList()
                        })
                        .ToList()
                    }).ToList();                
                Message.Success(ref objMsg, "");
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message UserInfo(User obj)
        {
            Message objMsg = new Message();
            try
            {
                dynamic data = new ExpandoObject();                
                data.User = new
                {
                    obj.Name,
                    Type = obj.UserType,
                    obj.Theme,                    
                    obj.Company,
                    obj.Api,
                    AuthToken = Util.CreateJwtToken(obj)
                };
                data.Company = db.Company.FirstOrDefault(cp => App.ActiveStatus.Contains(cp.Status) && cp.Id == obj.CompanyId);
                data.AppMenu = AppMenu(obj).data;
                objMsg.status = Message.Type.success;
                objMsg.statusText = "Login sucess";
                objMsg.data = data;
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
    }
}
