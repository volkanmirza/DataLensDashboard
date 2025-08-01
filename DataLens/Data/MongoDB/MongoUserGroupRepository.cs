using DataLens.Data.Interfaces;
using DataLens.Models;
using MongoDB.Driver;

namespace DataLens.Data.MongoDB
{
    public class MongoUserGroupRepository : IUserGroupRepository
    {
        private readonly IMongoCollection<UserGroup> _collection;
        private readonly IMongoCollection<UserGroupMember> _memberCollection;
        private readonly IMongoCollection<User> _userCollection;

        public MongoUserGroupRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<UserGroup>("UserGroups");
            _memberCollection = database.GetCollection<UserGroupMember>("UserGroupMembers");
            _userCollection = database.GetCollection<User>("Users");
        }

        public async Task<UserGroup?> GetByIdAsync(string id)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<UserGroup>> GetAllAsync()
        {
            return await _collection.Find(_ => true).SortBy(x => x.GroupName).ToListAsync();
        }

        public async Task<IEnumerable<UserGroup>> GetByConditionAsync(string condition, object? parameters = null)
        {
            // For MongoDB, we'll implement basic filtering
            // This is a simplified implementation - in practice, you'd want more sophisticated filtering
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<string> AddAsync(UserGroup entity)
        {
            await _collection.InsertOneAsync(entity);
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(UserGroup entity)
        {
            var result = await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _collection.DeleteOneAsync(x => x.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            var count = await _collection.CountDocumentsAsync(x => x.Id == id);
            return count > 0;
        }

        public async Task<int> CountAsync()
        {
            return (int)await _collection.CountDocumentsAsync(_ => true);
        }

        public async Task<IEnumerable<UserGroup>> GetByCreatedByAsync(string createdBy)
        {
            return await _collection.Find(x => x.CreatedBy == createdBy)
                .SortBy(x => x.GroupName)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserGroup>> GetActiveGroupsAsync()
        {
            return await _collection.Find(x => x.IsActive)
                .SortBy(x => x.GroupName)
                .ToListAsync();
        }

        public async Task<bool> IsGroupNameExistsAsync(string groupName)
        {
            var count = await _collection.CountDocumentsAsync(x => x.GroupName == groupName);
            return count > 0;
        }

        public async Task<IEnumerable<User>> GetGroupMembersAsync(string groupId)
        {
            var memberUserIds = await _memberCollection
                .Find(x => x.GroupId == groupId)
                .Project(x => x.UserId)
                .ToListAsync();

            return await _userCollection
                .Find(x => memberUserIds.Contains(x.Id))
                .SortBy(x => x.UserName)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserGroup>> GetUserGroupsAsync(string userId)
        {
            var memberGroupIds = await _memberCollection
                .Find(x => x.UserId == userId)
                .Project(x => x.GroupId)
                .ToListAsync();

            return await _collection
                .Find(x => memberGroupIds.Contains(x.Id) && x.IsActive)
                .SortBy(x => x.GroupName)
                .ToListAsync();
        }

        public async Task<bool> AddUserToGroupAsync(string userId, string groupId, string addedBy)
        {
            var member = new UserGroupMember
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                GroupId = groupId,
                JoinedDate = DateTime.UtcNow,
                AddedBy = addedBy
            };

            await _memberCollection.InsertOneAsync(member);
            return true;
        }

        public async Task<bool> RemoveUserFromGroupAsync(string userId, string groupId)
        {
            var result = await _memberCollection.DeleteOneAsync(x => x.UserId == userId && x.GroupId == groupId);
            return result.DeletedCount > 0;
        }

        public async Task<bool> IsUserInGroupAsync(string userId, string groupId)
        {
            var count = await _memberCollection.CountDocumentsAsync(x => x.UserId == userId && x.GroupId == groupId);
            return count > 0;
        }
    }
}