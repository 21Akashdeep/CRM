using Microsoft.EntityFrameworkCore;

namespace CRMApi.Services
{
    public class Message
    {
        public Type status { get; set; }
        public string? statusText { get; set; }
        public dynamic data { get; set; } = new List<string>();
        public dynamic? obj { get; set; } = new { };
        public string? base64 { get; set; }
        public string? filePath { get; set; }
        public List<int> tranId { get; set; } = new List<int>();
        public dynamic company { get; set; } = new { };
        public string? redirectPage { get; set; }
        
        public static void Add(ref Message objMsg, int status, string text)
        {
            if (status > 0)
            {
                objMsg.status = Type.success;
                objMsg.statusText = "Record has saved successfully.\n" + text;
            }
            else
            {
                objMsg.status = Type.error;
                objMsg.statusText = "Record has not saved.!!!\n" + text;
            }
        }        
        public static void Update(ref Message objMsg, int status, string text)
        {
            if (status > 0)
            {
                objMsg.status = Type.success;
                objMsg.statusText = "Record has updated successfully.\n" + text;
            }
            else
            {
                objMsg.status = Type.error;
                objMsg.statusText = "Record has not updated.!!!\n" + text;
            }
        }
        public static void Close(ref Message objMsg, int status, string text)
        {
            if (status > 0)
            {
                objMsg.status = Type.success;
                objMsg.statusText = "Record has closed successfully.\n" + text;
            }
            else
            {
                objMsg.status = Type.error;
                objMsg.statusText = "Record has not closed.!!!\n" + text;
            }
        }
        public static void Get(ref Message objMsg, string text ="")
        {
            if (objMsg.data.Count > 0)
            {
                objMsg.status = Type.success;
                objMsg.statusText = "Record found.\n" + text;
            }
            else
            {
                objMsg.status = Type.error;
                objMsg.statusText = "Record did not find.!!!\n" + text;
            }
        }
        public static void Delete(ref Message objMsg, int status, string text)
        {
            if (status > 0)
            {
                objMsg.status = Type.success;
                objMsg.statusText = "Record has deleted successfully.\n" + text;
            }
            else
            {
                objMsg.status = Type.error;
                objMsg.statusText = "Record has not deleted.!!!.\n" + text;
            }
        }
        public static void Enable(ref Message objMsg, int status, string text)
        {
            if (status > 0)
            {
                objMsg.status = Type.success;
                objMsg.statusText = "Record has enabled successfully.\n" + text;
            }
            else
            {
                objMsg.status = Type.error;
                objMsg.statusText = "Record has not enabled.!!!.\n" + text;
            }
        }
        public static void Approve(ref Message objMsg, int status, string text)
        {
            if (status > 0)
            {
                objMsg.status = Type.success;
                objMsg.statusText = "Record has Approved successfully.\n" + text;
            }
            else
            {
                objMsg.status = Type.error;
                objMsg.statusText = "Record has not Approved.!!!\n" + text;
            }
        }
        public static void Reject(ref Message objMsg, int status, string text)
        {
            if (status > 0)
            {
                objMsg.status = Type.success;
                objMsg.statusText = "Record has been Rejected.\n" + text;
            }
            else
            {
                objMsg.status = Type.error;
                objMsg.statusText = "Record has not Rejected.!!!\n" + text;
            }
        }
        public static void Submit(ref Message objMsg, int status, string text)
        {
            if (status > 0)
            {
                objMsg.status = Type.success;
                objMsg.statusText = "Record has been Submitted.\n" + text;
            }
            else
            {
                objMsg.status = Type.error;
                objMsg.statusText = "Record has not been Submitted.!!!\n" + text;
            }
        }
        public static void Exception(ref Message objMsg, Exception ex)
        {
            objMsg.status = Type.error;
            objMsg.statusText = ex.InnerException != null ? String.Concat(ex.Message, "<br>", ex.InnerException.Message) : ex.Message;            
        }
        public static void Success(ref Message objMsg, string text)
        {
            objMsg.status = Type.success;
            objMsg.statusText = text;
        }
        public static void Warning(ref Message objMsg, string text)
        {
            objMsg.status = Type.warning;
            objMsg.statusText = text;
        }
        public static void Info(ref Message objMsg, string text)
        {
            objMsg.status = Type.info;
            objMsg.statusText = text;
        }
        public static void Error(ref Message objMsg, string text)
        {
            objMsg.status = Type.error;
            objMsg.statusText = text;
        }
        public static void Duplicate(ref Message objMsg, string text)
        {
            objMsg.status = Type.error;
            objMsg.statusText = "Duplicate entry.<br>" + text;
        }
        public static void UnAuthorized(ref Message objMsg, string text)
        {
            objMsg.status = Message.Type.unauthorized;
            objMsg.statusText = "Un-Authorized access.<br>" + text;
            objMsg.redirectPage = "/";
        }
        public static void Redirect(ref Message objMsg, string text, string redirectUrl)
        {
            objMsg.status = Message.Type.redirect;
            objMsg.statusText = text;
            objMsg.redirectPage = redirectUrl;
        }
        public enum Type
        {
            success = 200,
            warning = 204,
            info = 202,
            error = 500,
            redirect = 302,
            unauthorized = 401
        }
    }
}
