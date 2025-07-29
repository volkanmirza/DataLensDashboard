# DataLens Dashboard - Service Kuralları

## Service Layer Geliştirme Kuralları

### 1. Service Interface Tanımları
```csharp
public interface IServiceName
{
    Task<Model> GetByIdAsync(string id);
    Task<IEnumerable<Model>> GetAllAsync();
    Task<string> CreateAsync(Model model);
    Task<bool> UpdateAsync(Model model);
    Task<bool> DeleteAsync(string id);
}
```

### 2. Service Implementation Yapısı
```csharp
public class ServiceName : IServiceName
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ServiceName> _logger;

    public ServiceName(IUnitOfWork unitOfWork, ILogger<ServiceName> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
}
```

### 3. Service Sınıfları

#### IUserService & UserService
```csharp
public interface IUserService
{
    // CRUD Operations
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
    Task<string> CreateUserAsync(User user, string password);
    Task<bool> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(string id);
    
    // Authentication
    Task<User?> ValidateUserAsync(string username, string password);
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<bool> ResetPasswordAsync(string userId, string newPassword);
    
    // User Management
    Task<bool> ActivateUserAsync(string userId);
    Task<bool> DeactivateUserAsync(string userId);
    Task<bool> UpdateLastLoginAsync(string userId);
    Task<bool> IsUsernameAvailableAsync(string username);
    Task<bool> IsEmailAvailableAsync(string email);
}
```

#### IDashboardService & DashboardService
```csharp
public interface IDashboardService
{
    // CRUD Operations
    Task<Dashboard?> GetDashboardByIdAsync(string id);
    Task<IEnumerable<Dashboard>> GetAllDashboardsAsync();
    Task<IEnumerable<Dashboard>> GetDashboardsByUserAsync(string userId);
    Task<IEnumerable<Dashboard>> GetPublicDashboardsAsync();
    Task<string> CreateDashboardAsync(Dashboard dashboard);
    Task<bool> UpdateDashboardAsync(Dashboard dashboard);
    Task<bool> DeleteDashboardAsync(string id);
    
    // Dashboard Management
    Task<Dashboard?> CloneDashboardAsync(string dashboardId, string newName, string userId);
    Task<bool> PublishDashboardAsync(string dashboardId);
    Task<bool> UnpublishDashboardAsync(string dashboardId);
    Task<bool> IncrementViewCountAsync(string dashboardId);
    
    // Permission Management
    Task<bool> GrantUserPermissionAsync(string dashboardId, string userId, string permissionType, string grantedBy);
    Task<bool> GrantGroupPermissionAsync(string dashboardId, string groupId, string permissionType, string grantedBy);
    Task<bool> RevokeUserPermissionAsync(string dashboardId, string userId);
    Task<bool> RevokeGroupPermissionAsync(string dashboardId, string groupId);
    Task<bool> UpdateUserPermissionAsync(string dashboardId, string userId, string permissionType);
    Task<bool> UpdateGroupPermissionAsync(string dashboardId, string groupId, string permissionType);
    
    // Permission Queries
    Task<IEnumerable<DashboardPermission>> GetUserPermissionsAsync(string userId);
    Task<IEnumerable<DashboardPermission>> GetGroupPermissionsAsync(string groupId);
    Task<IEnumerable<DashboardPermission>> GetDashboardPermissionsAsync(string dashboardId);
    Task<bool> HasPermissionAsync(string dashboardId, string userId, string permissionType);
}
```

#### IUserGroupService & UserGroupService
```csharp
public interface IUserGroupService
{
    // CRUD Operations
    Task<UserGroup?> GetByIdAsync(string id);
    Task<IEnumerable<UserGroup>> GetAllGroupsAsync();
    Task<string> CreateGroupAsync(UserGroup group);
    Task<bool> UpdateGroupAsync(UserGroup group);
    Task<bool> DeleteGroupAsync(string id);
    
    // Member Management
    Task<bool> AddMemberAsync(string groupId, string userId);
    Task<bool> RemoveMemberAsync(string groupId, string userId);
    Task<IEnumerable<User>> GetGroupMembersAsync(string groupId);
    Task<IEnumerable<UserGroup>> GetUserGroupsAsync(string userId);
    Task<bool> IsUserInGroupAsync(string groupId, string userId);
}
```

#### IJwtService & JwtService
```csharp
public interface IJwtService
{
    string GenerateToken(User user);
    bool IsTokenValid(string token);
    ClaimsPrincipal? GetPrincipalFromToken(string token);
    string? GetUserIdFromToken(string token);
    string? GetUsernameFromToken(string token);
    string? GetRoleFromToken(string token);
    DateTime? GetTokenExpiration(string token);
}
```

#### INotificationService & NotificationService
```csharp
public interface INotificationService
{
    // CRUD Operations
    Task<Notification?> GetByIdAsync(string id);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId);
    Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(string userId);
    Task<string> CreateNotificationAsync(Notification notification);
    Task<bool> DeleteNotificationAsync(string id);
    
    // Notification Management
    Task<bool> MarkAsReadAsync(string notificationId);
    Task<bool> MarkAllAsReadAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
    
    // Notification Creation Helpers
    Task<string> CreateUserNotificationAsync(string userId, string title, string message, string type = "Info");
    Task<bool> NotifyGroupAsync(string groupId, string title, string message, string type = "Info");
    Task<bool> NotifyAllUsersAsync(string title, string message, string type = "Info");
}
```

### 4. Business Logic Kuralları

#### Transaction Management
```csharp
public async Task<string> CreateUserAsync(User user, string password)
{
    try
    {
        await _unitOfWork.BeginTransactionAsync();
        
        // Business logic
        user.PasswordHash = HashPassword(password);
        var userId = await _unitOfWork.Users.AddAsync(user);
        
        await _unitOfWork.CommitTransactionAsync();
        return userId;
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackTransactionAsync();
        _logger.LogError(ex, "Error creating user");
        throw;
    }
}
```

#### Error Handling
```csharp
public async Task<User?> GetByIdAsync(string id)
{
    try
    {
        return await _unitOfWork.Users.GetByIdAsync(id);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting user by id: {UserId}", id);
        throw;
    }
}
```

#### Validation
```csharp
public async Task<bool> UpdateUserAsync(User user)
{
    if (user == null)
        throw new ArgumentNullException(nameof(user));
        
    if (string.IsNullOrEmpty(user.Id))
        throw new ArgumentException("User ID is required", nameof(user));
        
    // Business validation
    var existingUser = await _unitOfWork.Users.GetByIdAsync(user.Id);
    if (existingUser == null)
        return false;
        
    // Update logic
}
```

### 5. Password Management
```csharp
public class UserService : IUserService
{
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
    
    private bool VerifyPassword(string password, string hash)
    {
        var hashedPassword = HashPassword(password);
        return hashedPassword == hash;
    }
}
```

### 6. Logging Kuralları
```csharp
// Information logging
_logger.LogInformation("User {UserId} logged in successfully", userId);

// Warning logging
_logger.LogWarning("Failed login attempt for username: {Username}", username);

// Error logging
_logger.LogError(ex, "Error updating user {UserId}", userId);

// Debug logging
_logger.LogDebug("Processing dashboard permission for user {UserId}", userId);
```

### 7. Async/Await Best Practices
- Tüm database operations async
- ConfigureAwait(false) library kodlarında
- CancellationToken kullanımı uzun işlemlerde
- Task.WhenAll parallel operations için

### 8. Dependency Injection Registration
```csharp
// Program.cs
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IUserGroupService, UserGroupService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
```

### 9. Service Method Naming
- GetByIdAsync: ID ile tekil kayıt
- GetAllAsync: Tüm kayıtlar
- GetByXAsync: Belirli kritere göre
- CreateAsync: Yeni kayıt oluşturma
- UpdateAsync: Kayıt güncelleme
- DeleteAsync: Kayıt silme
- ValidateAsync: Doğrulama işlemleri
- ProcessAsync: İşlem metodları

### 10. Return Types
- `Task<Model?>` nullable single entity
- `Task<IEnumerable<Model>>` multiple entities
- `Task<string>` ID return for create operations
- `Task<bool>` success/failure operations
- `Task<int>` count operations

### 11. Exception Handling Strategy
- Service layer'da business exceptions
- Repository layer'da data exceptions
- Controller layer'da user-friendly messages
- Global exception handler for unhandled exceptions

### 12. Caching Strategy
- Memory cache for frequently accessed data
- Distributed cache for scalability
- Cache invalidation on data changes
- Cache keys with proper naming convention

### 13. Performance Considerations
- Lazy loading where appropriate
- Pagination for large datasets
- Bulk operations for multiple records
- Database query optimization

### 14. Security in Services
- Input sanitization
- Authorization checks
- Audit logging for sensitive operations
- Rate limiting for expensive operations

### 15. Testing Considerations
- Unit testable methods
- Mockable dependencies
- Clear method contracts
- Predictable behavior