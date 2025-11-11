using CRMApi.Models;
using CRMApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;
using System.Net.Mail;

namespace CRMApi.Repository
{
    public class RepoUser
    {
        private readonly DBCRM db;
        private AppSetting App = Util.AppSetting;
        //private RepoApi RepoApi;
        private RepoApprovalRole RepoApprovalRole;
        public RepoUser(DBCRM _db) 
        {
            db = _db;            
            RepoApprovalRole = new RepoApprovalRole(db);
        }        
        
        public async Task<Message> GetViewOption() 
        {
            Message objMsg = new Message();
            try 
            {
                var dbUser = await db.User.ToListAsync();
                var dbSetting = await db.Setting.Where(st => App.ActiveStatus.Contains(st.Status)).ToListAsync();
                
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
        public async Task<Message> GetAddOption(int Id) 
        {
            Message objMsg = new Message();
            try 
            {
                dynamic Options = new ExpandoObject();
                
                //Get Setting Option
                var dbSetting = await db.Setting.Where(st => App.ActiveStatus.Contains(st.Status)).ToListAsync();
                Options.UserType = dbSetting.Where(st => st.Name == App.SettingName.UserType).Select(st => new { st.Value, st.Description }).ToList();

                //Get User
                var dbUser = await db.User.Where(ur => App.ActiveStatus.Contains(ur.Status)).ToListAsync();                                                
                Options.User = dbUser.Select(ur => new
                {
                    ur.Id,
                    ur.UserId,
                    ur.Name
                    //ur.Gender
                }).ToList();
                //Get User Api
                Options.Api = await (
                    from api in db.Api
                    join apg in db.ApiGroup on api.ApiGroupId equals apg.Id
                    join set in db.Setting on new { Value = api.ApiType, Name = App.SettingName.ApiType } equals new { set.Value, set.Name }
                    join uap in db.UserApi on new { ApiId = api.Id, UserId = Id, Status = App.Status.Enable } equals new { uap.ApiId, uap.UserId, uap.Status } into UserApi
                    from uap in UserApi.DefaultIfEmpty()
                    where App.ActiveStatus.Contains(api.Status)
                    select new UserApi
                    {
                        UserId = Id,
                        ApiId = api.Id,
                        ApiDesc = api.Description,
                        ApiTypeDesc = set.Description,
                        ApiGroupDesc = apg.Description,
                        View = uap != null && uap.View,
                        Add = uap != null && uap.Add,
                        Update = uap != null && uap.Update,
                        Delete = uap != null && uap.Delete,
                        Enable = uap != null && uap.Enable,
                        Print = uap != null && uap.Print,
                        Import = uap != null && uap.Import,
                        Export = uap != null && uap.Export,
                        IsAdded = uap != null
                    }
                ).ToListAsync();                
                //Get User Location
                Options.Location = await (
                    from loc in db.Location
                    join ulo in db.UserLocation on new { LocationId = loc.Id, UserId = Id, Status = App.Status.Enable } equals new { ulo.LocationId, ulo.UserId, ulo.Status } into UserLocation
                    from ulo in UserLocation.DefaultIfEmpty()
                    where App.ActiveStatus.Contains(loc.Status)
                    select new UserLocation
                    {                        
                        UserId = Id,
                        LocationId = loc.Id,
                        LocationDesc = loc.Description,
                        IsAdded = ulo != null,
                    }
                ).ToListAsync();
                //Get User Approval Role
                Options.ApprovalRole = await (
                    from apr in db.ApprovalRole
                    join upr in db.UserApprovalRole on new { ApprovalRoleId = apr.Id , UserId = Id, Status = App.Status.Enable } equals new { upr.ApprovalRoleId, upr.UserId, upr.Status } into AprRole
                    from upr in AprRole.DefaultIfEmpty()
                    where App.ActiveStatus.Contains(apr.Id)
                    select new UserApprovalRole
                    {                        
                        UserId = Id,
                        ApprovalRoleId = apr.Id,
                        ApprovalRoleDesc = apr.Description,
                        IsAdded = upr != null,
                    }
                ).ToListAsync();
                //Get Derpatment
                Options.Department = await db.Department.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description
                }).ToListAsync();
                //Get Designation
                Options.Designation = await db.Designation.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description
                }).ToListAsync();
                objMsg.data = Options;
                Message.Success(ref objMsg, "");
            } 
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task< List<User>> ListAsync(User? obj, User objLogger)
        {            
            obj ??= new User();
            obj.ListStatus = obj.ListStatus.Count == 0 ? App.ActiveStatus : obj.ListStatus;
            var dbUser = db.User.Where(x => App.ActiveStatus.Contains(x.Status)).AsQueryable();
            if (obj.ListUserType.Any())
                dbUser = dbUser.Where(x => obj.ListUserType.Contains(x.UserType));
            if(obj.ListId.Any())
                dbUser = dbUser.Where(x => obj.ListId.Contains(x.Id));

            var ListUserId = dbUser.Select(x => x.Id).ToList();

            var dbUserApi = (await (
                from upi in db.UserApi
                join api in db.Api on upi.ApiId equals api.Id                              
                select new UserApi
                {
                    Id = upi.Id,
                    UserId = upi.UserId,                    
                    ApiId = upi.ApiId,
                    ApiDesc = api.Description,
                    View = upi.View,
                    Add = upi.Add,
                    Update = upi.Update,
                    Delete = upi.Delete,
                    Enable = upi.Enable,
                    Print = upi.Print,
                    Import = upi.Import,
                    Export = upi.Export,
                    Status = upi.Status
                }
            ).ToListAsync()).ToLookup(x=> x.UserId);

            var dbUserLocation = (await (
                from ulo in db.UserLocation
                join loc in db.Location on ulo.LocationId equals loc.Id
                select new UserLocation
                {
                    Id = ulo.Id,
                    UserId = ulo.UserId,
                    LocationId = ulo.LocationId,
                    LocationDesc = loc.Description,                    
                }
            ).ToListAsync()).ToLookup(x => x.UserId);

            var dbUserApprovalRole = (await (
                from uar in db.UserApprovalRole
                join apr in db.ApprovalRole on uar.ApprovalRoleId equals apr.Id
                select new UserApprovalRole
                {
                    Id = uar.Id,
                    UserId = uar.UserId,
                    ApprovalRoleId = uar.ApprovalRoleId,
                    ApprovalRoleDesc = apr.Description,
                }
            ).ToListAsync()).ToLookup(x => x.UserId);

            var User = await (
                from ur in dbUser                
                join ut in db.Setting on new { Value = ur.UserType, Name = App.SettingName.UserType } equals new { ut.Value, ut.Name }
                join dt in db.Department on ur.DepartmentId equals dt.Id
                join ds in db.Designation on ur.DesignationId equals ds.Id
                join st in db.Setting on new { Value = ur.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                join cb in db.User on ur.CreatedBy equals cb.Id
                join ub in db.User on ur.UpdatedBy equals ub.Id
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
                    DepartmentId = ur.DepartmentId,
                    DepartmentDesc = dt.Description,
                    DesignationId = ur.DesignationId,
                    DesignationDesc = ds.Description,
                    ContactNo = ur.ContactNo,
                    Email = ur.Email,
                    UserType = ur.UserType,
                    UserTypeDesc = ut.Description,                    
                    PasswordExpiredAt = ur.PasswordExpiredAt,                    
                    Api = dbUserApi[ur.Id].ToList(),
                    ApprovalRole = dbUserApprovalRole[ur.Id].ToList(),
                    Location = dbUserLocation[ur.Id].ToList(),                    
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
            ).ToListAsync();
            return User;
        }        
        public async Task<Message> Print(User obj, User objLogger)
        {
            Message objMsg = new Message();
            try
            {
                objMsg.data = await ListAsync(obj, objLogger);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> Export(User obj, User objLooger)
        {
            Message objMsg = new Message();
            try
            {
                //Get Party
                var objCompany = await db.Company.FirstOrDefaultAsync(pt => App.ActiveStatus.Contains(pt.Status));
                if (objCompany == null)
                {
                    Message.Error(ref objMsg, "Company Info did found");
                    return objMsg;
                }
                //Get User
                var dbUser = await ListAsync(obj, objLooger);
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
                    Api = String.Join(", ", u.Api.Select(c => c.ApiDesc).ToList()),
                    ApprovalRole = String.Join(", ", u.ApprovalRole.Select(c => c.ApprovalRoleDesc).ToList()),
                    Location = String.Join(", ", u.Location.Select(c => c.LocationDesc).ToList()),
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
        public async Task<Message> Add(User obj, User objLogger) 
        {
            Message objMsg = new Message();
            try 
            {
                obj.UserId = Util.SanitizeInput(obj.UserId, $"{App.Regexp.AlphaNum}_")?? "";
                var dbUser = await db.User.Where(ur => App.ActiveStatus.Contains(ur.Status)).ToListAsync();
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
                obj.Api.ForEach(x =>
                {
                    x.CreatedBy = objLogger.Id;
                    x.UpdatedBy = objLogger.Id;
                });
                obj.ApprovalRole.ForEach(x =>
                {
                    x.CreatedBy = objLogger.Id;
                    x.UpdatedBy = objLogger.Id;
                });
                obj.Location.ForEach(x =>
                {
                    x.CreatedBy = objLogger.Id;
                    x.UpdatedBy = objLogger.Id;
                });
                db.Add(obj);
                Message.Add(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {
                    obj.ListId.Add(obj.Id);
                    objMsg.obj = (await ListAsync(obj, objLogger)).FirstOrDefault();
                    dynamic Option = new ExpandoObject();
                    Option.ViewOption = (await GetViewOption()).data;
                    Option.AddOption = (await GetAddOption(0)).data;
                    objMsg.data = Option;
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
            var obj = db.User.FirstOrDefault(ur => App.ActiveStatus.Contains(ur.Status) && ur.Id == Id);
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
            Util.SentMail(db, objMail, $"User Login Credential", matter, "info", ref objMsg);
        }
        public async Task<Message> Update(User obj, User objLogger) 
        {
            Message objMsg = new Message();
            try 
            {
                obj.UserId = Util.SanitizeInput(obj.UserId, $"{App.Regexp.AlphaNum}_") ?? "";
                var dbUser = await db.User.Where(x => App.ActiveStatus.Contains(x.Status)).ToListAsync();
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
                UpdateUser.Gender = obj.Gender;
                UpdateUser.FatherName = obj.FatherName;
                UpdateUser.ContactNo = obj.ContactNo;
                UpdateUser.Email = obj.Email;
                UpdateUser.PasswordExpiredAt = obj.PasswordExpiredAt == null || obj.PasswordExpiredAt == default(DateTime) ? UpdateUser.CreatedAt.AddMonths(24) : obj.PasswordExpiredAt;                                
                UpdateUser.DepartmentId = obj.DepartmentId;
                UpdateUser.DesignationId = obj.DesignationId;                                
                UpdateUser.UpdatedBy = objLogger.Id;
                UpdateUser.UpdatedAt = DateTime.Now;
                db.Update(UpdateUser);

                //Update User Api
                var UpdateUserApi = db.UserApi.Where(x => x.UserId == UpdateUser.Id).ToList();
                UpdateUserApi.ForEach(x =>
                {
                    var UserApi = obj.Api.FirstOrDefault(x1 => x1.UserId == x.UserId && x1.ApiId == x.ApiId);
                    x.View = UserApi != null && UserApi.View;
                    x.Add = UserApi != null && UserApi.Add;
                    x.Update = UserApi != null && UserApi.Update;
                    x.Delete = UserApi != null && UserApi.Delete;
                    x.Enable = UserApi != null && UserApi.Enable;
                    x.Print = UserApi != null && UserApi.Print;
                    x.Import = UserApi != null && UserApi.Import;
                    x.Export = UserApi != null && UserApi.Export;
                    x.Status = UserApi != null ? App.Status.Enable : App.Status.Cancel;
                    x.UpdatedBy = objLogger.Id;
                    x.UpdatedAt = DateTime.Now;
                });
                db.UpdateRange(UpdateUserApi);
                //Add New User Api
                var AddUserApi = obj.Api.Where(x => x.Id == 0 && !UpdateUserApi.Any(x1 => x1.UserId == x.UserId && x1.ApiId == x.ApiId)).ToList();
                AddUserApi.ForEach(x =>
                {
                    x.Status = App.Status.Enable;
                    x.CreatedBy = objLogger.Id;
                    x.CreatedAt = DateTime.Now;
                    x.UpdatedBy = objLogger.Id;
                    x.UpdatedAt = DateTime.Now;
                });
                db.AddRange(AddUserApi);
                
                //Update User Approval Role
                var UpdateUserApprovalRole = db.UserApprovalRole.Where(x => x.UserId == UpdateUser.Id).ToList();
                UpdateUserApprovalRole.ForEach(x =>
                {
                    var UserApprovalRole = obj.ApprovalRole.FirstOrDefault(x1 => x1.UserId == x.UserId && x1.ApprovalRoleId == x.ApprovalRoleId);
                    x.Status = UserApprovalRole != null ? App.Status.Enable : App.Status.Cancel;
                    x.UpdatedBy = objLogger.Id;
                    x.UpdatedAt = DateTime.Now;
                });
                db.UpdateRange(UpdateUserApprovalRole);
                //Add New User Approval Role
                var AddUserApprovalRole = obj.ApprovalRole.Where(x => x.Id == 0 && !UpdateUserApprovalRole.Any(x1 => x1.UserId == x.UserId && x1.ApprovalRoleId == x.ApprovalRoleId)).ToList();
                AddUserApprovalRole.ForEach(x =>
                {
                    x.Status = App.Status.Enable;
                    x.CreatedBy = objLogger.Id;
                    x.CreatedAt = DateTime.Now;
                    x.UpdatedBy = objLogger.Id;
                    x.UpdatedAt = DateTime.Now;
                });
                db.AddRange(AddUserApprovalRole);

                //Update User Location
                var UpdateUserLocation = db.UserLocation.Where(x => x.UserId == UpdateUser.Id).ToList();
                UpdateUserLocation.ForEach(x =>
                {
                    var UserLocation = obj.Location.FirstOrDefault(x1 => x1.UserId == x.UserId && x1.LocationId == x.LocationId);
                    x.Status = UserLocation != null ? App.Status.Enable : x.Status = App.Status.Cancel;                    
                    x.UpdatedBy = objLogger.Id;
                    x.UpdatedAt = DateTime.Now;
                });
                db.UpdateRange(UpdateUserLocation);
                //Add New User Location
                var AddUserUserLocation = obj.Location.Where(x => x.Id == 0 && !UpdateUserLocation.Any(x1 => x1.UserId == x.UserId && x1.LocationId == x.LocationId)).ToList();
                AddUserUserLocation.ForEach(x =>
                {
                    x.Status = App.Status.Enable;
                    x.CreatedBy = objLogger.Id;
                    x.CreatedAt = DateTime.Now;
                    x.UpdatedBy = objLogger.Id;
                    x.UpdatedAt = DateTime.Now;
                });
                db.AddRange(AddUserUserLocation);
                Message.Update(ref objMsg, (await db.SaveChangesAsync()), "");
                if (objMsg.status == Message.Type.success) 
                {
                    obj.ListId.Add(UpdateUser.Id);
                    objMsg.obj = (await ListAsync(obj, objLogger)).FirstOrDefault();
                    objMsg.data = (await GetViewOption()).data;
                }
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        //This Method Only use by self User
        public async Task<Message> UpdateProfile(User obj)
        {
            Message objMsg = new Message();
            try
            {
                var dbUser = await db.User.FirstOrDefaultAsync(usr => App.ActiveStatus.Contains(usr.Status) && usr.Id == obj.Id);
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
                Message.Update(ref objMsg, (await db.SaveChangesAsync()), "");
                objMsg.data = (await UserInfo(dbUser)).data;
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        //This Method only use by self user
        public async Task<Message> UpdatePassword(User obj)
        {
            Message objMsg = new Message();
            try
            {                
                var UpdateUser = await db.User.FirstOrDefaultAsync(x => App.ActiveStatus.Contains(x.Status) && x.Id == obj.Id && x.Password == Util.Encrypt(obj.Password));
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
                Message.Update(ref objMsg, (await db.SaveChangesAsync()), "");                
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }        
        public async Task<Message> Delete(User obj, User objLogger) 
        {
            Message objMsg = new Message();
            try 
            {
                var DeleteUser = await db.User.FirstOrDefaultAsync(ap => App.ActiveStatus.Contains(ap.Status) && ap.Id == obj.Id);
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
                Message.Delete(ref objMsg, (await db.SaveChangesAsync()), "");
                if (objMsg.status == Message.Type.success)
                {
                    obj.ListId.Add(obj.Id);
                    obj.ListStatus.Add(App.Status.Delete);
                    objMsg.obj = (await ListAsync(obj, objLogger)).FirstOrDefault();
                    objMsg.data = (await GetViewOption()).data;
                }
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> Enable(User obj, User objLogger)
        {
            Message objMsg = new Message();
            try
            {
                var EnableUser = await db.User.FirstOrDefaultAsync(ap => App.Status.Delete == ap.Status && ap.Id == obj.Id);
                if (EnableUser == null)
                {
                    Message.Error(ref objMsg, "User did not find for enable.");
                    return objMsg;
                }
                EnableUser.Status = App.Status.Enable;
                EnableUser.UpdatedBy = objLogger.Id;
                EnableUser.UpdatedAt = DateTime.Now;
                db.Update(EnableUser);
                Message.Delete(ref objMsg, (await db.SaveChangesAsync()), "");
                if (objMsg.status == Message.Type.success)
                {
                    obj.ListId.Add(obj.Id);
                    objMsg.obj = (await ListAsync(obj, objLogger)).FirstOrDefault();
                    objMsg.data = (await GetViewOption()).data;
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }        
        public async Task<Message> UserInfo(User obj)
        {
            Message objMsg = new Message();
            try
            {                
                var User = new
                {
                    obj.Name,
                    Type = obj.UserType,
                    obj.Theme,
                    AuthToken = Util.CreateJwtToken(obj),                                        
                };

                var UserApi = await (
                    from ape in db.UserApi
                    join api in db.Api on ape.ApiId equals api.Id
                    join apg in db.ApiGroup on api.ApiGroupId equals apg.Id
                    where api.ApiType == obj.ApiType && ape.UserId == obj.Id && App.ActiveStatus.Contains(ape.Status) && App.ActiveStatus.Contains(api.Status)
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
                ).ToListAsync();

                var UserMenu = UserApi.OrderBy(ape => ape.ApiGroupSeqNo).Where(ape => ape.ApiGroupParentId == 0)
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
                        Menu = ape.OrderBy(me => me.ApiSeqNo)
                        .Select(me => new
                        {
                            me.ApiId,
                            me.ApiIcon,
                            me.ApiName,
                            me.ApiDesc
                        }).ToList(),
                        SubGroup = UserApi.OrderBy(sm => sm.ApiGroupSeqNo).Where(sm => sm.ApiGroupParentId > 0 && sm.ApiGroupParentId == ape.Key.ApiGroupId)
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
                            Menu = UserApi.OrderBy(me1 => me1.ApiSeqNo).Where(me1 => me1.ApiGroupParentId > 0 && me1.ApiGroupId == sm.Key.ApiGroupId).Select(me1 => new
                            {
                                me1.ApiId,
                                me1.ApiIcon,
                                me1.ApiName,
                                me1.ApiDesc
                            }).ToList()
                        })
                        .ToList()
                    }).ToList();
                objMsg.data = new 
                {
                    User,
                    UserApi,                    
                    UserMenu,
                    Company = db.Company.FirstOrDefault(cp => App.ActiveStatus.Contains(cp.Status)),
                };
                Message.Success(ref objMsg, "Login Success");                
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
    }
}
