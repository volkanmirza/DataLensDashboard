# DataLens Dashboard - Genel Proje Kuralları ve Best Practices

## Genel Proje Kuralları

### 1. Kod Standartları

#### Naming Conventions
```csharp
// Classes, Interfaces, Methods, Properties - PascalCase
public class UserService : IUserService
{
    public string UserName { get; set; }
    public async Task<User> GetUserByIdAsync(string userId) { }
}

// Fields, Parameters, Local Variables - camelCase
private readonly ILogger _logger;
public async Task CreateUserAsync(string userName, string email)
{
    var hashedPassword = HashPassword(password);
}

// Constants - PascalCase
public const string DefaultRole = "User";
public static readonly TimeSpan TokenExpiration = TimeSpan.FromHours(24);

// Private fields - underscore prefix
private readonly IUserService _userService;
private readonly ILogger<AccountController> _logger;

// Interface naming - 'I' prefix
public interface IUserRepository
public interface IDashboardService

// Async methods - 'Async' suffix
public async Task<User> GetUserAsync(string id)
public async Task<bool> ValidateUserAsync(User user)
```

#### File Organization
```
DataLens/
├── Controllers/
│   ├── AccountController.cs
│   ├── HomeController.cs
│   └── BaseController.cs
├── Models/
│   ├── User.cs
│   ├── Dashboard.cs
│   └── ViewModels/
│       ├── LoginViewModel.cs
│       └── DashboardViewModel.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IUserService.cs
│   │   └── IDashboardService.cs
│   └── Implementations/
│       ├── UserService.cs
│       └── DashboardService.cs
├── Repositories/
│   ├── Interfaces/
│   │   ├── IUserRepository.cs
│   │   └── IDashboardRepository.cs
│   ├── SqlServer/
│   │   ├── UserRepository.cs
│   │   └── DashboardRepository.cs
│   └── MongoDB/
│       ├── UserRepository.cs
│       └── DashboardRepository.cs
├── Areas/
│   ├── Admin/
│   ├── Dashboard/
│   └── Profile/
├── Middleware/
│   ├── JwtCookieMiddleware.cs
│   ├── ErrorHandlingMiddleware.cs
│   └── RequestLoggingMiddleware.cs
├── Configuration/
│   ├── DatabaseSettings.cs
│   ├── JwtSettings.cs
│   └── DevExpressSettings.cs
├── Extensions/
│   ├── ServiceCollectionExtensions.cs
│   └── ApplicationBuilderExtensions.cs
├── Utilities/
│   ├── PasswordHasher.cs
│   ├── JwtTokenGenerator.cs
│   └── EmailSender.cs
└── wwwroot/
    ├── css/
    ├── js/
    ├── images/
    └── lib/
```

### 2. Error Handling ve Logging

#### Global Exception Handling
```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred. RequestId: {RequestId}", 
                context.TraceIdentifier);
            
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ErrorResponse
        {
            RequestId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };
        
        switch (exception)
        {
            case ValidationException validationEx:
                response.StatusCode = 400;
                response.Message = "Validation failed";
                response.Details = validationEx.Errors;
                break;
                
            case UnauthorizedAccessException:
                response.StatusCode = 401;
                response.Message = "Unauthorized access";
                break;
                
            case NotFoundException notFoundEx:
                response.StatusCode = 404;
                response.Message = notFoundEx.Message;
                break;
                
            case BusinessLogicException businessEx:
                response.StatusCode = 422;
                response.Message = businessEx.Message;
                break;
                
            default:
                response.StatusCode = 500;
                response.Message = "An internal server error occurred";
                break;
        }
        
        context.Response.StatusCode = response.StatusCode;
        
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        await context.Response.WriteAsync(jsonResponse);
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public object? Details { get; set; }
}
```

#### Structured Logging
```csharp
public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    
    public async Task<User?> GetUserByIdAsync(string userId)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["UserId"] = userId,
            ["Operation"] = nameof(GetUserByIdAsync)
        });
        
        try
        {
            _logger.LogInformation("Retrieving user with ID: {UserId}", userId);
            
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", userId);
                return null;
            }
            
            _logger.LogInformation("Successfully retrieved user: {Username}", user.Username);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID: {UserId}", userId);
            throw;
        }
    }
    
    public async Task<string> CreateUserAsync(User user, string password)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Username"] = user.Username,
            ["Email"] = user.Email,
            ["Operation"] = nameof(CreateUserAsync)
        });
        
        try
        {
            _logger.LogInformation("Creating new user: {Username}", user.Username);
            
            // Validation
            if (await _userRepository.ExistsByUsernameAsync(user.Username))
            {
                _logger.LogWarning("Attempt to create user with existing username: {Username}", user.Username);
                throw new BusinessLogicException($"Username '{user.Username}' already exists");
            }
            
            if (await _userRepository.ExistsByEmailAsync(user.Email))
            {
                _logger.LogWarning("Attempt to create user with existing email: {Email}", user.Email);
                throw new BusinessLogicException($"Email '{user.Email}' already exists");
            }
            
            // Hash password
            user.PasswordHash = _passwordHasher.HashPassword(password);
            user.CreatedDate = DateTime.UtcNow;
            user.IsActive = true;
            
            var userId = await _userRepository.CreateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Successfully created user: {Username} with ID: {UserId}", 
                user.Username, userId);
            
            // Audit log
            await _auditService.LogAsync("Create", "User", userId, null, new
            {
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            });
            
            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Username}", user.Username);
            throw;
        }
    }
}
```

### 3. Performance Optimization

#### Caching Strategy
```csharp
public class CachedUserService : IUserService
{
    private readonly IUserService _userService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedUserService> _logger;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);
    
    public CachedUserService(
        IUserService userService,
        IMemoryCache cache,
        ILogger<CachedUserService> logger)
    {
        _userService = userService;
        _cache = cache;
        _logger = logger;
    }
    
    public async Task<User?> GetUserByIdAsync(string userId)
    {
        var cacheKey = $"user:{userId}";
        
        if (_cache.TryGetValue(cacheKey, out User? cachedUser))
        {
            _logger.LogDebug("User found in cache: {UserId}", userId);
            return cachedUser;
        }
        
        var user = await _userService.GetUserByIdAsync(userId);
        
        if (user != null)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpiration,
                SlidingExpiration = TimeSpan.FromMinutes(10),
                Priority = CacheItemPriority.Normal
            };
            
            _cache.Set(cacheKey, user, cacheOptions);
            _logger.LogDebug("User cached: {UserId}", userId);
        }
        
        return user;
    }
    
    public async Task<string> CreateUserAsync(User user, string password)
    {
        var userId = await _userService.CreateUserAsync(user, password);
        
        // Cache the newly created user
        var cacheKey = $"user:{userId}";
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheExpiration,
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };
        
        user.Id = userId;
        _cache.Set(cacheKey, user, cacheOptions);
        
        return userId;
    }
    
    public async Task UpdateUserAsync(User user)
    {
        await _userService.UpdateUserAsync(user);
        
        // Invalidate cache
        var cacheKey = $"user:{user.Id}";
        _cache.Remove(cacheKey);
        _logger.LogDebug("User cache invalidated: {UserId}", user.Id);
    }
}
```

#### Database Query Optimization
```csharp
public class OptimizedDashboardRepository : IDashboardRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<OptimizedDashboardRepository> _logger;
    
    public async Task<IEnumerable<Dashboard>> GetUserDashboardsAsync(string userId, int page = 1, int pageSize = 20)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT d.Id, d.Name, d.Title, d.Description, d.CreatedDate, d.IsActive,
                   u.Username as CreatedByUsername
            FROM Dashboards d
            INNER JOIN Users u ON d.CreatedBy = u.Id
            INNER JOIN DashboardPermissions dp ON d.Id = dp.DashboardId
            WHERE dp.UserId = @UserId 
                AND d.IsActive = 1
                AND dp.PermissionType IN ('Owner', 'Editor', 'Viewer')
            ORDER BY d.CreatedDate DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY
        ";
        
        var parameters = new
        {
            UserId = userId,
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        };
        
        var stopwatch = Stopwatch.StartNew();
        var dashboards = await connection.QueryAsync<Dashboard>(sql, parameters);
        stopwatch.Stop();
        
        _logger.LogDebug("Retrieved {Count} dashboards for user {UserId} in {ElapsedMs}ms", 
            dashboards.Count(), userId, stopwatch.ElapsedMilliseconds);
        
        return dashboards;
    }
    
    public async Task<Dashboard?> GetDashboardWithDataAsync(string dashboardId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Use multiple result sets to avoid N+1 queries
        var sql = @"
            SELECT Id, Name, Title, Description, DashboardData, CreatedBy, CreatedDate, IsActive
            FROM Dashboards 
            WHERE Id = @DashboardId AND IsActive = 1;
            
            SELECT dp.UserId, dp.PermissionType, u.Username
            FROM DashboardPermissions dp
            INNER JOIN Users u ON dp.UserId = u.Id
            WHERE dp.DashboardId = @DashboardId;
        ";
        
        using var multi = await connection.QueryMultipleAsync(sql, new { DashboardId = dashboardId });
        
        var dashboard = await multi.ReadSingleOrDefaultAsync<Dashboard>();
        if (dashboard == null) return null;
        
        var permissions = await multi.ReadAsync<DashboardPermission>();
        dashboard.Permissions = permissions.ToList();
        
        return dashboard;
    }
}
```

### 4. Security Best Practices

#### Input Validation
```csharp
public class SafeStringAttribute : ValidationAttribute
{
    private static readonly Regex SafeStringRegex = new Regex(@"^[a-zA-Z0-9\s\-_\.@]+$", RegexOptions.Compiled);
    
    public override bool IsValid(object? value)
    {
        if (value == null) return true; // Use [Required] for null checks
        
        var stringValue = value.ToString();
        if (string.IsNullOrWhiteSpace(stringValue)) return true;
        
        return SafeStringRegex.IsMatch(stringValue);
    }
    
    public override string FormatErrorMessage(string name)
    {
        return $"{name} contains invalid characters. Only letters, numbers, spaces, hyphens, underscores, dots, and @ symbols are allowed.";
    }
}

public class LoginModel
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [SafeString]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; } = string.Empty;
    
    public bool RememberMe { get; set; }
}

public class CreateUserModel
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3)]
    [SafeString]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", 
        ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, one digit, and one special character")]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Role is required")]
    [RegularExpression(@"^(Admin|Designer|User)$", ErrorMessage = "Invalid role")]
    public string Role { get; set; } = "User";
}
```

#### SQL Injection Prevention
```csharp
public class SecureUserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        // GOOD: Using parameterized queries
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = "SELECT * FROM Users WHERE Username = @Username AND IsActive = 1";
        var user = await connection.QuerySingleOrDefaultAsync<User>(sql, new { Username = username });
        
        return user;
    }
    
    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, string role, int page, int pageSize)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var whereConditions = new List<string> { "IsActive = 1" };
        var parameters = new DynamicParameters();
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            // Safe LIKE query with parameterization
            whereConditions.Add("(Username LIKE @SearchTerm OR Email LIKE @SearchTerm)");
            parameters.Add("SearchTerm", $"%{searchTerm.Replace("%", "[%]").Replace("_", "[_]")}%");
        }
        
        if (!string.IsNullOrWhiteSpace(role))
        {
            whereConditions.Add("Role = @Role");
            parameters.Add("Role", role);
        }
        
        parameters.Add("Offset", (page - 1) * pageSize);
        parameters.Add("PageSize", pageSize);
        
        var sql = $@"
            SELECT * FROM Users 
            WHERE {string.Join(" AND ", whereConditions)}
            ORDER BY CreatedDate DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY
        ";
        
        return await connection.QueryAsync<User>(sql, parameters);
    }
}
```

### 5. Code Quality ve Maintainability

#### SOLID Principles Implementation
```csharp
// Single Responsibility Principle
public class UserValidator
{
    public ValidationResult ValidateUser(User user)
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrWhiteSpace(user.Username))
            result.AddError("Username is required");
        
        if (string.IsNullOrWhiteSpace(user.Email))
            result.AddError("Email is required");
        
        if (!IsValidEmail(user.Email))
            result.AddError("Invalid email format");
        
        return result;
    }
    
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

// Open/Closed Principle
public abstract class NotificationSender
{
    public abstract Task SendAsync(string recipient, string subject, string message);
}

public class EmailNotificationSender : NotificationSender
{
    private readonly IEmailService _emailService;
    
    public EmailNotificationSender(IEmailService emailService)
    {
        _emailService = emailService;
    }
    
    public override async Task SendAsync(string recipient, string subject, string message)
    {
        await _emailService.SendEmailAsync(recipient, subject, message);
    }
}

public class SmsNotificationSender : NotificationSender
{
    private readonly ISmsService _smsService;
    
    public SmsNotificationSender(ISmsService smsService)
    {
        _smsService = smsService;
    }
    
    public override async Task SendAsync(string recipient, string subject, string message)
    {
        await _smsService.SendSmsAsync(recipient, $"{subject}: {message}");
    }
}

// Dependency Inversion Principle
public class NotificationService
{
    private readonly IEnumerable<NotificationSender> _senders;
    
    public NotificationService(IEnumerable<NotificationSender> senders)
    {
        _senders = senders;
    }
    
    public async Task SendNotificationAsync(string type, string recipient, string subject, string message)
    {
        var sender = _senders.FirstOrDefault(s => s.GetType().Name.StartsWith(type));
        if (sender != null)
        {
            await sender.SendAsync(recipient, subject, message);
        }
    }
}
```

#### Clean Code Practices
```csharp
// BAD: Long method with multiple responsibilities
public async Task<bool> ProcessUserRegistrationBad(string username, string email, string password, string role)
{
    // Validation
    if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
        return false;
    if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
        return false;
    if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        return false;
    
    // Check if user exists
    var existingUser = await _userRepository.GetByUsernameAsync(username);
    if (existingUser != null)
        return false;
    
    var existingEmail = await _userRepository.GetByEmailAsync(email);
    if (existingEmail != null)
        return false;
    
    // Hash password
    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
    
    // Create user
    var user = new User
    {
        Id = Guid.NewGuid().ToString(),
        Username = username,
        Email = email,
        PasswordHash = hashedPassword,
        Role = role,
        CreatedDate = DateTime.UtcNow,
        IsActive = true
    };
    
    await _userRepository.CreateAsync(user);
    
    // Send welcome email
    await _emailService.SendWelcomeEmailAsync(email, username);
    
    // Log activity
    _logger.LogInformation($"User {username} registered successfully");
    
    return true;
}

// GOOD: Clean, single responsibility methods
public async Task<RegistrationResult> ProcessUserRegistrationAsync(UserRegistrationRequest request)
{
    var validationResult = await ValidateRegistrationRequestAsync(request);
    if (!validationResult.IsValid)
        return RegistrationResult.Failed(validationResult.Errors);
    
    var user = await CreateUserAsync(request);
    await SendWelcomeNotificationAsync(user);
    
    _logger.LogInformation("User {Username} registered successfully with ID {UserId}", 
        user.Username, user.Id);
    
    return RegistrationResult.Success(user.Id);
}

private async Task<ValidationResult> ValidateRegistrationRequestAsync(UserRegistrationRequest request)
{
    var validator = new UserRegistrationValidator(_userRepository);
    return await validator.ValidateAsync(request);
}

private async Task<User> CreateUserAsync(UserRegistrationRequest request)
{
    var user = new User
    {
        Id = Guid.NewGuid().ToString(),
        Username = request.Username,
        Email = request.Email,
        PasswordHash = _passwordHasher.HashPassword(request.Password),
        Role = request.Role,
        CreatedDate = DateTime.UtcNow,
        IsActive = true
    };
    
    await _userRepository.CreateAsync(user);
    await _unitOfWork.SaveChangesAsync();
    
    return user;
}

private async Task SendWelcomeNotificationAsync(User user)
{
    try
    {
        await _notificationService.SendWelcomeNotificationAsync(user.Email, user.Username);
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Failed to send welcome notification to {Email}", user.Email);
        // Don't fail registration if notification fails
    }
}
```

### 6. Documentation Standards

#### XML Documentation
```csharp
/// <summary>
/// Service for managing user operations including authentication, registration, and profile management.
/// </summary>
/// <remarks>
/// This service handles all user-related business logic and coordinates with the repository layer
/// for data persistence. It includes password hashing, validation, and audit logging.
/// </remarks>
public class UserService : IUserService
{
    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to retrieve.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// the user if found; otherwise, null.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when userId is null or empty.</exception>
    /// <example>
    /// <code>
    /// var user = await userService.GetUserByIdAsync("user-123");
    /// if (user != null)
    /// {
    ///     Console.WriteLine($"Found user: {user.Username}");
    /// }
    /// </code>
    /// </example>
    public async Task<User?> GetUserByIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        
        return await _userRepository.GetByIdAsync(userId);
    }
    
    /// <summary>
    /// Creates a new user with the specified details and password.
    /// </summary>
    /// <param name="user">The user object containing the user details.</param>
    /// <param name="password">The plain text password for the user.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// the unique identifier of the created user.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when user is null.</exception>
    /// <exception cref="ArgumentException">Thrown when password is null or empty.</exception>
    /// <exception cref="BusinessLogicException">Thrown when username or email already exists.</exception>
    /// <example>
    /// <code>
    /// var user = new User
    /// {
    ///     Username = "johndoe",
    ///     Email = "john@example.com",
    ///     Role = "User"
    /// };
    /// 
    /// var userId = await userService.CreateUserAsync(user, "SecurePassword123!");
    /// Console.WriteLine($"Created user with ID: {userId}");
    /// </code>
    /// </example>
    public async Task<string> CreateUserAsync(User user, string password)
    {
        // Implementation...
    }
}
```

#### README Documentation
```markdown
# DataLens Dashboard

A comprehensive dashboard application built with ASP.NET Core and DevExpress components.

## Features

- **Multi-Database Support**: SQL Server, PostgreSQL, and MongoDB
- **Role-Based Access Control**: Admin, Designer, and User roles
- **Dashboard Designer**: Drag-and-drop dashboard creation with DevExpress
- **Real-time Updates**: SignalR integration for live data
- **Responsive Design**: Mobile-friendly interface
- **Security**: JWT authentication, input validation, and audit logging

## Quick Start

### Prerequisites

- .NET 8.0 SDK
- SQL Server, PostgreSQL, or MongoDB
- Visual Studio 2022 or VS Code

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/your-org/DataLensDashboard.git
   cd DataLensDashboard
   ```

2. Configure the database connection in `appsettings.json`:
   ```json
   {
     "DatabaseSettings": {
       "DatabaseType": "SqlServer"
     },
     "ConnectionStrings": {
       "SqlServer": "Server=localhost;Database=DataLensDashboard;Trusted_Connection=true;"
     }
   }
   ```

3. Run database migrations:
   ```bash
   dotnet run --migrate
   ```

4. Start the application:
   ```bash
   dotnet run
   ```

5. Open your browser and navigate to `https://localhost:5001`

### Default Login

- **Username**: admin
- **Password**: Admin123!

## Architecture

The application follows a layered architecture pattern:

```
┌─────────────────┐
│   Presentation  │  Controllers, Views, Areas
├─────────────────┤
│    Business     │  Services, Business Logic
├─────────────────┤
│   Data Access   │  Repositories, Unit of Work
├─────────────────┤
│    Database     │  SQL Server, PostgreSQL, MongoDB
└─────────────────┘
```

## Configuration

### Database Types

Supported database types:
- `SqlServer`: Microsoft SQL Server
- `PostgreSQL`: PostgreSQL
- `MongoDB`: MongoDB

### JWT Configuration

```json
{
  "Jwt": {
    "Key": "your-secret-key-here",
    "Issuer": "DataLensDashboard",
    "Audience": "DataLensDashboard",
    "ExpirationMinutes": 1440
  }
}
```

## Development

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Code Style

This project follows the [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions).

### Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## Deployment

### Docker

```bash
# Build image
docker build -t datalens-dashboard .

# Run container
docker run -p 80:80 datalens-dashboard
```

### Production Considerations

- Use HTTPS in production
- Configure proper logging levels
- Set up database backups
- Monitor application performance
- Implement rate limiting

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
```

### 7. Git Workflow

#### Branch Strategy
```
main
├── develop
│   ├── feature/user-management
│   ├── feature/dashboard-designer
│   └── feature/notification-system
├── hotfix/security-patch
└── release/v1.0.0
```

#### Commit Message Convention
```
type(scope): description

[optional body]

[optional footer]
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

Examples:
```
feat(auth): add JWT token refresh functionality

Implement automatic token refresh when tokens are about to expire.
This improves user experience by preventing unexpected logouts.

Closes #123

fix(dashboard): resolve data loading issue in dashboard viewer

The dashboard viewer was not properly handling null data responses
from the API, causing the application to crash.

Fixes #456

docs(readme): update installation instructions

Add missing steps for database migration and clarify
prerequisites for development setup.
```

#### Pull Request Template
```markdown
## Description

Brief description of the changes made.

## Type of Change

- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update

## Testing

- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Manual testing completed
- [ ] Code coverage maintained/improved

## Checklist

- [ ] Code follows the project's coding standards
- [ ] Self-review of code completed
- [ ] Code is properly commented
- [ ] Documentation updated (if applicable)
- [ ] No new warnings introduced
- [ ] Security considerations addressed

## Screenshots (if applicable)

## Related Issues

Closes #issue_number
```

### 8. Monitoring ve Analytics

#### Application Performance Monitoring
```csharp
public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
    private readonly DiagnosticSource _diagnosticSource;
    
    public PerformanceMonitoringMiddleware(
        RequestDelegate next,
        ILogger<PerformanceMonitoringMiddleware> logger,
        DiagnosticSource diagnosticSource)
    {
        _next = next;
        _logger = logger;
        _diagnosticSource = diagnosticSource;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = context.TraceIdentifier;
        
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            var statusCode = context.Response.StatusCode;
            var method = context.Request.Method;
            var path = context.Request.Path;
            
            _logger.LogInformation(
                "Request {RequestId} {Method} {Path} completed in {ElapsedMs}ms with status {StatusCode}",
                requestId, method, path, elapsedMs, statusCode);
            
            // Send metrics to monitoring system
            _diagnosticSource.Write("Request.Completed", new
            {
                RequestId = requestId,
                Method = method,
                Path = path.Value,
                StatusCode = statusCode,
                ElapsedMilliseconds = elapsedMs,
                Timestamp = DateTimeOffset.UtcNow
            });
            
            // Alert on slow requests
            if (elapsedMs > 5000) // 5 seconds
            {
                _logger.LogWarning(
                    "Slow request detected: {RequestId} {Method} {Path} took {ElapsedMs}ms",
                    requestId, method, path, elapsedMs);
            }
        }
    }
}
```

Bu dosya, DataLens Dashboard projesinin genel kurallarını, best practice'lerini ve geliştirme standartlarını kapsamlı bir şekilde tanımlamaktadır.