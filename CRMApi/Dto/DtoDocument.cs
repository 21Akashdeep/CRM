namespace CRMApi.Dto
{
    public class DtoDocument
    {
        public class DtoDocAdd
        {
            public string? FileName { get; set; } = string.Empty;
            public byte[] Base64 { get; set; } = Array.Empty<byte>();
            public string? MimeType { get; set; } = string.Empty;            
            public string FilePath { get; set; } = string.Empty;
        }
    }
}
