using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMApi.Models
{
    public class GatePass
    {
        public int Id { get; set; }
        [MaxLength(20)] public string GatePassNo { get; set; } = string.Empty;
        [MaxLength(2)] public string Type { get; set; } = string.Empty;
        [NotMapped] public string TypeDesc { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        [NotMapped] public string EmployeeDesc { get; set; } = string.Empty;
        [MaxLength(200)] public string IdentifyMark { get; set; } = string.Empty;
        [MaxLength(20)] public string WorkOrderNo { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        [NotMapped] public string CompanyDesc { get; set; } = string.Empty;
        public int LocationId { get; set; }
        [NotMapped] public string LocationDesc { get; set; } = string.Empty;
        [MaxLength(100)] public string Department { get; set; } = string.Empty;
        public DateTime IssuedOn { get; set; }
        public DateTime ExpiryOn { get; set; }
        [MaxLength(100)] public string SafetyPassNo { get; set; } = string.Empty;
        public DateTime TrainingExpiryOn { get; set; }
        public DateTime MedicalExpiryOn { get; set; }
        public DateTime LabourLicenseExpiryOn { get; set; }
        public int Status { get; set; } = 1;
        [NotMapped] public string StatusDesc { get; set; } = string.Empty;
        [NotMapped] public string? StatusCss { get; set; }
        public int CreatedBy { get; set; }
        [NotMapped] public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [NotMapped] public string UpdatedByName { get; set; } = string.Empty;
        public int UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [NotMapped] public List<int> ListId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListStatus { get; set; } = new List<int>();
        [NotMapped] public List<int> ListLocation { get; set; } = new List<int>();
        [NotMapped] public List<int> ListDepartment { get; set; } = new List<int>();
        [NotMapped] public List<int> ListCompany { get; set; } = new List<int>();
        [NotMapped] public List<string> ListPassType { get; set; } = new List<string>();

        [NotMapped] public bool IsEdit { get; set; } = false;
        [NotMapped] public bool IsDelete { get; set; } = false;
        [NotMapped] public bool IsEnable { get; set; } = false;
        [NotMapped] public bool IsDuplicate { get; set; } = false;
    }
}
