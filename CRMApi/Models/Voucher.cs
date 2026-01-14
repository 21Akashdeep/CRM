using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMApi.Models
{
    public class Voucher
    {
        [Key] public int Id { get; set; }
        [StringLength(50)] public string Type { get; set; } = string.Empty;
        [StringLength(16)] public string No { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int? PartyId { get; set; }
        [NotMapped] public int StateId { get; set; }
        [NotMapped] public int StoreId { get; set; }
        [NotMapped] public string? PartyDesc { get; set; }       
        [StringLength(100)] public string? ConName { get; set; }
        [StringLength(100)] public string? ConAdd1 { get; set; }
        [StringLength(100)] public string? ConAdd2 { get; set; }
        [StringLength(6)] public string? ConPincode { get; set; }
        [StringLength(100)] public string? ConPostOffice { get; set; }
        [StringLength(3)] public string? ConStateCode { get; set; }
        [StringLength(100)] public string? ConStateName { get; set; }

        [StringLength(50)] public string? RefNo { get; set; }
        public DateTime? RefDate { get; set; }
        public string? ListVoucherId { get; set; }
        [StringLength(12)] public string? EwayNo { get; set; }
        public DateTime? EwayDate { get; set; }
        [StringLength(100)] public string? Remarks { get; set; }
        [NotMapped] public DateTime FromDate { get; set; }
        [NotMapped] public DateTime ToDate { get; set; }
        public int Status { get; set; } = 1;
        [NotMapped] public string? StatusDesc { get; set; }
        [NotMapped] public string? StatusCss { get; set; }
        public int CreatedBy { get; set; }
        [NotMapped] public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; }
        [NotMapped] public string? UpdatedByName { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public List<VoucherItem> VoucherItem { get; set; } = new List<VoucherItem>();
        [NotMapped] public List<int> ListId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListPartyId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListNo { get; set; } = new List<int>();
        [NotMapped] public List<int> ListStatus { get; set; } = new List<int>();
        [NotMapped] public List<string> ListType { get; set; } = new List<string>();
        [NotMapped] public bool IsEdit { get; set; } = false;
        [NotMapped] public bool IsPrint { get; set; } = false;
        [NotMapped] public bool IsDelete { get; set; } = false;
        [NotMapped] public bool IsEnable { get; set; } = false;
        [NotMapped] public bool IsDuplicate { get; set; } = false;
    }
}
