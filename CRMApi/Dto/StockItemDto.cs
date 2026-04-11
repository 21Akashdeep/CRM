using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMApi.Dto
{
    public class StockItemDto
    {        
        public int ItemId { get; set; }
        public string? ItemDesc { get; set; }
        public string? ItemSubDesc { get; set; }
        public string? UnitDesc { get; set; }
        //public int StoreId { get; set; }
        //public string? StoreDesc { get; set; }        
        public string? SerialNo { get; set; }
        public DateTime? ExpiryOn { get; set; }
        public decimal Qty { get; set; }
        public decimal BaseQty { get; set; }
        public string? StockType { get; set; }
        public int StoreId { get; set; }
        public bool IsReturnable { get; set; }
        public bool IsReturned { get; set; }
    }
}
