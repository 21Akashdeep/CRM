using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CRMApi.Models
{
    public class Voucher
    {
        [Key] public int Id { get; set; }
        [StringLength(16)] public string No { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime RefDate { get; set; }
        public DateTime EwayDate { get; set; }
        public int CustomerId { get; set; }
        //public int VoucherId { get; set; }
        [StringLength(100)] public string ConName { get; set; } = string.Empty;
        [StringLength(100)] public string ConAdd1 { get; set; } = string.Empty;
        [StringLength(100)] public string ConAdd2 { get; set; } = string.Empty;
        [StringLength(6)] public string ConPinCode { get; set; } = string.Empty;
        [StringLength(100)] public string ConPostOffice { get; set; } = string.Empty;
        [StringLength(3)] public string ConStateCode { get; set; } = string.Empty;
        [StringLength(100)] public string ConStateName { get; set; } = string.Empty;
        [StringLength(50)] public string RefNo { get; set; } = string.Empty;
        [StringLength(12)] public string EwayNo { get; set; } = string.Empty;
        
        [StringLength(100)] public string? Remarks { get; set; }
        //public decimal NetAmount { get; set; }
        public int Status { get; set; } = 1;
        [NotMapped] public string? StatusDesc { get; set; }
        [NotMapped] public string? StatusCss { get; set; }
        public int CreatedBy { get; set; }
        [NotMapped] public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; }
        [NotMapped] public string? UpdatedByName { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public List<Item> Item { get; set; } = new List<Item>();
        [NotMapped] public DateTime FromDate { get; set; }
        [NotMapped] public DateTime ToDate { get; set; }
        [NotMapped] public List<int> ListId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListPartyId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListShiftId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListStatus { get; set; } = new List<int>();
        [NotMapped] public bool IsEdit { get; set; } = false;
        [NotMapped] public bool IsPrint { get; set; } = false;
        [NotMapped] public bool IsDelete { get; set; } = false;
    }
}
