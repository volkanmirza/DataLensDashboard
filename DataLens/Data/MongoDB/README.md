# MongoDB Collection Initialization Scripts

Bu klasör MongoDB veritabanı için gerekli collection'ları ve başlangıç verilerini oluşturmak için kullanılan script'leri içerir.

## Dosyalar

### 1. InitializeCollections.js
MongoDB collection'larını validation schema'ları ve index'leri ile birlikte oluşturur.

**Özellikler:**
- Collection validation schema'ları
- Unique index'ler
- Performance index'leri
- Data integrity constraints

**Kullanım:**
```bash
# MongoDB Shell ile
mongosh "mongodb://localhost:27017/DataLensDb" InitializeCollections.js

# veya MongoDB Compass'ta script olarak çalıştırın
```

### 2. SeedData.js
Veritabanına başlangıç verilerini ekler.

**İçerik:**
- 3 varsayılan kullanıcı (admin, designer, viewer)
- 3 kullanıcı grubu
- Örnek dashboard
- Dashboard izinleri

**Varsayılan Kullanıcılar:**
- **admin** / admin123 (Admin rolü)
- **designer** / designer123 (Designer rolü)
- **viewer** / viewer123 (Viewer rolü)

**Kullanım:**
```bash
# MongoDB Shell ile
mongosh "mongodb://localhost:27017/DataLensDb" SeedData.js
```

### 3. MongoDbInitializer.cs
C# tabanlı MongoDB initialization service'i.

**Özellikler:**
- Programmatic collection creation
- Index management
- Seed data insertion
- Error handling ve logging

**Kullanım:**
```csharp
// Program.cs'te service registration
builder.Services.AddScoped<IMongoDbInitializer, MongoDbInitializer>();

// Startup'ta initialization
var mongoInitializer = app.Services.GetRequiredService<IMongoDbInitializer>();
if (!await mongoInitializer.IsDatabaseInitializedAsync())
{
    await mongoInitializer.InitializeAsync();
    await mongoInitializer.SeedDataAsync();
}
```

## Kurulum Adımları

### 1. MongoDB Kurulumu
```bash
# MongoDB Community Server'ı indirin ve kurun
# https://www.mongodb.com/try/download/community

# MongoDB servisini başlatın
net start MongoDB
```

### 2. Database Initialization

#### Yöntem 1: JavaScript Scripts (Önerilen)
```bash
# 1. Collection'ları oluşturun
mongosh "mongodb://localhost:27017/DataLensDb" InitializeCollections.js

# 2. Seed data'yı ekleyin
mongosh "mongodb://localhost:27017/DataLensDb" SeedData.js
```

#### Yöntem 2: C# Service
```csharp
// Program.cs'te MongoDB initialization ekleyin
if (databaseType == "MongoDB")
{
    builder.Services.AddScoped<IMongoDbInitializer, MongoDbInitializer>();
}

// Application startup'ta
if (databaseType == "MongoDB")
{
    var mongoInitializer = app.Services.GetRequiredService<IMongoDbInitializer>();
    if (!await mongoInitializer.IsDatabaseInitializedAsync())
    {
        await mongoInitializer.InitializeAsync();
        await mongoInitializer.SeedDataAsync();
    }
}
```

### 3. Connection String Konfigürasyonu
```json
{
  "DatabaseSettings": {
    "DatabaseType": "MongoDB",
    "ConnectionStrings": {
      "MongoDB": "mongodb://localhost:27017/DataLensDb"
    }
  }
}
```

## Collection Yapısı

### Users
- Username (unique)
- Email (unique)
- PasswordHash
- FirstName, LastName
- Role (Admin, Designer, Viewer)
- IsActive
- CreatedAt, UpdatedAt, LastLoginAt

### UserGroups
- GroupName (unique)
- Description
- IsActive
- CreatedAt, UpdatedAt

### UserGroupMembers
- UserId (ObjectId reference)
- GroupId (ObjectId reference)
- IsActive
- JoinedAt, LeftAt

### Dashboards
- Title
- Description, Category, Tags
- CreatedBy (ObjectId reference)
- IsPublic, IsActive
- CreatedAt, UpdatedAt, LastModifiedAt
- DashboardData (JSON object)

### DashboardPermissions
- DashboardId (ObjectId reference)
- UserId/GroupId (ObjectId reference)
- PermissionType (View, Edit, Delete, Share)
- IsActive
- GrantedAt, RevokedAt
- GrantedBy (ObjectId reference)

## Troubleshooting

### MongoDB Connection Issues
```bash
# MongoDB servisinin çalıştığını kontrol edin
net start MongoDB

# Connection string'i doğrulayın
mongosh "mongodb://localhost:27017/DataLensDb"
```

### Script Execution Issues
```bash
# MongoDB Shell version'ını kontrol edin
mongosh --version

# Legacy mongo shell kullanıyorsanız:
mongo "mongodb://localhost:27017/DataLensDb" InitializeCollections.js
```

### Permission Issues
```bash
# MongoDB authentication gerekiyorsa:
mongosh "mongodb://username:password@localhost:27017/DataLensDb" InitializeCollections.js
```

## Notlar

- Script'ler idempotent'tir (birden fazla kez çalıştırılabilir)
- Mevcut data varsa seed data eklenmez
- Collection'lar zaten varsa tekrar oluşturulmaz
- Tüm script'ler error handling içerir
- Production ortamında güçlü şifreler kullanın
- MongoDB authentication ve authorization yapılandırın