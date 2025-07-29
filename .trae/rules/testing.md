# DataLens Dashboard - Test ve Deployment Kuralları

## Test Kuralları

### 1. Unit Testing

#### Test Project Structure
```
DataLens.Tests/
├── Unit/
│   ├── Controllers/
│   ├── Services/
│   ├── Repositories/
│   ├── Models/
│   └── Middleware/
├── Integration/
│   ├── Controllers/
│   ├── Database/
│   └── API/
├── Fixtures/
├── Helpers/
└── TestData/
```

#### Base Test Classes
```csharp
public abstract class BaseUnitTest
{
    protected readonly Mock<ILogger> MockLogger;
    protected readonly Mock<IConfiguration> MockConfiguration;
    
    protected BaseUnitTest()
    {
        MockLogger = new Mock<ILogger>();
        MockConfiguration = new Mock<IConfiguration>();
        SetupMockConfiguration();
    }
    
    protected virtual void SetupMockConfiguration()
    {
        MockConfiguration.Setup(c => c["DatabaseSettings:DatabaseType"]).Returns("SqlServer");
        MockConfiguration.Setup(c => c["Jwt:Key"]).Returns("test-key-that-is-at-least-32-characters-long");
        MockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        MockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
    }
}

public abstract class BaseIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    
    protected BaseIntegrationTest(WebApplicationFactory<Program> factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }
    
    protected async Task<string> GetJwtTokenAsync(string username = "testuser", string role = "User")
    {
        // JWT token oluşturma logic'i
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("test-key-that-is-at-least-32-characters-long");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("UserId", Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    
    protected void SetAuthorizationHeader(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
```

#### Service Unit Tests
```csharp
public class UserServiceTests : BaseUnitTest
{
    private readonly Mock<IUnitOfWorkFactory> _mockUnitOfWorkFactory;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly UserService _userService;
    
    public UserServiceTests()
    {
        _mockUnitOfWorkFactory = new Mock<IUnitOfWorkFactory>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockAuditService = new Mock<IAuditService>();
        
        _mockUnitOfWorkFactory.Setup(f => f.Create()).Returns(_mockUnitOfWork.Object);
        _mockUnitOfWork.Setup(u => u.Users).Returns(_mockUserRepository.Object);
        
        _userService = new UserService(
            _mockUnitOfWorkFactory.Object,
            _mockPasswordHasher.Object,
            _mockAuditService.Object,
            MockLogger.Object.As<ILogger<UserService>>());
    }
    
    [Fact]
    public async Task GetUserByIdAsync_ValidId_ReturnsUser()
    {
        // Arrange
        var userId = "test-user-id";
        var expectedUser = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            Role = "User"
        };
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(expectedUser);
        
        // Act
        var result = await _userService.GetUserByIdAsync(userId);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUser.Id, result.Id);
        Assert.Equal(expectedUser.Username, result.Username);
        Assert.Equal(expectedUser.Email, result.Email);
        
        _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }
    
    [Fact]
    public async Task GetUserByIdAsync_InvalidId_ReturnsNull()
    {
        // Arrange
        var userId = "invalid-id";
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);
        
        // Act
        var result = await _userService.GetUserByIdAsync(userId);
        
        // Assert
        Assert.Null(result);
        _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }
    
    [Fact]
    public async Task CreateUserAsync_ValidUser_ReturnsUserId()
    {
        // Arrange
        var user = new User
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Role = "User"
        };
        var password = "Password123!";
        var hashedPassword = "hashed-password";
        var expectedUserId = "new-user-id";
        
        _mockPasswordHasher.Setup(h => h.HashPassword(password))
            .Returns(hashedPassword);
        _mockUserRepository.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(expectedUserId);
        
        // Act
        var result = await _userService.CreateUserAsync(user, password);
        
        // Assert
        Assert.Equal(expectedUserId, result);
        Assert.Equal(hashedPassword, user.PasswordHash);
        
        _mockPasswordHasher.Verify(h => h.HashPassword(password), Times.Once);
        _mockUserRepository.Verify(r => r.CreateAsync(It.Is<User>(u => u.PasswordHash == hashedPassword)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockAuditService.Verify(a => a.LogAsync("Create", "User", expectedUserId, null, It.IsAny<object>()), Times.Once);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateUserAsync_InvalidPassword_ThrowsArgumentException(string password)
    {
        // Arrange
        var user = new User { Username = "testuser", Email = "test@example.com" };
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _userService.CreateUserAsync(user, password));
    }
}
```

#### Controller Unit Tests
```csharp
public class AccountControllerTests : BaseUnitTest
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly AccountController _controller;
    
    public AccountControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockJwtService = new Mock<IJwtService>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        
        _controller = new AccountController(
            _mockUserService.Object,
            _mockJwtService.Object,
            _mockPasswordHasher.Object,
            MockLogger.Object.As<ILogger<AccountController>>());
        
        // Mock HttpContext
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }
    
    [Fact]
    public async Task Login_ValidCredentials_ReturnsSuccessWithToken()
    {
        // Arrange
        var loginModel = new LoginModel
        {
            Username = "testuser",
            Password = "password123"
        };
        
        var user = new User
        {
            Id = "user-id",
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashed-password",
            IsActive = true
        };
        
        var token = "jwt-token";
        
        _mockUserService.Setup(s => s.GetUserByUsernameAsync(loginModel.Username))
            .ReturnsAsync(user);
        _mockPasswordHasher.Setup(h => h.VerifyPassword(loginModel.Password, user.PasswordHash))
            .Returns(true);
        _mockJwtService.Setup(s => s.GenerateTokenAsync(user))
            .ReturnsAsync(token);
        
        // Act
        var result = await _controller.Login(loginModel);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        
        Assert.NotNull(response);
        
        _mockUserService.Verify(s => s.GetUserByUsernameAsync(loginModel.Username), Times.Once);
        _mockPasswordHasher.Verify(h => h.VerifyPassword(loginModel.Password, user.PasswordHash), Times.Once);
        _mockJwtService.Verify(s => s.GenerateTokenAsync(user), Times.Once);
        _mockUserService.Verify(s => s.UpdateLastLoginAsync(user.Id), Times.Once);
    }
    
    [Fact]
    public async Task Login_InvalidUsername_ReturnsUnauthorized()
    {
        // Arrange
        var loginModel = new LoginModel
        {
            Username = "invaliduser",
            Password = "password123"
        };
        
        _mockUserService.Setup(s => s.GetUserByUsernameAsync(loginModel.Username))
            .ReturnsAsync((User?)null);
        
        // Act
        var result = await _controller.Login(loginModel);
        
        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid username or password", unauthorizedResult.Value);
        
        _mockUserService.Verify(s => s.GetUserByUsernameAsync(loginModel.Username), Times.Once);
        _mockPasswordHasher.Verify(h => h.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var loginModel = new LoginModel
        {
            Username = "testuser",
            Password = "wrongpassword"
        };
        
        var user = new User
        {
            Id = "user-id",
            Username = "testuser",
            PasswordHash = "hashed-password",
            IsActive = true
        };
        
        _mockUserService.Setup(s => s.GetUserByUsernameAsync(loginModel.Username))
            .ReturnsAsync(user);
        _mockPasswordHasher.Setup(h => h.VerifyPassword(loginModel.Password, user.PasswordHash))
            .Returns(false);
        
        // Act
        var result = await _controller.Login(loginModel);
        
        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid username or password", unauthorizedResult.Value);
        
        _mockPasswordHasher.Verify(h => h.VerifyPassword(loginModel.Password, user.PasswordHash), Times.Once);
        _mockJwtService.Verify(s => s.GenerateTokenAsync(It.IsAny<User>()), Times.Never);
    }
}
```

### 2. Integration Testing

#### Database Integration Tests
```csharp
public class DatabaseIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    
    public DatabaseIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task UserRepository_CreateAndRetrieve_Success()
    {
        // Arrange
        using var unitOfWork = _fixture.CreateUnitOfWork();
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = "integrationtest",
            Email = "integration@test.com",
            PasswordHash = "hashed-password",
            Role = "User",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        
        // Act
        var userId = await unitOfWork.Users.CreateAsync(user);
        await unitOfWork.SaveChangesAsync();
        
        var retrievedUser = await unitOfWork.Users.GetByIdAsync(userId);
        
        // Assert
        Assert.NotNull(retrievedUser);
        Assert.Equal(user.Username, retrievedUser.Username);
        Assert.Equal(user.Email, retrievedUser.Email);
        Assert.Equal(user.Role, retrievedUser.Role);
    }
    
    [Fact]
    public async Task DashboardRepository_CreateWithPermissions_Success()
    {
        // Arrange
        using var unitOfWork = _fixture.CreateUnitOfWork();
        
        var user = await CreateTestUserAsync(unitOfWork);
        var dashboard = new Dashboard
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Dashboard",
            Title = "Integration Test Dashboard",
            Description = "Dashboard for integration testing",
            DashboardData = "<xml></xml>",
            CreatedBy = user.Id,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };
        
        // Act
        var dashboardId = await unitOfWork.Dashboards.CreateAsync(dashboard);
        await unitOfWork.SaveChangesAsync();
        
        var permission = new DashboardPermission
        {
            Id = Guid.NewGuid().ToString(),
            DashboardId = dashboardId,
            UserId = user.Id,
            PermissionType = "Owner",
            GrantedDate = DateTime.UtcNow
        };
        
        await unitOfWork.DashboardPermissions.CreateAsync(permission);
        await unitOfWork.SaveChangesAsync();
        
        // Assert
        var retrievedDashboard = await unitOfWork.Dashboards.GetByIdAsync(dashboardId);
        var userPermissions = await unitOfWork.DashboardPermissions.GetByUserIdAsync(user.Id);
        
        Assert.NotNull(retrievedDashboard);
        Assert.Equal(dashboard.Name, retrievedDashboard.Name);
        Assert.Single(userPermissions);
        Assert.Equal("Owner", userPermissions.First().PermissionType);
    }
    
    private async Task<User> CreateTestUserAsync(IUnitOfWork unitOfWork)
    {
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = $"testuser_{Guid.NewGuid():N}"[..20],
            Email = $"test_{Guid.NewGuid():N}@example.com",
            PasswordHash = "hashed-password",
            Role = "User",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        
        await unitOfWork.Users.CreateAsync(user);
        await unitOfWork.SaveChangesAsync();
        
        return user;
    }
}

public class DatabaseFixture : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _connectionString;
    
    public DatabaseFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json")
            .Build();
        
        _connectionString = configuration.GetConnectionString("TestDatabase") 
            ?? "Server=(localdb)\\mssqllocaldb;Database=DataLensDashboard_Test;Trusted_Connection=true;MultipleActiveResultSets=true";
        
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();
        services.AddLogging();
        
        _serviceProvider = services.BuildServiceProvider();
        
        InitializeDatabase();
    }
    
    public IUnitOfWork CreateUnitOfWork()
    {
        var factory = _serviceProvider.GetRequiredService<IUnitOfWorkFactory>();
        return factory.Create();
    }
    
    private void InitializeDatabase()
    {
        // Test veritabanını oluştur ve migrate et
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        
        // Create database if not exists
        var createDbSql = @"
            IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'DataLensDashboard_Test')
            CREATE DATABASE DataLensDashboard_Test;
        ";
        
        connection.Execute(createDbSql);
        
        // Run migrations
        var migrationService = _serviceProvider.GetRequiredService<IMigrationService>();
        migrationService.MigrateAsync().Wait();
    }
    
    public void Dispose()
    {
        // Test veritabanını temizle
        using var connection = new SqlConnection(_connectionString.Replace("DataLensDashboard_Test", "master"));
        connection.Open();
        
        var dropDbSql = @"
            IF EXISTS (SELECT name FROM sys.databases WHERE name = 'DataLensDashboard_Test')
            BEGIN
                ALTER DATABASE DataLensDashboard_Test SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE DataLensDashboard_Test;
            END
        ";
        
        connection.Execute(dropDbSql);
        
        _serviceProvider.Dispose();
    }
}
```

#### API Integration Tests
```csharp
public class DashboardApiTests : BaseIntegrationTest
{
    public DashboardApiTests(WebApplicationFactory<Program> factory) : base(factory) { }
    
    [Fact]
    public async Task GetDashboards_Authenticated_ReturnsOk()
    {
        // Arrange
        var token = await GetJwtTokenAsync("testuser", "User");
        SetAuthorizationHeader(token);
        
        // Act
        var response = await Client.GetAsync("/api/dashboard/list");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }
    
    [Fact]
    public async Task GetDashboards_Unauthenticated_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/dashboard/list");
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateDashboard_ValidData_ReturnsCreated()
    {
        // Arrange
        var token = await GetJwtTokenAsync("designer", "Designer");
        SetAuthorizationHeader(token);
        
        var dashboard = new
        {
            Name = "Test Dashboard",
            Title = "API Test Dashboard",
            Description = "Dashboard created via API test",
            DashboardData = "<xml></xml>",
            Category = "Test"
        };
        
        var json = JsonSerializer.Serialize(dashboard);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // Act
        var response = await Client.PostAsync("/api/dashboard/save", content);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(result.TryGetProperty("id", out var idProperty));
        Assert.False(string.IsNullOrEmpty(idProperty.GetString()));
    }
}
```

### 3. Performance Testing

#### Load Testing
```csharp
public class PerformanceTests
{
    [Fact]
    public async Task DashboardService_GetDashboards_PerformanceTest()
    {
        // Arrange
        var mockUnitOfWorkFactory = new Mock<IUnitOfWorkFactory>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockDashboardRepository = new Mock<IDashboardRepository>();
        var mockLogger = new Mock<ILogger<DashboardService>>();
        
        var dashboards = GenerateTestDashboards(1000);
        
        mockUnitOfWorkFactory.Setup(f => f.Create()).Returns(mockUnitOfWork.Object);
        mockUnitOfWork.Setup(u => u.Dashboards).Returns(mockDashboardRepository.Object);
        mockDashboardRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(dashboards);
        
        var service = new DashboardService(
            mockUnitOfWorkFactory.Object,
            Mock.Of<IDashboardPermissionRepository>(),
            Mock.Of<IUserGroupMembershipRepository>(),
            mockLogger.Object);
        
        // Act & Assert
        var stopwatch = Stopwatch.StartNew();
        
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => service.GetAllDashboardsAsync())
            .ToArray();
        
        await Task.WhenAll(tasks);
        
        stopwatch.Stop();
        
        // Performance assertion - should complete within 5 seconds
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
            $"Performance test failed. Elapsed time: {stopwatch.ElapsedMilliseconds}ms");
    }
    
    private List<Dashboard> GenerateTestDashboards(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => new Dashboard
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"Dashboard {i}",
                Title = $"Test Dashboard {i}",
                Description = $"Description for dashboard {i}",
                DashboardData = "<xml></xml>",
                CreatedBy = "test-user",
                CreatedDate = DateTime.UtcNow.AddDays(-i),
                IsActive = true
            })
            .ToList();
    }
}
```

### 4. Test Configuration

#### appsettings.Test.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  
  "DatabaseSettings": {
    "DatabaseType": "SqlServer"
  },
  
  "ConnectionStrings": {
    "TestDatabase": "Server=(localdb)\\mssqllocaldb;Database=DataLensDashboard_Test;Trusted_Connection=true;MultipleActiveResultSets=true",
    "SqlServer": "Server=(localdb)\\mssqllocaldb;Database=DataLensDashboard_Test;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  
  "Jwt": {
    "Key": "test-key-that-is-at-least-32-characters-long-for-testing",
    "Issuer": "TestIssuer",
    "Audience": "TestAudience",
    "ExpirationMinutes": 60
  }
}
```

## Deployment Kuralları

### 1. Environment Configuration

#### Development Environment
```yaml
# docker-compose.dev.yml
version: '3.8'
services:
  app:
    build:
      context: .
      dockerfile: Dockerfile.dev
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:80
    volumes:
      - .:/app
      - /app/bin
      - /app/obj
    depends_on:
      - sqlserver
      - mongodb
  
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
  
  mongodb:
    image: mongo:6.0
    ports:
      - "27017:27017"
    environment:
      - MONGO_INITDB_ROOT_USERNAME=admin
      - MONGO_INITDB_ROOT_PASSWORD=password
    volumes:
      - mongodb_data:/data/db

volumes:
  sqlserver_data:
  mongodb_data:
```

#### Production Environment
```yaml
# docker-compose.prod.yml
version: '3.8'
services:
  app:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "80:80"
      - "443:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=https://+:443;http://+:80
      - ASPNETCORE_Kestrel__Certificates__Default__Password=certificatepassword
      - ASPNETCORE_Kestrel__Certificates__Default__Path=/https/certificate.pfx
    volumes:
      - ./certificates:/https:ro
      - ./logs:/app/logs
    depends_on:
      - sqlserver
    restart: unless-stopped
  
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=${SQL_SERVER_PASSWORD}
    volumes:
      - sqlserver_data:/var/opt/mssql
    restart: unless-stopped
  
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
      - ./certificates:/etc/nginx/certificates:ro
    depends_on:
      - app
    restart: unless-stopped

volumes:
  sqlserver_data:
```

### 2. Dockerfile Configuration

#### Development Dockerfile
```dockerfile
# Dockerfile.dev
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DataLens/DataLens.csproj", "DataLens/"]
RUN dotnet restore "DataLens/DataLens.csproj"
COPY . .
WORKDIR "/src/DataLens"
RUN dotnet build "DataLens.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "DataLens.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install development tools
RUN apt-get update && apt-get install -y curl

ENTRYPOINT ["dotnet", "DataLens.dll"]
```

#### Production Dockerfile
```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DataLens/DataLens.csproj", "DataLens/"]
RUN dotnet restore "DataLens/DataLens.csproj"
COPY . .
WORKDIR "/src/DataLens"
RUN dotnet build "DataLens.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataLens.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set ownership and permissions
RUN chown -R appuser:appuser /app
USER appuser

ENTRYPOINT ["dotnet", "DataLens.dll"]
```

### 3. CI/CD Pipeline

#### GitHub Actions Workflow
```yaml
# .github/workflows/ci-cd.yml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    
    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          ACCEPT_EULA: Y
          SA_PASSWORD: YourStrong@Passw0rd
        ports:
          - 1433:1433
        options: >-
          --health-cmd "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q 'SELECT 1'"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore --configuration Release
    
    - name: Test
      run: dotnet test --no-build --configuration Release --verbosity normal --collect:"XPlat Code Coverage"
    
    - name: Upload coverage reports
      uses: codecov/codecov-action@v3
      with:
        file: ./coverage.xml
        flags: unittests
        name: codecov-umbrella
  
  security-scan:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Run security scan
      uses: securecodewarrior/github-action-add-sarif@v1
      with:
        sarif-file: security-scan-results.sarif
  
  build-and-deploy:
    needs: [test, security-scan]
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Build Docker image
      run: |
        docker build -t datalens-dashboard:${{ github.sha }} .
        docker tag datalens-dashboard:${{ github.sha }} datalens-dashboard:latest
    
    - name: Deploy to staging
      run: |
        # Deploy to staging environment
        echo "Deploying to staging..."
    
    - name: Run integration tests
      run: |
        # Run integration tests against staging
        echo "Running integration tests..."
    
    - name: Deploy to production
      if: success()
      run: |
        # Deploy to production environment
        echo "Deploying to production..."
```

### 4. Monitoring and Logging

#### Application Insights Configuration
```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:ConnectionString"]);

// Custom telemetry
builder.Services.AddSingleton<ITelemetryInitializer, CustomTelemetryInitializer>();

public class CustomTelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.Component.Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        telemetry.Context.Cloud.RoleName = "DataLens-Dashboard";
    }
}
```

#### Health Checks Configuration
```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database")
    .AddCheck<DevExpressHealthCheck>("devexpress")
    .AddCheck("self", () => HealthCheckResult.Healthy());

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

### 5. Backup and Recovery

#### Database Backup Script
```sql
-- backup-script.sql
DECLARE @BackupPath NVARCHAR(255) = 'C:\Backups\DataLensDashboard_' + FORMAT(GETDATE(), 'yyyyMMdd_HHmmss') + '.bak'

BACKUP DATABASE [DataLensDashboard] 
TO DISK = @BackupPath
WITH 
    FORMAT,
    COMPRESSION,
    CHECKSUM,
    STATS = 10

PRINT 'Backup completed: ' + @BackupPath
```

#### Automated Backup Service
```csharp
public class BackupService : BackgroundService
{
    private readonly ILogger<BackupService> _logger;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _backupInterval;
    
    public BackupService(ILogger<BackupService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _backupInterval = TimeSpan.FromHours(configuration.GetValue<int>("Backup:IntervalHours", 24));
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformBackupAsync();
                await Task.Delay(_backupInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during backup operation");
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken); // Retry after 30 minutes
            }
        }
    }
    
    private async Task PerformBackupAsync()
    {
        _logger.LogInformation("Starting database backup");
        
        var connectionString = _configuration.GetConnectionString("SqlServer");
        var backupPath = Path.Combine(
            _configuration["Backup:Path"] ?? "./backups",
            $"DataLensDashboard_{DateTime.UtcNow:yyyyMMdd_HHmmss}.bak");
        
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        
        var backupSql = $@"
            BACKUP DATABASE [DataLensDashboard] 
            TO DISK = '{backupPath}'
            WITH FORMAT, COMPRESSION, CHECKSUM, STATS = 10
        ";
        
        await connection.ExecuteAsync(backupSql, commandTimeout: 3600); // 1 hour timeout
        
        _logger.LogInformation("Database backup completed: {BackupPath}", backupPath);
        
        // Clean up old backups
        await CleanupOldBackupsAsync();
    }
    
    private async Task CleanupOldBackupsAsync()
    {
        var backupDirectory = _configuration["Backup:Path"] ?? "./backups";
        var retentionDays = _configuration.GetValue<int>("Backup:RetentionDays", 30);
        
        if (!Directory.Exists(backupDirectory))
            return;
        
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
        var oldBackups = Directory.GetFiles(backupDirectory, "*.bak")
            .Where(file => File.GetCreationTimeUtc(file) < cutoffDate)
            .ToList();
        
        foreach (var oldBackup in oldBackups)
        {
            try
            {
                File.Delete(oldBackup);
                _logger.LogInformation("Deleted old backup: {BackupFile}", oldBackup);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete old backup: {BackupFile}", oldBackup);
            }
        }
    }
}
```