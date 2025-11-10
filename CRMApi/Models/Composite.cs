using System.ComponentModel.DataAnnotations.Schema;

namespace CRMApi.Models
{
    public class Composite
    {                        
        public class ApprovalSeq
        {
            public int ApprovalRoleId { get; set; }
            public int SeqNo { get; set; }
        }
        public class AddressDesc 
        {
            public string? Add1 { get; set; }
            public string? Add2 { get; set; }
            public string? PinCode { get; set; }
            public string? PostOffice { get; set; }
            public string? District { get; set; }
            public string? State { get; set; }
            public string? StateCode { get; set; }
            public string? Country { get; set; }
            public string? OtherText { get; set; }
        }
        public class ComplaintAssignTo
        {
            public int Id { get; set; }
            public string Name { get; set; } = String.Empty;
        }
        public class Document
        {
            public byte[] Base64 { get; set; } = Array.Empty<byte>();
            public string Base64String { get; set; } = String.Empty;
            public string MimeType { get; set; } = String.Empty;
            public string FilePath { get; set; } = String.Empty;
            public string FileName { get; set; } = String.Empty;
        }
    }
}


