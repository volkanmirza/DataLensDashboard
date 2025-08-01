using MongoDB.Driver;
using DataLens.Data.Interfaces;
using System.Data;

namespace DataLens.Data.MongoDB
{
    public class MongoUnitOfWork : IUnitOfWork
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IMongoDatabase _database;
        private IClientSessionHandle? _session;
        private bool _disposed = false;
        private bool _transactionStarted = false;

        // Repository instances
        private IUserRepository? _users;
        private IUserGroupRepository? _userGroups;
        private IUserGroupMemberRepository? _userGroupMembers;
        private IUserGroupMembershipRepository? _userGroupMemberships;
        private IDashboardRepository? _dashboards;
        private IDashboardPermissionRepository? _dashboardPermissions;
        private INotificationRepository? _notifications;
        private IUserSettingsRepository? _userSettings;

        public MongoUnitOfWork(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            _database = connectionFactory.CreateMongoDatabase();
        }

        // Repository properties with lazy initialization
        public IUserRepository Users
        {
            get
            {
                if (_users == null)
                {
                    _users = new MongoUserRepository(_connectionFactory);
                }
                return _users;
            }
        }

        public IUserGroupRepository UserGroups
        {
            get
            {
                if (_userGroups == null)
                {
                    _userGroups = new MongoUserGroupRepository(_database);
                }
                return _userGroups;
            }
        }

        public IUserGroupMemberRepository UserGroupMembers
        {
            get
            {
                if (_userGroupMembers == null)
                {
                    _userGroupMembers = new MongoUserGroupMemberRepository(_database);
                }
                return _userGroupMembers;
            }
        }

        public IUserGroupMembershipRepository UserGroupMemberships
        {
            get
            {
                if (_userGroupMemberships == null)
                {
                    _userGroupMemberships = new MongoUserGroupMembershipRepository(_database);
                }
                return _userGroupMemberships;
            }
        }

        public IDashboardRepository Dashboards
        {
            get
            {
                if (_dashboards == null)
                {
                    _dashboards = new MongoDashboardRepository(_database);
                }
                return _dashboards;
            }
        }

        public IDashboardPermissionRepository DashboardPermissions
        {
            get
            {
                if (_dashboardPermissions == null)
                {
                    _dashboardPermissions = new MongoDashboardPermissionRepository(_database);
                }
                return _dashboardPermissions;
            }
        }

        public INotificationRepository Notifications
        {
            get
            {
                if (_notifications == null)
                {
                    _notifications = new MongoNotificationRepository(_database);
                }
                return _notifications;
            }
        }

        public IUserSettingsRepository UserSettings
        {
            get
            {
                if (_userSettings == null)
                {
                    _userSettings = new MongoUserSettingsRepository(_database);
                }
                return _userSettings;
            }
        }

        // Not applicable for MongoDB
        public IDbConnection? Connection => null;
        public IDbTransaction? Transaction => null;

        public async Task BeginTransactionAsync()
        {
            // MongoDB için transaction yok, no-op
            await Task.CompletedTask;
        }

        public async Task CommitAsync()
        {
            // MongoDB için transaction yok, no-op
            await Task.CompletedTask;
        }

        public async Task RollbackAsync()
        {
            // MongoDB için transaction yok, no-op
            await Task.CompletedTask;
        }

        public async Task<int> SaveChangesAsync()
        {
            // For MongoDB, changes are typically committed immediately
            // This method can be used for additional logic if needed
            await Task.CompletedTask;
            return 0;
        }

        public async Task<bool> SaveChangesMongoAsync()
        {
            // MongoDB specific save changes logic
            // This can be used for bulk operations or session-based operations
            try
            {
                if (_session != null && _transactionStarted)
                {
                    // Transaction is managed separately
                    return true;
                }
                
                // For non-transactional operations, changes are already saved
                await Task.CompletedTask;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (_session != null)
                {
                    if (_transactionStarted)
                    {
                        try
                        {
                            _session.AbortTransaction();
                        }
                        catch
                        {
                            // Ignore errors during cleanup
                        }
                    }
                    _session.Dispose();
                }
                _disposed = true;
            }
        }
        public string DatabaseType => "MongoDB";
    }
}