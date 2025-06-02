using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security;

namespace CRMApi.Models
{    
    public class User
    {
        public int Id { get; set; }
        [MaxLength(100)] public string UserId { get; set; } = string.Empty;
        [MaxLength(100)] public string Password { get; set; } = string.Empty;
        [MaxLength(10)] public string UserType { get; set; } = string.Empty;
        [NotMapped] public string UserTypeDesc { get; set; } = string.Empty;        
        [MaxLength(10)] public string? Code { get; set; }
        [MaxLength(100)] public string Name { get; set; } = string.Empty;        
        public DateTime? DateOfBirth { get; set; }
        [MaxLength(1)] public string? Gender { get; set; }
        [MaxLength(100)] public string? FatherName { get; set; }
        public int DepartmentId { get; set; }
        [NotMapped] public string DepartmentDesc { get; set; } = string.Empty;
        public int DesignationId { get; set; }
        [NotMapped] public string DesignationDesc { get; set; } = string.Empty;
        [MaxLength(10)] public string ContactNo { get; set; } = string.Empty;
        [MaxLength(100)] public string Email { get; set; } = string.Empty;
        public DateTime? PasswordExpiredAt { get; set; }
        public List<Composite.UserCompany> Company { get; set; } = new List<Composite.UserCompany>();
        public List<Composite.UserApi> Api { get; set; } = new List<Composite.UserApi>();
        public List<Composite.UserApprovalRole> ApprovalRole { get; set; } = new List<Composite.UserApprovalRole>();
        public string Theme { get; set; } = String.Empty;
        public int Status { get; set; } = 1;
        [NotMapped] public string StatusDesc { get; set; } = string.Empty;
        [NotMapped] public string? StatusCss { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        [NotMapped] public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; }
        [NotMapped] public string UpdatedByName { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        [NotMapped] public List<int> ListId { get; set; } = new List<int>();
        [NotMapped] public List<string> ListUserType { get; set; } = new List<string>();
        [NotMapped] public List<int> ListStatus { get; set; } = new List<int>();
        [NotMapped] [MaxLength(100)] public string? NewPassword { get; set; }
        [NotMapped] [MaxLength(100)] public string? ConfirmPassword { get; set; }
        [NotMapped] public DateTime TokenExpiry { get; set; }
        [NotMapped] public int ApiId { get; set; }
        [NotMapped] public string ApiName { get; set; } = string.Empty;
        [NotMapped] public string ApiType { get; set; } = string.Empty;
        [NotMapped] public int CompanyId { get; set; }        
        [NotMapped] public bool IsReSentPassword { get; set; } = false;
        [NotMapped] public bool IsEdit { get; set; } = false;
        [NotMapped] public bool IsDuplicate { get; set; } = false;
        [NotMapped] public bool IsDelete { get; set; } = false;
        [NotMapped] public bool IsEnable { get; set; } = false;
    }
}
