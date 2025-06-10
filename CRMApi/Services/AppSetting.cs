namespace CRMApi.Services
{
    public class AppSetting
    {
        public static AppSetting Init()
        {
            return new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build().GetSection("AppSetting").Get<AppSetting>()!;
        }
        public ConnectionStrings ConnectionStrings { get; set; } = new ConnectionStrings();
        public string DbName { get; set; } = string.Empty;
        public string CrosOrigin { get; set; } = string.Empty;        
        public AppJwt Jwt { get; set; } = new AppJwt();
        public SettingName SettingName { get; set; } = new SettingName();
        public Status Status { get; set; } = new Status();
        public List<int> AllActiveStatus { get; set; } = new List<int>();
        public List<int> ActiveStatus { get; set; } = new List<int>();
        public List<int> UnderProcess { get; set; } = new List<int>();
        public List<int> AllInActiveStatus { get; set; } = new List<int>();
        public List<int> InActiveStatus { get; set; } = new List<int>();
        public List<int> ApprovalStatus { get; set; } = new List<int>();
        public UserType UserType { get; set; } = new UserType();
        public List<string> ByPassUserType { get; set; } = new List<string>();
        public ApiType ApiType { get; set; } = new ApiType();
        public ApiName ApiName { get; set; } = new ApiName();
        public AprvRole AprvRole { get; set; } = new AprvRole();
        public Regexp Regexp { get; set; } = new Regexp();
        public RefType RefType { get; set; } = new RefType();

    }
    public class ConnectionStrings
    {
        public string? MySql { get; set; }
        public string? Oracle { get; set; }
        public string? SqlServer { get; set; }
    }    
    public class AppJwt
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string TokenExpireTimeInMinutes { get; set; } = string.Empty;
    }
    public class SettingName
    {
        public string Status { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public string AdminDivType { get; set; } = string.Empty;
        public string PostalType { get; set; } = string.Empty;
        public string ApiType { get; set; } = string.Empty;
        public string UQCG { get; set; } = string.Empty;
        public string SupplyType { get; set; } = string.Empty;
        public string SupportMode { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Returnable { get; set; } = string.Empty;
    }
    public class Status
    {
        public int AutoReject { get; set; }
        public int Reject { get; set; }
        public int Cancel { get; set; }
        public int ItemDelete { get; set; }
        public int Delete { get; set; }
        public int Enable { get; set; }
        public int Approved { get; set; }
        public int AutoApproved { get; set; }
        public int Completed { get; set; }
        public int SaveAsDraft { get; set; }        
        public int Pending { get; set; }
        public int Scheduled { get; set; }
        public int WaitingForApproval { get; set; }
        public int Processing { get; set; }        
        public int PartialCompleted { get; set; }
        public int Verified { get; set; }
        public int UnVerified { get; set; }
    }
    public class Request
    {
        public HttpRequest? HttpRequest { get; set; }
        
        public ActionType ActionType { get; set; }
    }
    public class Action
    {
        public string View { get; set; } = "View";
        public string Add { get; set; } = "Add";
        public string Update { get; set; } = "Update";
        public string Cancel { get; set; } = "Cancel";
        public string Delete { get; set; } = "Delete";
        public string Enable { get; set; } = "Enable";
        public string Print { get; set; } = "Print";
        public string Export { get; set; } = "Export";
    }
    public enum ActionType
    {
        All = 0,
        View = 1,
        Add = 2,
        Update = 3,
        Delete = 4,
        Enable = 5,
        Duplicate = 6,
        Print = 7,
        Import = 8,
        Export = 9,
        Option = 10,
        UnAuthorised = 11
    }
    public class UserType
    {
        public string SysAdmin { get; set; } = null!;
        public string Admin { get; set; } = null!;
        public string Employee { get; set; } = null!;
        public string ContractWorker { get; set; } = null!;
        public string Vendor { get; set; } = null!;
        public string Customer { get; set; } = null!;
    }
    public class ApiType
    {
        public string WebApp { get; set; } = null!;
        public string RestApi { get; set; } = null!;
        public string Android { get; set; } = null!;        
    }
    public class ApiName
    {
        public string OutPass { get; set; } = string.Empty;
        public string InPass { get; set; } = string.Empty;
    }
    public class AprvRole 
    {
        public string SupportTeam { get; set; } = null!;
        public string Manager { get; set; } = null!;
        public string Supervisor { get; set; } = null!;
    }
    public class Regexp
    {
        public string Alpha { get; set; } = null!;
        public string AlphaLg { get; set; } = null!;
        public string AlphaSm { get; set; } = null!;
        public string Num { get; set; } = null!;
        public string AlphaNum { get; set; } = null!;
        public string AlphaLgNum { get; set; } = null!;
        public string AlphaSmNum { get; set; } = null!;
    }
    public class RefType
    {
        public string Complaint { get; set; } = string.Empty;
    }
    public class ControllerInfo
    {
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? ReturnType { get; set; }
        public string? Attributes { get; set; }
    }
    
}
