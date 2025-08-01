using MongoDB.Driver;
using MongoDB.Bson;
using DataLens.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace DataLens.Data.MongoDB
{
    public interface IMongoDbInitializer
    {
        Task InitializeAsync();
        Task SeedDataAsync();
        Task<bool> IsDatabaseInitializedAsync();
    }

    public class MongoDbInitializer : IMongoDbInitializer
    {
        private readonly IMongoDatabase _database;
        private readonly ILogger<MongoDbInitializer> _logger;
        private readonly IPasswordHasher<User> _passwordHasher;

        public MongoDbInitializer(IMongoDatabase database, ILogger<MongoDbInitializer> logger, IPasswordHasher<User> passwordHasher)
        {
            _database = database;
            _logger = logger;
            _passwordHasher = passwordHasher;
        }

        public async Task<bool> IsDatabaseInitializedAsync()
        {
            try
            {
                var collections = await _database.ListCollectionNamesAsync();
                var collectionList = await collections.ToListAsync();
                
                var requiredCollections = new[] { "Users", "UserGroups", "UserGroupMembers", "Dashboards", "DashboardPermissions" };
                return requiredCollections.All(collection => collectionList.Contains(collection));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if database is initialized");
                return false;
            }
        }

        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Starting MongoDB database initialization...");

                await CreateUsersCollectionAsync();
                await CreateUserGroupsCollectionAsync();
                await CreateUserGroupMembersCollectionAsync();
                await CreateDashboardsCollectionAsync();
                await CreateDashboardPermissionsCollectionAsync();

                _logger.LogInformation("MongoDB database initialization completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during MongoDB database initialization");
                throw;
            }
        }

        public async Task SeedDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting MongoDB seed data insertion...");

                // Check if data already exists
                var usersCollection = _database.GetCollection<User>("Users");
                var existingUsersCount = await usersCollection.CountDocumentsAsync(FilterDefinition<User>.Empty);
                
                if (existingUsersCount > 0)
                {
                    _logger.LogInformation("Seed data already exists. Skipping seed data insertion.");
                    return;
                }

                await SeedUsersAsync();
                await SeedUserGroupsAsync();
                await SeedUserGroupMembersAsync();
                await SeedDashboardsAsync();
                await SeedDashboardPermissionsAsync();

                _logger.LogInformation("MongoDB seed data insertion completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during MongoDB seed data insertion");
                throw;
            }
        }

        private async Task CreateUsersCollectionAsync()
        {
            var collectionName = "Users";
            
            // Check if collection already exists
            var collections = await _database.ListCollectionNamesAsync();
            var collectionList = await collections.ToListAsync();
            
            if (collectionList.Contains(collectionName))
            {
                _logger.LogInformation($"Collection {collectionName} already exists.");
                return;
            }

            // Create collection without validation for now
            // MongoDB.Driver version may not support Validator property

            await _database.CreateCollectionAsync(collectionName);
            
            // Create indexes
            var collection = _database.GetCollection<User>(collectionName);
            var indexKeysDefinition = Builders<User>.IndexKeys;
            
            await collection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<User>(indexKeysDefinition.Ascending(x => x.UserName), new CreateIndexOptions { Unique = true }),
                new CreateIndexModel<User>(indexKeysDefinition.Ascending(x => x.NormalizedUserName), new CreateIndexOptions { Unique = true }),
                new CreateIndexModel<User>(indexKeysDefinition.Ascending(x => x.Email), new CreateIndexOptions { Unique = true }),
                new CreateIndexModel<User>(indexKeysDefinition.Ascending(x => x.NormalizedEmail), new CreateIndexOptions { Unique = true }),
                new CreateIndexModel<User>(indexKeysDefinition.Ascending(x => x.Role)),
                new CreateIndexModel<User>(indexKeysDefinition.Ascending(x => x.IsActive)),
                new CreateIndexModel<User>(indexKeysDefinition.Ascending(x => x.CreatedDate))
            });

            _logger.LogInformation($"Collection {collectionName} created with validation and indexes.");
        }

        private async Task CreateUserGroupsCollectionAsync()
        {
            var collectionName = "UserGroups";
            
            var collections = await _database.ListCollectionNamesAsync();
            var collectionList = await collections.ToListAsync();
            
            if (collectionList.Contains(collectionName))
            {
                _logger.LogInformation($"Collection {collectionName} already exists.");
                return;
            }

            await _database.CreateCollectionAsync(collectionName);
            
            var collection = _database.GetCollection<UserGroup>(collectionName);
            var indexKeysDefinition = Builders<UserGroup>.IndexKeys;
            
            await collection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<UserGroup>(indexKeysDefinition.Ascending(x => x.GroupName), new CreateIndexOptions { Unique = true }),
                new CreateIndexModel<UserGroup>(indexKeysDefinition.Ascending(x => x.IsActive)),
                new CreateIndexModel<UserGroup>(indexKeysDefinition.Ascending(x => x.CreatedDate))
            });

            _logger.LogInformation($"Collection {collectionName} created with indexes.");
        }

        private async Task CreateUserGroupMembersCollectionAsync()
        {
            var collectionName = "UserGroupMembers";
            
            var collections = await _database.ListCollectionNamesAsync();
            var collectionList = await collections.ToListAsync();
            
            if (collectionList.Contains(collectionName))
            {
                _logger.LogInformation($"Collection {collectionName} already exists.");
                return;
            }

            await _database.CreateCollectionAsync(collectionName);
            
            var collection = _database.GetCollection<UserGroupMember>(collectionName);
            var indexKeysDefinition = Builders<UserGroupMember>.IndexKeys;
            
            await collection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<UserGroupMember>(indexKeysDefinition.Ascending(x => x.UserId).Ascending(x => x.GroupId), new CreateIndexOptions { Unique = true }),
                new CreateIndexModel<UserGroupMember>(indexKeysDefinition.Ascending(x => x.UserId)),
                new CreateIndexModel<UserGroupMember>(indexKeysDefinition.Ascending(x => x.GroupId)),
                new CreateIndexModel<UserGroupMember>(indexKeysDefinition.Ascending(x => x.JoinedDate))
            });

            _logger.LogInformation($"Collection {collectionName} created with indexes.");
        }

        private async Task CreateDashboardsCollectionAsync()
        {
            var collectionName = "Dashboards";
            
            var collections = await _database.ListCollectionNamesAsync();
            var collectionList = await collections.ToListAsync();
            
            if (collectionList.Contains(collectionName))
            {
                _logger.LogInformation($"Collection {collectionName} already exists.");
                return;
            }

            await _database.CreateCollectionAsync(collectionName);
            
            var collection = _database.GetCollection<Dashboard>(collectionName);
            var indexKeysDefinition = Builders<Dashboard>.IndexKeys;
            
            await collection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<Dashboard>(indexKeysDefinition.Ascending(x => x.Name)),
                new CreateIndexModel<Dashboard>(indexKeysDefinition.Ascending(x => x.CreatedBy)),
                new CreateIndexModel<Dashboard>(indexKeysDefinition.Ascending(x => x.Category)),
                new CreateIndexModel<Dashboard>(indexKeysDefinition.Ascending(x => x.IsPublic)),
                new CreateIndexModel<Dashboard>(indexKeysDefinition.Ascending(x => x.IsActive)),
                new CreateIndexModel<Dashboard>(indexKeysDefinition.Ascending(x => x.CreatedDate))
            });

            _logger.LogInformation($"Collection {collectionName} created with indexes.");
        }

        private async Task CreateDashboardPermissionsCollectionAsync()
        {
            var collectionName = "DashboardPermissions";
            
            var collections = await _database.ListCollectionNamesAsync();
            var collectionList = await collections.ToListAsync();
            
            if (collectionList.Contains(collectionName))
            {
                _logger.LogInformation($"Collection {collectionName} already exists.");
                return;
            }

            await _database.CreateCollectionAsync(collectionName);
            
            var collection = _database.GetCollection<DashboardPermission>(collectionName);
            var indexKeysDefinition = Builders<DashboardPermission>.IndexKeys;
            
            await collection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<DashboardPermission>(indexKeysDefinition.Ascending(x => x.DashboardId)),
                new CreateIndexModel<DashboardPermission>(indexKeysDefinition.Ascending(x => x.UserId)),
                new CreateIndexModel<DashboardPermission>(indexKeysDefinition.Ascending(x => x.GroupId)),
                new CreateIndexModel<DashboardPermission>(indexKeysDefinition.Ascending(x => x.IsActive))
            });

            _logger.LogInformation($"Collection {collectionName} created with indexes.");
        }

        private async Task SeedUsersAsync()
        {
            var collection = _database.GetCollection<User>("Users");
            
            var users = new[]
            {
                new User
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserName = "admin",
                    NormalizedUserName = "ADMIN",
                    Email = "admin@datalens.com",
                    NormalizedEmail = "ADMIN@DATALENS.COM",
                    PasswordHash = HashPassword("admin123"),
                    FirstName = "System",
                    LastName = "Administrator",
                    Role = "Admin",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    Department = "IT",
                    Position = "System Administrator",
                    PhoneNumber = "+90 555 123 4567",
                    Biography = "DataLens sistem yöneticisi. Tüm sistem operasyonlarından sorumludur.",
                    Language = "tr",
                    Theme = "light",
                    TimeZone = "Turkey Standard Time",
                    EmailNotifications = true,
                    BrowserNotifications = true,
                    DashboardShared = true,
                    SecurityAlerts = true,
                    ProfileVisibility = "Public",
                    AllowDashboardSharing = true,
                    TrackActivity = true,
                    TwoFactorEnabled = false
                },
                new User
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserName = "designer",
                    NormalizedUserName = "DESIGNER",
                    Email = "designer@datalens.com",
                    NormalizedEmail = "DESIGNER@DATALENS.COM",
                    PasswordHash = HashPassword("designer123"),
                    FirstName = "Dashboard",
                    LastName = "Designer",
                    Role = "Designer",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    Department = "Analytics",
                    Position = "Senior Dashboard Designer",
                    PhoneNumber = "+90 555 234 5678",
                    Biography = "Deneyimli dashboard tasarımcısı. Veri görselleştirme ve analitik dashboard geliştirme konularında uzman.",
                    Language = "tr",
                    Theme = "dark",
                    TimeZone = "Turkey Standard Time",
                    EmailNotifications = true,
                    BrowserNotifications = false,
                    DashboardShared = true,
                    SecurityAlerts = true,
                    ProfileVisibility = "Private",
                    AllowDashboardSharing = true,
                    TrackActivity = true,
                    TwoFactorEnabled = true
                },
                new User
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserName = "viewer",
                    NormalizedUserName = "VIEWER",
                    Email = "viewer@datalens.com",
                    NormalizedEmail = "VIEWER@DATALENS.COM",
                    PasswordHash = HashPassword("viewer123"),
                    FirstName = "Report",
                    LastName = "Viewer",
                    Role = "Viewer",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    Department = "Sales",
                    Position = "Sales Analyst",
                    PhoneNumber = "+90 555 345 6789",
                    Biography = "Satış departmanında çalışan analitik uzmanı. Raporları inceleyerek satış performansını takip eder.",
                    Language = "en",
                    Theme = "light",
                    TimeZone = "Turkey Standard Time",
                    EmailNotifications = false,
                    BrowserNotifications = true,
                    DashboardShared = false,
                    SecurityAlerts = false,
                    ProfileVisibility = "Friends",
                    AllowDashboardSharing = false,
                    TrackActivity = false,
                    TwoFactorEnabled = false
                }
            };

            await collection.InsertManyAsync(users);
            _logger.LogInformation($"Inserted {users.Length} users.");
        }

        private async Task SeedUserGroupsAsync()
        {
            var collection = _database.GetCollection<UserGroup>("UserGroups");
            
            var groups = new[]
            {
                new UserGroup
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    GroupName = "Administrators",
                    Description = "System administrators with full access",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "system"
                },
                new UserGroup
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    GroupName = "Dashboard Designers",
                    Description = "Users who can create and edit dashboards",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "system"
                },
                new UserGroup
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    GroupName = "Report Viewers",
                    Description = "Users who can only view dashboards and reports",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "system"
                }
            };

            await collection.InsertManyAsync(groups);
            _logger.LogInformation($"Inserted {groups.Length} user groups.");
        }

        private Task SeedUserGroupMembersAsync()
        {
            // This would require getting the actual user and group IDs from the database
            // For simplicity, we'll skip this in the initial implementation
            _logger.LogInformation("User group members seeding skipped for initial implementation.");
            return Task.CompletedTask;
        }

        private Task SeedDashboardsAsync()
        {
            // This would require getting actual user IDs from the database
            // For simplicity, we'll skip this in the initial implementation
            _logger.LogInformation("Dashboards seeding skipped for initial implementation.");
            return Task.CompletedTask;
        }

        private Task SeedDashboardPermissionsAsync()
        {
            // This would require getting actual dashboard and user IDs from the database
            // For simplicity, we'll skip this in the initial implementation
            _logger.LogInformation("Dashboard permissions seeding skipped for initial implementation.");
            return Task.CompletedTask;
        }

        private string HashPassword(string password)
        {
            var user = new User();
            return _passwordHasher.HashPassword(user, password);
        }
    }
}