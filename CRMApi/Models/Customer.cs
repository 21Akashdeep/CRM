using System.ComponentModel.DataAnnotations.Schema;

namespace CRMApi.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int LocationId { get; set; }
        [NotMapped] public string LocationDesc { get; set; } = string.Empty;
        public string? CinNo { get; set; }
        public string GstNo { get; set; } = "URP";
        public string? PanNo { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? PinCode { get; set; }
        public string? PostOffice { get; set; }
        public string? District { get; set; }
        public int AdminDivId { get; set; }
        [NotMapped]public string? AdminDivDesc { get; set; }
        public int CountryId { get; set; }
        [NotMapped] public string? CountryDesc { get; set; }
        public string? ContactNo { get; set; }
        public string? Email { get; set; }
        public string? AccountNo { get; set; }
        public string? IfscCode { get; set; }
        public string? BankName { get; set; }
        public string? BankAddress { get; set; }        
        public int Status { get; set; } = 1;
        [NotMapped] public string StatusName { get; set; } = string.Empty;
        [NotMapped] public string StatusCss { get; set; } = string.Empty;
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
