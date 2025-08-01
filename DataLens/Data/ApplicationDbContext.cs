using DataLens.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataLens.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Dashboard> Dashboards { get; set; }
        public DbSet<DashboardPermission> DashboardPermissions { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<UserGroupMember> UserGroupMembers { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // User configuration
            builder.Entity<User>(entity =>
            {
                entity.Property(e => e.Role).HasMaxLength(20).IsRequired();
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // Dashboard configuration
            builder.Entity<Dashboard>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsPublic).HasDefaultValue(false);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.ViewCount).HasDefaultValue(0);
            });

            // DashboardPermission configuration
            builder.Entity<DashboardPermission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PermissionType).HasMaxLength(20).IsRequired();
                entity.Property(e => e.GrantedDate).HasDefaultValueSql("GETUTCDATE()");
            });

            // UserGroup configuration
            builder.Entity<UserGroup>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.GroupName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // UserGroupMember configuration
            builder.Entity<UserGroupMember>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
                entity.Property(e => e.GroupId).HasMaxLength(450).IsRequired();
                entity.Property(e => e.JoinedDate).HasDefaultValueSql("GETUTCDATE()");
            });

            // Notification configuration
            builder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Message).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsRead).HasDefaultValue(false);
            });

            // UserSettings configuration
            builder.Entity<UserSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
                entity.Property(e => e.Language).HasMaxLength(10).HasDefaultValue("tr-TR");
                entity.Property(e => e.Theme).HasMaxLength(20).HasDefaultValue("default");
                entity.Property(e => e.TimeZone).HasMaxLength(50).HasDefaultValue("Turkey Standard Time");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}