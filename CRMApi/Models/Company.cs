using System.ComponentModel.DataAnnotations.Schema;

namespace CRMApi.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? CinNo { get; set; }
        public string GstNo { get; set; } = "URP";
        public string? PanNo { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? PinCode { get; set; }
        public string? PostOffice { get; set; }
        public string? District { get; set; }
        public int AdminDivId { get; set; }
        public int CountryId { get; set; }
        public string? ContactNo { get; set; }
        public string? Email { get; set; }        
        public int Status { get; set; } = 1;
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; }        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        [NotMapped] public string SheetName { get; set; } = string.Empty;
        [NotMapped] public string ReportDesc { get; set; } = string.Empty;
    }
}
