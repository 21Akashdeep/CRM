using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CRMApi.Models
{
    public class Approval
    {
        public int Id { get; set; }
        public int ApprovalConfigId { get; set; }
        public int ApiId { get; set; }
        [NotMapped] public string ApiName { get; set; } = string.Empty;
        public int ReqId { get; set; }
        public string ReqNo { get; set; } = string.Empty;
        public DateTime ReqDate { get; set; } = DateTime.Now;        
        public decimal NetAmount { get; set; }
        public bool IsLastApprover { get; set; } = false;        
        public string? Remarks { get; set; }
        public int SeqNo { get; set; }
        public int UserId { get; set; }
        [NotMapped] public string UserName { get; set; } = String.Empty;
        [NotMapped] public string UserEmail { get; set; } = String.Empty;
        [NotMapped] public string DepartmentDesc { get; set; } = String.Empty;
        public int CompanyId { get; set; } = 1;
        [NotMapped] public string CompanyDesc { get; set; } = string.Empty;
        public int Status { get; set; } = 1;
        [NotMapped] public string StatusDesc { get; set; } = string.Empty;
        [NotMapped] public string? StatusCss { get; set; }
        public int CreatedBy { get; set; }
        [NotMapped] public string CreatedByName { get; set; } = string.Empty;
        public int UpdatedBy { get; set; }
        [NotMapped] public string UpdatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        [NotMapped] public bool IsView { get; set; } = false;
        [NotMapped] public bool IsApprove { get; set; } = false;
        [NotMapped] public bool IsReject { get; set; } = false;
        [NotMapped] public int FirstApproverId { get; set; }
        [NotMapped] public DateTime FromDate { get; set; }
        [NotMapped] public DateTime TillDate { get; set; }
        [NotMapped] public List<int> ListId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListApiId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListReqId { get; set; } = new List<int>();
        [NotMapped] public List<string> ListReqNo { get; set; } = new List<string>();
        [NotMapped] public List<int> ListUserId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListStatus { get; set; } = new List<int>();
    }
}
