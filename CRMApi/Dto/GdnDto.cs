using CRMApi.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMApi.Dto
{
    public class GdnDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string No { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int StoreId { get; set; }
        public int? PartyId { get; set; }
        [StringLength(50)] public string? ChallanNo { get; set; }
        public DateTime? ChallanDate { get; set; }
        [StringLength(50)] public string? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        [StringLength(200)] public string? PartyDesc { get; set; }
        [StringLength(100)] public string? ConName { get; set; }
        [StringLength(100)] public string? ConAdd1 { get; set; }
        [StringLength(100)] public string? ConAdd2 { get; set; }
        [StringLength(6)] public string? ConPincode { get; set; }
        [StringLength(100)] public string? ConPostOffice { get; set; }
        [StringLength(3)] public string? ConStateCode { get; set; }
        [StringLength(100)] public string? ConStateName { get; set; }        
        [StringLength(50)] public string? PoNo { get; set; }
        public DateTime? PoDate { get; set; }        
        [StringLength(12)] public string? EwayNo { get; set; }
        public DateTime? EwayDate { get; set; }
        [StringLength(100)] public string? Remarks { get; set; }        
        public int Status { get; set; } = 1;
        public string? StatusDesc { get; set; }
        public string? StoreDesc { get; set; }
        public string? StatusCss { get; set; }
        public int CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; }
        public string? UpdatedByName { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public List<GdnItemDto> GdnItem { get; set; } = new List<GdnItemDto>();

        public string? ListVoucherId { get; set; }

        public bool IsEdit { get; set; } = false;
        public bool IsPrint { get; set; } = false;
        public bool IsDelete { get; set; } = false;
        public bool IsEnable { get; set; } = false;
        public bool IsDuplicate { get; set; } = false;
    }
}
