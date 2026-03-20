namespace CRMApi.Dto
{
    public class VoucherFltrDto
    {
        public List<int> ListId { get; set; } = new List<int>();
        public List<int> ListNotContainId { get; set; } = new List<int>();
        public List<string> ListType { get; set; } = new List<string>();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<int> ListStoreId { get; set; } = new List<int>();
        public List<int> ListPartyId { get; set; } = new List<int>();
        public List<int> ListStatus { get; set; } = new List<int>();
    }
}
