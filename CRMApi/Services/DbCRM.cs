using CRMApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;



namespace CRMApi.Services
{
    public class DbCRM:DbContext
    {
        public DbCRM(DbContextOptions<DbCRM> options): base(options) 
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);

            //Json Convert User Company Permisssion
            var jsonUserCompany = new ValueConverter<List<Composite.UserCompany>, string>(
                v => JsonConvert.SerializeObject(v),//Serialize List<CustomObject> to JSON string
                v => JsonConvert.DeserializeObject<List<Composite.UserCompany>>(v)! //Deserialize JSON string back to List<CustomObject>
            );            
            modelBuilder.Entity<User>().Property(e => e.Company).HasConversion(jsonUserCompany).HasColumnType("json");

            //Json Convert User Api Permisssion
            var jsonUserApi = new ValueConverter<List<Composite.UserApi>, string>(
                v => JsonConvert.SerializeObject(v),//Serialize List<CustomObject> to JSON string
                v => JsonConvert.DeserializeObject<List<Composite.UserApi>>(v)! //Deserialize JSON string back to List<CustomObject>
            );            
            modelBuilder.Entity<User>().Property(e => e.Api).HasConversion(jsonUserApi).HasColumnType("json");

            //Json Convert User Approval Role
            var jsonUserApprovalRole = new ValueConverter<List<Composite.UserApprovalRole>, string>(
                v => JsonConvert.SerializeObject(v),//Serialize List<CustomObject> to JSON string
                v => JsonConvert.DeserializeObject<List<Composite.UserApprovalRole>>(v)! //Deserialize JSON string back to List<CustomObject>
            );            
            modelBuilder.Entity<User>().Property(e => e.ApprovalRole).HasConversion(jsonUserApprovalRole).HasColumnType("json");

            //Json Convert Approval Seq for Approval Config
            var jsonApprovalSeq = new ValueConverter<List<Composite.ApprovalSeq>, string>(
                v => JsonConvert.SerializeObject(v),//Serialize List<CustomObject> to JSON string
                v => JsonConvert.DeserializeObject<List<Composite.ApprovalSeq>>(v)! //Deserialize JSON string back to List<CustomObject>
            );            
            modelBuilder.Entity<ApprovalConfig>().Property(e => e.ApprovalSeq).HasConversion(jsonApprovalSeq).HasColumnType("json");

            //Json Convert Approval Seq for Approval Config
            var jsonComplaintAssignTo = new ValueConverter<List<Composite.ComplaintAssignTo>, string>(
                v => JsonConvert.SerializeObject(v),//Serialize List<CustomObject> to JSON string
                v => JsonConvert.DeserializeObject<List<Composite.ComplaintAssignTo>>(v)! //Deserialize JSON string back to List<CustomObject>
            );
            modelBuilder.Entity<ComplaintSchedule>().Property(e => e.AssignTo).HasConversion(jsonComplaintAssignTo).HasColumnType("json");
        }
        public DbSet<User> User { get; set; }
        public DbSet<Setting> Setting { get; set; }
        public DbSet<MailService> MailService { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<Designation> Designation { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<Zone> Zone { get; set; }
        public DbSet<AdminDiv> AdminDiv { get; set; }
        public DbSet<Company> Company { get; set; }
        public DbSet<Log> Log { get; set; }
        public DbSet<ApiGroup> ApiGroup { get; set; }
        public DbSet<Api> Api { get; set; }
        public DbSet<ApprovalRole> ApprovalRole { get; set; }
        public DbSet<ApprovalConfig> ApprovalConfig { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Approval> Approval { get; set; }
        public DbSet<AccountGroup> AccountGroup { get; set; }
        public DbSet<Godown> Godown { get; set; }
        public DbSet<Unit> Unit { get; set; }
        public DbSet<ItemGroup> ItemGroup { get; set; }
        public DbSet<ItemSubGroup> ItemSubGroup { get; set; }
        public DbSet<Item> Item { get; set; }
        public DbSet<Complaint> Complaint { get; set; }
        public DbSet<ComplaintSchedule> ComplaintSchedule { get; set; }
        public DbSet<ComplaintSchedule> ComplaintStatus { get; set; }
    }
}
