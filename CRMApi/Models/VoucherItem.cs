using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMApi.Models
{
    public class VoucherItem
    {

        [Key]
        public int Id { get; set; }
        // Foreign Keys
        public int VoucherId { get; set; }
        public int ItemId { get; set; }
        public int StoreId { get; set; }

        [NotMapped] public string? VoucherDesc { get; set; }
        

        // Item Details
        [MaxLength(50)]
        public string? SerialNo { get; set; }

        [MaxLength(20)]
        public string? BatchNo { get; set; }

        public DateTime? ExpiryOn { get; set; }

        // Quantity & Rate
        [Column(TypeName = "decimal(18,6)")]
        public decimal Qty { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal Rate { get; set; } = 0;

        [Column(TypeName = "decimal(18,6)")]
        public decimal Amount { get; set; } = 0;

        // Discount
        [Column(TypeName = "decimal(18,6)")]
        public decimal DiscountRate { get; set; } = 0;

        [Column(TypeName = "decimal(18,6)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,6)")]
        public decimal TotalAmount { get; set; } = 0;

        // Tax
        public string ListTax { get; set; } = "[]";

        [Column(TypeName = "decimal(6,3)")]
        public decimal TaxRate { get; set; } = 0;

        [Column(TypeName = "decimal(18,6)")]
        public decimal TaxAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,6)")]
        public decimal GrossAmount { get; set; } = 0;
        [MaxLength(100)] public string? ImageUrl { get; set; }
        public string? ReasonCode { get; set; }
        [NotMapped] public string? ReasonDesc { get; set; }
        [MaxLength(200)] public string? Remarks { get; set; }
        public bool IsScanned { get; set; }
        public string StockType { get; set; } = "N";
        public bool IsReturnable { get; set; }
        public bool IsReturned { get; set; }
        public int Status { get; set; } = 1;
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        [NotMapped] public string? VoucherNo { get; set; }

        [NotMapped] public string? ItemDesc { get; set; }
        [NotMapped] public int? SourceVoucherId { get; set; } 

        [NotMapped] public string StatusDesc { get; set; } = string.Empty;
        [NotMapped] public string Itemgroup { get; set; } = string.Empty;
        [NotMapped] public string ItemSubGroup { get; set; } = string.Empty;
        [NotMapped] public string StatusCss { get; set; } = string.Empty;

        [NotMapped]
        public string? StoreDesc { get; set; }
        [NotMapped]
        public string? UnitDesc { get; set; }

        [NotMapped]
        public string CreatedByName { get; set; } = string.Empty;

        [NotMapped]
        public string UpdatedByName { get; set; } = string.Empty;

        [NotMapped]
        public bool IsEdit { get; set; } = false;

        [NotMapped]
        public bool IsDelete { get; set; } = false;

        [NotMapped]
        public DateTime FromDate { get; set; }

        [NotMapped] public bool IsEnable { get; set; } = false;
        [NotMapped] public bool IsDuplicate { get; set; } = false;

        [NotMapped]
        public DateTime ToDate { get; set; }

        [NotMapped]
        public List<int> ListStatus { get; set; } = new List<int>();

        [NotMapped]
        public List<int> ListId { get; set; } = new List<int>();

        [NotMapped]
        public List<int> ListVoucherId { get; set; } = new List<int>();

        [NotMapped]
        public List<int> ListStoreId { get; set; } = new List<int>();

       

        [NotMapped]
        public List<string> ListReasonCode { get; set; } = new List<string>();




    }
}
