using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CRMApi.Models
{
    public class Project
    {
        public int Id { get; set; }
        [MaxLength(16)] public string Code { get; set; } = string.Empty;
        [MaxLength(100)] public string Name { get; set; } = string.Empty;
        [MaxLength(200)] public string Description { get; set; } = string.Empty;
        [MaxLength(20)] public string PoNo { get; set; } = string.Empty;
        [NotMapped] public Composite.Document? Document { get; set; } = new Composite.Document();
        public int CustomerId { get; set; } = 1;
        [NotMapped] public string? CustomerDesc { get; set; }

        
        public DateTime DeadLineDate { get; set; }
        public DateTime PoDate { get; set; }
        public DateTime StartDate { get; set; }



        public string? TechnicalDoc { get; set; }


    
        [NotMapped] public string CompanyDesc { get; set; } = string.Empty;
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
        [NotMapped] public List<int> ListProjectGroupId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListProjectSubGroupId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListStatus { get; set; } = new List<int>();
        [NotMapped] public bool IsEdit { get; set; } = false;
        [NotMapped] public bool IsDelete { get; set; } = false;
        [NotMapped] public bool IsEnable { get; set; } = false;
        [NotMapped] public bool IsDuplicate { get; set; } = false;
    }
}
