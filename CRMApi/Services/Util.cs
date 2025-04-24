using ClosedXML.Excel;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using CRMApi.Models;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;
using QRCoder;
using System.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;


namespace CRMApi.Services
{
    public class Util
    {
        public static readonly AppSetting AppSetting = AppSetting.Init();
        
        private static readonly string EncryptionKey = "SecurityKey@123456789~!@#$%^&*()"; // Key should be 32 bytes for AES-256        
        public static string Encrypt(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(EncryptionKey);
                aesAlg.IV = new byte[16]; // Initialization vector (IV) set to zero (for simplicity)

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }

                    byte[] encrypted = msEncrypt.ToArray();
                    return Convert.ToBase64String(encrypted); // Returning base64 encoded encrypted string
                }
            }
        }        
        public static string Decrypt(string cipherText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(EncryptionKey);
                aesAlg.IV = new byte[16]; // Initialization vector (IV) set to zero (for simplicity)

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd(); // Decrypted plain text
                        }
                    }
                }
            }
        }
        public static string RandomAlphaNum()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            return finalString;
        }
        public static List<ControllerInfo> GetControllerInfo()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            List<ControllerInfo> ControllerInfo = asm.GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type))
                .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
                .Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false).Any())
                .Select(x => new ControllerInfo
                {
                    Controller = x.DeclaringType!.Name.Replace("Controller", ""),
                    Action = x.Name,
                    ReturnType = x.ReturnType.Name,
                    Attributes = String.Join(",", x.GetCustomAttributes().Select(a => a.GetType().Name.Replace("Attribute", "")))
                }).ToList();
            return ControllerInfo;
        }
        public static DataTable ExcelToDataTable(IFormFile file)
        {
            DataTable objDataTable = new DataTable();
            string fileExtension = Path.GetExtension(file.FileName);
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                stream.Position = 0;
                if (fileExtension == ".xls")
                {
                    using (var rdr = ExcelReaderFactory.CreateBinaryReader(stream))
                    {
                        var conf = new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = true
                            }
                        };
                        objDataTable = rdr.AsDataSet(conf).Tables[0];
                    }
                }
                else if (fileExtension == ".xlsx")
                {
                    using (var rdr = ExcelReaderFactory.CreateOpenXmlReader(stream))
                    {
                        var conf = new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = true
                            }
                        };
                        objDataTable = rdr.AsDataSet(conf).Tables[0];
                    }
                }
                return objDataTable;
            }
        }
        public static void IsColumnExistInDataTable(DataTable objDataTable, List<string> FieldNameList, ref Message objMsg)
        {
            if (objDataTable == null || objDataTable.Columns == null)
            {
                Message.Error(ref objMsg, "Data table is empty.");
                return;
            }
            if (FieldNameList.Count == 0)
            {
                Message.Error(ref objMsg, "Field Name list is empty.");
                return;
            }
            foreach (string FieldName in FieldNameList)
            {
                if (!objDataTable.Columns.Contains(FieldName))
                {
                    Message.Error(ref objMsg, "Field Name : " + FieldName + " is missing.");
                    break;
                }
            }
        }
        public static DataTable ListToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null)!;
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }
        public static string DataTableToBase64(DataTable objDataTable, Company obj)
        {
            string base64;
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add(obj.SheetName);

                ws.Cell("A1")
                    .SetValue(obj.Name)
                    .Style.Font.SetFontSize(18)
                    .Font.SetBold(true)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                string headerMergeRange = String.Concat("A1:", Util.ExcelColName(objDataTable.Columns.Count), "1");
                ws.Range(headerMergeRange).Merge(true).
                    Style.
                    Border.SetInsideBorder(XLBorderStyleValues.Thin).
                    Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                ws.Cell("A2")
                    .SetValue(obj.ReportDesc)
                    .Style.Font.SetFontSize(14)
                    .Font.SetBold(true)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Range(headerMergeRange.Replace("1", "2")).Merge(true).
                    Style.
                    Border.SetInsideBorder(XLBorderStyleValues.Thin).
                    Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                ws.Cell("A3").InsertTable(objDataTable).
                    Style.
                    Border.SetInsideBorder(XLBorderStyleValues.Thin).
                    Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    base64 = Convert.ToBase64String(stream.ToArray(), 0, stream.ToArray().Length);
                }
            }
            return base64;
        }
        public static string ExcelColName(int index)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var value = "";
            if (index > 701)
            {
                value = "ZZ";
            }
            else if (index <= letters.Length)
            {
                value += letters[index - 1];
            }
            else
            {
                value += letters[index / letters.Length - 1];
                value += letters[index % letters.Length - 1];
            }
            return value;
        }
        public static string CreateJwtToken(User obj)
        {
            string JwtToken = "";
            string userJsonString = JsonConvert.SerializeObject(new { obj.Id, obj.TokenExpiry });
            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSetting.Jwt.Key)),
                SecurityAlgorithms.HmacSha512Signature
            );
            var subject = new ClaimsIdentity(new[]
            {
                    new Claim(JwtRegisteredClaimNames.Sub, userJsonString),
            });

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = subject,
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(AppSetting.Jwt.TokenExpireTimeInMinutes)),
                Issuer = AppSetting.Jwt.Issuer,
                Audience = AppSetting.Jwt.Audience,
                SigningCredentials = signingCredentials
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            JwtToken = tokenHandler.WriteToken(token);
            return JwtToken;
        }
        public static User? RequestVerify(Request Request, DbCRM db, ref Message objMsg)
        {
            User? User = new User();
            var AuthorizationToken = Request.HttpRequest!.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var ClientIp = Request.HttpRequest.HttpContext.Connection.RemoteIpAddress!.ToString();
            if (String.IsNullOrEmpty(AuthorizationToken))
            {
                Message.UnAuthorized(ref objMsg, "Authorization token did not find.");
                return null;
            }
            
            var JwtHandler = new JwtSecurityTokenHandler();
            var AuthTokenSub = JwtHandler.ReadJwtToken(AuthorizationToken).Subject;
            
            //Deserialize User from JWT Token
            var AuthUser = JsonConvert.DeserializeObject<User>(AuthTokenSub);
            if (AuthUser == null)
            {
                Message.UnAuthorized(ref objMsg, "Invalid auth token.");
                return null;
            }            
            //Check Is Token Expiry
            if (AuthUser.TokenExpiry < DateTime.Now)
            {
                Message.UnAuthorized(ref objMsg, "Your session has been expired.<br>Please re-login.");
                return null;
            }
            //Get App setting
            var App = Util.AppSetting;
            User = db.User.FirstOrDefault(x => App.ActiveStatus.Contains(x.Status) && x.Id == AuthUser.Id);
            if (User == null) 
            {
                Message.UnAuthorized(ref objMsg, "User did not find.");
                return null;
            }
            User.TokenExpiry = AuthUser.TokenExpiry;
            //Set Api Name
            User.ApiName = Request.HttpRequest.RouteValues["controller"]?.ToString() ?? "";            
            //Is User Type Sys Admin Or Request Action Type UnAuthorised            
            if (User.UserType == App.UserType.SysAdmin || Request.ActionType == ActionType.UnAuthorised)
            {
                return User;
            }

            //Get Api Info
            var dbApi = db.Api.Where(ap => ap.Name == User.ApiName).ToList();
            if (!dbApi.Any()) 
            {
                Message.Error(ref objMsg, $"Api Name ({User.ApiName}) not listed.");
                return null;
            }
            User.ApiId = dbApi.FirstOrDefault()?.Id ?? 0;
            
            
            //Check Permission            
            var dbApiPermission = (
                from ape in User.Api
                join api in dbApi on ape.ApiId equals api.Id
                where api.Name == User.ApiName
                select new 
                {                    
                    ApiName = api.Name,
                    ape.View,
                    ape.Add,
                    ape.Update,
                    ape.Delete,
                    ape.Enable,                    
                    ape.Print,
                    ape.Import,
                    ape.Export
                }
            ).ToList();

            // Check Action Permission
            string ActionName = Request.ActionType.ToString();
            dbApiPermission = dbApiPermission.Where(ape => ape.GetType().GetProperty(ActionName)?.GetValue(ape) as bool? == true).ToList();

            if (dbApiPermission.Count == 0)
            {
                Message.UnAuthorized(ref objMsg, $"You are not authorized to access {User.ApiName}/{ActionName}");
                return null;
            }
            return User;
        }
        public static void SentMail(DbCRM db, MailMessage Mail, string Subject, string Matter, string ColorType, ref Message objMsg)
        {
            try
            {
                MailService objMailService = db.MailService.Where(x => x.Status == 1).AsEnumerable().FirstOrDefault()!;
                if (objMailService == null)
                {
                    Message.Error(ref objMsg, "Mail serviceces did not find");
                    return;
                }
                Thread bgThread = new Thread(() => Send(objMailService, Mail, Subject, Matter, ColorType));
                bgThread.Start();
                Message.Success(ref objMsg, "Email send");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
        }
        private static void Send(MailService objMailService, MailMessage Mail, string MailSubject, string Matter, string ColorType)
        {
            if (objMailService != null)
            {
                if (objMailService.EnableMailSending)
                {

                    SmtpClient client = new SmtpClient(objMailService.SmtpClientAddress);
                    client.Port = Convert.ToInt32(objMailService.PortNumber);
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = objMailService.UseDefaultCredentials;
                    NetworkCredential credentials = new NetworkCredential(objMailService.MailId, objMailService.MailPassword);
                    client.EnableSsl = objMailService.EnableSSL;
                    client.Credentials = credentials;
                    string body = MailFormatter(MailSubject, Matter, ColorType, objMailService);
                    var mail = Mail;
                    MailAddress From = new MailAddress(objMailService.MailId, objMailService.MailTitle);
                    mail.From = From;
                    mail.Subject = MailSubject;
                    mail.IsBodyHtml = true;
                    mail.Body = body;
                    if (!objMailService.ServerCertificateValidation)
                    {
                        ServicePointManager.ServerCertificateValidationCallback = delegate (object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
                        {
                            return true;
                        }
                        !;
                    }
                    client.Send(mail);
                }
            }
        }
        private static string MailFormatter(string MailSubject, string Matter, string ColorType, MailService objMailService)
        {
            Message objMsg = new Message();            
            string MailBody = "";
            try
            {
                string Color = "";
                switch (ColorType)
                {
                    case "primary":
                        Color = "#007bff";
                        break;
                    case "secondary":
                        Color = "#6c757d";
                        break;
                    case "success":
                        Color = "#28a745";
                        break;
                    case "danger":
                        Color = "#dc3545";
                        break;
                    case "warning":
                        Color = "#ffc107";
                        break;
                    case "info":
                        Color = "#17a2b8";
                        break;
                    case "light":
                        Color = "#f8f9fa";
                        break;
                    case "dark":
                        Color = "#343a40";
                        break;
                    default:
                        Color = "#fff";
                        break;
                }

                MailBody = @$"
                    <div id='EmailCard' style='border: 1px solid {Color}; border-radius: 5px; width: 98%; font-family: Calibri; font-size:14px;'>
                        <div id='EmailCardHeader' style='background-color: {Color}; padding:5px; font-size:20px; font-weight:bold; color:white;'>
                            {MailSubject}
                        </div>
                        <div id='EmailCardBody' style='padding:5px;'>
                            <p>Dear Sir/Madam,</p>
                            {Matter}
                            <p><br>Regards,<br>{objMailService.AppName}<br>{objMailService.CompanyName}<br></p>
                        </div>
                    </div>
                ";
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return MailBody;
        }
        public static string SanitizeInput(string text, string regExp)
        {
            return Regex.Replace(text, regExp, string.Empty);
        }
        public static string QRCodeBase64(string text) 
        {
            string base64 = "";
            QRCodeGenerator QrGenerator = new QRCodeGenerator();
            QRCodeData QRCodeData = QrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.L);
            PngByteQRCode QRCode = new PngByteQRCode(QRCodeData);
            byte[] qrCodeImage = QRCode.GetGraphic(70);
            base64 = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(qrCodeImage));            
            return base64;
        }
    }
}
