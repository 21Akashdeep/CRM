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
                dynamic Option = new ExpandoObject();

                DateTime today = DateTime.Today;
                DateTime limitDate = today.AddDays(15);

                Option.PassTotal = db.GatePass.Count();
                Option.MedicalTotal = db.GatePass.Count(gp => gp.MedicalExpiryOn != null);
                Option.TrainingTotal = db.GatePass.Count(gp => gp.TrainingExpiryOn != null);
                Option.LabourTotal = db.GatePass.Count(gp => gp.LabourLicenseExpiryOn != null);

                Option.PassExpirySoon = db.GatePass.Count(gp =>
                gp.ExpiryOn.Date >= today && gp.ExpiryOn.Date <= limitDate
                );
                Option.MedicalExpirySoon = db.GatePass.Count(gp =>
                gp.MedicalExpiryOn.Date >= today && gp.MedicalExpiryOn.Date <= limitDate
                );
                Option.TrainingExpirySoon = db.GatePass.Count(gp =>
                gp.TrainingExpiryOn.Date >= today && gp.TrainingExpiryOn.Date <= limitDate
                );
                Option.LabourExpirySoon = db.GatePass.Count(gp =>
                gp.LabourLicenseExpiryOn.Date >= today && gp.LabourLicenseExpiryOn.Date <= limitDate
                );

                Option.PassExpired = db.GatePass.Count(gp =>
                gp.ExpiryOn.Date <= today
                );
                Option.MedicalExpired = db.GatePass.Count(gp =>
                gp.MedicalExpiryOn.Date <= today
                );

                Option.TrainingExpired = db.GatePass.Count(gp =>
                gp.TrainingExpiryOn.Date <= today
                );

                Option.LabourExpired = db.GatePass.Count(gp =>
                gp.LabourLicenseExpiryOn.Date <= today
                );



                objMsg.data = Option;

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
