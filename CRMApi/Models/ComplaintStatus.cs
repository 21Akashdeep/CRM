using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CRMApi.Models
{
    public class ComplaintStatus
    {
        public int Id { get; set; }
        public int ComplaintId { get; set; }
        [NotMapped] public string? ComplaintNo { get; set; }
        public string? Remarks { get; set; }
        [NotMapped] public string OtpNo { get; set; } = String.Empty;
        public int CompanyId { get; set; }
        [NotMapped] public string? CompanyDesc { get; set; }
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
        [NotMapped] public List<int> ListComplaintId { get; set; } = new List<int>();
        [NotMapped] public DateTime FromDate { get; set; }
        [NotMapped] public DateTime ToDate { get; set; }        
        [NotMapped] public List<int> ListStatus { get; set; } = new List<int>();        
        [NotMapped] public bool IsDelete { get; set; } = false;
    }
}
