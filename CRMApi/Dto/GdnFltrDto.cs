namespace CRMApi.Dto
{
    public class GdnFltrDto
    {
        public List<int> ListId { get; set; } = new List<int>();    
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }        
        public List<int> ListPartyId { get; set; } = new List<int>();
        public List<int> ListStatus { get; set; } = new List<int>();
    }
}
