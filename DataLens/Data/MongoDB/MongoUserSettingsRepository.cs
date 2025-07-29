using MongoDB.Driver;
using DataLens.Data.Interfaces;
using DataLens.Models;
using MongoDB.Bson;

namespace DataLens.Data.MongoDB
{
    public class MongoUserSettingsRepository : IUserSettingsRepository
    {
        private readonly IMongoCollection<UserSettings> _userSettings;

        public MongoUserSettingsRepository(IMongoDatabase database)
        {
            _userSettings = database.GetCollection<UserSettings>("UserSettings");
        }

        public async Task<UserSettings?> GetByIdAsync(string id)
        {
            return await _userSettings.Find(s => s.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<UserSettings>> GetAllAsync()
        {
            return await _userSettings.Find(_ => true)
                .SortByDescending(s => s.UpdatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserSettings>> GetByConditionAsync(string condition, object? parameters = null)
        {
            // Simple implementation - can be enhanced based on needs
            return await GetAllAsync();
        }

        public async Task<string> AddAsync(UserSettings entity)
        {
            if (string.IsNullOrEmpty(entity.Id))
            {
                entity.Id = ObjectId.GenerateNewId().ToString();
            }
            entity.CreatedDate = DateTime.UtcNow;
            entity.UpdatedDate = DateTime.UtcNow;
            await _userSettings.InsertOneAsync(entity);
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(UserSettings entity)
        {
            entity.UpdatedDate = DateTime.UtcNow;
            var result = await _userSettings.ReplaceOneAsync(s => s.Id == entity.Id, entity);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _userSettings.DeleteOneAsync(s => s.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            var count = await _userSettings.CountDocumentsAsync(s => s.Id == id);
            return count > 0;
        }

        public async Task<int> CountAsync()
        {
            return (int)await _userSettings.CountDocumentsAsync(_ => true);
        }

        public async Task<UserSettings?> GetByUserIdAsync(string userId)
        {
            var filter = Builders<UserSettings>.Filter.Eq(s => s.UserId, userId);
            return await _userSettings.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateSettingAsync(string userId, string settingKey, object settingValue)
        {
            var filter = Builders<UserSettings>.Filter.Eq(s => s.UserId, userId);
            var update = Builders<UserSettings>.Update
                .Set(settingKey, settingValue)
                .Set(s => s.UpdatedDate, DateTime.UtcNow);
            
            var result = await _userSettings.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<T?> GetSettingAsync<T>(string userId, string settingKey)
        {
            var userSettings = await GetByUserIdAsync(userId);
            if (userSettings != null)
            {
                try
                {
                    var property = typeof(UserSettings).GetProperty(settingKey);
                    if (property != null)
                    {
                        var value = property.GetValue(userSettings);
                        return (T)value;
                    }
                }
                catch
                {
                    return default(T);
                }
            }
            return default(T);
        }

        public async Task<bool> DeleteSettingAsync(string userId, string settingKey)
        {
            var filter = Builders<UserSettings>.Filter.Eq(s => s.UserId, userId);
            var update = Builders<UserSettings>.Update
                .Unset(settingKey)
                .Set(s => s.UpdatedDate, DateTime.UtcNow);
            
            var result = await _userSettings.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> ResetUserSettingsAsync(string userId)
        {
            var filter = Builders<UserSettings>.Filter.Eq(s => s.UserId, userId);
            var update = Builders<UserSettings>.Update
                .Set(s => s.Language, "tr")
                .Set(s => s.Theme, "light")
                .Set(s => s.TimeZone, "Europe/Istanbul")
                .Set(s => s.EmailNotifications, true)
                .Set(s => s.BrowserNotifications, false)
                .Set(s => s.DashboardNotifications, true)
                .Set(s => s.ProfileVisibility, "private")
                .Set(s => s.ShowEmail, false)
                .Set(s => s.ShowLastLogin, false)
                .Set(s => s.UpdatedDate, DateTime.UtcNow);
            
            var result = await _userSettings.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateNotificationSettingsAsync(string userId, Dictionary<string, bool> settings)
        {
            var filter = Builders<UserSettings>.Filter.Eq(s => s.UserId, userId);
            var updateBuilder = Builders<UserSettings>.Update.Set(s => s.UpdatedDate, DateTime.UtcNow);
            
            foreach (var setting in settings)
            {
                updateBuilder = updateBuilder.Set(setting.Key, setting.Value);
            }
            
            var result = await _userSettings.UpdateOneAsync(filter, updateBuilder);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdatePrivacySettingsAsync(string userId, Dictionary<string, object> settings)
        {
            var filter = Builders<UserSettings>.Filter.Eq(s => s.UserId, userId);
            var updateBuilder = Builders<UserSettings>.Update.Set(s => s.UpdatedDate, DateTime.UtcNow);
            
            foreach (var setting in settings)
            {
                updateBuilder = updateBuilder.Set(setting.Key, setting.Value);
            }
            
            var result = await _userSettings.UpdateOneAsync(filter, updateBuilder);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateGeneralSettingsAsync(string userId, Dictionary<string, string> settings)
        {
            var filter = Builders<UserSettings>.Filter.Eq(s => s.UserId, userId);
            var updateBuilder = Builders<UserSettings>.Update.Set(s => s.UpdatedDate, DateTime.UtcNow);
            
            foreach (var setting in settings)
            {
                updateBuilder = updateBuilder.Set(setting.Key, setting.Value);
            }
            
            var result = await _userSettings.UpdateOneAsync(filter, updateBuilder);
            return result.ModifiedCount > 0;
        }

        public async Task<UserSettings> CreateDefaultSettingsAsync(string userId)
        {
            var defaultSettings = new UserSettings
            {
                Id = ObjectId.GenerateNewId().ToString(),
                UserId = userId,
                Language = "en",
                Theme = "light",
                TimeZone = "UTC",
                EmailNotifications = true,
                BrowserNotifications = true,
                DashboardNotifications = true,
                ProfileVisibility = "public",
                ShowEmail = false,
                ShowLastLogin = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _userSettings.InsertOneAsync(defaultSettings);
            return defaultSettings;
        }

        public async Task<bool> ResetToDefaultAsync(string userId)
        {
            // Delete existing settings
            await _userSettings.DeleteOneAsync(s => s.UserId == userId);
            
            // Create new default settings
            await CreateDefaultSettingsAsync(userId);
            return true;
        }

        public async Task<IEnumerable<UserSettings>> GetByLanguageAsync(string language)
        {
            var filter = Builders<UserSettings>.Filter.Eq(s => s.Language, language);
            return await _userSettings.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<UserSettings>> GetByThemeAsync(string theme)
        {
            var filter = Builders<UserSettings>.Filter.Eq(s => s.Theme, theme);
            return await _userSettings.Find(filter).ToListAsync();
        }

        public async Task<bool> BulkUpdateSettingAsync(string settingName, object value)
        {
            var filter = Builders<UserSettings>.Filter.Empty;
            var update = Builders<UserSettings>.Update
                .Set(settingName, value)
                .Set(s => s.UpdatedDate, DateTime.UtcNow);
            
            var result = await _userSettings.UpdateManyAsync(filter, update);
            return result.ModifiedCount > 0;
        }
    }
}