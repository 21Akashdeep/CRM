using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMApi.Models
{
    public class Location
    {
        public int Id { get; set; }
        [MaxLength(16)] public string Code { get; set; } = String.Empty;
        [MaxLength(100)] public string Name { get; set; } = String.Empty;
        [MaxLength(200)] public string Description { get; set; } = String.Empty;
        public int Status { get; set; } = 1;
        [NotMapped] public string StatusName { get; set; } = string.Empty;
        [NotMapped] public string StatusCss { get; set; } = string.Empty;
        [NotMapped] public string StatusIcon { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        [NotMapped] public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; }
        [NotMapped] public string UpdatedByName { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        [NotMapped] public bool IsEdit { get; set; } = false;
        [NotMapped] public bool IsDelete { get; set; } = false;
        [NotMapped] public bool IsEnable { get; set; } = false;
        [NotMapped] public bool IsDuplicate { get; set; } = false;
        [NotMapped] public List<int> ListId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListStatus { get; set; } = new List<int>();
    }
}
