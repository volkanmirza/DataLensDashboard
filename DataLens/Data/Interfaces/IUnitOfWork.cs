using System.Data;

namespace DataLens.Data.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Repository properties
        IUserRepository Users { get; }
        IUserGroupRepository UserGroups { get; }
        IUserGroupMemberRepository UserGroupMembers { get; }
        IUserGroupMembershipRepository UserGroupMemberships { get; }
        IDashboardRepository Dashboards { get; }
        IDashboardPermissionRepository DashboardPermissions { get; }
        INotificationRepository Notifications { get; }
        IUserSettingsRepository UserSettings { get; }

        // Transaction management
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        
        // Save changes
        Task<int> SaveChangesAsync();
        
        // Connection management
        IDbConnection? Connection { get; }
        IDbTransaction? Transaction { get; }

        // Database type
        string DatabaseType { get; }
        
        // MongoDB specific (for MongoDB implementations)
        Task<bool> SaveChangesMongoAsync();
    }
}