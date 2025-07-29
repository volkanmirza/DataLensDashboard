using DataLens.Data.Interfaces;
using DataLens.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace DataLens.Data.MongoDB
{
    public class MongoDashboardPermissionRepository : IDashboardPermissionRepository
    {
        private readonly IMongoCollection<DashboardPermission> _collection;
        private readonly IMongoCollection<Dashboard> _dashboardCollection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<UserGroup> _groupCollection;

        public MongoDashboardPermissionRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<DashboardPermission>("DashboardPermissions");
            _dashboardCollection = database.GetCollection<Dashboard>("Dashboards");
            _userCollection = database.GetCollection<User>("Users");
            _groupCollection = database.GetCollection<UserGroup>("UserGroups");
        }

        public async Task<DashboardPermission?> GetByIdAsync(string id)
        {
            return await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<DashboardPermission>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<IEnumerable<DashboardPermission>> GetByDashboardIdAsync(string dashboardId)
        {
            return await _collection.Find(p => p.DashboardId == dashboardId && p.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<DashboardPermission>> GetByUserIdAsync(string userId)
        {
            return await _collection.Find(p => p.UserId == userId && p.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<DashboardPermission>> GetByGroupIdAsync(string groupId)
        {
            return await _collection.Find(p => p.GroupId == groupId && p.IsActive).ToListAsync();
        }

        public async Task<DashboardPermission?> GetByUserAndDashboardAsync(string userId, string dashboardId)
        {
            return await _collection.Find(p => p.UserId == userId && p.DashboardId == dashboardId && p.IsActive).FirstOrDefaultAsync();
        }

        public async Task<DashboardPermission?> GetUserPermissionAsync(string dashboardId, string userId)
        {
            return await _collection.Find(p => p.DashboardId == dashboardId && p.UserId == userId && p.IsActive).FirstOrDefaultAsync();
        }

        public async Task<DashboardPermission?> GetByGroupAndDashboardAsync(string groupId, string dashboardId)
        {
            return await _collection.Find(p => p.GroupId == groupId && p.DashboardId == dashboardId && p.IsActive).FirstOrDefaultAsync();
        }

        public async Task<DashboardPermission?> GetGroupPermissionAsync(string dashboardId, string groupId)
        {
            return await _collection.Find(p => p.DashboardId == dashboardId && p.GroupId == groupId && p.IsActive).FirstOrDefaultAsync();
        }

        public async Task<bool> HasGroupPermissionAsync(string dashboardId, string groupId, string permissionType)
        {
            var permission = await _collection.Find(p => 
                p.DashboardId == dashboardId && 
                p.GroupId == groupId && 
                p.PermissionType == permissionType && 
                p.IsActive &&
                (p.ExpiryDate == null || p.ExpiryDate > DateTime.UtcNow)
            ).FirstOrDefaultAsync();
            
            return permission != null;
        }

        public async Task<IEnumerable<DashboardPermission>> GetActivePermissionsAsync()
        {
            return await _collection.Find(p => p.IsActive && (p.ExpiryDate == null || p.ExpiryDate > DateTime.UtcNow)).ToListAsync();
        }

        public async Task<bool> DeleteByDashboardIdAsync(string dashboardId)
        {
            var result = await _collection.DeleteManyAsync(p => p.DashboardId == dashboardId);
            return result.DeletedCount > 0;
        }

        public async Task<bool> DeleteByUserAndDashboardAsync(string userId, string dashboardId)
        {
            var result = await _collection.DeleteManyAsync(p => p.UserId == userId && p.DashboardId == dashboardId);
            return result.DeletedCount > 0;
        }

        public async Task<bool> DeleteByGroupAndDashboardAsync(string groupId, string dashboardId)
        {
            var result = await _collection.DeleteManyAsync(p => p.GroupId == groupId && p.DashboardId == dashboardId);
            return result.DeletedCount > 0;
        }

        public async Task<bool> RevokeUserPermissionAsync(string dashboardId, string userId)
        {
            var update = Builders<DashboardPermission>.Update.Set(p => p.IsActive, false);
            var result = await _collection.UpdateManyAsync(p => 
                p.UserId == userId && 
                p.DashboardId == dashboardId, 
                update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> RevokeGroupPermissionAsync(string dashboardId, string groupId)
        {
            var update = Builders<DashboardPermission>.Update.Set(p => p.IsActive, false);
            var result = await _collection.UpdateManyAsync(p => 
                p.GroupId == groupId && 
                p.DashboardId == dashboardId, 
                update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> RevokeAllDashboardPermissionsAsync(string dashboardId)
        {
            var update = Builders<DashboardPermission>.Update.Set(p => p.IsActive, false);
            var result = await _collection.UpdateManyAsync(p => p.DashboardId == dashboardId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> HasPermissionAsync(string userId, string dashboardId, string permissionType)
        {
            // Check direct user permission
            var userPermission = await _collection.Find(p => 
                p.UserId == userId && 
                p.DashboardId == dashboardId && 
                p.PermissionType == permissionType && 
                p.IsActive &&
                (p.ExpiryDate == null || p.ExpiryDate > DateTime.UtcNow)
            ).FirstOrDefaultAsync();

            if (userPermission != null)
                return true;

            // Check group permissions
            var userGroups = await _collection.Database.GetCollection<UserGroupMembership>("UserGroupMemberships")
                .Find(m => m.UserId == userId && m.IsActive)
                .ToListAsync();

            foreach (var membership in userGroups)
            {
                var groupPermission = await _collection.Find(p => 
                    p.GroupId == membership.GroupId && 
                    p.DashboardId == dashboardId && 
                    p.PermissionType == permissionType && 
                    p.IsActive &&
                    (p.ExpiryDate == null || p.ExpiryDate > DateTime.UtcNow)
                ).FirstOrDefaultAsync();

                if (groupPermission != null)
                    return true;
            }

            return false;
        }

        public async Task<IEnumerable<DashboardPermission>> GetUserPermissionsAsync(string userId, string dashboardId)
        {
            return await _collection.Find(p => p.UserId == userId && p.DashboardId == dashboardId && p.IsActive)
                                   .ToListAsync();
        }

        public async Task<string> AddAsync(DashboardPermission permission)
        {
            if (string.IsNullOrEmpty(permission.Id))
            {
                permission.Id = ObjectId.GenerateNewId().ToString();
            }
            
            await _collection.InsertOneAsync(permission);
            return permission.Id;
        }

        public async Task<bool> UpdateAsync(DashboardPermission permission)
        {
            var result = await _collection.ReplaceOneAsync(p => p.Id == permission.Id, permission);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _collection.DeleteOneAsync(p => p.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            var count = await _collection.CountDocumentsAsync(p => p.Id == id);
            return count > 0;
        }

        public Task<IEnumerable<DashboardPermission>> GetByConditionAsync(string condition, object? parameters = null)
        {
            // MongoDB doesn't use SQL conditions, return empty for now
            return Task.FromResult<IEnumerable<DashboardPermission>>(new List<DashboardPermission>());
        }

        public async Task<int> CountAsync()
        {
            return (int)await _collection.CountDocumentsAsync(_ => true);
        }

        public async Task RevokePermissionAsync(string userId, string dashboardId, string permissionType)
        {
            var update = Builders<DashboardPermission>.Update.Set(p => p.IsActive, false);
            await _collection.UpdateOneAsync(p => 
                p.UserId == userId && 
                p.DashboardId == dashboardId && 
                p.PermissionType == permissionType, 
                update);
        }

        public async Task RevokeGroupPermissionAsync(string groupId, string dashboardId, string permissionType)
        {
            var update = Builders<DashboardPermission>.Update.Set(p => p.IsActive, false);
            await _collection.UpdateOneAsync(p => 
                p.GroupId == groupId && 
                p.DashboardId == dashboardId && 
                p.PermissionType == permissionType, 
                update);
        }

        public async Task RevokeAllUserPermissionsAsync(string userId, string dashboardId)
        {
            var update = Builders<DashboardPermission>.Update.Set(p => p.IsActive, false);
            await _collection.UpdateManyAsync(p => 
                p.UserId == userId && 
                p.DashboardId == dashboardId, 
                update);
        }

        public async Task RevokeAllGroupPermissionsAsync(string groupId, string dashboardId)
        {
            var update = Builders<DashboardPermission>.Update.Set(p => p.IsActive, false);
            await _collection.UpdateManyAsync(p => 
                p.GroupId == groupId && 
                p.DashboardId == dashboardId, 
                update);
        }

        public async Task<int> CountByDashboardAsync(string dashboardId)
        {
            return (int)await _collection.CountDocumentsAsync(p => p.DashboardId == dashboardId && p.IsActive);
        }

        public async Task<int> CountByUserAsync(string userId)
        {
            return (int)await _collection.CountDocumentsAsync(p => p.UserId == userId && p.IsActive);
        }

        public async Task<IEnumerable<DashboardPermission>> GetExpiredPermissionsAsync()
        {
            return await _collection.Find(p => 
                p.IsActive && 
                p.ExpiryDate != null && 
                p.ExpiryDate <= DateTime.UtcNow
            ).ToListAsync();
        }

        public async Task CleanupExpiredPermissionsAsync()
        {
            var update = Builders<DashboardPermission>.Update.Set(p => p.IsActive, false);
            await _collection.UpdateManyAsync(p => 
                p.IsActive && 
                p.ExpiryDate != null && 
                p.ExpiryDate <= DateTime.UtcNow, 
                update);
        }
        public async Task<bool> UpdateUserPermissionAsync(string dashboardId, string userId, string permissionType)
        {
            var filter = Builders<DashboardPermission>.Filter.And(
                Builders<DashboardPermission>.Filter.Eq(p => p.DashboardId, dashboardId),
                Builders<DashboardPermission>.Filter.Eq(p => p.UserId, userId),
                Builders<DashboardPermission>.Filter.Eq(p => p.IsActive, true)
            );
            
            var update = Builders<DashboardPermission>.Update.Set(p => p.PermissionType, permissionType);
            
            var result = await _collection.UpdateOneAsync(filter, update);
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
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<IEnumerable<DashboardPermission>> GetGroupPermissionsAsync(string groupId, string dashboardId)
        {
            return await _collection.Find(p => p.GroupId == groupId && p.DashboardId == dashboardId && p.IsActive)
                                   .ToListAsync();
        }
    }
}