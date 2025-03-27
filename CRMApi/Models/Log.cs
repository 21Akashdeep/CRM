using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace CRMApi.Models
{
    public class Log
    {
        public int Id { get; set; }
        [MaxLength(10)] public string Action { get; set; } = String.Empty;
        [MaxLength(100)] public string RefType { get; set; } = String.Empty;
        public int RefId { get; set; }
        public string Data { get; set; } = "[]"; 
        public int CompanyId { get; set; } = 1;
        [NotMapped] public string CompanyDesc { get; set; } = string.Empty;
        public int Status { get; set; } = 1;
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;        
    }
}
