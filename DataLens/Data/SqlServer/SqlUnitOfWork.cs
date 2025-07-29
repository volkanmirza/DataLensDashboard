using System.Data;
using DataLens.Data.Interfaces;
using DataLens.Data.SqlServer;

namespace DataLens.Data.SqlServer
{
    public class SqlUnitOfWork : IUnitOfWork
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private IDbConnection? _connection;
        private IDbTransaction? _transaction;
        private bool _disposed = false;

        // Repository instances
        private IUserRepository? _users;
        private IUserGroupRepository? _userGroups;
        private IUserGroupMemberRepository? _userGroupMembers;
        private IUserGroupMembershipRepository? _userGroupMemberships;
        private IDashboardRepository? _dashboards;
        private IDashboardPermissionRepository? _dashboardPermissions;
        private INotificationRepository? _notifications;
        private IUserSettingsRepository? _userSettings;

        public SqlUnitOfWork(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        // Repository properties with lazy initialization
        public IUserRepository Users
        {
            get
            {
                if (_users == null)
                {
                    EnsureConnection();
                    _users = new SqlUserRepository(_connectionFactory);
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
                    EnsureConnection();
                    _userGroups = new SqlUserGroupRepository(_connectionFactory);
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
                    EnsureConnection();
                    _userGroupMembers = new SqlUserGroupMemberRepository(_connectionFactory);
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
                    EnsureConnection();
                    _userGroupMemberships = new SqlUserGroupMembershipRepository(_connectionFactory);
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
                    EnsureConnection();
                    _dashboards = new SqlDashboardRepository(_connectionFactory.ConnectionString);
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
                    EnsureConnection();
                    _dashboardPermissions = new SqlDashboardPermissionRepository(_connectionFactory.ConnectionString);
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
                    EnsureConnection();
                    _notifications = new SqlNotificationRepository(_connectionFactory.ConnectionString);
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
                    EnsureConnection();
                    _userSettings = new SqlUserSettingsRepository(_connectionFactory.ConnectionString);
                }
                return _userSettings;
            }
        }

        public IDbConnection? Connection => _connection;
        public IDbTransaction? Transaction => _transaction;

        public async Task BeginTransactionAsync()
        {
            EnsureConnection();
            if (_transaction != null)
                throw new InvalidOperationException("Transaction already started");

            if (_connection!.State != ConnectionState.Open)
                await Task.Run(() => _connection.Open());

            _transaction = _connection.BeginTransaction();
        }

        public async Task CommitAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("No transaction to commit");

            try
            {
                await Task.Run(() => _transaction.Commit());
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction == null)
                return;

            try
            {
                await Task.Run(() => _transaction.Rollback());
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            // For SQL databases, changes are committed when transaction is committed
            // This method can be used for additional logic if needed
            await Task.CompletedTask;
            return 0; // Return number of affected rows if needed
        }

        public async Task<bool> SaveChangesMongoAsync()
        {
            // Not applicable for SQL databases
            await Task.CompletedTask;
            throw new NotSupportedException("SaveChangesMongoAsync is not supported for SQL databases");
        }

        private void EnsureConnection()
        {
            if (_connection == null)
            {
                _connection = _connectionFactory.CreateConnection();
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
                _transaction?.Dispose();
                _connection?.Dispose();
                _disposed = true;
            }
        }
    }
}