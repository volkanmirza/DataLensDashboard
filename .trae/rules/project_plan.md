# DataLens - Dashboard Yönetim Sistemi Proje Planı

## Proje Genel Bakış

Orta ölçekli şirketler için kullanıcı yetkilendirmeli dashboard yönetim sistemi. DevExpress Business Intelligence Dashboard altyapısı kullanılarak geliştirilecek.

## Sistem Gereksinimleri

### Kullanıcı Rolleri
- **Viewer**: Sadece atanmış dashboardları görüntüleyebilir
- **Designer**: Dashboard oluşturabilir, düzenleyebilir ve görüntüleyebilir
- **Admin**: Tüm sistem yönetimi, kullanıcı yönetimi ve dashboard yönetimi

### Temel Özellikler
- Kullanıcı ve kullanıcı grubu yönetimi
- Dashboard oluşturma, düzenleme ve görüntüleme
- Dashboard'ların kullanıcı/grup bazlı atanması
- Rol tabanlı erişim kontrolü
- DevExpress BI Dashboard entegrasyonu

## Geliştirme Aşamaları

### Aşama 1: Proje Altyapısı ve Veritabanı Tasarımı

#### 1.1 Proje Yapısı Oluşturma
- [x] .NET Core MVC projesi oluşturma
- [x] AdminLTE ASP.NET MVC template entegrasyonu
- [x] Proje klasör yapısını organize etme (Models, Views, Controllers, Services)
- [x] NuGet paketleri ve bağımlılıkları ekleme:
  - Dapper, MongoDB.Driver
  - DevExpress BI Dashboard
  - AdminLTE dependencies (jQuery, Bootstrap 3.x)
- [x] Areas yapısı kurulumu (Admin, Dashboard, Profile)
- [x] Profile Area Yeniden Yapılandırması:
  - User area'sı Profile area'sına dönüştürüldü
  - ProfileController: Kullanıcı profil yönetimi
  - SettingsController: Kullanıcı ayarları ve gizlilik
  - NotificationController: Bildirim yönetimi
- [x] AdminLTE static files organizasyonu (CSS, JS, Images)

#### 1.2 Veritabanı Tasarımı (Çoklu DB Desteği)
- [x] ORM kurulumları:
  - Dapper (SQL Server/PostgreSQL için)
  - MongoDB.Driver (MongoDB için)
- [x] Veritabanı şeması tasarımı (tüm DB'ler için uyumlu):
  - Users collection/table (Id, Username, Email, PasswordHash, Role, CreatedDate, IsActive)
  - UserGroups collection/table (Id, GroupName, Description, CreatedDate)
  - UserGroupMembers collection/table (UserId, GroupId)
  - Dashboards collection/table (Id, Name, Description, DashboardData, CreatedBy, CreatedDate, IsActive)
  - DashboardPermissions collection/table (DashboardId, UserId, GroupId, PermissionType)
- [x] SQL script dosyalarını oluşturma (SQL Server/PostgreSQL için)
- [x] MongoDB collection initialization scripts
- [x] Database connection factory implementasyonu
- [x] Seed data ekleme (tüm DB türleri için)
- [x] Configuration-based database selection (appsettings.json)

### Aşama 2: Backend ve MVC Controller Geliştirme

#### 2.1 Data Access Layer (Repository + Adapter Pattern)
- [x] Generic repository interface tanımları (IUserRepository, IUserGroupRepository, IDashboardRepository)
- [x] Repository pattern implementasyonu
- [x] Adapter/Strategy pattern ile çoklu veritabanı desteği:
  - SqlUserRepository (Dapper ile SQL Server)
  - MongoUserRepository (MongoDB.Driver ile)
  - PostgreSqlUserRepository (Dapper ile PostgreSQL)
- [x] Database factory pattern implementasyonu
- [x] Unit of Work pattern implementasyonu ✅
- [x] Dependency Injection konfigürasyonu (appsettings.json'dan db seçimi)
- [ ] SQL query optimization ve stored procedures

#### 2.2 Kimlik Doğrulama ve Yetkilendirme ✅
- [x] ASP.NET Core Identity implementasyonu (JWT'den geçiş)
- [x] MongoDB için özel Identity Store implementasyonu:
  - MongoUserStore: Kullanıcı yönetimi için özel store
  - MongoRoleStore: Rol yönetimi için özel store
- [x] Cookie-based authentication (JWT middleware kaldırıldı)
- [x] Role-based authorization attributes
- [x] Custom authorization policies
- [x] Password hashing ve güvenlik
- [x] MongoDB varsayılan veritabanı konfigürasyonu
- [x] Çoklu veritabanı desteği (SQL Server, PostgreSQL, MongoDB)
- [x] Identity servislerinin SQL veritabanları için koşullu konfigürasyonu
- [x] MongoDB için tam ASP.NET Core Identity desteği

#### 2.3 Kullanıcı Yönetimi Controllers ✅
- [x] UserController (Admin Area)
  - Index: Kullanıcı listesi görünümü
  - Create: Yeni kullanıcı oluşturma
  - Edit: Kullanıcı düzenleme
  - Delete: Kullanıcı silme
  - Details: Kullanıcı detayları
- [x] AccountController
  - Login/Logout actions
  - Register action
  - Profile management

#### 2.4 Kullanıcı Grubu Yönetimi Controllers
- [x] UserGroupController (Admin Area)
  - Index: Grup listesi görünümü
  - Create: Yeni grup oluşturma
  - Edit: Grup düzenleme
  - Delete: Grup silme
  - ManageMembers: Grup üyesi yönetimi

#### 2.5 Dashboard Yönetimi Controllers
- [x] DashboardController
  - Index: Dashboard listesi (rol bazlı filtreleme)
  - Create: Dashboard oluşturma (Designer, Admin)
  - Edit: Dashboard düzenleme (izin bazlı)
  - View: Dashboard görüntüleme (izin bazlı)
  - Delete: Dashboard silme
  - Designer: Dashboard tasarım modu
  - Viewer: Dashboard görüntüleme modu
  - SaveData: Dashboard verilerini kaydetme
  - Clone: Dashboard kopyalama
  - Share: Dashboard paylaşım ayarları

#### 2.6 Dashboard İzin Yönetimi ✅
- [x] DashboardPermissionController (Dashboard Area)
  - Manage: İzin yönetimi görünümü
  - GrantUserPermission: Kullanıcıya izin verme
  - GrantGroupPermission: Gruba izin verme
  - RevokeUserPermission: Kullanıcı iznini iptal etme
  - RevokeGroupPermission: Grup iznini iptal etme
  - UpdateUserPermission: Kullanıcı iznini güncelleme
  - UpdateGroupPermission: Grup iznini güncelleme
  - GetUserPermissions: Kullanıcı izinlerini getirme
  - GetGroupPermissions: Grup izinlerini getirme
- [x] IDashboardService arayüzüne eksik metotlar eklendi:
  - GetUserPermissionsAsync: Kullanıcı izinlerini getirme
  - GetGroupPermissionsAsync: Grup izinlerini getirme
  - UpdateUserPermissionAsync: Kullanıcı iznini güncelleme
  - UpdateGroupPermissionAsync: Grup iznini güncelleme
- [x] DashboardService sınıfına metot implementasyonları eklendi
- [x] IDashboardPermissionRepository arayüzüne eksik metotlar eklendi
- [x] Repository implementasyonları güncellendi:
  - SqlDashboardPermissionRepository: SQL Server desteği
  - MongoDashboardPermissionRepository: MongoDB desteği
  - DashboardPermissionRepository: Base repository
- [x] Controller metot çağrıları doğru parametre sayısı ile güncellendi

#### 2.7 Kullanıcı Profil Yönetimi Controllers ✅
- [x] ProfileController (Profile Area)
  - Index: Kullanıcı profil bilgilerini görüntüleme
  - Edit: Profil bilgilerini düzenleme (GET/POST)
  - ChangePassword: Şifre değiştirme (GET/POST)
- [x] SettingsController (Profile Area)
  - Index: Genel kullanıcı ayarları (dil, tema, bildirimler)
  - Privacy: Gizlilik ayarları
  - ExportData: Veri dışa aktarma (GDPR uyumlu)
  - DeleteAccount: Hesap silme işlemi
- [x] NotificationController (Profile Area)
  - Index: Bildirim listesi görüntüleme
  - Preferences: Bildirim tercihleri yönetimi
  - MarkAsRead/MarkAllAsRead: Bildirim durumu güncelleme
  - Delete: Bildirim silme
  - GetUnreadCount: Okunmamış bildirim sayısı (AJAX)

### Aşama 3: DevExpress BI Dashboard Entegrasyonu DevExpress 25.1 uyumlu

#### 3.1 DevExpress Kurulumu DevExpress 25.1
- [x] DevExpress BI Dashboard NuGet paketlerini ekleme
- [x] Dashboard Designer component kurulumu
- [x] Dashboard Viewer component kurulumu
- [x] Lisans konfigürasyonu
- [x] DevExpress JavaScript ve CSS dosyalarının eklenmesi (placeholder dosyalar)
- [x] Dashboard storage provider implementasyonu (database entegrasyonu)

#### 3.2 MVC Entegrasyonu
- [x] Dashboard storage provider konfigürasyonu (DatabaseDashboardStorage)
- [x] Dashboard controller konfigürasyonu
- [x] Data source konfigürasyonu (connection parameters implementasyonu)
- [x] Dashboard export/import functionality
- [x] Custom dashboard endpoints (API Controller)

#### 3.3 View Entegrasyonu
- [x] Dashboard Designer Razor Views ✅
- [x] Dashboard Viewer Razor Views ✅
- [x] JavaScript component entegrasyonu (placeholder dosyalarla) ✅
- [x] Responsive design implementasyonu (AdminLTE ile) ✅
- [x] Partial views için dashboard components ✅

### Aşama 4: MVC Views ve UI Geliştirme

#### 4.1 AdminLTE Layout ve Temel UI Yapısı
- [x] AdminLTE template entegrasyonu ve kurulumu
- [x] _Layout.cshtml (AdminLTE Master Layout)
- [x] _AdminLayout.cshtml (Admin Area Layout)
- [x] AdminLTE Login sayfası (Login, Register)
- [x] AdminLTE Dashboard ana sayfa
- [x] Navigation partial views (AdminLTE sidebar menu - role-based)
- [x] AdminLTE Header ve Footer partial views
- [x] Error handling views (AdminLTE error pages)
- [x] AdminLTE responsive design optimizasyonu

#### 4.2 Kullanıcı Yönetimi Views (AdminLTE)
- [x] Users/Index.cshtml (AdminLTE DataTables entegrasyonu - kullanıcı listesi)
- [x] Users/Create.cshtml (AdminLTE form components - yeni kullanıcı)
- [x] Users/Edit.cshtml (AdminLTE form components - kullanıcı düzenleme)
- [x] Users/Details.cshtml (AdminLTE profile box - kullanıcı detayları)
- [x] Account/Profile.cshtml (AdminLTE user profile template)
- [x] UserGroups/Index.cshtml (AdminLTE DataTables - grup listesi)
- [x] UserGroups/Create.cshtml (AdminLTE form components - yeni grup)
- [x] UserGroups/Edit.cshtml (AdminLTE form components - grup düzenleme)
- [x] UserGroups/Details.cshtml (AdminLTE profile box - grup detayları)
- [x] AdminLTE user widgets ve istatistik kartları

#### 4.3 Kullanıcı Grubu Yönetimi Views (AdminLTE)
- [x] UserGroups/Index.cshtml (AdminLTE DataTables - grup listesi)
- [x] UserGroups/Create.cshtml (AdminLTE form components - yeni grup)
- [x] UserGroups/Edit.cshtml (AdminLTE form components - grup düzenleme)
- [x] UserGroups/Details.cshtml (AdminLTE profile box - grup detayları)
- [x] AdminLTE box components ile grup detayları
- [x] AdminLTE modal dialogs ile hızlı grup işlemleri
- [x] Üye yönetimi entegrasyonu (Edit ve Details sayfalarında)

#### 4.4 Dashboard Yönetimi Views (AdminLTE)
- [x] Dashboards/Index.cshtml (AdminLTE card/box grid layout - dashboard listesi)
- [x] Dashboards/Create.cshtml (AdminLTE form components - dashboard oluşturma)
- [x] Dashboards/Edit.cshtml (AdminLTE form components - dashboard düzenleme)
- [x] Dashboards/Details.cshtml (AdminLTE box components - dashboard detayları)
- [x] Dashboards/Manage.cshtml (AdminLTE permission widgets - izin yönetimi)
- [x] AdminLTE info-box ve small-box components ile dashboard kartları
- [x] AdminLTE timeline components ile dashboard aktivite geçmişi

#### 4.5 Kullanıcı Profil Yönetimi Views (AdminLTE)
- [x] Profile/Index.cshtml (AdminLTE user profile template - profil görüntüleme)
- [x] Profile/Edit.cshtml (AdminLTE form components - profil düzenleme)
- [x] Profile/ChangePassword.cshtml (AdminLTE form components - şifre değiştirme)
- [x] Settings/Index.cshtml (AdminLTE form components - genel ayarlar)
- [x] Settings/Privacy.cshtml (AdminLTE form components - gizlilik ayarları)
- [x] Notification/Index.cshtml (AdminLTE timeline/list - bildirim listesi)
- [x] Notification/Preferences.cshtml (AdminLTE form components - bildirim tercihleri)
- [x] AdminLTE user widgets ve profil kartları
- [x] AdminLTE notification badges ve alert components

#### 4.6 DevExpress Dashboard Views Entegrasyonu (AdminLTE) DevExpress 25.1 uyumlu
- [ ] DashboardDesigner.cshtml (AdminLTE box container + DevExpress Designer)
- [ ] DashboardViewer.cshtml (AdminLTE box container + DevExpress Viewer)
- [ ] AdminLTE toolbar entegrasyonu ile DevExpress custom toolbar
- [ ] AdminLTE modal dialogs ile dashboard save/load işlemleri
- [ ] AdminLTE notification system ile dashboard işlem bildirimleri
- [ ] AdminLTE progress bars ile dashboard loading states
- [ ] JavaScript helper functions (AdminLTE + DevExpress)
- [ ] AdminLTE sidebar ile dashboard navigation menu

### Aşama 5: Güvenlik ve Optimizasyon

#### 5.1 Güvenlik Önlemleri
- [ ] Input validation ve sanitization
- [ ] SQL injection koruması
- [ ] XSS koruması
- [ ] CSRF koruması
- [ ] Rate limiting
- [ ] Audit logging

#### 5.2 Performans Optimizasyonu
- [ ] Database indexing
- [ ] Caching stratejisi (Redis/In-Memory)
- [ ] Response compression
- [ ] View caching ve output caching
- [ ] JavaScript/CSS bundling ve minification
- [ ] Lazy loading ve partial view optimization
- [ ] SignalR entegrasyonu (real-time updates)

### Aşama 6: Test ve Deployment

#### 6.1 Test Geliştirme
- [ ] Unit tests (Controllers, Services, Models)
- [ ] Integration tests (MVC Actions)
- [ ] View tests (Razor Views)
- [ ] End-to-end tests (Selenium)
- [ ] Performance tests
- [ ] Authentication ve authorization tests

#### 6.2 Deployment Hazırlığı
- [ ] Docker containerization
- [ ] CI/CD pipeline kurulumu
- [ ] Environment configuration
- [ ] Database migration scripts
- [ ] Monitoring ve logging setup

## Teknoloji Stack

### Backend & Frontend (MVC)
- .NET Core 6/7 MVC
- Multi-Database Support:
  - Dapper ORM (SQL Server/PostgreSQL)
  - MongoDB.Driver (MongoDB)
- Repository + Adapter/Strategy Pattern
- SQL Server / PostgreSQL / MongoDB
- ASP.NET Core Identity (Cookie Authentication)
- AutoMapper
- FluentValidation
- DevExpress BI Dashboard DevExpress 25.1 uyumlu

### Frontend Technologies
- AdminLTE ASP.NET MVC Template
- Razor Views (.cshtml)
- Bootstrap 3.x (AdminLTE Base)
- jQuery
- AdminLTE Components (Charts, Widgets, Forms)
- DevExpress Dashboard JavaScript Components DevExpress 25.1 uyumlu
- SignalR (Real-time updates)
- AJAX / Fetch API
- AdminLTE CSS/SCSS Framework

### DevOps
- Docker
- Azure/AWS
- GitHub Actions / Azure DevOps
- Redis (Caching)
- Application Insights / Monitoring

## Proje Zaman Çizelgesi

- **Aşama 1**: 1 hafta
- **Aşama 2**: 2 hafta
- **Aşama 3**: 1.5 hafta
- **Aşama 4**: 2.5 hafta
- **Aşama 5**: 1 hafta
- **Aşama 6**: 1 hafta

**Toplam Süre**: 9 hafta

## Sonraki Adımlar

1. Proje yapısını oluşturmak için Aşama 1.1'i başlatın
2. Veritabanı tasarımını detaylandırın
3. DevExpress lisansını temin edin
4. Geliştirme ortamını hazırlayın

---

*Bu doküman proje geliştirme sürecinde güncellenecektir.*