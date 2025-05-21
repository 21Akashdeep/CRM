using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CRMApi.Models
{
    public class ComplaintItem
    {
        public int Id { get; set; }        
        public int ComplaintId { get; set; }
        [NotMapped] public string? ComplaintNo { get; set; }        
        public int ItemId { get; set; }
        [NotMapped] public string? ItemDesc { get; set; }
        public string? Problem { get; set; }        
        public int CompanyId { get; set; }
        [NotMapped] public string? CompanyDesc { get; set; }
        public int Status { get; set; } = 2;
        [NotMapped] public string StatusDesc { get; set; } = string.Empty;
        [NotMapped] public string? StatusCss { get; set; }
        public int CreatedBy { get; set; }
        [NotMapped] public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; }
        [NotMapped] public string UpdatedByName { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        [NotMapped] public bool IsDelete { get; set; } = false;
    }
}
