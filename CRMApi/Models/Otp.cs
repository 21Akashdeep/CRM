using System.ComponentModel.DataAnnotations.Schema;

namespace CRMApi.Models
{
    public class Otp
    {
        public int Id { get; set; }
        public string RefType { get; set; } = string.Empty;
        public int RefId { get; set; }
        public string RefNo { get; set; } = string.Empty;
        public string OtpNo { get; set; } = string.Empty;
        public DateTime Expiry { get; set; } = DateTime.Now;        
        public int Status { get; set; }
        [NotMapped] public string StatusDesc { get; set; } = string.Empty;
        [NotMapped] public string? StatusCss { get; set; }
        public int CreatedBy { get; set; }
        [NotMapped] public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; }
        [NotMapped] public string UpdatedByName { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        [NotMapped] public List<int> ListId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListRefType { get; set; } = new List<int>();
        [NotMapped] public List<int> ListRefId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListRefNo { get; set; } = new List<int>();
        [NotMapped] public DateTime FromDate { get; set; }
        [NotMapped] public DateTime ToDate { get; set; }
        [NotMapped] public List<int> ListStatus { get; set; } = new List<int>();
        [NotMapped] public bool IsDelete { get; set; } = false;
    }
}
