namespace CRMApi.Models
{
    public class MailService
    {
        public int Id { get; set; }
        public string MailId { get; set; } = String.Empty;
        public string MailPassword { get; set; } = String.Empty;
        public string MailTitle { get; set; } = String.Empty;
        public string AppName { get; set; } = String.Empty;
        public string CompanyName { get; set; } = String.Empty;
        public string PortNumber { get; set; } = String.Empty;
        public bool EnableSSL { get; set; }
        public bool EnableMailSending { get; set; }
        public string SmtpClientAddress { get; set; } = String.Empty;
        public bool UseDefaultCredentials { get; set; }
        public bool ServerCertificateValidation { get; set; }
        public string AppUrl { get; set; } = String.Empty;
        public int Status { get; set; } = 1;
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
