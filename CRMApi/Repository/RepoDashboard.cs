using CRMApi.Models;
using CRMApi.Services;
using System.Data;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoDashboard
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoDashboard(DBCRM db)
        {
            this.db = db;
        }
        public Message GetViewOption()
        {
            Message objMsg = new Message();
            try
            {
                dynamic option = new ExpandoObject();

                option.ExpiryCount = db.GatePass.Count();
                option.MedicalExpiryCount = db.GatePass.Count(gp => gp.MedicalExpiryOn != null);
                option.TrainingExpiryCount = db.GatePass.Count(gp => gp.TrainingExpiryOn != null);
                option.LabourExpiryCount = db.GatePass.Count(gp => gp.LabourLicenseExpiryOn != null);


                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        
    }
}
