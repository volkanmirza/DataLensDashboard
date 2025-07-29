using DataLens.Models;
using DataLens.Data.Interfaces;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DataLens.Repositories
{
    public class DashboardPermissionRepository : IDashboardPermissionRepository
    {
        private readonly IMongoCollection<DashboardPermission> _permissions;
        private readonly IConfiguration _configuration;

        public DashboardPermissionRepository(IMongoDatabase database, IConfiguration configuration)
        {
            _permissions = database.GetCollection<DashboardPermission>("DashboardPermissions");
            _configuration = configuration;
        }

        public async Task<IEnumerable<DashboardPermission>> GetAllAsync()
        {
            return await _permissions.Find(_ => true).ToListAsync();
        }

        public async Task<DashboardPermission?> GetByIdAsync(string id)
        {
            return await _permissions.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<DashboardPermission>> GetByDashboardIdAsync(string dashboardId)
        {
            return await _permissions.Find(p => p.DashboardId == dashboardId && p.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<DashboardPermission>> GetByUserIdAsync(string userId)
        {
            return await _permissions.Find(p => p.UserId == userId && p.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<DashboardPermission>> GetByGroupIdAsync(string groupId)
        {
            return await _permissions.Find(p => p.GroupId == groupId && p.IsActive).ToListAsync();
        }

        public async Task<DashboardPermission?> GetByUserAndDashboardAsync(string userId, string dashboardId)
        {
            return await _permissions.Find(p => p.UserId == userId && p.DashboardId == dashboardId && p.IsActive)
                                   .FirstOrDefaultAsync();
        }

        public async Task<DashboardPermission?> GetByGroupAndDashboardAsync(string groupId, string dashboardId)
        {
            return await _permissions.Find(p => p.GroupId == groupId && p.DashboardId == dashboardId && p.IsActive)
                                   .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<DashboardPermission>> GetUserPermissionsAsync(string userId, string dashboardId)
        {
            return await _permissions.Find(p => p.UserId == userId && p.DashboardId == dashboardId && p.IsActive)
                                   .ToListAsync();
        }

        public async Task<IEnumerable<DashboardPermission>> GetGroupPermissionsAsync(string groupId, string dashboardId)
        {
            return await _permissions.Find(p => p.GroupId == groupId && p.DashboardId == dashboardId && p.IsActive)
                                   .ToListAsync();
        }

        public async Task CreateAsync(DashboardPermission permission)
        {
            permission.Id = ObjectId.GenerateNewId().ToString();
            permission.GrantedDate = DateTime.UtcNow;
            await _permissions.InsertOneAsync(permission);
        }

        public async Task<bool> UpdateAsync(DashboardPermission permission)
        {
            var result = await _permissions.ReplaceOneAsync(p => p.Id == permission.Id, permission);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _permissions.DeleteOneAsync(p => p.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> DeleteByDashboardIdAsync(string dashboardId)
        {
            var result = await _permissions.DeleteManyAsync(p => p.DashboardId == dashboardId);
            return result.DeletedCount > 0;
        }

        public async Task<bool> DeleteByUserAndDashboardAsync(string userId, string dashboardId)
        {
            var result = await _permissions.DeleteOneAsync(p => p.UserId == userId && p.DashboardId == dashboardId);
            return result.DeletedCount > 0;
        }

        public async Task<bool> DeleteByGroupAndDashboardAsync(string groupId, string dashboardId)
        {
            var result = await _permissions.DeleteOneAsync(p => p.GroupId == groupId && p.DashboardId == dashboardId);
            return result.DeletedCount > 0;
        }

        public async Task<bool> ExistsAsync(string userId, string groupId, string dashboardId, string permissionType)
        {
            var filter = Builders<DashboardPermission>.Filter.And(
                Builders<DashboardPermission>.Filter.Eq(p => p.DashboardId, dashboardId),
                Builders<DashboardPermission>.Filter.Eq(p => p.PermissionType, permissionType),
                Builders<DashboardPermission>.Filter.Eq(p => p.IsActive, true)
            );

            if (!string.IsNullOrEmpty(userId))
            {
                filter = filter & Builders<DashboardPermission>.Filter.Eq(p => p.UserId, userId);
            }

            if (!string.IsNullOrEmpty(groupId))
            {
                filter = filter & Builders<DashboardPermission>.Filter.Eq(p => p.GroupId, groupId);
            }

            var count = await _permissions.CountDocumentsAsync(filter);
            return count > 0;
        }

        public async Task<bool> HasPermissionAsync(string userId, string dashboardId, string permissionType)
        {
            var filter = Builders<DashboardPermission>.Filter.And(
                Builders<DashboardPermission>.Filter.Eq(p => p.UserId, userId),
                Builders<DashboardPermission>.Filter.Eq(p => p.DashboardId, dashboardId),
                Builders<DashboardPermission>.Filter.Eq(p => p.PermissionType, permissionType),
                Builders<DashboardPermission>.Filter.Eq(p => p.IsActive, true)
            );

            var count = await _permissions.CountDocumentsAsync(filter);
            return count > 0;
        }

        public async Task<bool> HasGroupPermissionAsync(string groupId, string dashboardId, string permissionType)
        {
            var filter = Builders<DashboardPermission>.Filter.And(
                Builders<DashboardPermission>.Filter.Eq(p => p.GroupId, groupId),
                Builders<DashboardPermission>.Filter.Eq(p => p.DashboardId, dashboardId),
                Builders<DashboardPermission>.Filter.Eq(p => p.PermissionType, permissionType),
                Builders<DashboardPermission>.Filter.Eq(p => p.IsActive, true)
            );

            var count = await _permissions.CountDocumentsAsync(filter);
            return count > 0;
        }

        public async Task<long> CountByDashboardAsync(string dashboardId)
        {
            return await _permissions.CountDocumentsAsync(p => p.DashboardId == dashboardId && p.IsActive);
        }

        // IRepository<DashboardPermission> interface implementations
        public async Task<IEnumerable<DashboardPermission>> GetByConditionAsync(string condition, object? parameters = null)
        {
            // Simple implementation - can be enhanced based on needs
            return await GetAllAsync();
        }

        public async Task<string> AddAsync(DashboardPermission entity)
        {
            await CreateAsync(entity);
            return entity.Id;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            var permission = await GetByIdAsync(id);
            return permission != null;
        }

        public async Task<int> CountAsync()
        {
            var count = await _permissions.CountDocumentsAsync(_ => true);
            return (int)count;
        }

        public async Task<IEnumerable<DashboardPermission>> GetActivePermissionsAsync()
        {
            var filter = Builders<DashboardPermission>.Filter.Eq(p => p.IsActive, true);
            return await _permissions.Find(filter).ToListAsync();
        }

        public async Task<bool> RevokeUserPermissionAsync(string dashboardId, string userId)
        {
            return await DeleteByUserAndDashboardAsync(userId, dashboardId);
        }

        public async Task<bool> RevokeGroupPermissionAsync(string dashboardId, string groupId)
        {
            return await DeleteByGroupAndDashboardAsync(groupId, dashboardId);
        }

        public async Task<bool> RevokeAllDashboardPermissionsAsync(string dashboardId)
        {
            return await DeleteByDashboardIdAsync(dashboardId);
        }

        public async Task<DashboardPermission?> GetUserPermissionAsync(string dashboardId, string userId)
        {
            var filter = Builders<DashboardPermission>.Filter.And(
                Builders<DashboardPermission>.Filter.Eq(p => p.DashboardId, dashboardId),
                Builders<DashboardPermission>.Filter.Eq(p => p.UserId, userId),
                Builders<DashboardPermission>.Filter.Eq(p => p.IsActive, true)
            );
            return await _permissions.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<DashboardPermission?> GetGroupPermissionAsync(string dashboardId, string groupId)
        {
            var filter = Builders<DashboardPermission>.Filter.And(
                Builders<DashboardPermission>.Filter.Eq(p => p.DashboardId, dashboardId),
                Builders<DashboardPermission>.Filter.Eq(p => p.GroupId, groupId),
                Builders<DashboardPermission>.Filter.Eq(p => p.IsActive, true)
            );
            return await _permissions.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateUserPermissionAsync(string dashboardId, string userId, string permissionType)
        {
            var filter = Builders<DashboardPermission>.Filter.And(
                Builders<DashboardPermission>.Filter.Eq(p => p.DashboardId, dashboardId),
                Builders<DashboardPermission>.Filter.Eq(p => p.UserId, userId),
                Builders<DashboardPermission>.Filter.Eq(p => p.IsActive, true)
            );
            
            var update = Builders<DashboardPermission>.Update.Set(p => p.PermissionType, permissionType);
            
            var result = await _permissions.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateGroupPermissionAsync(string dashboardId, string groupId, string permissionType)
        {
            var filter = Builders<DashboardPermission>.Filter.And(
                Builders<DashboardPermission>.Filter.Eq(p => p.DashboardId, dashboardId),
                Builders<DashboardPermission>.Filter.Eq(p => p.GroupId, groupId),
                Builders<DashboardPermission>.Filter.Eq(p => p.IsActive, true)
            );
            
            var update = Builders<DashboardPermission>.Update.Set(p => p.PermissionType, permissionType);
            
            var result = await _permissions.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
    }
}