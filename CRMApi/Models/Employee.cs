using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMApi.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [MaxLength(100)] public string Name { get; set; } = string.Empty;
        [MaxLength(1)] public string Gender { get; set; } = string.Empty;
        public DateTime DOB { get; set; }
        [MaxLength(1)] public string GuardianRelation { get; set; } = string.Empty;
        [MaxLength(200)] public string GuardianName { get; set; } = string.Empty;
        [MaxLength(1)] public string MaritalStatus { get; set; } = string.Empty;
        [MaxLength(1)] public string JntvtiCategory { get; set; } = string.Empty;
        public int QualificationId { get; set; }
        [NotMapped] public string QualificationDesc { get; set; } = string.Empty;
        [MaxLength(10)] public string ContactNo { get; set; } = string.Empty;
        [MaxLength(100)] public string Email { get; set; } = string.Empty;
        public int IsEpfDeduct { get; set; }
        [MaxLength(12)] public string UanNo { get; set; } = string.Empty;
        public int IsEsiDeduct { get; set; }
        [MaxLength(12)] public string EsiNo { get; set; } = string.Empty;
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
        [NotMapped] public List<string> ListGender { get; set; } = new List<string>();


        [NotMapped] public bool IsEdit { get; set; } = false;
        [NotMapped] public bool IsDelete { get; set; } = false;
        [NotMapped] public bool IsEnable { get; set; } = false;
        [NotMapped] public bool IsDuplicate { get; set; } = false;
    }
}
