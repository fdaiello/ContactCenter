using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using DBConverter.Models.data.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ContactCenter.Core.Models;

namespace ContactCenter.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public string CurrentUserId { get; set; }
        public DbSet<ChattingLog> ChattingLogs { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<ExternalAccount> ExternalAccounts { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<WeekSchedule> WeekSchedules { get; set; }
        public DbSet<ChatChannel> ChatChannels { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Stage> Stages { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<DataListValue> DataListValues { get; set; }
        public DbSet<Field> Fields { get; set; }
        public DbSet<Filter> Filters { get; set; }
        public DbSet<BoardField> BoardFields { get; set; }
        public DbSet<ContactField> ContactFields { get; set; }
        public DbSet<ContactFieldValue> ContactFieldValues { get; set; }
        public DbSet<CardFieldValue> CardFieldValues { get; set; }
        public DbSet<BotSettings> BotSettings { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Sending> Sendings { get; set; }
        public DbSet<UnReadChat> UnReadChats { get; set; }
        public DbSet<Import> Imports { get; set; }
        public DbSet<SmartHit> SmartHits { get; set; }
        public DbSet<GroupCampaign> GroupCampaigns { get; set; }
        public DbSet<WhatsGroup> WhatsGroups { get; set; }
        public DbSet<Landing> Landings { get; set; }
        public DbSet<LandingHit> LandingHits { get; set; }
        public virtual DbSet<DashboardAgentView> DashboardAgentViews { get; set; }
        public virtual DbSet<ContactSenderView> ContactSenderView { get; set; }
        public virtual DbSet<SendingReportView> SendingReportView { get; set; }
        public virtual DbSet<GroupCampaignView> GroupCampaignPageView { get; set; }
        public virtual DbSet<DashboardContactsBySourceView> DashboardContactsBySourceView { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>().HasMany(u => u.Claims).WithOne().HasForeignKey(c => c.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.Entity<ApplicationUser>().HasMany(u => u.Roles).WithOne().HasForeignKey(r => r.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationRole>().HasMany(r => r.Claims).WithOne().HasForeignKey(c => c.RoleId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.Entity<ApplicationRole>().HasMany(r => r.Users).WithOne().HasForeignKey(r => r.RoleId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ChattingLog>().HasIndex(p => p.Id);
        }
        public override int SaveChanges()
        {
            UpdateAuditEntities();
            return base.SaveChanges();
        }


        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            UpdateAuditEntities();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }


        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            UpdateAuditEntities();
            return base.SaveChangesAsync(cancellationToken);
        }


        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            UpdateAuditEntities();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }


        private void UpdateAuditEntities()
        {
            var modifiedEntries = ChangeTracker.Entries()
                .Where(x => x.Entity is IAuditableEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));


            foreach (var entry in modifiedEntries)
            {
                var entity = (IAuditableEntity)entry.Entity;
                DateTime now = DateTime.UtcNow;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedDate = now;
                    entity.CreatedBy = CurrentUserId;
                }
                else
                {
                    base.Entry(entity).Property(x => x.CreatedBy).IsModified = false;
                    base.Entry(entity).Property(x => x.CreatedDate).IsModified = false;
                }

                entity.UpdatedDate = now;
                entity.UpdatedBy = CurrentUserId;
            }
        }
    }
}
