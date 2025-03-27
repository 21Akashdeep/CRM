namespace CRMApi.Models
{
    public class Composite
    {
        //User Permission
        public class UserCompany
        {            
            public int CompanyId { get; set; }
            public string CompanyName { get; set; } = string.Empty;
            public string CompanyDesc { get; set; } = string.Empty;
            public bool IsDefault { get; set; }            
        }
        public class UserApi
        {                        
            public int ApiId { get; set; }
            public string ApiName { get; set; } = string.Empty;
            public string ApiDesc { get; set; } = string.Empty;
            public bool View { get; set; }
            public bool Add { get; set; }
            public bool Update { get; set; }
            public bool Delete { get; set; }
            public bool Enable { get; set; }
            public bool Print { get; set; }
            public bool Import { get; set; }
            public bool Export { get; set; }            
        }
        public class UserApprovalRole
        {            
            public int ApprovalRoleId { get; set; }
            public string ApprovalRoleName { get; set; } = String.Empty;
            public string ApprovalRoleDesc { get; set; } = String.Empty;            
        }
        public class ApprovalSeq
        {
            public int ApprovalRoleId { get; set; }
            public int SeqNo { get; set; }
        }
    }
}


