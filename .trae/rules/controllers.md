# DataLens Dashboard - Controller Kuralları

## Controller Geliştirme Kuralları

### 1. Base Controller Yapısı
```csharp
[Area("AreaName")] // Area controllers için
public class ControllerNameController : Controller
{
    private readonly IServiceName _serviceName;
    private readonly ILogger<ControllerNameController> _logger;

    public ControllerNameController(IServiceName serviceName, ILogger<ControllerNameController> logger)
    {
        _serviceName = serviceName;
        _logger = logger;
    }
}
```

### 2. Action Method Kuralları

#### HTTP Verb Attributes
- `[HttpGet]` for GET requests
- `[HttpPost]` for POST requests
- `[HttpPut]` for PUT requests
- `[HttpDelete]` for DELETE requests

#### Action Naming
- Index: Liste görünümü
- Details: Detay görünümü
- Create: Oluşturma (GET/POST)
- Edit: Düzenleme (GET/POST)
- Delete: Silme (GET/POST)
- Custom actions: Açıklayıcı isimler

### 3. Authorization Kuralları
```csharp
[Authorize] // Tüm authenticated users
[Authorize(Roles = "Admin")] // Sadece Admin
[Authorize(Roles = "Admin,Designer")] // Admin veya Designer
[AllowAnonymous] // Herkes erişebilir
```

### 4. Controller Sınıfları

#### AccountController (Root)
- Login (GET/POST): Kullanıcı girişi
- Logout (POST): Kullanıcı çıkışı
- Register (GET/POST): Kullanıcı kaydı
- ForgotPassword (GET/POST): Şifre sıfırlama
- Profile (GET): Profil görünümü

#### HomeController (Root)
- Index: Ana sayfa
- Privacy: Gizlilik politikası
- Error: Hata sayfası

#### Admin Area Controllers

##### UserController
```csharp
[Authorize(Roles = "Admin")]
public class UserController : Controller
{
    // Index: Kullanıcı listesi
    // Create: Yeni kullanıcı oluşturma
    // Edit: Kullanıcı düzenleme
    // Details: Kullanıcı detayları
    // Delete: Kullanıcı silme
    // ChangeRole: Rol değiştirme
    // ResetPassword: Şifre sıfırlama
}
```

##### UserGroupController
```csharp
[Authorize(Roles = "Admin")]
public class UserGroupController : Controller
{
    // Index: Grup listesi
    // Create: Yeni grup oluşturma
    // Edit: Grup düzenleme
    // Details: Grup detayları
    // Delete: Grup silme
    // ManageMembers: Üye yönetimi
    // AddMember: Üye ekleme
    // RemoveMember: Üye çıkarma
}
```

#### Dashboard Area Controllers

##### DashboardController
```csharp
[Authorize]
public class DashboardController : Controller
{
    // Index: Dashboard listesi (rol bazlı)
    // Create: Dashboard oluşturma (Designer, Admin)
    // Edit: Dashboard düzenleme (izin bazlı)
    // View: Dashboard görüntüleme (izin bazlı)
    // Delete: Dashboard silme
    // Designer: Dashboard tasarım modu
    // Viewer: Dashboard görüntüleme modu
    // Clone: Dashboard kopyalama
    // Export: Dashboard dışa aktarma
    // Import: Dashboard içe aktarma
}
```

##### DashboardPermissionController
```csharp
[Authorize(Roles = "Admin,Designer")]
public class DashboardPermissionController : Controller
{
    // Manage: İzin yönetimi
    // GrantUserPermission: Kullanıcıya izin verme
    // GrantGroupPermission: Gruba izin verme
    // RevokeUserPermission: Kullanıcı iznini iptal
    // RevokeGroupPermission: Grup iznini iptal
    // UpdateUserPermission: Kullanıcı iznini güncelleme
    // UpdateGroupPermission: Grup iznini güncelleme
}
```

#### Profile Area Controllers

##### ProfileController
```csharp
[Authorize]
public class ProfileController : Controller
{
    // Index: Profil görüntüleme
    // Edit: Profil düzenleme
    // ChangePassword: Şifre değiştirme
    // UploadAvatar: Avatar yükleme
}
```

##### SettingsController
```csharp
[Authorize]
public class SettingsController : Controller
{
    // Index: Genel ayarlar
    // Privacy: Gizlilik ayarları
    // Notifications: Bildirim ayarları
    // ExportData: Veri dışa aktarma
    // DeleteAccount: Hesap silme
}
```

##### NotificationController
```csharp
[Authorize]
public class NotificationController : Controller
{
    // Index: Bildirim listesi
    // MarkAsRead: Okundu işaretleme
    // MarkAllAsRead: Tümünü okundu işaretle
    // Delete: Bildirim silme
    // GetUnreadCount: Okunmamış sayısı (AJAX)
}
```

### 5. Error Handling
```csharp
try
{
    // Action logic
    return View(model);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error in {ActionName}", nameof(ActionName));
    TempData["Error"] = "İşlem sırasında bir hata oluştu.";
    return RedirectToAction("Index");
}
```

### 6. Model Validation
```csharp
[HttpPost]
public async Task<IActionResult> Create(ModelClass model)
{
    if (!ModelState.IsValid)
    {
        return View(model);
    }
    
    // Process valid model
}
```

### 7. TempData Usage
- Success messages: `TempData["Success"]`
- Error messages: `TempData["Error"]`
- Warning messages: `TempData["Warning"]`
- Info messages: `TempData["Info"]`

### 8. ViewBag/ViewData Usage
- Minimal kullanım
- Strongly typed models tercih edilmeli
- Dropdown lists için ViewBag kullanılabilir

### 9. Async/Await Pattern
- Tüm database operations async olmalı
- `async Task<IActionResult>` return type
- `await` keyword kullanımı
- ConfigureAwait(false) gerektiğinde

### 10. API Controller Kuralları
```csharp
[ApiController]
[Route("api/[controller]")]
public class ApiControllerNameController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Model>>> Get()
    {
        // API logic
    }
}
```

### 11. Route Attributes
```csharp
[Route("[area]/[controller]/[action]/{id?}")]
[Route("custom-route")]
```

### 12. Action Filters
- Custom authorization filters
- Logging filters
- Validation filters
- Exception filters

### 13. Dependency Injection
- Constructor injection kullanımı
- Service interfaces kullanımı
- Minimal dependencies
- Single responsibility principle

### 14. Return Types
- `IActionResult` for flexible returns
- `ActionResult<T>` for API controllers
- `ViewResult` for views
- `RedirectResult` for redirects
- `JsonResult` for JSON responses

### 15. Security Considerations
- Input validation
- XSS prevention
- CSRF protection
- SQL injection prevention
- Authorization checks