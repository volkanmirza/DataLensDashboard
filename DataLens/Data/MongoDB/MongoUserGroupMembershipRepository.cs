using DataLens.Data.Interfaces;
using DataLens.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace DataLens.Data.MongoDB
{
    public class MongoUserGroupMembershipRepository : IUserGroupMembershipRepository
    {
        private readonly IMongoCollection<UserGroupMembership> _collection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<UserGroup> _groupCollection;

        public MongoUserGroupMembershipRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<UserGroupMembership>("UserGroupMemberships");
            _userCollection = database.GetCollection<User>("Users");
            _groupCollection = database.GetCollection<UserGroup>("UserGroups");
        }

        public async Task<UserGroupMembership?> GetByIdAsync(string id)
        {
            return await _collection.Find(m => m.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<UserGroupMembership>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<IEnumerable<UserGroupMembership>> GetByUserIdAsync(string userId)
        {
            return await _collection.Find(m => m.UserId == userId && m.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<UserGroupMembership>> GetByGroupIdAsync(string groupId)
        {
            return await _collection.Find(m => m.GroupId == groupId && m.IsActive).ToListAsync();
        }

        public async Task<UserGroupMembership?> GetByUserAndGroupAsync(string userId, string groupId)
        {
            return await _collection.Find(m => m.UserId == userId && m.GroupId == groupId).FirstOrDefaultAsync();
        }

        public async Task<bool> ExistsAsync(string userId, string groupId)
        {
            var count = await _collection.CountDocumentsAsync(m => m.UserId == userId && m.GroupId == groupId && m.IsActive);
            return count > 0;
        }

        public async Task<string> AddAsync(UserGroupMembership membership)
        {
            if (string.IsNullOrEmpty(membership.Id))
            {
                membership.Id = ObjectId.GenerateNewId().ToString();
            }
            
            await _collection.InsertOneAsync(membership);
            return membership.Id;
        }

        public async Task<bool> UpdateAsync(UserGroupMembership membership)
        {
            var result = await _collection.ReplaceOneAsync(m => m.Id == membership.Id, membership);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _collection.DeleteOneAsync(m => m.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            var count = await _collection.CountDocumentsAsync(m => m.Id == id);
            return count > 0;
        }

        public async Task<IEnumerable<UserGroupMembership>> GetByConditionAsync(string condition, object? parameters = null)
        {
            // MongoDB doesn't use SQL conditions, return empty for now
            return await Task.FromResult(new List<UserGroupMembership>());
        }

        public async Task<int> CountAsync()
        {
            return (int)await _collection.CountDocumentsAsync(_ => true);
        }

        public async Task<int> CountByUserAsync(string userId)
        {
            return (int)await _collection.CountDocumentsAsync(m => m.UserId == userId && m.IsActive);
        }

        public async Task<int> CountByGroupAsync(string groupId)
        {
            return (int)await _collection.CountDocumentsAsync(m => m.GroupId == groupId && m.IsActive);
        }

        public async Task RemoveUserFromGroupAsync(string userId, string groupId, string removedBy)
        {
            var filter = Builders<UserGroupMembership>.Filter.And(
                Builders<UserGroupMembership>.Filter.Eq(m => m.UserId, userId),
                Builders<UserGroupMembership>.Filter.Eq(m => m.GroupId, groupId),
                Builders<UserGroupMembership>.Filter.Eq(m => m.IsActive, true)
            );

            var update = Builders<UserGroupMembership>.Update
                .Set(m => m.IsActive, false)
                .Set(m => m.RemovedDate, DateTime.UtcNow)
                .Set(m => m.RemovedBy, removedBy);

            await _collection.UpdateOneAsync(filter, update);
        }

        public async Task<IEnumerable<UserGroupMembership>> GetActiveByUserIdAsync(string userId)
        {
            return await _collection.Find(m => m.UserId == userId && m.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<UserGroupMembership>> GetActiveByGroupIdAsync(string groupId)
        {
            return await _collection.Find(m => m.GroupId == groupId && m.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<User>> GetGroupMembersAsync(string groupId)
        {
            var memberships = await GetByGroupIdAsync(groupId);
            var userIds = memberships.Select(m => m.UserId).ToList();
            return await _userCollection.Find(u => userIds.Contains(u.Id)).ToListAsync();
        }

        public async Task<IEnumerable<UserGroup>> GetUserGroupsAsync(string userId)
        {
            var memberships = await GetByUserIdAsync(userId);
            var groupIds = memberships.Select(m => m.GroupId).ToList();
            return await _groupCollection.Find(g => groupIds.Contains(g.Id)).ToListAsync();
        }

        public async Task<bool> IsUserInGroupAsync(string userId, string groupId)
        {
            return await ExistsAsync(userId, groupId);
        }

        public async Task<int> GetGroupMemberCountAsync(string groupId)
        {
            return await CountByGroupAsync(groupId);
        }

        public async Task<bool> RemoveUserFromAllGroupsAsync(string userId)
        {
            var filter = Builders<UserGroupMembership>.Filter.And(
                Builders<UserGroupMembership>.Filter.Eq(m => m.UserId, userId),
                Builders<UserGroupMembership>.Filter.Eq(m => m.IsActive, true)
            );

            var update = Builders<UserGroupMembership>.Update
                .Set(m => m.IsActive, false)
                .Set(m => m.RemovedDate, DateTime.UtcNow);

            var result = await _collection.UpdateManyAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> RemoveAllMembersFromGroupAsync(string groupId)
        {
            var filter = Builders<UserGroupMembership>.Filter.And(
                Builders<UserGroupMembership>.Filter.Eq(m => m.GroupId, groupId),
                Builders<UserGroupMembership>.Filter.Eq(m => m.IsActive, true)
            );

            var update = Builders<UserGroupMembership>.Update
                .Set(m => m.IsActive, false)
                .Set(m => m.RemovedDate, DateTime.UtcNow);

            var result = await _collection.UpdateManyAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<IEnumerable<UserGroupMembership>> GetActiveMembershipsAsync()
        {
            return await _collection.Find(m => m.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<UserGroupMembership>> GetMembershipHistoryAsync(string userId)
        {
            return await _collection.Find(m => m.UserId == userId).ToListAsync();
        }

        public async Task<bool> DeactivateMembershipAsync(string userId, string groupId, string removedBy)
        {
            var filter = Builders<UserGroupMembership>.Filter.And(
                Builders<UserGroupMembership>.Filter.Eq(m => m.UserId, userId),
                Builders<UserGroupMembership>.Filter.Eq(m => m.GroupId, groupId),
                Builders<UserGroupMembership>.Filter.Eq(m => m.IsActive, true)
            );

            var update = Builders<UserGroupMembership>.Update
                .Set(m => m.IsActive, false)
                .Set(m => m.RemovedDate, DateTime.UtcNow)
                .Set(m => m.RemovedBy, removedBy);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> ReactivateMembershipAsync(string userId, string groupId, string addedBy)
        {
            var filter = Builders<UserGroupMembership>.Filter.And(
                Builders<UserGroupMembership>.Filter.Eq(m => m.UserId, userId),
                Builders<UserGroupMembership>.Filter.Eq(m => m.GroupId, groupId),
                Builders<UserGroupMembership>.Filter.Eq(m => m.IsActive, false)
            );

            var update = Builders<UserGroupMembership>.Update
                .Set(m => m.IsActive, true)
                .Set(m => m.RemovedDate, (DateTime?)null)
                .Set(m => m.RemovedBy, (string?)null)
                .Set(m => m.AddedBy, addedBy);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
    }
}