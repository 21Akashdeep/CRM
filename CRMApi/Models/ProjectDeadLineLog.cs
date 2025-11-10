using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CRMApi.Models
{
    public class ProjectDeadLineLog
    {
        public int Id { get; set; }
        //[MaxLength(16)] public string Code { get; set; } = string.Empty;
        //[MaxLength(100)] public string Name { get; set; } = string.Empty;
        //[MaxLength(200)] public string Description { get; set; } = string.Empty;
        public DateTime DeadLineDate { get; set; }
        [MaxLength(200)] public string Reason { get; set; } = string.Empty;

        //[NotMapped] public string ProjectDeadLineLogGroupDesc { get; set; } = string.Empty;
        //public int ProjectDeadLineLogSubGroupId { get; set; }
        //[NotMapped] public string ProjectDeadLineLogSubGroupDesc { get; set; } = string.Empty;        
        //public int UnitId { get; set; }
        //[NotMapped] public string UnitDesc { get; set; } = string.Empty;
        //[MaxLength(8)] public string HsnCode { get; set; } = "00000000";
        //public bool IsImport { get; set; } = false;
        //public int CompanyId { get; set; } = 1;
        public int ProjectId { get; set; } = 1;
        [NotMapped] public string? ProjectDesc { get; set; }
        //[NotMapped] public string CompanyDesc { get; set; } = string.Empty;
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
        [NotMapped] public List<int> ListProjectDeadLineLogGroupId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListProjectDeadLineLogSubGroupId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListStatus { get; set; } = new List<int>();
        [NotMapped] public bool IsEdit { get; set; } = false;
        [NotMapped] public bool IsDelete { get; set; } = false;
        [NotMapped] public bool IsEnable { get; set; } = false;
        [NotMapped] public bool IsDuplicate { get; set; } = false;
    }
}
