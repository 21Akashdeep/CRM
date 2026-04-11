using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CRMApi.Models
{
    public class Item
    {
        public int Id { get; set; }
        [MaxLength(16)] public string Code { get; set; } = string.Empty;
        public string? Make { get; set; }
        [MaxLength(100)] public string Name { get; set; } = string.Empty;
        public string? Model { get; set; }
        [MaxLength(200)] public string Description { get; set; } = string.Empty;
        public int ItemGroupId { get; set; }
        [NotMapped] public string ItemGroupDesc { get; set; } = string.Empty;
        public int ItemSubGroupId { get; set; }
        [NotMapped] public string ItemSubGroupDesc { get; set; } = string.Empty;        
        public int UnitId { get; set; }
        [NotMapped] public string UnitDesc { get; set; } = string.Empty;
        [MaxLength(8)] public string HsnCode { get; set; } = "00000000";
        public bool IsImport { get; set; } = false;
        public int CompanyId { get; set; } = 1;
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
        public List<ItemUnit> ItemUnit { get; set; } = new List<ItemUnit>();
        [NotMapped] public List<int> ListId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListItemGroupId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListItemSubGroupId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListStatus { get; set; } = new List<int>();
        [NotMapped] public bool IsEdit { get; set; } = false;
        [NotMapped] public bool IsDelete { get; set; } = false;
        [NotMapped] public bool IsEnable { get; set; } = false;
        [NotMapped] public bool IsDuplicate { get; set; } = false;
    }
}
