using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CRMApi.Models
{
    public class ComplaintAssign
    {
        public int Id { get; set; }        
        public int ComplaintId { get; set; }
        [NotMapped] public string? ComplaintNo { get; set; }        
        public int UserId { get; set; }
        [NotMapped] public string UserName { get; set; } = string.Empty;
        public string? Remarks { get; set; }        
        public int Status { get; set; }
        [NotMapped] public string StatusDesc { get; set; } = string.Empty;
        [NotMapped] public string? StatusCss { get; set; }
        public int CreatedBy { get; set; }
        [NotMapped] public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; }
        [NotMapped] public string UpdatedByName { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;        
    }
}
