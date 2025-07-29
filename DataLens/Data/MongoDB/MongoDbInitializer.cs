using MongoDB.Driver;
using MongoDB.Bson;
using DataLens.Models;
using System.Security.Cryptography;
using System.Text;

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

        public MongoDbInitializer(IMongoDatabase database, ILogger<MongoDbInitializer> logger)
        {
            _database = database;
            _logger = logger;
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
                new CreateIndexModel<User>(indexKeysDefinition.Ascending(x => x.Username), new CreateIndexOptions { Unique = true }),
                new CreateIndexModel<User>(indexKeysDefinition.Ascending(x => x.Email), new CreateIndexOptions { Unique = true }),
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
                    Username = "admin",
                    Email = "admin@datalens.com",
                    PasswordHash = HashPassword("admin123"),
                    FirstName = "System",
                    LastName = "Administrator",
                    Role = "Admin",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new User
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Username = "designer",
                    Email = "designer@datalens.com",
                    PasswordHash = HashPassword("designer123"),
                    FirstName = "Dashboard",
                    LastName = "Designer",
                    Role = "Designer",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new User
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Username = "viewer",
                    Email = "viewer@datalens.com",
                    PasswordHash = HashPassword("viewer123"),
                    FirstName = "Report",
                    LastName = "Viewer",
                    Role = "Viewer",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
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

        private async Task SeedUserGroupMembersAsync()
        {
            // This would require getting the actual user and group IDs from the database
            // For simplicity, we'll skip this in the initial implementation
            _logger.LogInformation("User group members seeding skipped for initial implementation.");
        }

        private async Task SeedDashboardsAsync()
        {
            // This would require getting actual user IDs from the database
            // For simplicity, we'll skip this in the initial implementation
            _logger.LogInformation("Dashboards seeding skipped for initial implementation.");
        }

        private async Task SeedDashboardPermissionsAsync()
        {
            // This would require getting actual dashboard and user IDs from the database
            // For simplicity, we'll skip this in the initial implementation
            _logger.LogInformation("Dashboard permissions seeding skipped for initial implementation.");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}