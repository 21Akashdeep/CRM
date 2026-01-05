using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMApi.Models
{
    public class TaskAction
    {

        public int Id { get; set; }
        public int TaskId { get; set; }
        public int TaskItemId { get; set; }
        [MaxLength(200)] public string Remarks { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        [Range(0, 1)]
        public int NotifyToOwner { get; set; }
        public int Status { get; set; } = 1;
        [NotMapped] public string StatusDesc { get; set; } = string.Empty;
        [NotMapped] public string? StatusCss { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        [NotMapped] public string UpdatedByName { get; set; } = string.Empty;
        [NotMapped] public string CreatedByName { get; set; } = string.Empty;
        [NotMapped] public List<int> ListId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListTaskId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListTaskItemId { get; set; } = new List<int>();
        [NotMapped] public List<int> ListStatus { get; set; } = new List<int>();
        [NotMapped] public bool IsDelete { get; set; } = false;
    }
}
