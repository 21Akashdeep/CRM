using CRMApi.Models;
using CRMApi.Services;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoAuthentication
    {
        private readonly DbCRM db;
        private readonly AppSetting App = Util.AppSetting;
        private RepoUser RepoUser;
        public RepoAuthentication(DbCRM _db) 
        {
            db = _db;
            RepoUser = new RepoUser(db);
        }
        public Message AppInfo()
        {
            Message objMsg = new Message();
            try
            {
                objMsg.data = new { Info = App.AppInfo, App.UserType, App.ApiType, App.ApiName };
                Message.Success(ref objMsg, "App Info found.");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Auth(User obj)
        {
            Message objMsg = new Message();
            try
            {
                var objUser = db.User.Where(x => App.ActiveStatus.Contains(x.Status) && x.UserId == obj.UserId && x.Password == Util.Encrypt(obj.Password))
                              .AsEnumerable().FirstOrDefault();                
                if (objUser == null)
                {
                    Message.Error(ref objMsg, "Opps, Invalid login credentials.");
                    return objMsg;
                }
                objUser.ApiType = obj.ApiType;
                objUser.CompanyId = objUser.Company.Where(c => c.IsDefault).Select(c => c.CompanyId).FirstOrDefault();
                objMsg = RepoUser.UserInfo(objUser);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);                
            }
            return objMsg;
        }
        
    }
}
