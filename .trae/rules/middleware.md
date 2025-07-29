# DataLens Dashboard - Middleware ve Güvenlik Kuralları

## Middleware Kuralları

### 1. JWT Cookie Middleware

#### JwtCookieMiddleware Implementation
```csharp
public class JwtCookieMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtCookieMiddleware> _logger;
    private readonly string _cookieName;

    public JwtCookieMiddleware(RequestDelegate next, ILogger<JwtCookieMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _cookieName = configuration["Authentication:CookieName"] ?? "DataLensAuth";
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // JWT token'ı cookie'den al
            if (context.Request.Cookies.TryGetValue(_cookieName, out var token) && !string.IsNullOrEmpty(token))
            {
                // Authorization header'ı kontrol et
                if (!context.Request.Headers.ContainsKey("Authorization"))
                {
                    context.Request.Headers.Add("Authorization", $"Bearer {token}");
                }
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in JWT Cookie Middleware");
            await _next(context);
        }
    }
}
```

#### Middleware Registration
```csharp
// Program.cs
app.UseMiddleware<JwtCookieMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
```

### 2. Request Logging Middleware

#### RequestLoggingMiddleware
```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString();
        
        // Request bilgilerini logla
        _logger.LogInformation("Request {RequestId} started: {Method} {Path} from {RemoteIpAddress}",
            requestId,
            context.Request.Method,
            context.Request.Path,
            context.Connection.RemoteIpAddress);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            // Response bilgilerini logla
            _logger.LogInformation("Request {RequestId} completed: {StatusCode} in {ElapsedMilliseconds}ms",
                requestId,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
```

### 3. Error Handling Middleware

#### GlobalExceptionMiddleware
```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ErrorResponse();
        
        switch (exception)
        {
            case UnauthorizedAccessException:
                response.StatusCode = 401;
                response.Message = "Unauthorized access";
                break;
            case ArgumentException:
                response.StatusCode = 400;
                response.Message = "Bad request";
                break;
            case KeyNotFoundException:
                response.StatusCode = 404;
                response.Message = "Resource not found";
                break;
            default:
                response.StatusCode = 500;
                response.Message = "An error occurred while processing your request";
                break;
        }

        if (_environment.IsDevelopment())
        {
            response.Details = exception.ToString();
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
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

### 4. Rate Limiting Middleware

#### RateLimitingMiddleware
```csharp
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitOptions _options;

    public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache, ILogger<RateLimitingMiddleware> logger, IOptions<RateLimitOptions> options)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var key = $"rate_limit_{clientId}";
        
        if (_cache.TryGetValue(key, out int requestCount))
        {
            if (requestCount >= _options.MaxRequests)
            {
                _logger.LogWarning("Rate limit exceeded for client: {ClientId}", clientId);
                context.Response.StatusCode = 429; // Too Many Requests
                await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                return;
            }
            
            _cache.Set(key, requestCount + 1, TimeSpan.FromMinutes(_options.WindowMinutes));
        }
        else
        {
            _cache.Set(key, 1, TimeSpan.FromMinutes(_options.WindowMinutes));
        }

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // IP adresi ve kullanıcı ID'si kombinasyonu
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userId = context.User?.FindFirst("UserId")?.Value ?? "anonymous";
        return $"{ipAddress}_{userId}";
    }
}

public class RateLimitOptions
{
    public int MaxRequests { get; set; } = 100;
    public int WindowMinutes { get; set; } = 1;
}
```

### 5. Security Headers Middleware

#### SecurityHeadersMiddleware
```csharp
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Security headers ekle
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Add("Content-Security-Policy", 
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; " +
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
            "font-src 'self' https://fonts.gstatic.com; " +
            "img-src 'self' data: https:; " +
            "connect-src 'self';");
        
        // HTTPS yönlendirmesi için
        if (!context.Request.IsHttps && !context.Request.Host.Host.Contains("localhost"))
        {
            context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        }

        await _next(context);
    }
}
```

## Güvenlik Kuralları

### 1. Authentication Configuration

#### JWT Authentication Setup
```csharp
// Program.cs
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ClockSkew = TimeSpan.Zero
    };
    
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new { message = "You are not authorized" });
                return context.Response.WriteAsync(result);
            }
            return Task.CompletedTask;
        }
    };
});
```

### 2. Authorization Policies

#### Custom Authorization Policies
```csharp
builder.Services.AddAuthorization(options =>
{
    // Role-based policies
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Manager", "Admin"));
    options.AddPolicy("DashboardDesigner", policy => policy.RequireRole("Designer", "Manager", "Admin"));
    
    // Custom policies
    options.AddPolicy("CanViewDashboards", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "dashboard.view") ||
            context.User.IsInRole("Admin")));
    
    options.AddPolicy("CanEditDashboards", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "dashboard.edit") ||
            context.User.IsInRole("Designer") ||
            context.User.IsInRole("Admin")));
    
    options.AddPolicy("CanDeleteDashboards", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "dashboard.delete") ||
            context.User.IsInRole("Admin")));
});
```

### 3. Input Validation

#### Model Validation Attributes
```csharp
public class SafeStringAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value is string stringValue)
        {
            // XSS ve SQL injection kontrolü
            var dangerousPatterns = new[]
            {
                "<script", "</script>", "javascript:", "vbscript:",
                "onload=", "onerror=", "onclick=", "onmouseover=",
                "''", "--", "/*", "*/", "xp_", "sp_"
            };
            
            return !dangerousPatterns.Any(pattern => 
                stringValue.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }
        return true;
    }
    
    public override string FormatErrorMessage(string name)
    {
        return $"{name} contains potentially dangerous content.";
    }
}

// Kullanım
public class LoginModel
{
    [Required(ErrorMessage = "Username is required")]
    [SafeString]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}
```

### 4. Password Security

#### Password Hashing Service
```csharp
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 20;
    private const int Iterations = 10000;

    public string HashPassword(string password)
    {
        // Salt oluştur
        var salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Hash oluştur
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
        var hash = pbkdf2.GetBytes(HashSize);

        // Salt ve hash'i birleştir
        var hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            var hashBytes = Convert.FromBase64String(hash);
            
            // Salt'ı ayır
            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);
            
            // Hash'i hesapla
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
            var computedHash = pbkdf2.GetBytes(HashSize);
            
            // Karşılaştır
            for (int i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + SaltSize] != computedHash[i])
                    return false;
            }
            
            return true;
        }
        catch
        {
            return false;
        }
    }
}
```

### 5. CSRF Protection

#### CSRF Token Implementation
```csharp
// Program.cs
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "__RequestVerificationToken";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Controller'da kullanım
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CreateDashboard([FromBody] CreateDashboardRequest request)
{
    // Implementation
}

// AJAX için token alma
[HttpGet]
public IActionResult GetCsrfToken()
{
    var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
    return Json(new { token = tokens.RequestToken });
}
```

### 6. Session Security

#### Session Configuration
```csharp
// Program.cs
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Session helper
public static class SessionExtensions
{
    public static void SetObject<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T? GetObject<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }
}
```

### 7. API Security

#### API Key Authentication
```csharp
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>
{
    private readonly IApiKeyService _apiKeyService;

    public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IApiKeyService apiKeyService)
        : base(options, logger, encoder, clock)
    {
        _apiKeyService = apiKeyService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeaderValues))
        {
            return AuthenticateResult.NoResult();
        }

        var providedApiKey = apiKeyHeaderValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            return AuthenticateResult.NoResult();
        }

        var isValidApiKey = await _apiKeyService.IsValidApiKeyAsync(providedApiKey);
        if (!isValidApiKey)
        {
            return AuthenticateResult.Fail("Invalid API Key");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "API User"),
            new Claim("ApiKey", providedApiKey)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
```

### 8. Data Protection

#### Sensitive Data Encryption
```csharp
public interface IDataProtectionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}

public class DataProtectionService : IDataProtectionService
{
    private readonly IDataProtector _protector;

    public DataProtectionService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("DataLens.SensitiveData");
    }

    public string Encrypt(string plainText)
    {
        return _protector.Protect(plainText);
    }

    public string Decrypt(string cipherText)
    {
        return _protector.Unprotect(cipherText);
    }
}

// Program.cs
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"./keys/"))
    .SetApplicationName("DataLensDashboard");
```

### 9. Audit Logging

#### Audit Trail Implementation
```csharp
public class AuditLogEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}

public interface IAuditService
{
    Task LogAsync(string action, string entityType, string entityId, object? oldValues = null, object? newValues = null);
}

public class AuditService : IAuditService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditService> _logger;
    // Repository injection

    public async Task LogAsync(string action, string entityType, string entityId, object? oldValues = null, object? newValues = null)
    {
        var context = _httpContextAccessor.HttpContext;
        var userId = context?.User?.FindFirst("UserId")?.Value ?? "System";
        
        var auditEntry = new AuditLogEntry
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            IpAddress = context?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown",
            UserAgent = context?.Request?.Headers["User-Agent"].ToString() ?? "Unknown"
        };

        // Save to database
        await SaveAuditLogAsync(auditEntry);
        
        _logger.LogInformation("Audit log created: {Action} on {EntityType} {EntityId} by {UserId}", 
            action, entityType, entityId, userId);
    }
}
```

### 10. Security Monitoring

#### Security Event Detection
```csharp
public class SecurityMonitoringService
{
    private readonly ILogger<SecurityMonitoringService> _logger;
    private readonly IMemoryCache _cache;

    public async Task DetectSuspiciousActivity(string userId, string action, string ipAddress)
    {
        var key = $"security_monitor_{userId}_{ipAddress}";
        
        if (_cache.TryGetValue(key, out List<SecurityEvent> events))
        {
            events.Add(new SecurityEvent { Action = action, Timestamp = DateTime.UtcNow });
            
            // Son 5 dakikada 10'dan fazla başarısız giriş
            var recentFailedLogins = events
                .Where(e => e.Action == "FailedLogin" && e.Timestamp > DateTime.UtcNow.AddMinutes(-5))
                .Count();
            
            if (recentFailedLogins > 10)
            {
                await HandleSuspiciousActivity(userId, ipAddress, "Multiple failed login attempts");
            }
        }
        else
        {
            events = new List<SecurityEvent> { new SecurityEvent { Action = action, Timestamp = DateTime.UtcNow } };
        }
        
        _cache.Set(key, events, TimeSpan.FromHours(1));
    }

    private async Task HandleSuspiciousActivity(string userId, string ipAddress, string reason)
    {
        _logger.LogWarning("Suspicious activity detected: {Reason} for user {UserId} from {IpAddress}", 
            reason, userId, ipAddress);
        
        // IP'yi geçici olarak engelle
        await BlockIpTemporarily(ipAddress);
        
        // Admin'e bildirim gönder
        await NotifyAdministrators(userId, ipAddress, reason);
    }
}

public class SecurityEvent
{
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
```