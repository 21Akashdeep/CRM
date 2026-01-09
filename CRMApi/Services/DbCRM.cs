using CRMApi.Dto;
using CRMApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using Task = CRMApi.Models.Task;



namespace CRMApi.Services
{
    public class DBCRM : DbContext
    {
        public DBCRM(DbContextOptions<DBCRM> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);

            //Json Convert Approval Seq for Approval Config
            var jsonApprovalSeq = new ValueConverter<List<Composite.ApprovalSeq>, string>(
                v => JsonConvert.SerializeObject(v),//Serialize List<CustomObject> to JSON string
                v => JsonConvert.DeserializeObject<List<Composite.ApprovalSeq>>(v)! //Deserialize JSON string back to List<CustomObject>
            );
            modelBuilder.Entity<ApprovalConfig>().Property(e => e.ApprovalSeq).HasConversion(jsonApprovalSeq).HasColumnType("json");
            //Task Item Assing To
            var jsonTaskItemAssingTo = new ValueConverter<List<DtoTask.DtoTaskAssingTo>, string>(
                v => JsonConvert.SerializeObject(v),//Serialize List<CustomObject> to JSON string
                v => JsonConvert.DeserializeObject<List<DtoTask.DtoTaskAssingTo>>(v)! //Deserialize JSON string back to List<CustomObject>
            );
            modelBuilder.Entity<TaskItem>().Property(e => e.AssignToList).HasConversion(jsonTaskItemAssingTo).HasColumnType("json");
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
        public DbSet<UserApi> UserApi { get; set; }
        public DbSet<Location> Location { get; set; }
        public DbSet<UserLocation> UserLocation { get; set; }
        public DbSet<ApprovalRole> ApprovalRole { get; set; }
        public DbSet<UserApprovalRole> UserApprovalRole { get; set; }
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
        public DbSet<ComplaintAssign> ComplaintAssign { get; set; }
        public DbSet<ComplaintStatus> ComplaintStatus { get; set; }
        public DbSet<ComplaintItem> ComplaintItem { get; set; }
        public DbSet<Otp> Otp { get; set; }
        public DbSet<Project> Project { get; set; }
        public DbSet<ProjectDeadLineLog> ProjectDeadLineLog { get; set; }
        public DbSet<Qualification> Qualification { get; set; }
        public DbSet<Employee> Employee { get; set; }
        public DbSet<GatePass> GatePass { get; set; }
        public DbSet<ProjectModule> ProjectModule { get; set; }
        public DbSet<Task> Task { get; set; }
        public DbSet<TaskItem> TaskItem { get; set; }
        public DbSet<TaskAction> TaskAction { get; set; }
        public DbSet<Voucher> Voucher { get; set; }
        public DbSet<VoucherItem> VoucherItem { get; set; }
        public DbSet<Store> Store { get; set; }

    }


    
}
