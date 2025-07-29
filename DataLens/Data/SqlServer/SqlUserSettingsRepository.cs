using DataLens.Data.Interfaces;
using DataLens.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DataLens.Data.SqlServer
{
    public class SqlUserSettingsRepository : IUserSettingsRepository
    {
        private readonly string _connectionString;

        public SqlUserSettingsRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<UserSettings>> GetAllAsync()
        {
            var settings = new List<UserSettings>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM UserSettings WHERE IsActive = 1", connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                settings.Add(MapFromReader(reader));
            }
            
            return settings;
        }

        public async Task<UserSettings?> GetByIdAsync(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM UserSettings WHERE Id = @Id AND IsActive = 1", connection);
            command.Parameters.AddWithValue("@Id", id);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapFromReader(reader);
            }
            
            return null;
        }

        public async Task<string> AddAsync(UserSettings settings)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(@"
                INSERT INTO UserSettings (Id, UserId, Theme, Language, TimeZone, DateFormat, EmailNotifications, PushNotifications, CreatedDate, UpdatedDate, IsActive)
                VALUES (@Id, @UserId, @Theme, @Language, @TimeZone, @DateFormat, @EmailNotifications, @PushNotifications, @CreatedDate, @UpdatedDate, @IsActive)", connection);
            
            command.Parameters.AddWithValue("@Id", settings.Id);
            command.Parameters.AddWithValue("@UserId", settings.UserId);
            command.Parameters.AddWithValue("@Theme", settings.Theme);
            command.Parameters.AddWithValue("@Language", settings.Language);
            command.Parameters.AddWithValue("@TimeZone", settings.TimeZone);
            command.Parameters.AddWithValue("@DateFormat", settings.DateFormat);
            command.Parameters.AddWithValue("@EmailNotifications", settings.EmailNotifications);
            command.Parameters.AddWithValue("@PushNotifications", settings.PushNotifications);
            command.Parameters.AddWithValue("@CreatedDate", settings.CreatedDate);
            command.Parameters.AddWithValue("@UpdatedDate", settings.UpdatedDate);
            command.Parameters.AddWithValue("@IsActive", settings.IsActive);
            
            await command.ExecuteNonQueryAsync();
            return settings.Id;
        }

        public async Task<bool> UpdateAsync(UserSettings settings)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(@"
                UPDATE UserSettings SET 
                    UserId = @UserId, Theme = @Theme, Language = @Language, TimeZone = @TimeZone,
                    DateFormat = @DateFormat, EmailNotifications = @EmailNotifications, PushNotifications = @PushNotifications,
                    CreatedDate = @CreatedDate, UpdatedDate = @UpdatedDate, IsActive = @IsActive
                WHERE Id = @Id", connection);
            
            command.Parameters.AddWithValue("@Id", settings.Id);
            command.Parameters.AddWithValue("@UserId", settings.UserId);
            command.Parameters.AddWithValue("@Theme", settings.Theme);
            command.Parameters.AddWithValue("@Language", settings.Language);
            command.Parameters.AddWithValue("@TimeZone", settings.TimeZone);
            command.Parameters.AddWithValue("@DateFormat", settings.DateFormat);
            command.Parameters.AddWithValue("@EmailNotifications", settings.EmailNotifications);
            command.Parameters.AddWithValue("@PushNotifications", settings.PushNotifications);
            command.Parameters.AddWithValue("@CreatedDate", settings.CreatedDate);
            command.Parameters.AddWithValue("@UpdatedDate", settings.UpdatedDate);
            command.Parameters.AddWithValue("@IsActive", settings.IsActive);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("UPDATE UserSettings SET IsActive = 0 WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<UserSettings?> GetByUserIdAsync(string userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM UserSettings WHERE UserId = @UserId AND IsActive = 1", connection);
            command.Parameters.AddWithValue("@UserId", userId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapFromReader(reader);
            }
            
            return null;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT COUNT(*) FROM UserSettings WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            
            var count = (int)await command.ExecuteScalarAsync();
            return count > 0;
        }

        public async Task CreateAsync(UserSettings settings)
        {
            await AddAsync(settings);
        }

        public async Task<UserSettings> GetOrCreateByUserIdAsync(string userId)
        {
            var settings = await GetByUserIdAsync(userId);
            if (settings == null)
            {
                settings = new UserSettings
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Theme = "light",
                    Language = "en",
                    TimeZone = "UTC",
                    DateFormat = "MM/dd/yyyy",
                    EmailNotifications = true,
                    PushNotifications = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    IsActive = true
                };
                await CreateAsync(settings);
            }
            return settings;
        }

        public async Task<bool> UpdateNotificationSettingsAsync(string userId, Dictionary<string, bool> settings)
        {
            var userSettings = await GetByUserIdAsync(userId);
            if (userSettings == null) return false;

            if (settings.ContainsKey("EmailNotifications"))
                userSettings.EmailNotifications = settings["EmailNotifications"];
            if (settings.ContainsKey("PushNotifications"))
                userSettings.PushNotifications = settings["PushNotifications"];

            userSettings.UpdatedDate = DateTime.UtcNow;
            return await UpdateAsync(userSettings);
        }

        public async Task<bool> UpdatePrivacySettingsAsync(string userId, Dictionary<string, object> settings)
        {
            var userSettings = await GetByUserIdAsync(userId);
            if (userSettings == null) return false;

            userSettings.UpdatedDate = DateTime.UtcNow;
            return await UpdateAsync(userSettings);
        }

        public async Task<bool> UpdateGeneralSettingsAsync(string userId, Dictionary<string, string> settings)
        {
            var userSettings = await GetByUserIdAsync(userId);
            if (userSettings == null) return false;

            if (settings.ContainsKey("Theme"))
                userSettings.Theme = settings["Theme"];
            if (settings.ContainsKey("Language"))
                userSettings.Language = settings["Language"];
            if (settings.ContainsKey("TimeZone"))
                userSettings.TimeZone = settings["TimeZone"];
            if (settings.ContainsKey("DateFormat"))
                userSettings.DateFormat = settings["DateFormat"];

            userSettings.UpdatedDate = DateTime.UtcNow;
            return await UpdateAsync(userSettings);
        }

        public async Task<UserSettings> CreateDefaultSettingsAsync(string userId)
        {
            var settings = new UserSettings
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Theme = "light",
                Language = "en",
                TimeZone = "UTC",
                DateFormat = "MM/dd/yyyy",
                EmailNotifications = true,
                PushNotifications = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                IsActive = true
            };
            await CreateAsync(settings);
            return settings;
        }

        public async Task<bool> ResetToDefaultAsync(string userId)
        {
            var userSettings = await GetByUserIdAsync(userId);
            if (userSettings == null) return false;

            userSettings.Theme = "light";
            userSettings.Language = "en";
            userSettings.TimeZone = "UTC";
            userSettings.DateFormat = "MM/dd/yyyy";
            userSettings.EmailNotifications = true;
            userSettings.PushNotifications = true;
            userSettings.UpdatedDate = DateTime.UtcNow;

            return await UpdateAsync(userSettings);
        }

        public async Task<IEnumerable<UserSettings>> GetByLanguageAsync(string language)
        {
            var settings = new List<UserSettings>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM UserSettings WHERE Language = @Language AND IsActive = 1", connection);
            command.Parameters.AddWithValue("@Language", language);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                settings.Add(MapFromReader(reader));
            }
            
            return settings;
        }

        public async Task<IEnumerable<UserSettings>> GetByThemeAsync(string theme)
        {
            var settings = new List<UserSettings>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM UserSettings WHERE Theme = @Theme AND IsActive = 1", connection);
            command.Parameters.AddWithValue("@Theme", theme);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                settings.Add(MapFromReader(reader));
            }
            
            return settings;
        }

        public async Task<bool> BulkUpdateSettingAsync(string settingName, object value)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand($"UPDATE UserSettings SET {settingName} = @Value, UpdatedDate = GETDATE() WHERE IsActive = 1", connection);
            command.Parameters.AddWithValue("@Value", value);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<UserSettings>> GetByConditionAsync(string condition, object? parameter)
        {
            var settings = new List<UserSettings>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand($"SELECT * FROM UserSettings WHERE {condition} AND IsActive = 1", connection);
            if (parameter != null)
            {
                command.Parameters.AddWithValue("@Parameter", parameter);
            }
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                settings.Add(MapFromReader(reader));
            }
            
            return settings;
        }

        public async Task<int> CountAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT COUNT(*) FROM UserSettings WHERE IsActive = 1", connection);
            var count = (int)await command.ExecuteScalarAsync();
            return count;
        }

        private UserSettings MapFromReader(SqlDataReader reader)
        {
            return new UserSettings
            {
                Id = reader.GetString("Id"),
                UserId = reader.GetString("UserId"),
                Theme = reader.GetString("Theme"),
                Language = reader.GetString("Language"),
                TimeZone = reader.GetString("TimeZone"),
                DateFormat = reader.GetString("DateFormat"),
                EmailNotifications = reader.GetBoolean("EmailNotifications"),
                PushNotifications = reader.GetBoolean("PushNotifications"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                UpdatedDate = reader.GetDateTime("UpdatedDate"),
                IsActive = reader.GetBoolean("IsActive")
            };
        }
    }
}