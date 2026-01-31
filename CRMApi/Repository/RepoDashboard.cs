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
                Option.MedicalTotal = db.GatePass.Count(gp =>
                gp.MedicalExpiryOn != null && gp.MedicalExpiryOn != DateTime.MinValue);

                Option.TrainingTotal = db.GatePass.Count(gp =>
                gp.TrainingExpiryOn != null && gp.TrainingExpiryOn != DateTime.MinValue);

                Option.LabourTotal = db.GatePass.Count(gp =>
                gp.LabourLicenseExpiryOn != null && gp.LabourLicenseExpiryOn != DateTime.MinValue);

                Option.MedicalExpirySoon = db.GatePass.Count(gp =>
                 gp.MedicalExpiryOn != DateTime.MinValue &&
                 gp.MedicalExpiryOn.Date >= today &&
                 gp.MedicalExpiryOn.Date <= limitDate
                    );

                Option.TrainingExpirySoon = db.GatePass.Count(gp =>
                    gp.TrainingExpiryOn != DateTime.MinValue &&
                    gp.TrainingExpiryOn.Date >= today &&
                    gp.TrainingExpiryOn.Date <= limitDate
                );

                Option.LabourExpirySoon = db.GatePass.Count(gp =>
                    gp.LabourLicenseExpiryOn != DateTime.MinValue &&
                    gp.LabourLicenseExpiryOn.Date >= today &&
                    gp.LabourLicenseExpiryOn.Date <= limitDate
                );

                Option.PassExpired = db.GatePass.Count(gp =>
                gp.ExpiryOn != DateTime.MinValue &&
                gp.ExpiryOn.Date < today
                );

                Option.MedicalExpired = db.GatePass.Count(gp =>
                    gp.MedicalExpiryOn != DateTime.MinValue &&
                    gp.MedicalExpiryOn.Date < today
                );

                Option.TrainingExpired = db.GatePass.Count(gp =>
                    gp.TrainingExpiryOn != DateTime.MinValue &&
                    gp.TrainingExpiryOn.Date < today
                );

                Option.LabourExpired = db.GatePass.Count(gp =>
                    gp.LabourLicenseExpiryOn != DateTime.MinValue &&
                    gp.LabourLicenseExpiryOn.Date < today
                );





                DateTime monthStart = new DateTime(today.Year, today.Month, 1);
                DateTime monthEnd = monthStart.AddMonths(1);

                var voucherStats = db.Voucher
                    .GroupBy(v => v.Type)
                    .Select(g => new
                    {
                        Type = g.Key,
                        Total = g.Count(),
                        MonthTotal = g.Count(v =>
                            v.Date >= monthStart &&
                            v.Date < monthEnd
                        ),
                        TodayTotal = g.Count(v =>
                            v.Date >= today &&
                            v.Date < today.AddDays(1)
                        )
                    })
                    .ToList();

                // Receipt Note
                Option.ReceiptNoteTotal = voucherStats.FirstOrDefault(x => x.Type == "ReceiptNote")?.Total ?? 0;
                Option.ReceiptNoteMonth = voucherStats.FirstOrDefault(x => x.Type == "ReceiptNote")?.MonthTotal ?? 0;
                Option.ReceiptNoteToday = voucherStats.FirstOrDefault(x => x.Type == "ReceiptNote")?.TodayTotal ?? 0;

                // Stock In
                Option.StockInTotal = voucherStats.FirstOrDefault(x => x.Type == "StockIn")?.Total ?? 0;
                Option.StockInMonth = voucherStats.FirstOrDefault(x => x.Type == "StockIn")?.MonthTotal ?? 0;
                Option.StockInToday = voucherStats.FirstOrDefault(x => x.Type == "StockIn")?.TodayTotal ?? 0;

                // Delivery Note
                Option.DeliveryNoteTotal = voucherStats.FirstOrDefault(x => x.Type == "DeliveryNote")?.Total ?? 0;
                Option.DeliveryNoteMonth = voucherStats.FirstOrDefault(x => x.Type == "DeliveryNote")?.MonthTotal ?? 0;
                Option.DeliveryNoteToday = voucherStats.FirstOrDefault(x => x.Type == "DeliveryNote")?.TodayTotal ?? 0;

                // Return Note
                Option.ReturnNoteTotal = voucherStats.FirstOrDefault(x => x.Type == "ReturnNote")?.Total ?? 0;
                Option.ReturnNoteMonth = voucherStats.FirstOrDefault(x => x.Type == "ReturnNote")?.MonthTotal ?? 0;
                Option.ReturnNoteToday = voucherStats.FirstOrDefault(x => x.Type == "ReturnNote")?.TodayTotal ?? 0;

                // Stock Out
                Option.StockOutTotal = voucherStats.FirstOrDefault(x => x.Type == "StockOut")?.Total ?? 0;
                Option.StockOutMonth = voucherStats.FirstOrDefault(x => x.Type == "StockOut")?.MonthTotal ?? 0;
                Option.StockOutToday = voucherStats.FirstOrDefault(x => x.Type == "StockOut")?.TodayTotal ?? 0;

                // Stock Transfer
                Option.StockTransferTotal = voucherStats.FirstOrDefault(x => x.Type == "StockTransfer")?.Total ?? 0;
                Option.StockTransferMonth = voucherStats.FirstOrDefault(x => x.Type == "StockTransfer")?.MonthTotal ?? 0;
                Option.StockTransferToday = voucherStats.FirstOrDefault(x => x.Type == "StockTransfer")?.TodayTotal ?? 0;

                // Stock Adjustment
                Option.StockAdjustmentTotal = voucherStats.FirstOrDefault(x => x.Type == "StockAdjustment")?.Total ?? 0;
                Option.StockAdjustmentMonth = voucherStats.FirstOrDefault(x => x.Type == "StockAdjustment")?.MonthTotal ?? 0;
                Option.StockAdjustmentToday = voucherStats.FirstOrDefault(x => x.Type == "StockAdjustment")?.TodayTotal ?? 0;

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
