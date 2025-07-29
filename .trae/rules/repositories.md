# DataLens Dashboard - Repository Kuralları

## Repository Pattern Geliştirme Kuralları

### 1. Generic Repository Interface
```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<string> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
    Task<int> CountAsync();
}
```

### 2. Specific Repository Interfaces

#### IUserRepository
```csharp
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetByRoleAsync(string role);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<bool> IsUsernameExistsAsync(string username);
    Task<bool> IsEmailExistsAsync(string email);
    Task<bool> UpdatePasswordAsync(string userId, string passwordHash);
    Task<bool> UpdateLastLoginAsync(string userId);
}
```

#### IDashboardRepository
```csharp
public interface IDashboardRepository : IRepository<Dashboard>
{
    Task<IEnumerable<Dashboard>> GetByCreatorAsync(string creatorId);
    Task<IEnumerable<Dashboard>> GetPublicDashboardsAsync();
    Task<IEnumerable<Dashboard>> GetActiveDashboardsAsync();
    Task<IEnumerable<Dashboard>> GetByCategoryAsync(string category);
    Task<IEnumerable<Dashboard>> GetByTagAsync(string tag);
    Task<bool> UpdateViewCountAsync(string dashboardId);
    Task<bool> UpdateLastModifiedAsync(string dashboardId, string modifiedBy);
    Task<IEnumerable<Dashboard>> SearchDashboardsAsync(string searchTerm);
}
```

#### IUserGroupRepository
```csharp
public interface IUserGroupRepository : IRepository<UserGroup>
{
    Task<UserGroup?> GetByNameAsync(string groupName);
    Task<IEnumerable<UserGroup>> GetActiveGroupsAsync();
    Task<bool> IsGroupNameExistsAsync(string groupName);
}
```

#### IUserGroupMembershipRepository
```csharp
public interface IUserGroupMembershipRepository : IRepository<UserGroupMember>
{
    Task<IEnumerable<UserGroupMember>> GetByUserIdAsync(string userId);
    Task<IEnumerable<UserGroupMember>> GetByGroupIdAsync(string groupId);
    Task<UserGroupMember?> GetMembershipAsync(string userId, string groupId);
    Task<bool> AddMemberAsync(string groupId, string userId);
    Task<bool> RemoveMemberAsync(string groupId, string userId);
    Task<bool> IsMemberAsync(string groupId, string userId);
}
```

#### IDashboardPermissionRepository
```csharp
public interface IDashboardPermissionRepository : IRepository<DashboardPermission>
{
    Task<IEnumerable<DashboardPermission>> GetByDashboardIdAsync(string dashboardId);
    Task<IEnumerable<DashboardPermission>> GetByUserIdAsync(string userId);
    Task<IEnumerable<DashboardPermission>> GetByGroupIdAsync(string groupId);
    Task<DashboardPermission?> GetUserPermissionAsync(string dashboardId, string userId);
    Task<DashboardPermission?> GetGroupPermissionAsync(string dashboardId, string groupId);
    Task<bool> HasPermissionAsync(string dashboardId, string userId, string permissionType);
    Task<bool> RevokeUserPermissionAsync(string dashboardId, string userId);
    Task<bool> RevokeGroupPermissionAsync(string dashboardId, string groupId);
}
```

### 3. SQL Server Repository Implementation
```csharp
public class SqlUserRepository : IUserRepository
{
    private readonly IDbConnection _connection;
    private readonly ILogger<SqlUserRepository> _logger;

    public SqlUserRepository(IDbConnection connection, ILogger<SqlUserRepository> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        const string sql = "SELECT * FROM Users WHERE Id = @Id AND IsActive = 1";
        return await _connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        const string sql = "SELECT * FROM Users WHERE IsActive = 1 ORDER BY CreatedDate DESC";
        return await _connection.QueryAsync<User>(sql);
    }

    public async Task<string> AddAsync(User entity)
    {
        const string sql = @"
            INSERT INTO Users (Id, Username, Email, PasswordHash, Role, FirstName, LastName, CreatedDate, IsActive)
            VALUES (@Id, @Username, @Email, @PasswordHash, @Role, @FirstName, @LastName, @CreatedDate, @IsActive);
            SELECT @Id;";
        
        entity.Id = Guid.NewGuid().ToString();
        entity.CreatedDate = DateTime.UtcNow;
        
        return await _connection.QuerySingleAsync<string>(sql, entity);
    }
}
```

### 4. MongoDB Repository Implementation
```csharp
public class MongoUserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;
    private readonly ILogger<MongoUserRepository> _logger;

    public MongoUserRepository(IMongoDatabase database, ILogger<MongoUserRepository> logger)
    {
        _users = database.GetCollection<User>("Users");
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Id, id),
            Builders<User>.Filter.Eq(u => u.IsActive, true)
        );
        return await _users.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        var filter = Builders<User>.Filter.Eq(u => u.IsActive, true);
        var sort = Builders<User>.Sort.Descending(u => u.CreatedDate);
        return await _users.Find(filter).Sort(sort).ToListAsync();
    }

    public async Task<string> AddAsync(User entity)
    {
        entity.Id = ObjectId.GenerateNewId().ToString();
        entity.CreatedDate = DateTime.UtcNow;
        await _users.InsertOneAsync(entity);
        return entity.Id;
    }
}
```

### 5. Unit of Work Pattern
```csharp
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IDashboardRepository Dashboards { get; }
    IUserGroupRepository UserGroups { get; }
    IUserGroupMembershipRepository UserGroupMemberships { get; }
    IDashboardPermissionRepository DashboardPermissions { get; }
    INotificationRepository Notifications { get; }
    
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task<int> SaveChangesAsync();
}
```

### 6. SQL Unit of Work Implementation
```csharp
public class SqlUnitOfWork : IUnitOfWork
{
    private readonly IDbConnection _connection;
    private IDbTransaction? _transaction;
    private bool _disposed = false;

    public SqlUnitOfWork(IDbConnectionFactory connectionFactory)
    {
        _connection = connectionFactory.CreateConnection();
        _connection.Open();
        
        // Initialize repositories
        Users = new SqlUserRepository(_connection);
        Dashboards = new SqlDashboardRepository(_connection);
        // ... other repositories
    }

    public IUserRepository Users { get; private set; }
    public IDashboardRepository Dashboards { get; private set; }
    // ... other repository properties

    public async Task BeginTransactionAsync()
    {
        _transaction = await _connection.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
```

### 7. MongoDB Unit of Work Implementation
```csharp
public class MongoUnitOfWork : IUnitOfWork
{
    private readonly IMongoDatabase _database;
    private IClientSessionHandle? _session;
    private bool _disposed = false;

    public MongoUnitOfWork(IDbConnectionFactory connectionFactory)
    {
        _database = connectionFactory.CreateMongoDatabase();
        
        // Initialize repositories
        Users = new MongoUserRepository(_database);
        Dashboards = new MongoDashboardRepository(_database);
        // ... other repositories
    }

    public async Task BeginTransactionAsync()
    {
        _session = await _database.Client.StartSessionAsync();
        _session.StartTransaction();
    }

    public async Task CommitTransactionAsync()
    {
        if (_session != null && _session.IsInTransaction)
        {
            await _session.CommitTransactionAsync();
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_session != null && _session.IsInTransaction)
        {
            await _session.AbortTransactionAsync();
        }
    }
}
```

### 8. Database Connection Factory
```csharp
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
    IMongoDatabase CreateMongoDatabase();
    string GetDatabaseType();
}

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly IConfiguration _configuration;
    private readonly string _databaseType;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
        _databaseType = _configuration["DatabaseSettings:DatabaseType"] ?? "SqlServer";
    }

    public IDbConnection CreateConnection()
    {
        return _databaseType switch
        {
            "SqlServer" => new SqlConnection(GetConnectionString("SqlServer")),
            "PostgreSQL" => new NpgsqlConnection(GetConnectionString("PostgreSQL")),
            _ => throw new NotSupportedException($"Database type '{_databaseType}' is not supported")
        };
    }

    public IMongoDatabase CreateMongoDatabase()
    {
        if (_databaseType != "MongoDB")
            throw new InvalidOperationException("CreateMongoDatabase can only be used with MongoDB");

        var connectionString = GetConnectionString("MongoDB");
        var client = new MongoClient(connectionString);
        var databaseName = MongoUrl.Create(connectionString).DatabaseName ?? "DataLensDb";
        return client.GetDatabase(databaseName);
    }
}
```

### 9. Repository Best Practices

#### Error Handling
```csharp
public async Task<User?> GetByIdAsync(string id)
{
    try
    {
        // Repository logic
    }
    catch (SqlException ex)
    {
        _logger.LogError(ex, "SQL error getting user by id: {UserId}", id);
        throw new DataAccessException("Error accessing user data", ex);
    }
    catch (MongoException ex)
    {
        _logger.LogError(ex, "MongoDB error getting user by id: {UserId}", id);
        throw new DataAccessException("Error accessing user data", ex);
    }
}
```

#### Query Optimization
```csharp
// SQL Server - Use proper indexing
const string sql = @"
    SELECT u.*, ug.GroupName 
    FROM Users u
    LEFT JOIN UserGroupMembers ugm ON u.Id = ugm.UserId
    LEFT JOIN UserGroups ug ON ugm.GroupId = ug.Id
    WHERE u.IsActive = 1
    ORDER BY u.CreatedDate DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

// MongoDB - Use aggregation pipeline
var pipeline = new BsonDocument[]
{
    new BsonDocument("$match", new BsonDocument("IsActive", true)),
    new BsonDocument("$lookup", new BsonDocument
    {
        { "from", "UserGroupMembers" },
        { "localField", "_id" },
        { "foreignField", "UserId" },
        { "as", "groupMemberships" }
    }),
    new BsonDocument("$sort", new BsonDocument("CreatedDate", -1)),
    new BsonDocument("$skip", offset),
    new BsonDocument("$limit", pageSize)
};
```

### 10. Dependency Injection Registration
```csharp
// Program.cs
var databaseType = builder.Configuration["DatabaseSettings:DatabaseType"] ?? "SqlServer";

switch (databaseType)
{
    case "SqlServer":
    case "PostgreSQL":
        builder.Services.AddScoped<IUserRepository, SqlUserRepository>();
        builder.Services.AddScoped<IDashboardRepository, SqlDashboardRepository>();
        break;
    case "MongoDB":
        builder.Services.AddScoped<IUserRepository, MongoUserRepository>();
        builder.Services.AddScoped<IDashboardRepository, MongoDashboardRepository>();
        break;
}

builder.Services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();
builder.Services.AddScoped<IUnitOfWork>(provider => 
    provider.GetRequiredService<IUnitOfWorkFactory>().CreateUnitOfWork());
```

### 11. Testing Considerations
- Repository interfaces for mocking
- In-memory databases for integration tests
- Separate test configurations
- Database cleanup between tests

### 12. Performance Guidelines
- Use async methods consistently
- Implement pagination for large datasets
- Use proper indexing strategies
- Optimize query performance
- Consider caching for frequently accessed data

### 13. Security Considerations
- Parameterized queries to prevent SQL injection
- Input validation at repository level
- Proper connection string security
- Database user permissions

### 14. Monitoring and Logging
- Log all database operations
- Monitor query performance
- Track connection usage
- Alert on database errors

### 15. Migration and Versioning
- Database schema versioning
- Migration scripts for schema changes
- Backward compatibility considerations
- Data migration strategies