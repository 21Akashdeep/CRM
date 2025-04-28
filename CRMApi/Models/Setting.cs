using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CRMApi.Models
{
    public class Setting
    {
        public int Id { get; set; }
        [MaxLength(50)] public string Category { get; set; } = String.Empty;
        [MaxLength(100)] public string Name { get; set; } = String.Empty;
        [MaxLength(100)] public string Description { get; set; } = String.Empty;
        [MaxLength(50)] public string Value { get; set; } = String.Empty;
        [MaxLength(200)] public string? CssClass { get; set; }
        [MaxLength(100)] public string? Icon { get; set; }        
        public int Status { get; set; } = 1;
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        [NotMapped] public string StatusText { get; set; } = String.Empty;
        [NotMapped] public string? StatusCss { get; set; }
        [NotMapped] public string CreatedByName { get; set; } = String.Empty;
        [NotMapped] public string UpdatedByName { get; set; } = String.Empty;
        [NotMapped] public bool IsEdit { get; set; } = false;
        [NotMapped] public bool IsDuplicate { get; set; } = false;
        [NotMapped] public bool IsDelete { get; set; } = false;
        [NotMapped] public bool IsEnable { get; set; } = false;
        [NotMapped] public List<int> ListId { get; set; } = new List<int>();
        [NotMapped] public List<string> ListCategory { get; set; } = new List<string>();
        [NotMapped] public List<string> ListName { get; set; } = new List<string>();
        [NotMapped] public List<int> ListStatus { get; set; } = new List<int>();
        [NotMapped] public User User { get; set; } = new User();
    }
}
