# DataLens Dashboard - Model Kuralları

## Model Tanımlama Kuralları

### 1. Base Model Özellikleri
```csharp
// Her model için zorunlu alanlar
[BsonId]
[BsonRepresentation(BsonType.ObjectId)]
public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
public DateTime? UpdatedAt { get; set; }
public bool IsActive { get; set; } = true;
```

### 2. Validation Attributes
- `[Required]` zorunlu alanlar için
- `[StringLength(max)]` string alanlar için
- `[EmailAddress]` email alanları için
- `[Range(min, max)]` sayısal alanlar için
- Custom validation attributes gerektiğinde

### 3. MongoDB Attributes
- `[BsonId]` ID alanları için
- `[BsonRepresentation(BsonType.ObjectId)]` ObjectId referansları için
- `[BsonIgnore]` serialize edilmeyecek alanlar için
- `[BsonElement("field_name")]` farklı alan adları için

### 4. Model Sınıfları

#### User Model
- Username (unique, required, max 50 char)
- Email (unique, required, email format, max 100 char)
- PasswordHash (required, hashed)
- Role (Admin/Designer/Viewer, default: Viewer)
- FirstName, LastName (optional, max 100 char)
- FullName (computed property)
- CreatedDate, UpdatedAt, IsActive

#### Dashboard Model
- Name (required, max 200 char)
- Title (required, max 200 char)
- Description (optional, max 1000 char)
- DashboardData (required, JSON string)
- CreatedBy (required, User ID reference)
- Category (default: "General", max 50 char)
- Tags (List<string>)
- IsPublic (default: false)
- ViewCount (default: 0)
- CreatedDate, LastModifiedDate, IsActive

#### UserGroup Model
- GroupName (unique, required, max 100 char)
- Description (optional, max 500 char)
- CreatedDate, UpdatedAt, IsActive

#### UserGroupMember Model
- UserId (required, User ID reference)
- GroupId (required, UserGroup ID reference)
- JoinedAt (default: DateTime.UtcNow)
- LeftAt (nullable)
- IsActive (default: true)

#### DashboardPermission Model
- DashboardId (required, Dashboard ID reference)
- UserId (nullable, User ID reference)
- GroupId (nullable, UserGroup ID reference)
- PermissionType (View/Edit/Delete/Share)
- GrantedAt (default: DateTime.UtcNow)
- RevokedAt (nullable)
- GrantedBy (required, User ID reference)
- IsActive (default: true)

#### Notification Model
- UserId (required, User ID reference)
- Title (required, max 200 char)
- Message (required, max 1000 char)
- Type (Info/Warning/Error/Success)
- IsRead (default: false)
- ReadAt (nullable)
- CreatedDate, IsActive

#### UserSettings Model
- UserId (required, unique, User ID reference)
- Language (default: "tr-TR")
- Theme (default: "light")
- TimeZone (default: "Europe/Istanbul")
- EmailNotifications (default: true)
- PushNotifications (default: true)
- UpdatedAt

### 5. Naming Conventions
- PascalCase for class names
- PascalCase for property names
- Descriptive and meaningful names
- Avoid abbreviations
- Use singular nouns for class names

### 6. Property Guidelines
- Use nullable types for optional fields
- Provide default values where appropriate
- Use computed properties for derived data
- Keep models focused and cohesive

### 7. Relationships
- Use string IDs for references (MongoDB ObjectId)
- Avoid circular references
- Use navigation properties sparingly
- Implement foreign key constraints in repositories

### 8. Data Annotations
- Use System.ComponentModel.DataAnnotations
- Combine with MongoDB.Bson.Serialization.Attributes
- Ensure compatibility with both SQL and NoSQL

### 9. Model Validation
- Client-side validation with data annotations
- Server-side validation in controllers
- Business rule validation in services
- Database constraint validation in repositories

### 10. Serialization
- JSON serialization for API responses
- BSON serialization for MongoDB
- Handle DateTime serialization properly
- Use camelCase for JSON properties when needed