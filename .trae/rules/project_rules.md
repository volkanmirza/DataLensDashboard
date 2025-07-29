# DataLens Dashboard - Proje Kuralları ve Geliştirme Rehberi

Bu dosya, DataLens Dashboard projesinin tüm geliştirme kurallarını ve standartlarını içeren ana rehber dosyasıdır. Proje geliştirme sürecinde aşağıdaki detaylı kural dosyalarını referans alınız.

## 📋 İçindekiler

1. [Genel Proje Kuralları](#genel-proje-kuralları)
2. [Mimari ve Tasarım](#mimari-ve-tasarım)
3. [Katman Bazlı Kurallar](#katman-bazlı-kurallar)
4. [Güvenlik ve Middleware](#güvenlik-ve-middleware)
5. [Veritabanı ve Konfigürasyon](#veritabanı-ve-konfigürasyon)
6. [Test ve Deployment](#test-ve-deployment)
7. [DevExpress Entegrasyonu](#devexpress-entegrasyonu)
8. [Hızlı Başlangıç](#hızlı-başlangıç)

---

## Genel Proje Kuralları

### 📖 Ana Rehber
**Dosya:** [project-guidelines.md](./project-guidelines.md)

Bu dosya projenin temel kurallarını ve best practice'lerini içerir:
- **Kod Standartları**: Naming conventions, file organization
- **Error Handling**: Global exception handling, structured logging
- **Performance**: Caching strategy, query optimization
- **Security**: Input validation, SQL injection prevention
- **Code Quality**: SOLID principles, clean code practices
- **Documentation**: XML documentation, README standards
- **Git Workflow**: Branch strategy, commit conventions
- **Monitoring**: Performance monitoring, analytics

**Kullanım:** Tüm geliştirme süreçlerinde bu dosyayı referans alın.

---

## Mimari ve Tasarım

### 🏗️ Mimari Kuralları
**Dosya:** [architecture.md](./architecture.md)

Projenin genel mimarisini ve tasarım prensiplerini tanımlar:
- **Katmanlı Mimari**: Presentation, Business, Data Access katmanları
- **Dependency Injection**: Service registration ve lifecycle management
- **Repository Pattern**: Generic ve specific repository implementations
- **Service Pattern**: Business logic organization
- **Area Structure**: MVC area organization
- **Multi-Database Support**: SQL Server, PostgreSQL, MongoDB
- **Authentication/Authorization**: JWT, role-based access
- **Configuration Management**: Strongly-typed configurations

**Kullanım:** Yeni feature geliştirirken mimari kararlar için bu dosyayı inceleyin.

---

## Katman Bazlı Kurallar

### 🎯 Models Katmanı
**Dosya:** [models.md](./models.md)

Model sınıfları için kurallar:
- **Base Model Properties**: Id, CreatedDate, IsActive
- **Validation Attributes**: Required, StringLength, EmailAddress
- **MongoDB Attributes**: BsonId, BsonElement
- **Model Specifications**: User, Dashboard, UserGroup, DashboardPermission
- **Naming Conventions**: PascalCase for properties
- **Relationship Handling**: Navigation properties
- **Data Annotations**: Display, validation attributes

**Kullanım:** Yeni model oluştururken veya mevcut modelleri güncellerken.

### 🎮 Controllers Katmanı
**Dosya:** [controllers.md](./controllers.md)

Controller sınıfları için kurallar:
- **Base Controller Structure**: Common functionality
- **Action Method Guidelines**: HTTP verbs, naming
- **Authorization Rules**: Role-based, policy-based
- **Specific Controllers**: Account, Home, Admin, Dashboard, Profile
- **Error Handling**: Try-catch, model validation
- **Async/Await Patterns**: Best practices
- **API Controller Rules**: RESTful design

**Kullanım:** Controller action'ları geliştirirken.

### ⚙️ Services Katmanı
**Dosya:** [services.md](./services.md)

Service sınıfları için kurallar:
- **Service Interface Definitions**: Contract specifications
- **Implementation Structure**: Dependency injection, logging
- **Specific Services**: UserService, DashboardService, JwtService
- **Business Logic Rules**: Transaction management, validation
- **Password Management**: Hashing, verification
- **Async/Await Best Practices**: Performance considerations
- **Exception Handling**: Business logic exceptions

**Kullanım:** Business logic implementasyonu sırasında.

### 🗄️ Repositories Katmanı
**Dosya:** [repositories.md](./repositories.md)

Repository sınıfları için kurallar:
- **Generic Repository Interfaces**: Common CRUD operations
- **Specific Repository Interfaces**: Domain-specific operations
- **SQL Server Implementations**: Entity Framework, Dapper
- **MongoDB Implementations**: MongoDB.Driver
- **Unit of Work Pattern**: Transaction management
- **Database Connection Factory**: Multi-database support
- **Query Optimization**: Performance best practices

**Kullanım:** Data access layer geliştirirken.

### 🎨 Views Katmanı
**Dosya:** [views.md](./views.md)

View ve UI geliştirme kuralları:
- **Razor View Development**: Layout, partial views
- **Area Views Structure**: Organized view hierarchy
- **Form Validation**: Client-side, server-side
- **AJAX Operations**: Asynchronous requests
- **DataTables Configuration**: Grid components
- **Modal Operations**: Dialog management
- **Responsive Design**: Mobile-friendly UI
- **Security**: XSS prevention

**Kullanım:** UI geliştirme ve view oluştururken.

---

## Güvenlik ve Middleware

### 🔒 Middleware ve Güvenlik
**Dosya:** [middleware.md](./middleware.md)

Güvenlik ve middleware kuralları:
- **JWT Cookie Middleware**: Token handling
- **Request Logging Middleware**: Request/response logging
- **Error Handling Middleware**: Global exception handling
- **Rate Limiting Middleware**: API protection
- **Security Headers Middleware**: HTTPS, HSTS, CSP
- **Authentication Configuration**: JWT setup
- **Authorization Policies**: Role-based, custom policies
- **Input Validation**: Safe string attributes
- **Password Security**: Hashing, verification
- **Audit Logging**: Security events

**Kullanım:** Güvenlik implementasyonu ve middleware geliştirirken.

---

## Veritabanı ve Konfigürasyon

### 💾 Database ve Configuration
**Dosya:** [database.md](./database.md)

Veritabanı ve konfigürasyon kuralları:
- **Multi-Database Support**: SQL Server, PostgreSQL, MongoDB
- **Unit of Work Patterns**: SQL ve MongoDB için
- **Database Migration**: Schema versioning
- **Data Seeding**: Initial data setup
- **Database Health Checks**: Monitoring
- **Configuration Rules**: appsettings.json structure
- **Strongly-Typed Configuration**: Configuration models
- **Environment-Specific Configs**: Development, production

**Kullanım:** Database setup ve configuration management için.

---

## Test ve Deployment

### 🧪 Testing ve Deployment
**Dosya:** [testing.md](./testing.md)

Test ve deployment kuralları:
- **Unit Testing**: Base test classes, mocking
- **Integration Testing**: Database, API tests
- **Performance Testing**: Load testing
- **Test Configuration**: Test-specific settings
- **Docker Deployment**: Development, production containers
- **CI/CD Pipeline**: GitHub Actions workflow
- **Monitoring**: Application Insights, health checks
- **Backup/Recovery**: Database backup strategies

**Kullanım:** Test yazarken ve deployment süreçlerinde.

---

## DevExpress Entegrasyonu

### 📊 DevExpress Kuralları
**Dosya:** [devexpress.md](./devexpress.md)

DevExpress bileşenleri için kurallar:
- **NuGet Packages**: Required packages
- **Program.cs Configuration**: Service registration
- **Custom Dashboard Storage**: Implementation
- **Dashboard Controller**: MVC integration
- **Dashboard Views**: Designer, viewer templates
- **Data Source Configuration**: Connection providers
- **Dashboard Security**: Access control
- **Export/Import Functionality**: Dashboard management
- **Real-time Updates**: SignalR integration
- **Performance Optimization**: Caching strategies

**Kullanım:** DevExpress dashboard bileşenleri geliştirirken.

---

## Hızlı Başlangıç

### 🚀 Yeni Geliştirici İçin Adımlar

1. **Proje Kurulumu**
   - [project-guidelines.md](./project-guidelines.md) dosyasındaki "Quick Start" bölümünü takip edin
   - [database.md](./database.md) dosyasından database setup yapın

2. **Geliştirme Ortamı**
   - [architecture.md](./architecture.md) dosyasından mimariyi anlayın
   - [testing.md](./testing.md) dosyasından test environment setup yapın

3. **İlk Feature Geliştirme**
   - [models.md](./models.md) → Model oluşturma
   - [repositories.md](./repositories.md) → Data access layer
   - [services.md](./services.md) → Business logic
   - [controllers.md](./controllers.md) → API endpoints
   - [views.md](./views.md) → UI components

4. **Güvenlik ve Test**
   - [middleware.md](./middleware.md) → Security implementation
   - [testing.md](./testing.md) → Unit ve integration testler

### 🔧 Mevcut Feature Geliştirme

1. **Analiz**: İlgili katman dosyalarını inceleyin
2. **Tasarım**: [architecture.md](./architecture.md) prensiplerini takip edin
3. **Implementation**: Katman-specific kurallara uyun
4. **Test**: [testing.md](./testing.md) kurallarını uygulayın
5. **Review**: [project-guidelines.md](./project-guidelines.md) code quality kurallarını kontrol edin

### 🐛 Bug Fix Süreci

1. **Debugging**: [project-guidelines.md](./project-guidelines.md) logging kurallarını kullanın
2. **Root Cause Analysis**: İlgili katman dosyalarını inceleyin
3. **Fix Implementation**: Katman kurallarına uygun çözüm geliştirin
4. **Testing**: [testing.md](./testing.md) regression test kurallarını uygulayın

---

## 📚 Dosya Referansları

| Dosya | Açıklama | Ana Kullanım Alanı |
|-------|----------|--------------------|
| [architecture.md](./architecture.md) | Genel mimari ve tasarım prensipleri | Mimari kararlar, yeni feature tasarımı |
| [models.md](./models.md) | Model katmanı kuralları | Entity ve DTO oluşturma |
| [controllers.md](./controllers.md) | Controller katmanı kuralları | API endpoint geliştirme |
| [services.md](./services.md) | Service katmanı kuralları | Business logic implementation |
| [repositories.md](./repositories.md) | Repository katmanı kuralları | Data access layer |
| [views.md](./views.md) | View ve UI kuralları | Frontend geliştirme |
| [middleware.md](./middleware.md) | Middleware ve güvenlik | Security implementation |
| [database.md](./database.md) | Database ve configuration | Database setup, migration |
| [testing.md](./testing.md) | Test ve deployment | Test yazma, CI/CD |
| [devexpress.md](./devexpress.md) | DevExpress entegrasyonu | Dashboard bileşenleri |
| [project-guidelines.md](./project-guidelines.md) | Genel proje kuralları | Code quality, best practices |

---

## ⚡ Hızlı Erişim

### Sık Kullanılan Kurallar
- **Naming Conventions**: [project-guidelines.md](./project-guidelines.md#naming-conventions)
- **Error Handling**: [project-guidelines.md](./project-guidelines.md#error-handling) + [middleware.md](./middleware.md#error-handling)
- **Database Connections**: [database.md](./database.md#multi-database-support)
- **Authentication**: [middleware.md](./middleware.md#authentication-configuration)
- **Validation**: [models.md](./models.md#validation-attributes) + [controllers.md](./controllers.md#model-validation)

### Katman Bazlı Hızlı Erişim
- **Model Oluşturma**: [models.md](./models.md#model-specifications)
- **Service Implementation**: [services.md](./services.md#service-implementation-structure)
- **Repository Pattern**: [repositories.md](./repositories.md#generic-repository-interfaces)
- **Controller Actions**: [controllers.md](./controllers.md#action-method-guidelines)
- **View Development**: [views.md](./views.md#razor-view-development)

---

**Not:** Bu dosya, proje geliştirme sürecinde sürekli güncellenmeli ve tüm ekip üyeleri tarafından referans alınmalıdır. Yeni kurallar eklendiğinde veya mevcut kurallar değiştirildiğinde, ilgili dosyalar güncellenmeli ve bu ana dosyada da referanslar güncellenmelidir.