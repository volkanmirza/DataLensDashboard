using MongoDB.Driver;
using DataLens.Data.Interfaces;
using DataLens.Models;
using MongoDB.Bson;

namespace DataLens.Data.MongoDB
{
    public class MongoUserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;

        public MongoUserRepository(IDbConnectionFactory connectionFactory)
        {
            var database = connectionFactory.CreateMongoDatabase();
            _users = database.GetCollection<User>("Users");
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _users.Find(_ => true)
                .SortByDescending(u => u.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetByConditionAsync(string condition, object? parameters = null)
        {
            // For MongoDB, we'll need to build filter expressions
            // This is a simplified implementation
            var filter = Builders<User>.Filter.Empty;
            return await _users.Find(filter).ToListAsync();
        }

        public async Task<string> AddAsync(User entity)
        {
            if (string.IsNullOrEmpty(entity.Id))
                entity.Id = ObjectId.GenerateNewId().ToString();
            
            await _users.InsertOneAsync(entity);
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(User entity)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, entity.Id);
            var result = await _users.ReplaceOneAsync(filter, entity);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var result = await _users.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var count = await _users.CountDocumentsAsync(filter);
            return count > 0;
        }

        public async Task<int> CountAsync()
        {
            return (int)await _users.CountDocumentsAsync(_ => true);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            var filter = Builders<User>.Filter.Eq(u => u.UserName, username);
            return await _users.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            return await _users.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(string role)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Role, role);
            return await _users.Find(filter)
                .SortBy(u => u.UserName)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            var filter = Builders<User>.Filter.Eq(u => u.IsActive, true);
            return await _users.Find(filter)
                .SortBy(u => u.UserName)
                .ToListAsync();
        }

        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            var filter = Builders<User>.Filter.Eq(u => u.UserName, username);
            var count = await _users.CountDocumentsAsync(filter);
            return count > 0;
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            var count = await _users.CountDocumentsAsync(filter);
            return count > 0;
        }

        public async Task<bool> UpdatePasswordAsync(string userId, string passwordHash)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update.Set(u => u.PasswordHash, passwordHash);
            var result = await _users.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateLastLoginAsync(string userId)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update.Set("LastLoginDate", DateTime.UtcNow);
            var result = await _users.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
    }
}