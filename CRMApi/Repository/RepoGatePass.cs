using CRMApi.Models;
using CRMApi.Services;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Intrinsics.Arm;
using System.Text;

namespace CRMApi.Repository
{
    public class RepoGatePass
    {

        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoGatePass(DBCRM _db)
        {
            db = _db;
        }
        public async Task<Message> GetViewOptionAsync()
        {
            Message objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();
                var ListStatus = await db.GatePass.GroupBy(x => new { x.Status }).Select(x => x.Key.Status.ToString()).ToListAsync();
                Option.Status = await db.Setting.Where(st => App.ActiveStatus.Contains(st.Status) && st.Name == App.SettingName.Status && ListStatus.Contains(st.Value)).Select(st => new
                {
                    st.Value,
                    st.Description
                }).ToListAsync();
                Option.Location = await db.Location.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.Name
                }).ToListAsync();
                Option.Department = await db.GatePass.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.Department
                }).ToListAsync();

                Option.PassType =  await db.Setting.Where(x => App.ActiveStatus.Contains(x.Status) && x.Category == "PassType").Select (x => new
                {
                    x.Value,
                    x.Description

                }).ToListAsync();


                objMsg.data = Option;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> GetAddOptionAsync()
        {
            Message objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();

                Option.Company = await db.Company.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.Description
                }).ToListAsync();
                Option.Employee = await db.Employee.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.Name
                }).ToListAsync();

                Option.PassType = await db.Setting.Where(x => App.ActiveStatus.Contains(x.Status) && x.Category == "PassType").Select(x => new
                {
                    x.Value,
                    x.Description

                }).ToListAsync();

                Option.Location = await db.Location.Where(x => App.ActiveStatus.Contains(x.Status)).Select(x => new
                {
                    x.Id,
                    x.Description,
                }).ToListAsync();

                objMsg.data = Option;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<List<GatePass>> ListAsync(GatePass? obj, User User)
        {
            obj = obj == null ? new GatePass() : obj;
            obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : App.AllActiveStatus;
            var dbGatePassQuery = db.GatePass.Where(co => obj.ListStatus.Contains(co.Status)).AsQueryable();

            if (obj.ListId.Any()) dbGatePassQuery = dbGatePassQuery.Where(co => obj.ListId.Contains(co.Id));
            dbGatePassQuery = obj.ListLocation.Any() ? dbGatePassQuery.Where(gt => obj.ListLocation.Contains(gt.LocationId)) : dbGatePassQuery;
            dbGatePassQuery = obj.ListDepartment.Any() ? dbGatePassQuery.Where(gt => obj.ListDepartment.Contains(gt.Id)) : dbGatePassQuery;
            if (obj.ListPassType.Any()) dbGatePassQuery = dbGatePassQuery.Where(co => obj.ListPassType.Contains(co.Type));

            var GatePassList = await (
                from gp in dbGatePassQuery
                join em in db.Employee on gp.EmployeeId equals em.Id into emJoin
                from em in emJoin.DefaultIfEmpty()
                join lo in db.Location on gp.LocationId equals lo.Id into lojoin
                from lo in lojoin.DefaultIfEmpty()
                join co in db.Company on gp.CompanyId equals co.Id into cojoin
                from co in cojoin.DefaultIfEmpty()
                join st1 in db.Setting on new { Category = "PassType", Value = gp.Type } equals new { Category = st1.Category, Value = st1.Value } into st1Join
                from st1 in st1Join.DefaultIfEmpty()
                join st in db.Setting on new { Name = App.SettingName.Status, Value = gp.Status.ToString() } equals new { st.Name, st.Value } into stJoin
                from st in stJoin.DefaultIfEmpty()
                join cb in db.User on gp.CreatedBy equals cb.Id
                join ub in db.User on gp.UpdatedBy equals ub.Id

                select new GatePass
                {
                    Id = gp.Id,
                    GatePassNo = gp.GatePassNo,
                    Type = gp.Type,
                    TypeDesc = st1 != null ? st1.Description : gp.Type,
                    EmployeeId = gp.EmployeeId,
                    EmployeeDesc = em != null ? em.Name : "-",
                    ExpiryOn = gp.ExpiryOn,
                    Department = gp.Department,
                    IdentifyMark = gp.IdentifyMark,
                    LocationId = gp.LocationId,
                    LocationDesc=lo.Name,
                    CompanyDesc=co.Name,
                    SafetyPassNo = gp.SafetyPassNo,
                    TrainingExpiryOn = gp.TrainingExpiryOn,
                    MedicalExpiryOn = gp.MedicalExpiryOn,
                    WorkOrderNo = gp.WorkOrderNo,
                    IssuedOn = gp.IssuedOn,
                    LabourLicenseExpiryOn = gp.LabourLicenseExpiryOn,
                    Status = gp.Status,
                    StatusDesc = st != null ? st.Description : "",
                    StatusCss = st != null ? st.CssClass : "",
                    CreatedBy = gp.CreatedBy,
                    CreatedByName = cb.Name,
                    CompanyId = gp.CompanyId,
                    CreatedAt = gp.CreatedAt,
                    UpdatedBy = gp.UpdatedBy,
                    UpdatedByName = ub.Name,
                    UpdatedAt = gp.UpdatedAt,
                    IsEdit = gp.Status == App.Status.Enable,
                    IsDuplicate = true,
                    IsDelete = gp.Status == App.Status.Enable,
                    IsEnable = gp.Status == App.Status.Delete
                }
            ).ToListAsync();

            return GatePassList;
        }
        public async Task<Message> GetAsync(GatePass obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                objMsg.data = await ListAsync(obj, User);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> PrintAsync(GatePass obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                objMsg.data = await ListAsync(obj, User);
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        //public async Task<Message> ExportAsync(GatePass obj, User User)
        //{
        //    Message objMsg = new Message();
        //    try
        //    {
        //        var dbGatePass = await ListAsync(obj, User);
        //        var GatePass = db.GatePass.Select(em => new
        //        {
        //            Name = em.EmployeeDesc,
        //            PassType = em.TypeDesc,
        //            PassNo = em.GatePassNo,
        //            PassIssuedOn = em.IssuedOn,
        //            PassExpiredOn = em.ExpiryOn,
        //            WorkOrderNo = em.WorkOrderNo,
        //            IdentityMark = em.IdentifyMark,
        //            Location = em.LocationDesc,
        //            Department = em.Department,
        //            MedicalExpiryOn = em.MedicalExpiryOn,
        //            LabourLicenseExpiryOn = em.LabourLicenseExpiryOn,
        //            TranningExpiryOn = em.TrainingExpiryOn,
        //            SafetyPassNo = em.SafetyPassNo,
        //            Company = em.CompanyId,
        //            Status = em.StatusDesc,


        //        }).ToList();

        //        DataTable objDataTable = Util.ListToDataTable(GatePass);

        //        var objCompany = await db.Company.FirstOrDefaultAsync(pt => App.ActiveStatus.Contains(pt.Status));
        //        if (objCompany == null)
        //        {
        //            Message.Error(ref objMsg, "Company Info did found");
        //            return objMsg;
        //        }

        //        objCompany.SheetName = "GatePass Item Wise List";
        //        objCompany.ReportDesc = $"GatePass Item Wise List Generated On - {DateTime.Now.ToString("dd-MMM-yyyy HH:mm")}";
        //        objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);

        //        if (String.IsNullOrEmpty(objMsg.base64))
        //        {
        //            Message.Error(ref objMsg, "Record did not find");
        //        }
        //        else
        //        {
        //            Message.Success(ref objMsg, "Record found");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Message.Exception(ref objMsg, ex);
        //    }

        //    return objMsg;
        //}

        //public async Task<Message> ExportAsync(GatePass obj, User User)
        //{
        //    Message objMsg = new Message();
        //    try
        //    {
        //        var list = await ListAsync(obj, User);

        //        if (!list.Any())
        //        {
        //            Message.Error(ref objMsg, "Record did not find");
        //            return objMsg;
        //        }

        //        DataTable dt = Util.ListToDataTable(list);

        //        var company = await db.Company
        //            .FirstOrDefaultAsync(x => App.ActiveStatus.Contains(x.Status));

        //        if (company == null)
        //        {
        //            Message.Error(ref objMsg, "Company Info did not found");
        //            return objMsg;
        //        }

        //        company.SheetName = "GatePass List";
        //        company.ReportDesc =
        //            $"GatePass List Generated On - {DateTime.Now:dd-MMM-yyyy HH:mm}";

        //        objMsg.base64 = Util.DataTableToBase64(dt, company);
        //        Message.Get(ref objMsg, "");
        //    }
        //    catch (Exception ex)
        //    {
        //        Message.Exception(ref objMsg, ex);
        //    }

        //    return objMsg;
        //}
        //public async Task<Message> ExportAsync(GatePass obj, User User)
        //{
        //    Message objMsg = new Message();
        //    try
        //    {
        //        // Get filtered GatePass list
        //        var dbGatePass = await ListAsync(obj, User);

        //        // Select ONLY required columns for export
        //        var gatePassExport = dbGatePass.Select(gp => new
        //        {
        //            Employee = gp.EmployeeDesc,
        //            PassType = gp.TypeDesc,
        //            PassNo = gp.GatePassNo,
        //            IdentifyMark = gp.IdentifyMark,
        //            WorkOrderNo = gp.WorkOrderNo,
        //            Company = gp.CompanyId,
        //            Location = gp.LocationId,
        //            Department = gp.Department,
        //            SafetyPassNo = gp.SafetyPassNo,
        //            IssuedOn = gp.IssuedOn.ToString("dd-MMM-yyyy"),
        //            ExpiryOn = gp.ExpiryOn.ToString("dd-MMM-yyyy"),
        //            MedicalExpiryOn = gp.MedicalExpiryOn.ToString("dd-MMM-yyyy"),
        //            LabourLicenceExpiryOn = gp.LabourLicenseExpiryOn.ToString("dd-MMM-yyyy"),
        //            TranningExpiryOn = gp.TrainingExpiryOn.ToString("dd-MMM-yyyy")
        //        }).ToList();


        //        DataTable objDataTable = Util.ListToDataTable(gatePassExport);

        //        var objCompany = await db.Company
        //            .FirstOrDefaultAsync(pt => App.ActiveStatus.Contains(pt.Status));

        //        if (objCompany == null)
        //        {
        //            Message.Error(ref objMsg, "Company Info not found");
        //            return objMsg;
        //        }

        //        objCompany.SheetName = "Gate Pass List";
        //        objCompany.ReportDesc =
        //            $"Gate Pass List Generated On - {DateTime.Now:dd-MMM-yyyy HH:mm}";

        //        objMsg.base64 = Util.DataTableToBase64(objDataTable, objCompany);

        //        if (string.IsNullOrEmpty(objMsg.base64))
        //            Message.Error(ref objMsg, "Record not found");
        //        else
        //            Message.Success(ref objMsg, "Record found");
        //    }
        //    catch (Exception ex)
        //    {
        //        Message.Exception(ref objMsg, ex);
        //    }
        //    return objMsg;
        //}


        //public async Task<Message> ExportAsync(GatePass obj, User User)
        //{
        //    Message objMsg = new Message();

        //    try
        //    {
        //        var dbGatePass = await ListAsync(obj, User);

        //        if (dbGatePass == null || !dbGatePass.Any())
        //        {
        //            Message.Error(ref objMsg, "Record not found");
        //            return objMsg;
        //        }

        //        var exportData = dbGatePass.Select(gp => new
        //        {
        //            Employee = gp.EmployeeDesc ?? "",
        //            PassType = gp.TypeDesc ?? "",
        //            PassNo = gp.GatePassNo ?? "",
        //            Location = gp.LocationDesc ?? "",
        //            Department = gp.Department?? "",
        //            IssuedOn = gp.IssuedOn.ToString("dd-MMM-yyyy"),
        //            ExpiryOn = gp.ExpiryOn.ToString("dd-MMM-yyyy"),
        //            SafetyPassNo = gp.SafetyPassNo ?? ""
        //        }).ToList();

        //        DataTable dt = Util.ListToDataTable(exportData);

        //        var company = await db.Company
        //            .FirstOrDefaultAsync(x => App.ActiveStatus.Contains(x.Status));

        //        using (var wb = new ClosedXML.Excel.XLWorkbook())
        //        {
        //            var ws = wb.Worksheets.Add("Gate Pass List");

        //            // ===== Header =====
        //            ws.Cell("A1").Value = company?.Description ?? "Gate Pass Report";
        //            ws.Range(1, 1, 1, dt.Columns.Count).Merge();
        //            ws.Row(1).Style.Font.Bold = true;
        //            ws.Row(1).Style.Font.FontSize = 18;
        //            ws.Row(1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

        //            ws.Cell("A2").Value = $"Generated On - {DateTime.Now:dd-MMM-yyyy HH:mm}";
        //            ws.Range(2, 1, 2, dt.Columns.Count).Merge();
        //            ws.Row(2).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

        //            // ===== Insert Data =====
        //            ws.Cell("A3").InsertTable(dt);

        //            var tableRange = ws.Range(3, 1, 3 + dt.Rows.Count, dt.Columns.Count);

        //            // ===== Header Color =====
        //            ws.Range(3, 1, 3, dt.Columns.Count)
        //              .Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;

        //            // ===== Column-wise Colors =====
        //            for (int col = 1; col <= dt.Columns.Count; col++)
        //            {
        //                string colName = dt.Columns[col - 1].ColumnName;

        //                if (colName == "Employee")
        //                    ws.Column(col).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightCyan;

        //                if (colName == "PassType")
        //                    ws.Column(col).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.AliceBlue;

        //                if (colName.Contains("Expiry"))
        //                    ws.Column(col).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.MistyRose;

        //                if (colName == "SafetyPassNo")
        //                    ws.Column(col).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightYellow;
        //            }

        //            // ===== Conditional Coloring =====
        //            for (int r = 4; r <= dt.Rows.Count + 3; r++)
        //            {
        //                DateTime expiry;
        //                if (DateTime.TryParse(ws.Cell(r, "G").GetString(), out expiry))
        //                {
        //                    if (expiry < DateTime.Today)
        //                        ws.Cell(r, "G").Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightPink;
        //                    else if (expiry <= DateTime.Today.AddDays(7))
        //                        ws.Cell(r, "G").Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightSalmon;
        //                }
        //            }

        //            ws.Columns().AdjustToContents();

        //            using (var stream = new MemoryStream())
        //            {
        //                wb.SaveAs(stream);
        //                objMsg.base64 = Convert.ToBase64String(stream.ToArray());
        //            }
        //        }

        //        Message.Success(ref objMsg, "Record found");
        //    }
        //    catch (Exception ex)
        //    {
        //        Message.Exception(ref objMsg, ex);
        //    }

        //    return objMsg;
        //}
        public async Task<Message> ExportAsync(GatePass obj, User User)
        {
            Message objMsg = new Message();

            try
            {
                var dbGatePass = await ListAsync(obj, User);

                if (dbGatePass == null || !dbGatePass.Any())
                {
                    Message.Error(ref objMsg, "Record not found");
                    return objMsg;
                }
                string FormatDate(DateTime date)
                {
                    return date == DateTime.MinValue ? "" : date.ToString("dd-MMM-yyyy");
                }
                var exportData = dbGatePass.Select(gp => new
                {
                    Employee = gp.EmployeeDesc ?? "",
                    PassType = gp.TypeDesc ?? "",
                    PassNo = gp.GatePassNo ?? "",
                    Location = gp.LocationDesc ?? "",
                    Department = gp.Department ?? "",
                    Company = gp.CompanyDesc ?? "",
                    SafetyPassNo = gp.SafetyPassNo ?? "",
                    WorkOrderNo=gp.WorkOrderNo ?? "",

                    IssuedOn = gp.IssuedOn.ToString("dd-MMM-yyyy"),
                    ExpiryOn = FormatDate(gp.ExpiryOn),
                    LabourLicenseExpiry = FormatDate(gp.LabourLicenseExpiryOn),
                    MedicalExpiry = FormatDate(gp.MedicalExpiryOn),
                    TrainingExpiry = FormatDate(gp.TrainingExpiryOn),


                    //ExpiryOn = gp.ExpiryOn.ToString("dd-MMM-yyyy"),
                    //LabourLicenseExpiry = gp.LabourLicenseExpiryOn.ToString("dd-MMM-yyyy"),
                    //MedicalExpiry = gp.MedicalExpiryOn.ToString("dd-MMM-yyyy"),
                    //TrainingExpiry = gp.TrainingExpiryOn.ToString("dd-MMM-yyyy"),

                  
                }).ToList();

                DataTable dt = Util.ListToDataTable(exportData);

                var company = await db.Company
                    .FirstOrDefaultAsync(x => App.ActiveStatus.Contains(x.Status));

                using (var wb = new ClosedXML.Excel.XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Gate Pass List");
                    int totalCols = dt.Columns.Count;

                    //TITLE
                    ws.Cell("A1").Value = company?.Description ?? "Gate Pass Report";
                    ws.Range(1, 1, 1, totalCols).Merge();
                    ws.Row(1).Style.Font.Bold = true;
                    ws.Row(1).Style.Font.FontSize = 18;
                    ws.Row(1).Style.Alignment.Horizontal =
                        ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                    ws.Cell("A2").Value = $"Pass Timeline Generated On - {DateTime.Now:dd-MMM-yyyy HH:mm}";
                    ws.Range(2, 1, 2, totalCols).Merge();
                    ws.Row(2).Style.Alignment.Horizontal =
                        ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                    //DATA 
                    ws.Cell("A3").InsertTable(dt);

                    // Header styling
                    ws.Range(3, 1, 3, totalCols)
                      .Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.YaleBlue;

                    ws.Range(3, 1, 3, totalCols)
                      .Style.Font.SetBold();


                    int headerRow = 3;
                    int dataStartRow = headerRow + 1;
                    int dataEndRow = dataStartRow + dt.Rows.Count - 1;

                    var fullRange = ws.Range(headerRow, 1, dataEndRow, totalCols);

                    fullRange.Style.Border.TopBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    fullRange.Style.Border.BottomBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    fullRange.Style.Border.LeftBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    fullRange.Style.Border.RightBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

                    fullRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

                    string[] expiryColumns =
                    {
                "ExpiryOn",
                "LabourLicenseExpiry",
                "MedicalExpiry",
                "TrainingExpiry"
            };

                    int startRow = 4;
                    int endRow = startRow + dt.Rows.Count - 1;

                    foreach (string colName in expiryColumns)
                    {
                        int colIndex = dt.Columns.IndexOf(colName) + 1;

                        for (int r = startRow; r <= endRow; r++)
                        {
                            var cell = ws.Cell(r, colIndex);

                            if (DateTime.TryParse(cell.GetString(), out DateTime expiryDate))
                            {
                                int dayGap = (expiryDate.Date - DateTime.Today).Days;

                                if (dayGap > 15)
                                    cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.Green;
                                else if (dayGap > 1)
                                    cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.Orange;
                                else
                                    cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.Red;
                            }
                        }
                    }

                    ws.Columns().AdjustToContents();
                    ws.SheetView.FreezeRows(3);

                    using (var stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        objMsg.base64 = Convert.ToBase64String(stream.ToArray());
                    }
                }

                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }

            return objMsg;
        }
        public async Task<Message> AddAsync(GatePass obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                // 1. Sanitize Input Strings
                obj.GatePassNo = Util.SanitizeInput(obj.GatePassNo, null) ?? "";
                obj.WorkOrderNo = Util.SanitizeInput(obj.WorkOrderNo, null) ?? "";           
                obj.Department = Util.SanitizeInput(obj.Department, null) ?? "";
                obj.SafetyPassNo = Util.SanitizeInput(obj.SafetyPassNo, null) ?? "";


                obj.CreatedBy = User.Id;
                obj.UpdatedBy = User.Id;
                obj.CreatedAt = DateTime.Now;
                obj.UpdatedAt = DateTime.Now;
                obj.Status = App.Status.Enable; 

                db.GatePass.Add(obj);
                Message.Add(ref objMsg, await db.SaveChangesAsync(), "");

               
                if (objMsg.status == Message.Type.success)
                {     
                    obj.ListId = new List<int> { obj.Id };
                    objMsg.obj = (await ListAsync(obj, User)).FirstOrDefault();
                    //objMsg.obj = (await ListAsync(new GatePass
                    //{
                    //    ListId = new List<int> { obj.Id },
                    //}, User)).FirstOrDefault();
                    objMsg.data = (await GetViewOptionAsync()).data;
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> EditAsync(int Id, User User)
        {
            Message objMsg = new Message();
            try
            {
                GatePass? obj = new GatePass();
                obj.ListId.Add(Id);
                obj.ListStatus.AddRange(App.AllActiveStatus);
                obj = (await ListAsync(obj, User)).FirstOrDefault();
                if (obj == null)
                {
                    Message.Error(ref objMsg, "GatePass did not find for edit.");
                    return objMsg;
                }
                var AddOption = await GetAddOptionAsync();
                objMsg.obj = obj;
                objMsg.data = AddOption.data;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> UpdateAsync(GatePass obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var dbGatePass = db.GatePass.ToList();

                //if (dbGatePass.Any(x => x.EmployeeId == obj.EmployeeId && x.Id != obj.Id))
                //{
                //    Message.Duplicate(ref objMsg, $"GatePass Name {obj.EmployeeDesc}");
                //    return objMsg;
                //}
                //if (!string.IsNullOrEmpty(obj.GatePassNo) &&
                //    dbGatePass.Any(x => x.GatePassNo == obj.GatePassNo && x.Id != obj.Id))
                //{
                //    Message.Duplicate(ref objMsg, $"Contact No {obj.GatePassNo}");
                //    return objMsg;
                //}

                var UpdateGatePass = dbGatePass.FirstOrDefault(x => x.Id == obj.Id);
                if (UpdateGatePass == null)
                {
                    Message.Error(ref objMsg, "GatePass not found for update.");
                    return objMsg;
                }
                UpdateGatePass.EmployeeId = obj.EmployeeId;
                UpdateGatePass.GatePassNo = obj.GatePassNo;
                UpdateGatePass.Type = obj.Type;
                UpdateGatePass.IdentifyMark = obj.IdentifyMark;
                UpdateGatePass.WorkOrderNo = obj.WorkOrderNo;
                UpdateGatePass.CompanyId = obj.CompanyId;
                UpdateGatePass.LocationId = obj.LocationId;
                UpdateGatePass.Department = obj.Department;
                UpdateGatePass.IssuedOn = obj.IssuedOn;
                UpdateGatePass.ExpiryOn = obj.ExpiryOn;
                UpdateGatePass.MedicalExpiryOn = obj.MedicalExpiryOn;
                UpdateGatePass.LabourLicenseExpiryOn = obj.LabourLicenseExpiryOn;
                UpdateGatePass.TrainingExpiryOn = obj.TrainingExpiryOn;
                UpdateGatePass.SafetyPassNo = obj.SafetyPassNo;
                UpdateGatePass.Status = obj.Status;

                UpdateGatePass.UpdatedBy = User.Id;
                UpdateGatePass.UpdatedAt = DateTime.Now;

                db.Update(UpdateGatePass);

                Message.Update(ref objMsg, await db.SaveChangesAsync(), "");

                if (objMsg.status == Message.Type.success)
                {
                    obj.ListId.Add(obj.Id);
                    objMsg.obj =await ListAsync(obj, User);
                    objMsg.data = await GetViewOptionAsync();
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> DeleteAsync(int Id, User User)
        {
            Message objMsg = new Message();
            try
            {
                var DeleteGatePass = db.GatePass.Where(it => it.Id == Id).AsEnumerable().FirstOrDefault();
                if (DeleteGatePass == null)
                {
                    Message.Error(ref objMsg, "Item did not find for delete.");
                    return objMsg;
                }
                DeleteGatePass.Status = App.Status.Delete;
                DeleteGatePass.UpdatedBy = User.Id;
                DeleteGatePass.UpdatedAt = DateTime.Now;
                db.Update(DeleteGatePass);


                Message.Delete(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {
                    DeleteGatePass.ListStatus.Add(App.Status.Delete);
                    DeleteGatePass.ListId.Add(Id);
                    objMsg.obj = await ListAsync(DeleteGatePass, User);
                    objMsg.data = await GetViewOptionAsync();
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Enable(int Id, User User)
        {
            Message objMsg = new Message();
            try
            {
                var EnableGatePass = db.GatePass.Where(it => it.Id == Id).AsEnumerable().FirstOrDefault();
                if (EnableGatePass == null)
                {
                    Message.Error(ref objMsg, "Pass did not find for delete.");
                    return objMsg;
                }
                EnableGatePass.Status = App.Status.Enable;
                EnableGatePass.UpdatedBy = User.Id;
                EnableGatePass.UpdatedAt = DateTime.Now;
                db.Update(EnableGatePass);

                Message.Enable(ref objMsg, db.SaveChanges(), "");
                if (objMsg.status == Message.Type.success)
                {
                    EnableGatePass.ListId.Add(Id);
                    objMsg.obj = ListAsync(EnableGatePass, User);
                    objMsg.data = GetViewOptionAsync();
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
