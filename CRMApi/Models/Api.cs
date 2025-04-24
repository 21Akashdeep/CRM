using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CRMApi.Models
{
    public class Api
    {
        public int Id { get; set; }
        [MaxLength(16)] public string Code { get; set; } = String.Empty;
        [MaxLength(100)] public string Name { get; set; } = String.Empty;
        [MaxLength(200)] public string Description { get; set; } = String.Empty;
        [MaxLength(010)] public string ApiType { get; set; } = String.Empty;
        [NotMapped] public string ApiTypeName { get; set; } = string.Empty;
        public int ApiGroupId { get; set; }
        [NotMapped] public string ApiGroupDesc { get; set; } = string.Empty;
        public bool IsApprovalRequired { get; set; } = false;
        public int SeqNo { get; set; }
        [MaxLength(100)] public string? Icon { get; set; }        
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
        [NotMapped] public List<int> ListApiGroupId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListStatus { get; set; } = new List<int>();        
    }
}
