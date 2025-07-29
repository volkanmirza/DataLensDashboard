using MongoDB.Driver;
using DataLens.Data.Interfaces;
using DataLens.Models;
using MongoDB.Bson;

namespace DataLens.Data.MongoDB
{
    public class MongoNotificationRepository : INotificationRepository
    {
        private readonly IMongoCollection<Notification> _notifications;

        public MongoNotificationRepository(IMongoDatabase database)
        {
            _notifications = database.GetCollection<Notification>("Notifications");
        }

        public async Task<Notification?> GetByIdAsync(string id)
        {
            return await _notifications.Find(n => n.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Notification>> GetAllAsync()
        {
            return await _notifications.Find(_ => true)
                .SortByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetByConditionAsync(string condition, object? parameters = null)
        {
            // Simple implementation - can be enhanced based on needs
            return await GetAllAsync();
        }

        public async Task<string> AddAsync(Notification entity)
        {
            if (string.IsNullOrEmpty(entity.Id))
            {
                entity.Id = ObjectId.GenerateNewId().ToString();
            }
            entity.CreatedAt = DateTime.UtcNow;
            await _notifications.InsertOneAsync(entity);
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(Notification entity)
        {
            var result = await _notifications.ReplaceOneAsync(n => n.Id == entity.Id, entity);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _notifications.DeleteOneAsync(n => n.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            var count = await _notifications.CountDocumentsAsync(n => n.Id == id);
            return count > 0;
        }

        public async Task<int> CountAsync()
        {
            return (int)await _notifications.CountDocumentsAsync(_ => true);
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(string userId)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.UserId, userId);
            return await _notifications.Find(filter)
                .SortByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(string userId)
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Eq(n => n.UserId, userId),
                Builders<Notification>.Filter.Eq(n => n.IsRead, false)
            );
            return await _notifications.Find(filter)
                .SortByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> MarkAsReadAsync(string notificationId)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.Id, notificationId);
            var update = Builders<Notification>.Update
                .Set(n => n.IsRead, true)
                .Set(n => n.ReadAt, DateTime.UtcNow);
            
            var result = await _notifications.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> MarkAsReadAsync(string notificationId, string userId)
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Eq(n => n.Id, notificationId),
                Builders<Notification>.Filter.Eq(n => n.UserId, userId)
            );
            var update = Builders<Notification>.Update
                .Set(n => n.IsRead, true)
                .Set(n => n.ReadAt, DateTime.UtcNow);
            
            var result = await _notifications.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Eq(n => n.UserId, userId),
                Builders<Notification>.Filter.Eq(n => n.IsRead, false)
            );
            var update = Builders<Notification>.Update
                .Set(n => n.IsRead, true)
                .Set(n => n.ReadAt, DateTime.UtcNow);
            
            var result = await _notifications.UpdateManyAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Eq(n => n.UserId, userId),
                Builders<Notification>.Filter.Eq(n => n.IsRead, false)
            );
            return (int)await _notifications.CountDocumentsAsync(filter);
        }

        public async Task<bool> DeleteOldNotificationsAsync(DateTime cutoffDate)
        {
            var filter = Builders<Notification>.Filter.Lt(n => n.CreatedAt, cutoffDate);
            var result = await _notifications.DeleteManyAsync(filter);
            return result.DeletedCount > 0;
        }

        public async Task<IEnumerable<Notification>> GetByTypeAsync(string userId, string type)
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Eq(n => n.UserId, userId),
                Builders<Notification>.Filter.Eq(n => n.Type, type)
            );
            return await _notifications.Find(filter).ToListAsync();
        }

        public async Task<bool> DeleteByUserIdAsync(string notificationId, string userId)
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Eq(n => n.Id, notificationId),
                Builders<Notification>.Filter.Eq(n => n.UserId, userId)
            );
            var result = await _notifications.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        public async Task<IEnumerable<Notification>> GetActiveNotificationsAsync(string userId)
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Eq(n => n.UserId, userId),
                Builders<Notification>.Filter.Eq(n => n.IsActive, true)
            );
            return await _notifications.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetByRelatedEntityAsync(string relatedEntityId, string relatedEntityType)
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Eq(n => n.RelatedEntityId, relatedEntityId),
                Builders<Notification>.Filter.Eq(n => n.RelatedEntityType, relatedEntityType)
            );
            return await _notifications.Find(filter).ToListAsync();
        }

        public async Task<bool> CreateBulkAsync(IEnumerable<Notification> notifications)
        {
            try
            {
                await _notifications.InsertManyAsync(notifications);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}