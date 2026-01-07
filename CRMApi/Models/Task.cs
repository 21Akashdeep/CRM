using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMApi.Models
{
    public class Task
    {
        public int Id { get; set; }
        [MaxLength(16)] public string Code { get; set; }= null!;
        [MaxLength(100)] public string Name { get; set; } = null!;
        [MaxLength(200)] public string Description { get; set; } = null!;
        public int CustomerId { get; set; }
        public string Category { get; set; } = string.Empty;
        [MaxLength(20)] public string? PoNo { get; set; }
        public DateTime? PoDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [NotMapped] public bool IsClose { get; set; } = false;
        [NotMapped] public bool IsAddStatus { get; set; } = false;
        [MaxLength(200)] public string? TechnicalDoc { get; set; }
        [MaxLength(200)] public string Remarks { get; set; } = string.Empty;
        public int Status { get; set; } = 1;        
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public List<TaskItem> TaskItem { get; set; } = new List<TaskItem>();
    }
}
