# DataLens Dashboard - DevExpress Entegrasyon Kuralları

## DevExpress BI Dashboard Entegrasyon Kuralları

### 1. DevExpress Kurulum ve Konfigürasyon

#### NuGet Packages
```xml
<!-- DevExpress.AspNetCore.Dashboard -->
<PackageReference Include="DevExpress.AspNetCore.Dashboard" Version="25.1.3" />
<PackageReference Include="DevExpress.Dashboard.Core" Version="25.1.3" />
<PackageReference Include="DevExpress.Dashboard.Web" Version="25.1.3" />
<PackageReference Include="DevExpress.Data" Version="25.1.3" />
<PackageReference Include="DevExpress.DataAccess" Version="25.1.3" />
```

#### Program.cs Configuration
```csharp
using DevExpress.AspNetCore;
using DevExpress.DashboardAspNetCore;
using DevExpress.DashboardCommon;
using DevExpress.DashboardWeb;

var builder = WebApplication.CreateBuilder(args);

// DevExpress services
builder.Services.AddDevExpressControls();
builder.Services.AddScoped<DashboardConfigurator>();

// Dashboard storage
builder.Services.AddScoped<IDashboardStorage, CustomDashboardStorage>();
builder.Services.AddScoped<IDataSourceWizardConnectionStringsProvider, CustomConnectionStringsProvider>();

var app = builder.Build();

// DevExpress middleware
app.UseDevExpressControls();

// Dashboard routing
app.MapDashboardRoute("api/dashboard", "DefaultDashboard");
```

### 2. Custom Dashboard Storage Implementation

#### IDashboardStorage Implementation
```csharp
public class CustomDashboardStorage : IDashboardStorage
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<CustomDashboardStorage> _logger;

    public CustomDashboardStorage(IDashboardService dashboardService, ILogger<CustomDashboardStorage> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    public XDocument LoadDashboard(string dashboardID)
    {
        try
        {
            var dashboard = _dashboardService.GetDashboardByIdAsync(dashboardID).Result;
            if (dashboard != null && !string.IsNullOrEmpty(dashboard.DashboardData))
            {
                return XDocument.Parse(dashboard.DashboardData);
            }
            throw new ApplicationException($"Dashboard with ID '{dashboardID}' not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard: {DashboardId}", dashboardID);
            throw;
        }
    }

    public string SaveDashboard(XDocument dashboard, string dashboardID)
    {
        try
        {
            var dashboardXml = dashboard.ToString();
            
            if (string.IsNullOrEmpty(dashboardID))
            {
                // Create new dashboard
                var newDashboard = new Dashboard
                {
                    Name = GetDashboardName(dashboard),
                    Title = GetDashboardTitle(dashboard),
                    DashboardData = dashboardXml,
                    CreatedBy = GetCurrentUserId(),
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };
                
                return _dashboardService.CreateDashboardAsync(newDashboard).Result;
            }
            else
            {
                // Update existing dashboard
                var existingDashboard = _dashboardService.GetDashboardByIdAsync(dashboardID).Result;
                if (existingDashboard != null)
                {
                    existingDashboard.DashboardData = dashboardXml;
                    existingDashboard.LastModifiedDate = DateTime.UtcNow;
                    existingDashboard.LastModifiedBy = GetCurrentUserId();
                    
                    _dashboardService.UpdateDashboardAsync(existingDashboard).Wait();
                    return dashboardID;
                }
                throw new ApplicationException($"Dashboard with ID '{dashboardID}' not found for update.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving dashboard: {DashboardId}", dashboardID);
            throw;
        }
    }

    public string[] GetAvailableDashboardsID()
    {
        try
        {
            var dashboards = _dashboardService.GetAllDashboardsAsync().Result;
            return dashboards.Select(d => d.Id).ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available dashboards");
            return Array.Empty<string>();
        }
    }

    private string GetDashboardName(XDocument dashboard)
    {
        var nameElement = dashboard.Root?.Attribute("Name");
        return nameElement?.Value ?? "Untitled Dashboard";
    }

    private string GetDashboardTitle(XDocument dashboard)
    {
        var titleElement = dashboard.Root?.Attribute("Title");
        return titleElement?.Value ?? GetDashboardName(dashboard);
    }

    private string GetCurrentUserId()
    {
        // Get current user ID from HTTP context or authentication
        return "current-user-id"; // Implement based on your authentication
    }
}
```

### 3. Dashboard Controller Implementation

#### DashboardController for DevExpress
```csharp
[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetDashboardList()
    {
        try
        {
            var dashboards = await _dashboardService.GetAllDashboardsAsync();
            var dashboardList = dashboards.Select(d => new
            {
                Id = d.Id,
                Name = d.Name,
                Title = d.Title,
                Description = d.Description,
                Category = d.Category,
                CreatedDate = d.CreatedDate,
                ViewCount = d.ViewCount
            });
            
            return Ok(dashboardList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard list");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDashboard(string id)
    {
        try
        {
            var dashboard = await _dashboardService.GetDashboardByIdAsync(id);
            if (dashboard == null)
            {
                return NotFound($"Dashboard with ID '{id}' not found.");
            }

            await _dashboardService.IncrementViewCountAsync(id);
            
            return Ok(new
            {
                Id = dashboard.Id,
                Name = dashboard.Name,
                Title = dashboard.Title,
                DashboardData = dashboard.DashboardData
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard: {DashboardId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("save")]
    public async Task<IActionResult> SaveDashboard([FromBody] SaveDashboardRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.DashboardData))
            {
                return BadRequest("Dashboard data is required.");
            }

            var dashboard = new Dashboard
            {
                Id = request.Id,
                Name = request.Name,
                Title = request.Title,
                Description = request.Description,
                DashboardData = request.DashboardData,
                Category = request.Category ?? "General",
                CreatedBy = GetCurrentUserId(),
                LastModifiedBy = GetCurrentUserId(),
                LastModifiedDate = DateTime.UtcNow
            };

            string dashboardId;
            if (string.IsNullOrEmpty(request.Id))
            {
                dashboardId = await _dashboardService.CreateDashboardAsync(dashboard);
            }
            else
            {
                await _dashboardService.UpdateDashboardAsync(dashboard);
                dashboardId = request.Id;
            }

            return Ok(new { Id = dashboardId, Message = "Dashboard saved successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving dashboard");
            return StatusCode(500, "Internal server error");
        }
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst("UserId")?.Value ?? "unknown";
    }
}

public class SaveDashboardRequest
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DashboardData { get; set; } = string.Empty;
    public string? Category { get; set; }
}
```

### 4. Dashboard Views Implementation

#### DashboardDesigner.cshtml
```html
@{
    ViewData["Title"] = "Dashboard Designer";
    Layout = "~/Areas/Dashboard/Views/Shared/_DashboardLayout.cshtml";
}

<div class="row">
    <div class="col-12">
        <div class="card">
            <div class="card-header">
                <h3 class="card-title">Dashboard Designer</h3>
                <div class="card-tools">
                    <button type="button" class="btn btn-primary" onclick="saveDashboard()">
                        <i class="fas fa-save"></i> Kaydet
                    </button>
                    <button type="button" class="btn btn-secondary" onclick="previewDashboard()">
                        <i class="fas fa-eye"></i> Önizleme
                    </button>
                </div>
            </div>
            <div class="card-body p-0">
                <div id="dashboardDesigner" style="height: 800px;"></div>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    <script src="~/lib/devexpress/js/dx.all.js"></script>
    <script src="~/lib/devexpress/js/dx-dashboard.min.js"></script>
    
    <script>
        var dashboardDesigner;
        
        $(document).ready(function() {
            initializeDashboardDesigner();
        });
        
        function initializeDashboardDesigner() {
            dashboardDesigner = new DevExpress.Dashboard.DashboardDesigner(document.getElementById("dashboardDesigner"), {
                endpoint: "/api/dashboard",
                workingMode: "Designer",
                allowSwitchToDesigner: true,
                onDashboardTitleChanged: function(e) {
                    updatePageTitle(e.DashboardTitle);
                },
                onDashboardSaved: function(e) {
                    showToast('success', 'Dashboard başarıyla kaydedildi.');
                },
                onDashboardSaving: function(e) {
                    // Custom save logic if needed
                }
            });
        }
        
        function saveDashboard() {
            if (dashboardDesigner) {
                dashboardDesigner.saveDashboard();
            }
        }
        
        function previewDashboard() {
            if (dashboardDesigner) {
                dashboardDesigner.switchToViewer();
            }
        }
        
        function updatePageTitle(title) {
            document.title = title + " - Dashboard Designer";
        }
    </script>
}

@section Styles {
    <link rel="stylesheet" href="~/lib/devexpress/css/dx.light.css">
    <link rel="stylesheet" href="~/lib/devexpress/css/dx-dashboard.min.css">
}
```

#### DashboardViewer.cshtml
```html
@model string
@{
    ViewData["Title"] = "Dashboard Viewer";
    Layout = "~/Areas/Dashboard/Views/Shared/_DashboardLayout.cshtml";
    var dashboardId = Model;
}

<div class="row">
    <div class="col-12">
        <div class="card">
            <div class="card-header">
                <h3 class="card-title">Dashboard Görünümü</h3>
                <div class="card-tools">
                    @if (User.IsInRole("Admin") || User.IsInRole("Designer"))
                    {
                        <a href="@Url.Action("Designer", new { id = dashboardId })" class="btn btn-warning btn-sm">
                            <i class="fas fa-edit"></i> Düzenle
                        </a>
                    }
                    <button type="button" class="btn btn-info btn-sm" onclick="exportDashboard()">
                        <i class="fas fa-download"></i> Dışa Aktar
                    </button>
                    <button type="button" class="btn btn-secondary btn-sm" onclick="refreshDashboard()">
                        <i class="fas fa-sync"></i> Yenile
                    </button>
                </div>
            </div>
            <div class="card-body p-0">
                <div id="dashboardViewer" style="height: 800px;"></div>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    <script src="~/lib/devexpress/js/dx.all.js"></script>
    <script src="~/lib/devexpress/js/dx-dashboard.min.js"></script>
    
    <script>
        var dashboardViewer;
        var currentDashboardId = '@dashboardId';
        
        $(document).ready(function() {
            initializeDashboardViewer();
        });
        
        function initializeDashboardViewer() {
            dashboardViewer = new DevExpress.Dashboard.DashboardViewer(document.getElementById("dashboardViewer"), {
                endpoint: "/api/dashboard",
                workingMode: "Viewer",
                dashboardId: currentDashboardId,
                allowSwitchToDesigner: @(User.IsInRole("Admin") || User.IsInRole("Designer") ? "true" : "false"),
                onDashboardChanged: function(e) {
                    updatePageTitle(e.DashboardTitle);
                },
                onItemClick: function(e) {
                    // Handle dashboard item clicks
                    console.log('Dashboard item clicked:', e);
                },
                onItemSelectionChanged: function(e) {
                    // Handle item selection changes
                    console.log('Item selection changed:', e);
                }
            });
        }
        
        function exportDashboard() {
            if (dashboardViewer) {
                dashboardViewer.exportDashboardToPdf();
            }
        }
        
        function refreshDashboard() {
            if (dashboardViewer) {
                dashboardViewer.refresh();
            }
        }
        
        function updatePageTitle(title) {
            document.title = title + " - Dashboard Viewer";
        }
    </script>
}

@section Styles {
    <link rel="stylesheet" href="~/lib/devexpress/css/dx.light.css">
    <link rel="stylesheet" href="~/lib/devexpress/css/dx-dashboard.min.css">
}
```

### 5. Data Source Configuration

#### Custom Connection Strings Provider
```csharp
public class CustomConnectionStringsProvider : IDataSourceWizardConnectionStringsProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CustomConnectionStringsProvider> _logger;

    public CustomConnectionStringsProvider(IConfiguration configuration, ILogger<CustomConnectionStringsProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Dictionary<string, string> GetConnectionDescriptions()
    {
        return new Dictionary<string, string>
        {
            { "SqlServerConnection", "SQL Server Database" },
            { "PostgreSQLConnection", "PostgreSQL Database" },
            { "MongoDBConnection", "MongoDB Database" },
            { "ODataConnection", "OData Service" },
            { "WebAPIConnection", "Web API Service" }
        };
    }

    public DataConnectionParametersBase GetDataConnectionParameters(string name)
    {
        try
        {
            return name switch
            {
                "SqlServerConnection" => new MsSqlConnectionParameters
                {
                    ConnectionString = _configuration.GetConnectionString("SqlServer")
                },
                "PostgreSQLConnection" => new PostgreSqlConnectionParameters
                {
                    ConnectionString = _configuration.GetConnectionString("PostgreSQL")
                },
                "ODataConnection" => new ODataConnectionParameters
                {
                    Uri = _configuration["DataSources:OData:Uri"]
                },
                "WebAPIConnection" => new WebApiConnectionParameters
                {
                    Uri = _configuration["DataSources:WebAPI:Uri"]
                },
                _ => throw new ArgumentException($"Unknown connection name: {name}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data connection parameters for: {ConnectionName}", name);
            throw;
        }
    }
}
```

### 6. Dashboard Security

#### Dashboard Authorization
```csharp
public class DashboardAuthorizationService
{
    private readonly IDashboardService _dashboardService;
    private readonly IUserGroupService _userGroupService;

    public DashboardAuthorizationService(IDashboardService dashboardService, IUserGroupService userGroupService)
    {
        _dashboardService = dashboardService;
        _userGroupService = userGroupService;
    }

    public async Task<bool> CanViewDashboard(string dashboardId, string userId)
    {
        // Check if user has direct permission
        if (await _dashboardService.HasPermissionAsync(dashboardId, userId, "View"))
        {
            return true;
        }

        // Check if user has permission through group membership
        var userGroups = await _userGroupService.GetUserGroupsAsync(userId);
        foreach (var group in userGroups)
        {
            var groupPermissions = await _dashboardService.GetGroupPermissionsAsync(group.Id);
            if (groupPermissions.Any(p => p.DashboardId == dashboardId && p.PermissionType == "View"))
            {
                return true;
            }
        }

        // Check if dashboard is public
        var dashboard = await _dashboardService.GetDashboardByIdAsync(dashboardId);
        return dashboard?.IsPublic == true;
    }

    public async Task<bool> CanEditDashboard(string dashboardId, string userId)
    {
        return await _dashboardService.HasPermissionAsync(dashboardId, userId, "Edit");
    }

    public async Task<bool> CanDeleteDashboard(string dashboardId, string userId)
    {
        return await _dashboardService.HasPermissionAsync(dashboardId, userId, "Delete");
    }
}
```

### 7. Dashboard Export/Import

#### Export Functionality
```csharp
[HttpGet("export/{id}")]
public async Task<IActionResult> ExportDashboard(string id, string format = "pdf")
{
    try
    {
        var dashboard = await _dashboardService.GetDashboardByIdAsync(id);
        if (dashboard == null)
        {
            return NotFound();
        }

        var dashboardXml = XDocument.Parse(dashboard.DashboardData);
        
        return format.ToLower() switch
        {
            "pdf" => await ExportToPdf(dashboardXml, dashboard.Title),
            "excel" => await ExportToExcel(dashboardXml, dashboard.Title),
            "image" => await ExportToImage(dashboardXml, dashboard.Title),
            _ => BadRequest("Unsupported export format")
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error exporting dashboard: {DashboardId}", id);
        return StatusCode(500, "Export failed");
    }
}

private async Task<IActionResult> ExportToPdf(XDocument dashboardXml, string title)
{
    // Implement PDF export logic using DevExpress Dashboard export API
    var pdfBytes = await GeneratePdfFromDashboard(dashboardXml);
    return File(pdfBytes, "application/pdf", $"{title}.pdf");
}
```

### 8. Real-time Dashboard Updates

#### SignalR Integration
```csharp
public class DashboardHub : Hub
{
    public async Task JoinDashboardGroup(string dashboardId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Dashboard_{dashboardId}");
    }

    public async Task LeaveDashboardGroup(string dashboardId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Dashboard_{dashboardId}");
    }
}

// In dashboard service
public async Task NotifyDashboardUpdate(string dashboardId)
{
    await _hubContext.Clients.Group($"Dashboard_{dashboardId}")
        .SendAsync("DashboardUpdated", dashboardId);
}
```

### 9. Performance Optimization

#### Caching Strategy
```csharp
public class CachedDashboardStorage : IDashboardStorage
{
    private readonly IDashboardStorage _baseStorage;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    public XDocument LoadDashboard(string dashboardID)
    {
        var cacheKey = $"dashboard_{dashboardID}";
        
        if (_cache.TryGetValue(cacheKey, out XDocument cachedDashboard))
        {
            return cachedDashboard;
        }

        var dashboard = _baseStorage.LoadDashboard(dashboardID);
        _cache.Set(cacheKey, dashboard, _cacheExpiration);
        
        return dashboard;
    }

    public string SaveDashboard(XDocument dashboard, string dashboardID)
    {
        var result = _baseStorage.SaveDashboard(dashboard, dashboardID);
        
        // Invalidate cache
        var cacheKey = $"dashboard_{dashboardID ?? result}";
        _cache.Remove(cacheKey);
        
        return result;
    }
}
```

### 10. Error Handling

#### Dashboard Error Handler
```javascript
function handleDashboardError(error) {
    console.error('Dashboard error:', error);
    
    var errorMessage = 'Dashboard yüklenirken bir hata oluştu.';
    
    if (error.message) {
        errorMessage = error.message;
    }
    
    showToast('error', errorMessage);
    
    // Show fallback content
    $('#dashboardViewer').html(`
        <div class="alert alert-danger text-center">
            <i class="fas fa-exclamation-triangle fa-3x mb-3"></i>
            <h4>Dashboard Yüklenemedi</h4>
            <p>${errorMessage}</p>
            <button class="btn btn-primary" onclick="location.reload()">
                <i class="fas fa-sync"></i> Tekrar Dene
            </button>
        </div>
    `);
}
```

### 11. Mobile Responsiveness

#### Responsive Dashboard Configuration
```javascript
function initializeResponsiveDashboard() {
    var isMobile = window.innerWidth < 768;
    
    var config = {
        endpoint: "/api/dashboard",
        workingMode: "Viewer",
        allowSwitchToDesigner: !isMobile,
        surfaceLeft: isMobile ? 0 : 200,
        surfaceTop: isMobile ? 0 : 100,
        allowMaximizeItem: true,
        showConfirmationOnBrowserClosing: false
    };
    
    if (isMobile) {
        config.mobileLayoutEnabled = true;
        config.reloadOnResize = true;
    }
    
    return new DevExpress.Dashboard.DashboardViewer(
        document.getElementById("dashboardViewer"), 
        config
    );
}
```

### 12. Testing Dashboard Components

#### Unit Tests for Dashboard Service
```csharp
[Test]
public async Task LoadDashboard_ValidId_ReturnsDashboard()
{
    // Arrange
    var dashboardId = "test-dashboard-id";
    var expectedDashboard = new Dashboard { Id = dashboardId, DashboardData = "<xml></xml>" };
    _mockDashboardService.Setup(s => s.GetDashboardByIdAsync(dashboardId))
        .ReturnsAsync(expectedDashboard);
    
    // Act
    var result = _dashboardStorage.LoadDashboard(dashboardId);
    
    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual("xml", result.Root.Name.LocalName);
}
```

### 13. Deployment Considerations

#### Production Configuration
```json
{
  "DevExpress": {
    "LicenseKey": "your-license-key",
    "Dashboard": {
      "CacheEnabled": true,
      "CacheExpiration": "00:30:00",
      "MaxDashboardSize": "10MB",
      "AllowedExportFormats": ["PDF", "Excel", "Image"]
    }
  }
}
```

### 14. Security Best Practices

#### Dashboard Access Control
```csharp
[Authorize]
[HttpGet("designer/{id?}")]
public async Task<IActionResult> Designer(string id)
{
    if (!User.IsInRole("Admin") && !User.IsInRole("Designer"))
    {
        return Forbid();
    }
    
    if (!string.IsNullOrEmpty(id))
    {
        var canEdit = await _authorizationService.CanEditDashboard(id, GetCurrentUserId());
        if (!canEdit)
        {
            return Forbid();
        }
    }
    
    return View(id);
}
```

### 15. Monitoring and Analytics

#### Dashboard Usage Tracking
```csharp
public async Task TrackDashboardUsage(string dashboardId, string userId, string action)
{
    var usage = new DashboardUsage
    {
        DashboardId = dashboardId,
        UserId = userId,
        Action = action, // View, Edit, Export, etc.
        Timestamp = DateTime.UtcNow,
        UserAgent = Request.Headers["User-Agent"],
        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString()
    };
    
    await _usageTrackingService.LogUsageAsync(usage);
}
```