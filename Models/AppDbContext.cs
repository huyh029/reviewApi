using Microsoft.EntityFrameworkCore;

namespace reviewApi.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<EvaluationFlow> EvaluationFlows { get; set; }
        public DbSet<EvaluationFlowRole> EvaluationFlowsRoles { get; set; }
        public DbSet<EvaluationFlowDepartment> EvaluationFlowsDepartments { get; set; }
        public DbSet<EvaluationObject> EvaluationObjects { get; set; }
        public DbSet<EvaluationObjectRole> EvaluationObjectRoles { get; set; }
        public DbSet<CriteriaSet> CriteriaSets { get; set; }
        public DbSet<CriteriaSetObject> CriteriaSetObjects { get; set; }
        public DbSet<Criteria> Criterias { get; set; }
        public DbSet<Classification> Classifications { get; set; }
        public DbSet<CriteriaSetPeriod> CriteriaSetPeriods { get; set; }
        public DbSet<Evaluation> Evaluations { get; set; }
        public DbSet<EvaluationChat> EvaluationChats { get; set; }
        public DbSet<EvaluationChatFile> EvaluationChatFiles { get; set; }
        public DbSet<EvaluationScore> EvaluationScores { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // === Khai báo Key ===
            modelBuilder.Entity<User>().HasKey(e => e.Id);
            modelBuilder.Entity<User>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Department>().HasKey(e => e.Code);
            modelBuilder.Entity<Role>().HasKey(e => e.Code);
            modelBuilder.Entity<EvaluationFlow>().HasKey(e => e.Code);
            modelBuilder.Entity<EvaluationFlowRole>().HasKey(e => e.Id);
            modelBuilder.Entity<EvaluationFlowRole>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<EvaluationFlowDepartment>().HasKey(e => e.Id);
            modelBuilder.Entity<EvaluationFlowDepartment>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<EvaluationObject>().HasKey(e => e.Code);
            modelBuilder.Entity<EvaluationObjectRole>().HasKey(e => e.Id);
            modelBuilder.Entity<EvaluationObjectRole>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<CriteriaSet>().HasKey(e => e.Id);
            modelBuilder.Entity<CriteriaSet>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<CriteriaSetObject>().HasKey(e => e.Id);
            modelBuilder.Entity<CriteriaSetObject>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Criteria>().HasKey(e => e.Id);
            modelBuilder.Entity<Criteria>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Classification>().HasKey(e => e.Id);
            modelBuilder.Entity<Classification>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<CriteriaSetPeriod>().HasKey(e => e.Id);
            modelBuilder.Entity<CriteriaSetPeriod>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Evaluation>().HasKey(e => e.Id);
            modelBuilder.Entity<Evaluation>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<EvaluationChat>().HasKey(e => e.Id);
            modelBuilder.Entity<EvaluationChat>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<EvaluationChatFile>().HasKey(e => e.Id);
            modelBuilder.Entity<EvaluationChatFile>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<EvaluationScore>().HasKey(e => e.Id);
            modelBuilder.Entity<EvaluationScore>().Property(e => e.Id).ValueGeneratedOnAdd();
            // === Khai báo Quan hệ ===

            //user - role
            modelBuilder.Entity<User>()
                .HasOne(i => i.Role)
                .WithMany(f => f.Users)
                .HasForeignKey(i => i.RoleCode);

            //user - department
            modelBuilder.Entity<User>()
                .HasOne(i => i.Department)
                .WithMany(f => f.Users)
                .HasForeignKey(i => i.DepartmentCode);
            // department - department (parent - child)
            modelBuilder.Entity<Department>()
                .HasOne(i => i.Parent)
                .WithMany(f => f.Children)
                .HasForeignKey(i => i.ParentCode);
            // EvaluationFlow - EvaluationFlowRole
            modelBuilder.Entity<EvaluationFlowRole>()
                .HasOne(i => i.EvaluationFlow)
                .WithMany(f => f.EvaluationFlowRoles)
                .HasForeignKey(i => i.EvaluationFlowCode);
            // EvaluationFlow - EvaluationFlowDepartment
            modelBuilder.Entity<EvaluationFlowDepartment>()
                .HasOne(i => i.EvaluationFlow)
                .WithMany(f => f.EvaluationFlowDepartments)
                .HasForeignKey(i => i.EvaluationFlowCode);
            //EvaluationFlowDepartment - EvaluationFlowDepartment (Parent - Child)
            modelBuilder.Entity<EvaluationFlowRole>()
                .HasOne(i => i.Parent)
                .WithMany(f => f.Child)
                .HasForeignKey(i => i.ParentId);
            // EvaluationFlowRole - Role
            modelBuilder.Entity<EvaluationFlowRole>()
                .HasOne(i => i.Role)
                .WithMany(f => f.EvaluationFlowRoles)
                .HasForeignKey(i => i.RoleCode);
            // EvaluationFlowDepartment - Department
            modelBuilder.Entity<EvaluationFlowDepartment>()
                .HasOne(i => i.Department)
                .WithMany(f => f.EvaluationFlowDepartments)
                .HasForeignKey(i => i.DepartmentCode);
            // EvaluationObject - EvaluationObjectRole
            modelBuilder.Entity<EvaluationObjectRole>()
               .HasOne(i => i.EvaluationObject)
                .WithMany(f => f.EvaluationObjectRoles)
                .HasForeignKey(i => i.EvaluationObjectCode);
            // EvaluationObjectRole - User
            modelBuilder.Entity<EvaluationObjectRole>()
               .HasOne(i => i.User)
                .WithMany(f => f.EvaluationObjectRoles)
                .HasForeignKey(i => i.UserId);
            // CriteriaSet - CriteriaSetObject
            modelBuilder.Entity<CriteriaSetObject>()
               .HasOne(i => i.CriteriaSet)
                .WithMany(f => f.CriteriaSetObjects)
                .HasForeignKey(i => i.CriteriaSetId);
            // CriteriaSetObject - EvaluationObject
            modelBuilder.Entity<CriteriaSetObject>()
               .HasOne(i => i.EvaluationObject)
                .WithMany(f => f.CriteriaSetObjects)
                .HasForeignKey(i => i.EvaluationObjectCode);
            // CriteriaSet - CriteriaSetPeriod
            modelBuilder.Entity<CriteriaSetPeriod>()
               .HasOne(i => i.CriteriaSet)
                .WithMany(f => f.CriteriaSetPeriods)
                .HasForeignKey(i => i.CriteriaSetId);
            // CriteriaSet - Criteria
            modelBuilder.Entity<Criteria>()
               .HasOne(i => i.CriteriaSet)
                .WithMany(f => f.Criterias)
                .HasForeignKey(i => i.CriteriaSetId);
            // Criteria - Criteria (Parent - Child)
            modelBuilder.Entity<Criteria>()
                .HasOne(i => i.Parent)
                .WithMany(f => f.Children)
                .HasForeignKey(i => i.parentId);
            // CriteriaSet - Classification
            modelBuilder.Entity<Classification>()
               .HasOne(i => i.CriteriaSet)
                .WithMany(f => f.Classifications)
                .HasForeignKey(i => i.CriteriaSetId);
            // Evaluation - User
            modelBuilder.Entity<Evaluation>()
               .HasOne(i => i.User)
                .WithMany(f => f.Evaluations)
                .HasForeignKey(i => i.UserId);
            // Evaluation - CriteriaSet
            modelBuilder.Entity<Evaluation>()
               .HasOne(i => i.CriteriaSet)
                .WithMany(f => f.Evaluations)
                .HasForeignKey(i => i.CriteriaSetId);
            // EvaluationChat - Evaluation
            modelBuilder.Entity<EvaluationChat>()
               .HasOne(i => i.Evaluation)
                .WithMany(f => f.EvaluationChats)
                .HasForeignKey(i => i.EvaluationId);
            // EvaluationChat - User
            modelBuilder.Entity<EvaluationChat>()
               .HasOne(i => i.Sender)
                .WithMany(f => f.EvaluationChats)
                .HasForeignKey(i => i.SenderId);
            // EvaluationChatFile - EvaluationChat
            modelBuilder.Entity<EvaluationChatFile>()
               .HasOne(i => i.EvaluationChat)
                .WithMany(f => f.EvaluationChatFiles)
                .HasForeignKey(i => i.EvaluationChatId);
            // EvaluationScore - Evaluation
            modelBuilder.Entity<EvaluationScore>()
               .HasOne(i => i.Evaluation)
                .WithMany(f => f.EvaluationScores)
                .HasForeignKey(i => i.EvaluationId);
            // EvaluationScore - Criteria
            modelBuilder.Entity<EvaluationScore>()
               .HasOne(i => i.Criteria)
                .WithMany(f => f.EvaluationScores)
                .HasForeignKey(i => i.CriteriaId);
        }
    }
}
