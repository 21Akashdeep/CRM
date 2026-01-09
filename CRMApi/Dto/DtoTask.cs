using CRMApi.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMApi.Dto
{
    public class DtoTask
    {
        public class DtoTaskAdd 
        {
            public int Id { get; set; }
            [MaxLength(16)] public string? Code { get; set; }
            [MaxLength(100)] public string Name { get; set; } = null!;
            [MaxLength(200)] public string Description { get; set; } = null!;
            public int PartyId { get; set; }
            public string Category { get; set; } = null!;
            [MaxLength(20)] public string? PoNo { get; set; }
            public DateTime? PoDate { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string? DocName { get; set; }
            public string? DocMimeType { get; set; }
            public string? DocBase64 { get; set; }
            [MaxLength(200)] public string Remarks { get; set; } = string.Empty;            
            public List<DtoTaskItemAdd> TaskItem { get; set; } = new List<DtoTaskItemAdd>();
        }
        public class  DtoTaskItemAdd
        {
            public int Id { get; set; } 
            public int TaskId { get; set; } = 1;
            [MaxLength(200)] public string Description { get; set; } = null!;
            public decimal EstimatedDays { get; set; }
            public DateTime? StartDateTime { get; set; }
            public DateTime? EndDateTime { get; set; }
            public string? DocName { get; set; }
            public string? DocMimeType { get; set; }
            public string? DocBase64 { get; set; }
            public List<DtoTask.DtoTaskAssingTo> AssignToList { get; set; } = new List<DtoTask.DtoTaskAssingTo>();
            [MaxLength(200)] public string Remarks { get; set; } = string.Empty;            
        }
        public class DtoTaskList
        {
            public int Id { get; set; }
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int PartyId { get; set; } = 1;
            public string PartyDesc { get; set; } = string.Empty;
            public string Category { get; set; } = null!;
           [NotMapped] public string CategoryDesc { get; set; } = null!;
            public string? PoNo { get; set; }
            public DateTime? PoDate { get; set; }                        
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string? DocName { get; set; }
            public string? DocMimeType { get; set; }
            public string? DocBase64 { get; set; }
            public string Remarks { get; set; } = string.Empty;
            public int Status { get; set; }
            public string StatusDesc { get; set; } = string.Empty;
            public string? StatusCss { get; set; }
            public int CreatedBy { get; set; }
            public string CreatedByName { get; set; } = null!;
            public DateTime CreatedAt { get; set; }
            public int UpdatedBy { get; set; }
            public string UpdatedByName { get; set; } = null!;
            public DateTime UpdatedAt { get; set; }
            public bool IsEdit { get; set; } = false;
            public bool IsDuplicate { get; set; } = true;
            public bool IsDelete {get; set;} = false;
            public bool IsEnable { get; set; } = false;
            public List<DtoTaskItemList> TaskItem { get; set; } = new List<DtoTaskItemList>();
            public List<int> ListTaskId { get; set; } = new List<int>();

            public bool IsAddStatus { get; set; } = false;
            public List<int> ListStatus { get; set; } = new List<int>();
        }
        public class DtoTaskItemList
        {
            public int Id { get; set; }
            public int TaskId { get; set; }
            public string? TaskDesc { get; set; }
            [MaxLength(200)] public string Description { get; set; } = string.Empty;
            public decimal EstimatedDays { get; set; }
            public DateTime? StartDateTime { get; set; }
            public DateTime? EndDateTime { get; set; }
            public string? DocName { get; set; }
            public string? DocMimeType { get; set; }
            public string? DocBase64 { get; set; }
            public List<DtoTaskAssingTo> AssignToList { get; set; } = new List<DtoTaskAssingTo>();                        
            public string Remarks { get; set; } = string.Empty;
            public int Status { get; set; }
            public string StatusDesc { get; set; } = null!;
            public string? StatusCss { get; set; }
            public int CreatedBy { get; set; }
            public string CreatedByName { get; set; } = null!;
            public DateTime CreatedAt { get; set; }
            public int UpdatedBy { get; set; }
            public string UpdatedByName { get; set; } = null!;
            public DateTime UpdatedAt { get; set; }
            public bool IsEdit { get; set; } = false;
            public bool IsDuplicate { get; set; } = true;
            public bool IsDelete { get; set; } = false;
            public bool IsEnable { get; set; } = false;
            public bool IsAddStatus { get; set; } = false;
        }
        public class DtoTaskAssingTo 
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
        }        
        public class DtoTaskFltr 
        {
            public int Id { get; set; }
            public List<int> ListId { get; set; } = new List<int>();
            public List<int> ListStatus { get; set; } = new List<int>();
            public List<int> ListPartyId { get; set; } = new List<int>();
            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }

        }
        public class DtoTaskItemFltr
        {
            public int Id { get; set; }
            public List<int> ListId { get; set; } = new List<int>();
            public List<int> ListStatus { get; set; } = new List<int>();
            public List<int> ListTaskId { get; set; } = new List<int>();
            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }

        }

        public class DtoTaskActionFilter
        {
            public List<int> ListStatus { get; set; } = new();
            public List<int> ListTaskId { get; set; } = new();
            public List<int> ListTaskItemId { get; set; } = new();
        }
    }
}
