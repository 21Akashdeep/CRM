namespace CRMApi.Dto
{
    public class StockItemFltrDto
    {        
        public List<int> ListNotContainId { get; set; } = new List<int>();                
        public DateTime ToDate { get; set; }
        public int StoreId { get; set; }
        public List<int> ListStoreId { get; set; } = new List<int>();        
        public List<int> ListStatus { get; set; } = new List<int>();
    }
}
