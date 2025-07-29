# DataLens Dashboard - Mimari Kuralları

## Genel Mimari Prensipler

### 1. Katmanlı Mimari (Layered Architecture)
- **Presentation Layer**: Controllers, Views, Areas
- **Business Logic Layer**: Services, Interfaces
- **Data Access Layer**: Repositories, Unit of Work
- **Data Layer**: Models, Database Connections
- **Cross-Cutting**: Middleware, Extensions, Utilities

### 2. Dependency Injection Kuralları
- Tüm servisler interface üzerinden tanımlanmalı
- Constructor injection kullanılmalı
- Scoped, Singleton, Transient yaşam döngüleri doğru seçilmeli
- Program.cs'de tüm bağımlılıklar kayıt edilmeli

### 3. Repository Pattern
- Her entity için ayrı repository interface'i
- Generic repository base interface kullanımı
- Unit of Work pattern ile transaction yönetimi
- Çoklu veritabanı desteği için adapter pattern

### 4. Service Layer
- Business logic sadece service katmanında
- Controller'lar sadece HTTP isteklerini yönetmeli
- Service'ler repository'leri kullanmalı
- Async/await pattern tutarlı kullanımı

### 5. Areas Yapısı
- **Admin Area**: Kullanıcı ve sistem yönetimi
- **Dashboard Area**: Dashboard işlemleri
- **Profile Area**: Kullanıcı profil yönetimi
- Her area kendi controller, model ve view'larına sahip

### 6. Çoklu Veritabanı Desteği
- SQL Server, PostgreSQL, MongoDB desteği
- Database factory pattern kullanımı
- Configuration-based database selection
- Database-agnostic repository implementations

### 7. Authentication & Authorization
- JWT token-based authentication
- Cookie-based token storage for web
- Role-based authorization (Admin, Designer, Viewer)
- Custom middleware for JWT cookie handling

### 8. Error Handling
- Global exception handling middleware
- Structured logging
- User-friendly error messages
- Database transaction rollback

### 9. Configuration Management
- appsettings.json for environment settings
- Separate development and production configs
- Database connection strings management
- JWT settings configuration

### 10. DevExpress Integration
- Dashboard Designer component
- Dashboard Viewer component
- Custom storage provider
- Data source configuration