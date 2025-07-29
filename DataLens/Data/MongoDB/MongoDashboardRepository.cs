using DataLens.Data.Interfaces;
using DataLens.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace DataLens.Data.MongoDB
{
    public class MongoDashboardRepository : IDashboardRepository
    {
        private readonly IMongoCollection<Dashboard> _collection;
        private readonly IMongoCollection<User> _userCollection;

        public MongoDashboardRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<Dashboard>("Dashboards");
            _userCollection = database.GetCollection<User>("Users");
        }

        public async Task<Dashboard?> GetByIdAsync(string id)
        {
            return await _collection.Find(d => d.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Dashboard>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<IEnumerable<Dashboard>> GetByUserIdAsync(string userId)
        {
            return await _collection.Find(d => d.CreatedBy == userId && d.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<Dashboard>> GetByCreatedByAsync(string createdBy)
        {
            return await _collection.Find(d => d.CreatedBy == createdBy && d.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<Dashboard>> GetActiveDashboardsAsync()
        {
            return await _collection.Find(d => d.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<Dashboard>> GetPublicDashboardsAsync()
        {
            return await _collection.Find(d => d.IsPublic && d.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<Dashboard>> GetUserAccessibleDashboardsAsync(string userId)
        {
            // Get dashboards created by user or public dashboards
            var filter = Builders<Dashboard>.Filter.And(
                Builders<Dashboard>.Filter.Eq(d => d.IsActive, true),
                Builders<Dashboard>.Filter.Or(
                    Builders<Dashboard>.Filter.Eq(d => d.CreatedBy, userId),
                    Builders<Dashboard>.Filter.Eq(d => d.IsPublic, true)
                )
            );
            
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Dashboard>> GetGroupAccessibleDashboardsAsync(string groupId)
        {
            // This would require checking dashboard permissions - simplified for now
            return await _collection.Find(d => d.IsPublic && d.IsActive).ToListAsync();
        }

        public async Task<bool> UpdateLastModifiedAsync(string dashboardId, string modifiedBy)
        {
            var update = Builders<Dashboard>.Update
                .Set(d => d.LastModifiedAt, DateTime.UtcNow)
                .Set(d => d.CreatedBy, modifiedBy); // Using CreatedBy as ModifiedBy for now
            
            var result = await _collection.UpdateOneAsync(d => d.Id == dashboardId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<IEnumerable<Dashboard>> SearchDashboardsAsync(string searchTerm)
        {
            return await SearchAsync(searchTerm);
        }

        public async Task<IEnumerable<Dashboard>> SearchAsync(string searchTerm)
        {
            var filter = Builders<Dashboard>.Filter.And(
                Builders<Dashboard>.Filter.Eq(d => d.IsActive, true),
                Builders<Dashboard>.Filter.Or(
                    Builders<Dashboard>.Filter.Regex(d => d.Title, new BsonRegularExpression(searchTerm, "i")),
                    Builders<Dashboard>.Filter.Regex(d => d.Description, new BsonRegularExpression(searchTerm, "i")),
                    Builders<Dashboard>.Filter.Regex(d => d.Category, new BsonRegularExpression(searchTerm, "i"))
                )
            );

            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Dashboard>> SearchAsync(string searchTerm, int skip = 0, int take = 10)
        {
            var filter = Builders<Dashboard>.Filter.And(
                Builders<Dashboard>.Filter.Eq(d => d.IsActive, true),
                Builders<Dashboard>.Filter.Or(
                    Builders<Dashboard>.Filter.Regex(d => d.Title, new BsonRegularExpression(searchTerm, "i")),
                    Builders<Dashboard>.Filter.Regex(d => d.Description, new BsonRegularExpression(searchTerm, "i")),
                    Builders<Dashboard>.Filter.Regex(d => d.Category, new BsonRegularExpression(searchTerm, "i"))
                )
            );

            return await _collection.Find(filter)
                .Skip(skip)
                .Limit(take)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(string id)
        {
            var count = await _collection.CountDocumentsAsync(d => d.Id == id);
            return count > 0;
        }

        public async Task<bool> ExistsAsync(string name, string? excludeId = null)
        {
            var filter = Builders<Dashboard>.Filter.And(
                Builders<Dashboard>.Filter.Eq(d => d.Title, name),
                Builders<Dashboard>.Filter.Eq(d => d.IsActive, true)
            );
            
            if (!string.IsNullOrEmpty(excludeId))
            {
                filter = Builders<Dashboard>.Filter.And(
                    filter,
                    Builders<Dashboard>.Filter.Ne(d => d.Id, excludeId)
                );
            }
            
            var count = await _collection.CountDocumentsAsync(filter);
            return count > 0;
        }

        public async Task<string> AddAsync(Dashboard dashboard)
        {
            if (string.IsNullOrEmpty(dashboard.Id))
            {
                dashboard.Id = ObjectId.GenerateNewId().ToString();
            }
            
            await _collection.InsertOneAsync(dashboard);
            return dashboard.Id;
        }

        public async Task<bool> UpdateAsync(Dashboard dashboard)
        {
            dashboard.LastModifiedAt = DateTime.UtcNow;
            var result = await _collection.ReplaceOneAsync(d => d.Id == dashboard.Id, dashboard);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var update = Builders<Dashboard>.Update.Set(d => d.IsActive, false);
            var result = await _collection.UpdateOneAsync(d => d.Id == id, update);
            return result.ModifiedCount > 0;
        }

        public Task<IEnumerable<Dashboard>> GetByConditionAsync(string condition, object? parameters = null)
        {
            // MongoDB doesn't use SQL conditions, return empty for now
            return Task.FromResult<IEnumerable<Dashboard>>(new List<Dashboard>());
        }

        public async Task<int> CountAsync()
        {
            return (int)await _collection.CountDocumentsAsync(d => d.IsActive);
        }

        public async Task<long> CountByUserAsync(string userId)
        {
            return await _collection.CountDocumentsAsync(d => d.CreatedBy == userId && d.IsActive);
        }

        public async Task<IEnumerable<Dashboard>> GetByCategoryAsync(string category)
        {
            return await _collection.Find(d => d.Category == category && d.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<Dashboard>> GetByTagAsync(string tag)
        {
            var filter = Builders<Dashboard>.Filter.And(
                Builders<Dashboard>.Filter.Eq(d => d.IsActive, true),
                Builders<Dashboard>.Filter.AnyEq(d => d.Tags, tag)
            );
            
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Dashboard>> GetRecentAsync(int count = 10)
        {
            return await _collection.Find(d => d.IsActive)
                .SortByDescending(d => d.CreatedDate)
                .Limit(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Dashboard>> GetMostViewedAsync(int count = 10)
        {
            return await _collection.Find(d => d.IsActive)
                .SortByDescending(d => d.ViewCount)
                .Limit(count)
                .ToListAsync();
        }

        public async Task IncrementViewCountAsync(string id)
        {
            var update = Builders<Dashboard>.Update.Inc(d => d.ViewCount, 1);
            await _collection.UpdateOneAsync(d => d.Id == id, update);
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            var filter = Builders<Dashboard>.Filter.Eq(d => d.IsActive, true);
            return await _collection.Distinct<string>("Category", filter).ToListAsync();
        }

        public async Task<IEnumerable<string>> GetTagsAsync()
        {
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$match", new BsonDocument("IsActive", true)),
                new BsonDocument("$unwind", "$Tags"),
                new BsonDocument("$group", new BsonDocument("_id", "$Tags")),
                new BsonDocument("$sort", new BsonDocument("_id", 1))
            };

            var result = await _collection.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return result.Select(doc => doc["_id"].AsString);
        }
    }
}