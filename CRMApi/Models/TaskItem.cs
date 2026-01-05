using CRMApi.Dto;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMApi.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public int TaskId { get; set; } = 1;
        [MaxLength(200)] public string Description { get; set; } = string.Empty;
        public decimal EstimatedDays { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string? TechnicalDoc { get; set; }
        public List<DtoTask.DtoTaskAssingTo> AssignToList { get; set; } = new List<DtoTask.DtoTaskAssingTo>();
        [MaxLength(200)] public string Remarks { get; set; } = string.Empty;
        public int Status { get; set; } = 1;        
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;        
    }
}
