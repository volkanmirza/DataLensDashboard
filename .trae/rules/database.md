# DataLens Dashboard - Veritabanı ve Konfigürasyon Kuralları

## Veritabanı Kuralları

### 1. Multi-Database Support

#### Database Type Enum
```csharp
public enum DatabaseType
{
    SqlServer,
    PostgreSQL,
    MongoDB
}
```

#### DbConnectionFactory Implementation
```csharp
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
    IMongoDatabase? CreateMongoDatabase();
    DatabaseType DatabaseType { get; }
}

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly IConfiguration _configuration;
    private readonly DatabaseType _databaseType;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
        _databaseType = Enum.Parse<DatabaseType>(_configuration["DatabaseSettings:DatabaseType"] ?? "SqlServer");
    }

    public DatabaseType DatabaseType => _databaseType;

    public IDbConnection CreateConnection()
    {
        return _databaseType switch
        {
            DatabaseType.SqlServer => new SqlConnection(_configuration.GetConnectionString("SqlServer")),
            DatabaseType.PostgreSQL => new NpgsqlConnection(_configuration.GetConnectionString("PostgreSQL")),
            DatabaseType.MongoDB => throw new InvalidOperationException("Use CreateMongoDatabase for MongoDB connections"),
            _ => throw new ArgumentException($"Unsupported database type: {_databaseType}")
        };
    }

    public IMongoDatabase? CreateMongoDatabase()
    {
        if (_databaseType != DatabaseType.MongoDB)
            return null;

        var connectionString = _configuration.GetConnectionString("MongoDB");
        var client = new MongoClient(connectionString);
        var databaseName = _configuration["DatabaseSettings:MongoDB:DatabaseName"] ?? "DataLensDashboard";
        return client.GetDatabase(databaseName);
    }
}
```

### 2. Unit of Work Pattern

#### IUnitOfWork Interface
```csharp
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IDashboardRepository Dashboards { get; }
    IUserGroupRepository UserGroups { get; }
    IUserGroupMembershipRepository UserGroupMemberships { get; }
    IDashboardPermissionRepository DashboardPermissions { get; }
    INotificationRepository Notifications { get; }
    IUserSettingsRepository UserSettings { get; }
    IAuditLogRepository AuditLogs { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

#### SQL Server UnitOfWork
```csharp
public class SqlServerUnitOfWork : IUnitOfWork
{
    private readonly IDbConnection _connection;
    private IDbTransaction? _transaction;
    private bool _disposed = false;

    // Repositories
    private IUserRepository? _users;
    private IDashboardRepository? _dashboards;
    private IUserGroupRepository? _userGroups;
    private IUserGroupMembershipRepository? _userGroupMemberships;
    private IDashboardPermissionRepository? _dashboardPermissions;
    private INotificationRepository? _notifications;
    private IUserSettingsRepository? _userSettings;
    private IAuditLogRepository? _auditLogs;

    public SqlServerUnitOfWork(IDbConnectionFactory connectionFactory)
    {
        _connection = connectionFactory.CreateConnection();
        _connection.Open();
    }

    public IUserRepository Users => _users ??= new SqlServerUserRepository(_connection, _transaction);
    public IDashboardRepository Dashboards => _dashboards ??= new SqlServerDashboardRepository(_connection, _transaction);
    public IUserGroupRepository UserGroups => _userGroups ??= new SqlServerUserGroupRepository(_connection, _transaction);
    public IUserGroupMembershipRepository UserGroupMemberships => _userGroupMemberships ??= new SqlServerUserGroupMembershipRepository(_connection, _transaction);
    public IDashboardPermissionRepository DashboardPermissions => _dashboardPermissions ??= new SqlServerDashboardPermissionRepository(_connection, _transaction);
    public INotificationRepository Notifications => _notifications ??= new SqlServerNotificationRepository(_connection, _transaction);
    public IUserSettingsRepository UserSettings => _userSettings ??= new SqlServerUserSettingsRepository(_connection, _transaction);
    public IAuditLogRepository AuditLogs => _auditLogs ??= new SqlServerAuditLogRepository(_connection, _transaction);

    public async Task<int> SaveChangesAsync()
    {
        // SQL Server'da Dapper kullanıldığı için bu method boş olabilir
        // Çünkü her repository kendi save işlemini yapar
        return await Task.FromResult(0);
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await Task.FromResult(_connection.BeginTransaction());
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await Task.Run(() => _transaction.Commit());
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await Task.Run(() => _transaction.Rollback());
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _transaction?.Dispose();
            _connection?.Dispose();
            _disposed = true;
        }
    }
}
```

#### MongoDB UnitOfWork
```csharp
public class MongoDbUnitOfWork : IUnitOfWork
{
    private readonly IMongoDatabase _database;
    private IClientSessionHandle? _session;
    private bool _disposed = false;

    // Repositories
    private IUserRepository? _users;
    private IDashboardRepository? _dashboards;
    private IUserGroupRepository? _userGroups;
    private IUserGroupMembershipRepository? _userGroupMemberships;
    private IDashboardPermissionRepository? _dashboardPermissions;
    private INotificationRepository? _notifications;
    private IUserSettingsRepository? _userSettings;
    private IAuditLogRepository? _auditLogs;

    public MongoDbUnitOfWork(IDbConnectionFactory connectionFactory)
    {
        _database = connectionFactory.CreateMongoDatabase() 
            ?? throw new InvalidOperationException("MongoDB database not available");
    }

    public IUserRepository Users => _users ??= new MongoDbUserRepository(_database, _session);
    public IDashboardRepository Dashboards => _dashboards ??= new MongoDbDashboardRepository(_database, _session);
    public IUserGroupRepository UserGroups => _userGroups ??= new MongoDbUserGroupRepository(_database, _session);
    public IUserGroupMembershipRepository UserGroupMemberships => _userGroupMemberships ??= new MongoDbUserGroupMembershipRepository(_database, _session);
    public IDashboardPermissionRepository DashboardPermissions => _dashboardPermissions ??= new MongoDbDashboardPermissionRepository(_database, _session);
    public INotificationRepository Notifications => _notifications ??= new MongoDbNotificationRepository(_database, _session);
    public IUserSettingsRepository UserSettings => _userSettings ??= new MongoDbUserSettingsRepository(_database, _session);
    public IAuditLogRepository AuditLogs => _auditLogs ??= new MongoDbAuditLogRepository(_database, _session);

    public async Task<int> SaveChangesAsync()
    {
        // MongoDB'de her repository kendi save işlemini yapar
        return await Task.FromResult(0);
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

    public void Dispose()
    {
        if (!_disposed)
        {
            _session?.Dispose();
            _disposed = true;
        }
    }
}
```

#### UnitOfWork Factory
```csharp
public interface IUnitOfWorkFactory
{
    IUnitOfWork Create();
}

public class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UnitOfWorkFactory(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public IUnitOfWork Create()
    {
        return _connectionFactory.DatabaseType switch
        {
            DatabaseType.SqlServer => new SqlServerUnitOfWork(_connectionFactory),
            DatabaseType.PostgreSQL => new PostgreSqlUnitOfWork(_connectionFactory),
            DatabaseType.MongoDB => new MongoDbUnitOfWork(_connectionFactory),
            _ => throw new ArgumentException($"Unsupported database type: {_connectionFactory.DatabaseType}")
        };
    }
}
```

### 3. Database Migration

#### Migration Interface
```csharp
public interface IDatabaseMigration
{
    string Version { get; }
    string Description { get; }
    Task UpAsync(IDbConnection connection);
    Task DownAsync(IDbConnection connection);
}

public abstract class BaseMigration : IDatabaseMigration
{
    public abstract string Version { get; }
    public abstract string Description { get; }
    public abstract Task UpAsync(IDbConnection connection);
    public abstract Task DownAsync(IDbConnection connection);
}
```

#### SQL Server Migration Example
```csharp
public class CreateUsersTable_001 : BaseMigration
{
    public override string Version => "001";
    public override string Description => "Create Users table";

    public override async Task UpAsync(IDbConnection connection)
    {
        var sql = @"
            CREATE TABLE Users (
                Id NVARCHAR(50) PRIMARY KEY,
                Username NVARCHAR(50) NOT NULL UNIQUE,
                Email NVARCHAR(100) NOT NULL UNIQUE,
                PasswordHash NVARCHAR(255) NOT NULL,
                Role NVARCHAR(20) NOT NULL DEFAULT 'User',
                IsActive BIT NOT NULL DEFAULT 1,
                CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                LastLoginDate DATETIME2 NULL,
                LastModifiedDate DATETIME2 NULL,
                LastModifiedBy NVARCHAR(50) NULL
            );
            
            CREATE INDEX IX_Users_Username ON Users(Username);
            CREATE INDEX IX_Users_Email ON Users(Email);
            CREATE INDEX IX_Users_Role ON Users(Role);
        ";
        
        await connection.ExecuteAsync(sql);
    }

    public override async Task DownAsync(IDbConnection connection)
    {
        var sql = "DROP TABLE IF EXISTS Users;";
        await connection.ExecuteAsync(sql);
    }
}
```

#### Migration Service
```csharp
public interface IMigrationService
{
    Task MigrateAsync();
    Task MigrateToVersionAsync(string version);
    Task RollbackAsync(string version);
    Task<IEnumerable<string>> GetAppliedMigrationsAsync();
}

public class MigrationService : IMigrationService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<MigrationService> _logger;
    private readonly List<IDatabaseMigration> _migrations;

    public MigrationService(IDbConnectionFactory connectionFactory, ILogger<MigrationService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        _migrations = LoadMigrations();
    }

    public async Task MigrateAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        
        await EnsureMigrationTableExistsAsync(connection);
        
        var appliedMigrations = await GetAppliedMigrationsAsync();
        var pendingMigrations = _migrations
            .Where(m => !appliedMigrations.Contains(m.Version))
            .OrderBy(m => m.Version);
        
        foreach (var migration in pendingMigrations)
        {
            _logger.LogInformation("Applying migration {Version}: {Description}", migration.Version, migration.Description);
            
            using var transaction = connection.BeginTransaction();
            try
            {
                await migration.UpAsync(connection);
                await RecordMigrationAsync(connection, migration.Version, migration.Description);
                transaction.Commit();
                
                _logger.LogInformation("Migration {Version} applied successfully", migration.Version);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Failed to apply migration {Version}", migration.Version);
                throw;
            }
        }
    }

    private async Task EnsureMigrationTableExistsAsync(IDbConnection connection)
    {
        var sql = _connectionFactory.DatabaseType switch
        {
            DatabaseType.SqlServer => @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='__MigrationHistory' AND xtype='U')
                CREATE TABLE __MigrationHistory (
                    Version NVARCHAR(50) PRIMARY KEY,
                    Description NVARCHAR(255) NOT NULL,
                    AppliedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                );",
            DatabaseType.PostgreSQL => @"
                CREATE TABLE IF NOT EXISTS __MigrationHistory (
                    Version VARCHAR(50) PRIMARY KEY,
                    Description VARCHAR(255) NOT NULL,
                    AppliedDate TIMESTAMP NOT NULL DEFAULT NOW()
                );",
            _ => throw new NotSupportedException($"Database type {_connectionFactory.DatabaseType} not supported for migrations")
        };
        
        await connection.ExecuteAsync(sql);
    }

    private List<IDatabaseMigration> LoadMigrations()
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IDatabaseMigration).IsAssignableFrom(t) && !t.IsAbstract)
            .Select(t => (IDatabaseMigration)Activator.CreateInstance(t)!)
            .OrderBy(m => m.Version)
            .ToList();
    }
}
```

### 4. Database Seeding

#### Data Seeder Interface
```csharp
public interface IDataSeeder
{
    Task SeedAsync();
    int Order { get; }
}

public class UserSeeder : IDataSeeder
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserSeeder> _logger;

    public int Order => 1;

    public UserSeeder(IUnitOfWorkFactory unitOfWorkFactory, IPasswordHasher passwordHasher, ILogger<UserSeeder> logger)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        using var unitOfWork = _unitOfWorkFactory.Create();
        
        // Admin kullanıcısı var mı kontrol et
        var adminUser = await unitOfWork.Users.GetByUsernameAsync("admin");
        if (adminUser == null)
        {
            _logger.LogInformation("Creating default admin user");
            
            var admin = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = "admin",
                Email = "admin@datalens.com",
                PasswordHash = _passwordHasher.HashPassword("Admin123!"),
                Role = "Admin",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            
            await unitOfWork.Users.CreateAsync(admin);
            await unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Default admin user created successfully");
        }
        
        // Demo kullanıcıları oluştur
        await CreateDemoUsersAsync(unitOfWork);
    }

    private async Task CreateDemoUsersAsync(IUnitOfWork unitOfWork)
    {
        var demoUsers = new[]
        {
            new { Username = "manager", Email = "manager@datalens.com", Role = "Manager" },
            new { Username = "designer", Email = "designer@datalens.com", Role = "Designer" },
            new { Username = "viewer", Email = "viewer@datalens.com", Role = "User" }
        };
        
        foreach (var demoUser in demoUsers)
        {
            var existingUser = await unitOfWork.Users.GetByUsernameAsync(demoUser.Username);
            if (existingUser == null)
            {
                var user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = demoUser.Username,
                    Email = demoUser.Email,
                    PasswordHash = _passwordHasher.HashPassword("Demo123!"),
                    Role = demoUser.Role,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                
                await unitOfWork.Users.CreateAsync(user);
                _logger.LogInformation("Demo user {Username} created", demoUser.Username);
            }
        }
        
        await unitOfWork.SaveChangesAsync();
    }
}
```

### 5. Database Health Check

#### Health Check Implementation
```csharp
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(IDbConnectionFactory connectionFactory, ILogger<DatabaseHealthCheck> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_connectionFactory.DatabaseType == DatabaseType.MongoDB)
            {
                return await CheckMongoDbHealthAsync(cancellationToken);
            }
            else
            {
                return await CheckSqlHealthAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }

    private async Task<HealthCheckResult> CheckSqlHealthAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        
        var result = await connection.QuerySingleAsync<int>("SELECT 1");
        
        return result == 1 
            ? HealthCheckResult.Healthy("Database connection is healthy")
            : HealthCheckResult.Unhealthy("Database query returned unexpected result");
    }

    private async Task<HealthCheckResult> CheckMongoDbHealthAsync(CancellationToken cancellationToken)
    {
        var database = _connectionFactory.CreateMongoDatabase();
        if (database == null)
        {
            return HealthCheckResult.Unhealthy("MongoDB database not available");
        }
        
        await database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cancellationToken);
        
        return HealthCheckResult.Healthy("MongoDB connection is healthy");
    }
}
```

## Konfigürasyon Kuralları

### 1. Configuration Structure

#### appsettings.json Structure
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  
  "DatabaseSettings": {
    "DatabaseType": "SqlServer",
    "MongoDB": {
      "DatabaseName": "DataLensDashboard",
      "CollectionNames": {
        "Users": "users",
        "Dashboards": "dashboards",
        "UserGroups": "usergroups",
        "UserGroupMemberships": "usergroupmemberships",
        "DashboardPermissions": "dashboardpermissions",
        "Notifications": "notifications",
        "UserSettings": "usersettings",
        "AuditLogs": "auditlogs"
      }
    }
  },
  
  "ConnectionStrings": {
    "SqlServer": "Server=(localdb)\\mssqllocaldb;Database=DataLensDashboard;Trusted_Connection=true;MultipleActiveResultSets=true",
    "PostgreSQL": "Host=localhost;Database=DataLensDashboard;Username=postgres;Password=password",
    "MongoDB": "mongodb://localhost:27017"
  },
  
  "Authentication": {
    "CookieName": "DataLensAuth",
    "CookieExpiration": "7.00:00:00",
    "RequireHttps": false,
    "SlidingExpiration": true
  },
  
  "Jwt": {
    "Key": "your-super-secret-key-that-is-at-least-32-characters-long",
    "Issuer": "DataLensDashboard",
    "Audience": "DataLensDashboard",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  
  "RateLimit": {
    "MaxRequests": 100,
    "WindowMinutes": 1
  },
  
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "noreply@datalens.com",
    "FromName": "DataLens Dashboard"
  },
  
  "FileUpload": {
    "MaxFileSize": 10485760,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".pdf", ".xlsx", ".csv"],
    "UploadPath": "uploads"
  },
  
  "Cache": {
    "DefaultExpirationMinutes": 30,
    "SlidingExpirationMinutes": 10,
    "MaxSize": 1000
  },
  
  "DevExpress": {
    "LicenseKey": "your-devexpress-license-key",
    "Dashboard": {
      "CacheEnabled": true,
      "CacheExpiration": "00:30:00",
      "MaxDashboardSize": "10MB",
      "AllowedExportFormats": ["PDF", "Excel", "Image"]
    }
  },
  
  "Security": {
    "PasswordPolicy": {
      "MinLength": 8,
      "RequireUppercase": true,
      "RequireLowercase": true,
      "RequireDigit": true,
      "RequireSpecialCharacter": true
    },
    "AccountLockout": {
      "MaxFailedAttempts": 5,
      "LockoutDurationMinutes": 30
    },
    "SessionTimeout": {
      "IdleTimeoutMinutes": 30,
      "AbsoluteTimeoutMinutes": 480
    }
  },
  
  "Monitoring": {
    "EnableHealthChecks": true,
    "EnableMetrics": true,
    "LogRetentionDays": 30
  }
}
```

### 2. Configuration Models

#### Strongly Typed Configuration
```csharp
public class DatabaseSettings
{
    public const string SectionName = "DatabaseSettings";
    
    public string DatabaseType { get; set; } = "SqlServer";
    public MongoDbSettings MongoDB { get; set; } = new();
}

public class MongoDbSettings
{
    public string DatabaseName { get; set; } = "DataLensDashboard";
    public CollectionNames CollectionNames { get; set; } = new();
}

public class CollectionNames
{
    public string Users { get; set; } = "users";
    public string Dashboards { get; set; } = "dashboards";
    public string UserGroups { get; set; } = "usergroups";
    public string UserGroupMemberships { get; set; } = "usergroupmemberships";
    public string DashboardPermissions { get; set; } = "dashboardpermissions";
    public string Notifications { get; set; } = "notifications";
    public string UserSettings { get; set; } = "usersettings";
    public string AuditLogs { get; set; } = "auditlogs";
}

public class JwtSettings
{
    public const string SectionName = "Jwt";
    
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}

public class EmailSettings
{
    public const string SectionName = "Email";
    
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

public class SecuritySettings
{
    public const string SectionName = "Security";
    
    public PasswordPolicy PasswordPolicy { get; set; } = new();
    public AccountLockout AccountLockout { get; set; } = new();
    public SessionTimeout SessionTimeout { get; set; } = new();
}

public class PasswordPolicy
{
    public int MinLength { get; set; } = 8;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialCharacter { get; set; } = true;
}
```

### 3. Configuration Validation

#### Configuration Validator
```csharp
public class ConfigurationValidator
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationValidator> _logger;

    public ConfigurationValidator(IConfiguration configuration, ILogger<ConfigurationValidator> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> ValidateAsync()
    {
        var isValid = true;
        
        // Database configuration validation
        isValid &= ValidateDatabaseConfiguration();
        
        // JWT configuration validation
        isValid &= ValidateJwtConfiguration();
        
        // Email configuration validation
        isValid &= ValidateEmailConfiguration();
        
        // Connection strings validation
        isValid &= await ValidateConnectionStringsAsync();
        
        return isValid;
    }

    private bool ValidateDatabaseConfiguration()
    {
        var databaseType = _configuration["DatabaseSettings:DatabaseType"];
        if (string.IsNullOrEmpty(databaseType))
        {
            _logger.LogError("DatabaseSettings:DatabaseType is not configured");
            return false;
        }
        
        if (!Enum.TryParse<DatabaseType>(databaseType, out _))
        {
            _logger.LogError("Invalid DatabaseType: {DatabaseType}", databaseType);
            return false;
        }
        
        return true;
    }

    private bool ValidateJwtConfiguration()
    {
        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
        {
            _logger.LogError("JWT Key must be at least 32 characters long");
            return false;
        }
        
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        
        if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
        {
            _logger.LogError("JWT Issuer and Audience must be configured");
            return false;
        }
        
        return true;
    }

    private async Task<bool> ValidateConnectionStringsAsync()
    {
        var databaseType = _configuration["DatabaseSettings:DatabaseType"];
        var connectionString = _configuration.GetConnectionString(databaseType);
        
        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogError("Connection string for {DatabaseType} is not configured", databaseType);
            return false;
        }
        
        // Test connection
        try
        {
            var connectionFactory = new DbConnectionFactory(_configuration);
            
            if (databaseType == "MongoDB")
            {
                var database = connectionFactory.CreateMongoDatabase();
                await database!.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
            }
            else
            {
                using var connection = connectionFactory.CreateConnection();
                await connection.OpenAsync();
            }
            
            _logger.LogInformation("Database connection test successful for {DatabaseType}", databaseType);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection test failed for {DatabaseType}", databaseType);
            return false;
        }
    }
}
```

### 4. Environment-Specific Configuration

#### appsettings.Development.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  
  "DatabaseSettings": {
    "DatabaseType": "SqlServer"
  },
  
  "ConnectionStrings": {
    "SqlServer": "Server=(localdb)\\mssqllocaldb;Database=DataLensDashboard_Dev;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  
  "Authentication": {
    "RequireHttps": false
  },
  
  "Security": {
    "PasswordPolicy": {
      "MinLength": 6,
      "RequireUppercase": false,
      "RequireLowercase": false,
      "RequireDigit": false,
      "RequireSpecialCharacter": false
    }
  }
}
```

#### appsettings.Production.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  
  "Authentication": {
    "RequireHttps": true
  },
  
  "Security": {
    "PasswordPolicy": {
      "MinLength": 12,
      "RequireUppercase": true,
      "RequireLowercase": true,
      "RequireDigit": true,
      "RequireSpecialCharacter": true
    },
    "AccountLockout": {
      "MaxFailedAttempts": 3,
      "LockoutDurationMinutes": 60
    }
  }
}
```

### 5. Configuration Registration

#### Program.cs Configuration Setup
```csharp
// Configuration validation
var configValidator = new ConfigurationValidator(builder.Configuration, 
    builder.Services.BuildServiceProvider().GetRequiredService<ILogger<ConfigurationValidator>>());

if (!await configValidator.ValidateAsync())
{
    throw new InvalidOperationException("Configuration validation failed");
}

// Strongly typed configuration
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(DatabaseSettings.SectionName));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));
builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection(SecuritySettings.SectionName));
builder.Services.Configure<RateLimitOptions>(builder.Configuration.GetSection("RateLimit"));

// Database configuration
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database")
    .AddCheck("self", () => HealthCheckResult.Healthy());
```