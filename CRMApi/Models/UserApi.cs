using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMApi.Models
{
    public class UserApi
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [NotMapped] public string UserName { get; set; } = string.Empty;
        public int ApiId { get; set; }
        [NotMapped] public string ApiDesc { get; set; } = string.Empty;
        [NotMapped] public string ApiTypeDesc { get; set; } = string.Empty;
        [NotMapped] public string ApiGroupDesc { get; set; } = string.Empty;
        public bool View { get; set; }
        public bool Add { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }
        public bool Enable { get; set; }
        public bool Print { get; set; }
        public bool Import { get; set; }
        public bool Export { get; set; }
        public int Status { get; set; } = 1;        
        public int CreatedBy { get; set; }        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; }        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        [NotMapped] public bool IsAdded { get; set; }
    }
}
