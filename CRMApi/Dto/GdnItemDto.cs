using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMApi.Dto
{
    public class GdnItemDto
    {
        public int Id { get; set; }
        public int VoucherId { get; set; }
        public int ItemId { get; set; }
        public string? ItemDesc { get; set; }
        public string? ItemSubDesc { get; set; }
        public string? UnitDesc { get; set; }
        public int StoreId { get; set; }
        public string? StoreDesc { get; set; }               
        public string? SerialNo { get; set; }
        public DateTime? ExpiryOn { get; set; }
        public decimal Qty { get; set; }
        public string? Remarks { get; set; }
        public string? BatchNo { get; set; }
        [NotMapped]
        public string? VoucherDesc { get; set; }
        public bool IsScanned { get; set; }
        public int Status { get; set; }
        public string? StatusDesc { get; set; }
        public string? StatusCss { get; set; }
        public int CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; }
        public string? UpdatedByName { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;        
        public bool IsEdit { get; set; } = false;
        public bool IsDelete { get; set; } = false;        
    }
}
