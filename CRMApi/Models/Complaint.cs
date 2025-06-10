using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMApi.Models
{
    public class Complaint
    {
        public int Id { get; set; }
        [MaxLength (16)] public string Code { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        [MaxLength(2)] public string SupportMode { get; set; } = string.Empty;
        [NotMapped] public string SupportModeDesc { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        [NotMapped] public string? CustomerDesc { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? PinCode { get; set; }
        public string? PostOffice { get; set; }
        public string? District { get; set; }
        public int AdminDivId { get; set; }
        [NotMapped] public string? AdminDivDesc { get; set; }
        public int CountryId { get; set; }
        [NotMapped] public string? CountryDesc { get; set; }
        [NotMapped] public string? CustomerAddress { get; set; }
        public string? ContactNo { get; set; }
        public string? Email { get; set; }
        public int DepartmentId { get; set; }
        [NotMapped] public string? DepartmentDesc { get; set; }
        public int? ForwardTo { get; set; }        
        [NotMapped] public string? ForwardToName { get; set; }
        public int Priority { get; set; }
        [NotMapped] public string? PriorityDesc { get; set; }
        public string Problem { get; set; } = string.Empty;
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public DateTime? CompletedDateTime { get; set; }
        public int CompanyId { get; set; }
        [NotMapped] public string? CompanyDesc { get; set; }
        public int Status { get; set; } = 1;
        [NotMapped] public string StatusDesc { get; set; } = string.Empty;
        [NotMapped] public string? StatusCss { get; set; }
        public int CreatedBy { get; set; }
        [NotMapped] public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; }
        [NotMapped] public string UpdatedByName { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        [NotMapped] public List<ComplaintAssign> AssignTo { get; set; } = new List<ComplaintAssign>();
        [NotMapped] public bool IsEdit { get; set; } = false;
        [NotMapped] public bool IsDelete { get; set; } = false;
        [NotMapped] public bool IsEnable { get; set; } = false;
        [NotMapped] public bool IsDuplicate { get; set; } = false;
        [NotMapped] public bool IsClose { get; set; } = false;
        [NotMapped] public bool IsAddStatus { get; set; } = false;
        [NotMapped] public DateTime FromDate { get; set; }
        [NotMapped] public DateTime ToDate { get; set; }        
        [NotMapped] public List<int> ListId { get; set; } = new List<int>();
        [NotMapped] public List<string> ListSupportMode { get; set; } = new List<string>();        
        [NotMapped] public List<int> ListCustomerId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListDeparmentId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListPriority { get; set; } = new List<int>();
        [NotMapped] public List<int> ListForwardTo { get; set; } = new List<int>();        
        [NotMapped] public List<int> ListStatus { get; set; } = new List<int>();        
    }
}
