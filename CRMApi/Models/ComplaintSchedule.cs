using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CRMApi.Models
{
    public class ComplaintSchedule
    {
        public int Id { get; set; }        
        public int ComplaintId { get; set; }
        [NotMapped] public string? ComplaintNo { get; set; }
        [NotMapped] public DateTime ComplaintDate { get; set; }        
        [NotMapped] public string? CustomerDesc { get; set; }
        [NotMapped] public string? CustomerAddress { get; set; }        
        [NotMapped] public string Problem{ get; set; } = string.Empty;        
        [NotMapped] public string UserName { get; set; } = string.Empty;
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public DateTime? CompletedDateTime { get; set; }        
        public List<Composite.ComplaintAssignTo> AssignTo { get; set; } = new List<Composite.ComplaintAssignTo>();
        public string? Remarks { get; set; }
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
        [NotMapped] public bool IsEdit { get; set; } = false;
        [NotMapped] public bool IsDelete { get; set; } = false;
    }
}
