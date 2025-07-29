using DataLens.Models;
using DataLens.Data.Interfaces;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DataLens.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly IMongoCollection<Dashboard> _dashboards;
        private readonly IConfiguration _configuration;

        public DashboardRepository(IMongoDatabase database, IConfiguration configuration)
        {
            _dashboards = database.GetCollection<Dashboard>("Dashboards");
            _configuration = configuration;
        }

        public async Task<IEnumerable<Dashboard>> GetAllAsync()
        {
            return await _dashboards.Find(_ => true).ToListAsync();
        }

        public async Task<Dashboard?> GetByIdAsync(string id)
        {
            return await _dashboards.Find(d => d.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Dashboard>> GetByCreatedByAsync(string createdBy)
        {
            return await _dashboards.Find(d => d.CreatedBy == createdBy).ToListAsync();
        }

        public async Task<IEnumerable<Dashboard>> GetByCategoryAsync(string category)
        {
            return await _dashboards.Find(d => d.Category == category && d.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<Dashboard>> GetPublicDashboardsAsync()
        {
            return await _dashboards.Find(d => d.IsPublic && d.IsActive).ToListAsync();
        }



        public async Task CreateAsync(Dashboard dashboard)
        {
            dashboard.Id = ObjectId.GenerateNewId().ToString();
            dashboard.CreatedDate = DateTime.UtcNow;
            await _dashboards.InsertOneAsync(dashboard);
        }

        public async Task<bool> UpdateAsync(Dashboard dashboard)
        {
            dashboard.LastModifiedDate = DateTime.UtcNow;
            var result = await _dashboards.ReplaceOneAsync(d => d.Id == dashboard.Id, dashboard);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _dashboards.DeleteOneAsync(d => d.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> ExistsAsync(string name, string? excludeId = null)
        {
            var filter = Builders<Dashboard>.Filter.Eq(d => d.Name, name);
            
            if (!string.IsNullOrEmpty(excludeId))
            {
                filter = filter & Builders<Dashboard>.Filter.Ne(d => d.Id, excludeId);
            }

            var count = await _dashboards.CountDocumentsAsync(filter);
            return count > 0;
        }

        public async Task<IEnumerable<Dashboard>> SearchAsync(string searchTerm)
        {
            var filter = Builders<Dashboard>.Filter.Or(
                Builders<Dashboard>.Filter.Regex(d => d.Name, new BsonRegularExpression(searchTerm, "i")),
                Builders<Dashboard>.Filter.Regex(d => d.Description, new BsonRegularExpression(searchTerm, "i")),
                Builders<Dashboard>.Filter.Regex(d => d.Category, new BsonRegularExpression(searchTerm, "i"))
            );

            filter = filter & Builders<Dashboard>.Filter.Eq(d => d.IsActive, true);

            return await _dashboards.Find(filter).ToListAsync();
        }

        public async Task<long> CountAsync()
        {
            return await _dashboards.CountDocumentsAsync(_ => true);
        }

        public async Task<long> CountByUserAsync(string userId)
        {
            return await _dashboards.CountDocumentsAsync(d => d.CreatedBy == userId && d.IsActive);
        }

        // IRepository<Dashboard> interface implementations
        Task<int> IRepository<Dashboard>.CountAsync()
        {
            return CountAsync().ContinueWith(t => (int)t.Result);
        }

        public async Task<IEnumerable<Dashboard>> GetByConditionAsync(string condition, object? parameters = null)
        {
            // Simple implementation - can be enhanced based on needs
            return await GetAllAsync();
        }

        public async Task<string> AddAsync(Dashboard entity)
        {
            await CreateAsync(entity);
            return entity.Id;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            var dashboard = await GetByIdAsync(id);
            return dashboard != null;
        }

        public async Task<IEnumerable<Dashboard>> SearchDashboardsAsync(string searchTerm)
        {
            return await SearchAsync(searchTerm);
        }

        public async Task<IEnumerable<Dashboard>> GetActiveDashboardsAsync()
        {
            var filter = Builders<Dashboard>.Filter.Eq(d => d.IsActive, true);
            return await _dashboards.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Dashboard>> GetByTagAsync(string tag)
        {
            var filter = Builders<Dashboard>.Filter.And(
                Builders<Dashboard>.Filter.AnyEq(d => d.Tags, tag),
                Builders<Dashboard>.Filter.Eq(d => d.IsActive, true)
            );
            return await _dashboards.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Dashboard>> GetUserAccessibleDashboardsAsync(string userId)
        {
            // Get dashboards where user is creator or has explicit access
            var filter = Builders<Dashboard>.Filter.And(
                Builders<Dashboard>.Filter.Or(
                    Builders<Dashboard>.Filter.Eq(d => d.CreatedBy, userId),
                    Builders<Dashboard>.Filter.Eq(d => d.IsPublic, true)
                ),
                Builders<Dashboard>.Filter.Eq(d => d.IsActive, true)
            );
            return await _dashboards.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Dashboard>> GetGroupAccessibleDashboardsAsync(string groupId)
        {
            // Simple implementation - can be enhanced with proper group permissions
            return await GetActiveDashboardsAsync();
        }

        public async Task<bool> UpdateLastModifiedAsync(string dashboardId, string modifiedBy)
        {
            var filter = Builders<Dashboard>.Filter.Eq(d => d.Id, dashboardId);
            var update = Builders<Dashboard>.Update
                .Set(d => d.LastModifiedDate, DateTime.UtcNow)
                .Set(d => d.LastModifiedBy, modifiedBy);
            
            var result = await _dashboards.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
    }
}