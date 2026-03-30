namespace CRMApi.Dto
{
    public class StockItemFltrDto
    {                            
        public List<int> ListExcludeViId { get; set; } = new List<int>();                
        public DateTime ToDate { get; set; }
        public int StoreId { get; set; }
        public List<int> ListStoreId { get; set; } = new List<int>();        
        public List<int> ListItemId { get; set; } = new List<int>();        
        public List<string?> ListSerialNo { get; set; } = new List<string?>();
        public List<string> ListStockType { get; set; } = new List<string>();
        public List<int> ListStatus { get; set; } = new List<int>();
    }
}
