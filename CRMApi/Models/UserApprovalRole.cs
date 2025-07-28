using System.ComponentModel.DataAnnotations.Schema;

namespace CRMApi.Models
{
    public class UserApprovalRole
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [NotMapped] public string UserName { get; set; } = string.Empty;
        public int ApprovalRoleId { get; set; }
        [NotMapped] public string ApprovalRoleDesc { get; set; } = string.Empty;
        public int Status { get; set; } = 1;
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        [NotMapped] public bool IsAdded { get; set; }
    }
}
