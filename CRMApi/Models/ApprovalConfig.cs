using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CRMApi.Models
{
    public class ApprovalConfig
    {
        public int Id { get; set; }        
        public int ApiId { get; set; }
        [NotMapped] public string ApiDesc { get; set; } = String.Empty;
        public string HtmlString { get; set; } = String.Empty;
        [NotMapped] public List<Composite.ApprovalSeq> ApprovalSeq { get; set; } = new List<Composite.ApprovalSeq>();
        [NotMapped] public string ApprovalSeqText { get; set; } = String.Empty;
        public int CompanyId { get; set; }
        public string CompanyDesc { get; set; } = String.Empty;
        public int Status { get; set; } = 1;
        [NotMapped] public string StatusDesc { get; set; } = string.Empty;
        [NotMapped] public string? StatusCss { get; set; } = string.Empty;
        [NotMapped] public string? StatusIcon { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        [NotMapped] public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; }
        [NotMapped] public string UpdatedByName { get; set; } = string.Empty;        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;        
        [NotMapped] public bool IsDelete { get; set; } = false;
        [NotMapped] public bool IsEnable { get; set; } = false;        
        [NotMapped] public List<int> ListId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListApiId { get; set; } = new List<int>();                        
        [NotMapped] public List<int> ListStatus { get; set; } = new List<int>();        
    }
}
