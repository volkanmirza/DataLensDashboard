using DataLens.Data.Interfaces;
using DataLens.Models;
using MongoDB.Driver;

namespace DataLens.Data.MongoDB
{
    public class MongoUserGroupMemberRepository : IUserGroupMemberRepository
    {
        private readonly IMongoCollection<UserGroupMember> _collection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<UserGroup> _groupCollection;

        public MongoUserGroupMemberRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<UserGroupMember>("UserGroupMembers");
            _userCollection = database.GetCollection<User>("Users");
            _groupCollection = database.GetCollection<UserGroup>("UserGroups");
        }

        public async Task<UserGroupMember?> GetByIdAsync(string id)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<UserGroupMember>> GetAllAsync()
        {
            return await _collection.Find(_ => true).SortBy(x => x.JoinedDate).ToListAsync();
        }

        public async Task<IEnumerable<UserGroupMember>> GetByConditionAsync(string condition, object? parameters = null)
        {
            // For MongoDB, we'll implement basic filtering
            // This is a simplified implementation - in practice, you'd want more sophisticated filtering
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<string> AddAsync(UserGroupMember entity)
        {
            await _collection.InsertOneAsync(entity);
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(UserGroupMember entity)
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

        public async Task<IEnumerable<UserGroupMember>> GetByUserIdAsync(string userId)
        {
            return await _collection.Find(x => x.UserId == userId)
                .SortBy(x => x.JoinedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserGroupMember>> GetByGroupIdAsync(string groupId)
        {
            return await _collection.Find(x => x.GroupId == groupId)
                .SortBy(x => x.JoinedDate)
                .ToListAsync();
        }

        public async Task<UserGroupMember?> GetByUserAndGroupAsync(string userId, string groupId)
        {
            return await _collection.Find(x => x.UserId == userId && x.GroupId == groupId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<User>> GetGroupMembersAsync(string groupId)
        {
            var memberUserIds = await _collection
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
            var memberGroupIds = await _collection
                .Find(x => x.UserId == userId)
                .Project(x => x.GroupId)
                .ToListAsync();

            return await _groupCollection
                .Find(x => memberGroupIds.Contains(x.Id) && x.IsActive)
                .SortBy(x => x.GroupName)
                .ToListAsync();
        }

        public async Task<bool> IsUserInGroupAsync(string userId, string groupId)
        {
            var count = await _collection.CountDocumentsAsync(x => x.UserId == userId && x.GroupId == groupId);
            return count > 0;
        }

        public async Task<int> GetGroupMemberCountAsync(string groupId)
        {
            return (int)await _collection.CountDocumentsAsync(x => x.GroupId == groupId);
        }

        public async Task<bool> RemoveUserFromAllGroupsAsync(string userId)
        {
            var result = await _collection.DeleteManyAsync(x => x.UserId == userId);
            return result.DeletedCount > 0;
        }

        public async Task<bool> RemoveAllMembersFromGroupAsync(string groupId)
        {
            var result = await _collection.DeleteManyAsync(x => x.GroupId == groupId);
            return result.DeletedCount > 0;
        }
    }
}