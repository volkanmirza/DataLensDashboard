using DataLens.Data.Interfaces;
using DataLens.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DataLens.Data.SqlServer
{
    public class SqlNotificationRepository : INotificationRepository
    {
        private readonly string _connectionString;

        public SqlNotificationRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Notification>> GetAllAsync()
        {
            var notifications = new List<Notification>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM Notifications WHERE IsActive = 1", connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                notifications.Add(MapFromReader(reader));
            }
            
            return notifications;
        }

        public async Task<Notification?> GetByIdAsync(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM Notifications WHERE Id = @Id AND IsActive = 1", connection);
            command.Parameters.AddWithValue("@Id", id);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapFromReader(reader);
            }
            
            return null;
        }

        public async Task<string> AddAsync(Notification notification)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(@"
                INSERT INTO Notifications (Id, UserId, Title, Message, Type, IsRead, CreatedAt, ReadAt, RelatedEntityId, RelatedEntityType, CreatedBy, IsActive)
                VALUES (@Id, @UserId, @Title, @Message, @Type, @IsRead, @CreatedAt, @ReadAt, @RelatedEntityId, @RelatedEntityType, @CreatedBy, @IsActive)", connection);
            
            command.Parameters.AddWithValue("@Id", notification.Id);
            command.Parameters.AddWithValue("@UserId", notification.UserId);
            command.Parameters.AddWithValue("@Title", notification.Title);
            command.Parameters.AddWithValue("@Message", notification.Message);
            command.Parameters.AddWithValue("@Type", notification.Type);
            command.Parameters.AddWithValue("@IsRead", notification.IsRead);
            command.Parameters.AddWithValue("@CreatedAt", notification.CreatedAt);
            command.Parameters.AddWithValue("@ReadAt", notification.ReadAt ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@RelatedEntityId", notification.RelatedEntityId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@RelatedEntityType", notification.RelatedEntityType ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CreatedBy", notification.CreatedBy);
            command.Parameters.AddWithValue("@IsActive", notification.IsActive);
            
            await command.ExecuteNonQueryAsync();
            return notification.Id;
        }

        public async Task<bool> UpdateAsync(Notification notification)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(@"
                UPDATE Notifications SET 
                    UserId = @UserId, Title = @Title, Message = @Message, Type = @Type,
                    IsRead = @IsRead, CreatedAt = @CreatedAt, ReadAt = @ReadAt, RelatedEntityId = @RelatedEntityId, RelatedEntityType = @RelatedEntityType, CreatedBy = @CreatedBy, IsActive = @IsActive
                WHERE Id = @Id", connection);
            
            command.Parameters.AddWithValue("@Id", notification.Id);
            command.Parameters.AddWithValue("@UserId", notification.UserId);
            command.Parameters.AddWithValue("@Title", notification.Title);
            command.Parameters.AddWithValue("@Message", notification.Message);
            command.Parameters.AddWithValue("@Type", notification.Type);
            command.Parameters.AddWithValue("@IsRead", notification.IsRead);
            command.Parameters.AddWithValue("@CreatedAt", notification.CreatedAt);
            command.Parameters.AddWithValue("@ReadAt", notification.ReadAt ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@RelatedEntityId", notification.RelatedEntityId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@RelatedEntityType", notification.RelatedEntityType ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CreatedBy", notification.CreatedBy);
            command.Parameters.AddWithValue("@IsActive", notification.IsActive);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("UPDATE Notifications SET IsActive = 0 WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(string userId)
        {
            var notifications = new List<Notification>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM Notifications WHERE UserId = @UserId AND IsActive = 1 ORDER BY CreatedAt DESC", connection);
            command.Parameters.AddWithValue("@UserId", userId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                notifications.Add(MapFromReader(reader));
            }
            
            return notifications;
        }

        public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(string userId)
        {
            var notifications = new List<Notification>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM Notifications WHERE UserId = @UserId AND IsRead = 0 AND IsActive = 1 ORDER BY CreatedAt DESC", connection);
            command.Parameters.AddWithValue("@UserId", userId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                notifications.Add(MapFromReader(reader));
            }
            
            return notifications;
        }

        public async Task<bool> MarkAsReadAsync(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("UPDATE Notifications SET IsRead = 1, ReadAt = GETDATE() WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("UPDATE Notifications SET IsRead = 1, ReadAt = GETDATE() WHERE UserId = @UserId AND IsRead = 0", connection);
            command.Parameters.AddWithValue("@UserId", userId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT COUNT(*) FROM Notifications WHERE UserId = @UserId AND IsRead = 0 AND IsActive = 1", connection);
            command.Parameters.AddWithValue("@UserId", userId);
            
            var count = (int)await command.ExecuteScalarAsync();
            return count;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT COUNT(*) FROM Notifications WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            
            var count = (int)await command.ExecuteScalarAsync();
            return count > 0;
        }

        public async Task CreateAsync(Notification notification)
        {
            await AddAsync(notification);
        }

        public async Task<IEnumerable<Notification>> GetByTypeAsync(string userId, string type)
        {
            var notifications = new List<Notification>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM Notifications WHERE UserId = @UserId AND Type = @Type AND IsActive = 1 ORDER BY CreatedAt DESC", connection);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@Type", type);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                notifications.Add(MapFromReader(reader));
            }
            
            return notifications;
        }

        public async Task<bool> MarkAsReadAsync(string notificationId, string userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("UPDATE Notifications SET IsRead = 1, ReadAt = GETDATE() WHERE Id = @Id AND UserId = @UserId", connection);
            command.Parameters.AddWithValue("@Id", notificationId);
            command.Parameters.AddWithValue("@UserId", userId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteByUserIdAsync(string notificationId, string userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("UPDATE Notifications SET IsActive = 0 WHERE Id = @Id AND UserId = @UserId", connection);
            command.Parameters.AddWithValue("@Id", notificationId);
            command.Parameters.AddWithValue("@UserId", userId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Notification>> GetActiveNotificationsAsync(string userId)
        {
            var notifications = new List<Notification>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM Notifications WHERE UserId = @UserId AND IsActive = 1 ORDER BY CreatedAt DESC", connection);
            command.Parameters.AddWithValue("@UserId", userId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                notifications.Add(MapFromReader(reader));
            }
            
            return notifications;
        }

        public async Task<IEnumerable<Notification>> GetByRelatedEntityAsync(string relatedEntityId, string relatedEntityType)
        {
            var notifications = new List<Notification>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM Notifications WHERE RelatedEntityId = @RelatedEntityId AND RelatedEntityType = @RelatedEntityType AND IsActive = 1 ORDER BY CreatedAt DESC", connection);
            command.Parameters.AddWithValue("@RelatedEntityId", relatedEntityId);
            command.Parameters.AddWithValue("@RelatedEntityType", relatedEntityType);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                notifications.Add(MapFromReader(reader));
            }
            
            return notifications;
        }

        public async Task<bool> CreateBulkAsync(IEnumerable<Notification> notifications)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                foreach (var notification in notifications)
                {
                    var command = new SqlCommand(@"
                        INSERT INTO Notifications (Id, UserId, Title, Message, Type, IsRead, CreatedAt, ReadAt, RelatedEntityId, RelatedEntityType, CreatedBy, IsActive)
                        VALUES (@Id, @UserId, @Title, @Message, @Type, @IsRead, @CreatedAt, @ReadAt, @RelatedEntityId, @RelatedEntityType, @CreatedBy, @IsActive)", connection, (SqlTransaction)transaction);
                    
                    command.Parameters.AddWithValue("@Id", notification.Id);
                    command.Parameters.AddWithValue("@UserId", notification.UserId);
                    command.Parameters.AddWithValue("@Title", notification.Title);
                    command.Parameters.AddWithValue("@Message", notification.Message);
                    command.Parameters.AddWithValue("@Type", notification.Type);
                    command.Parameters.AddWithValue("@IsRead", notification.IsRead);
                    command.Parameters.AddWithValue("@CreatedAt", notification.CreatedAt);
                    command.Parameters.AddWithValue("@ReadAt", notification.ReadAt ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@RelatedEntityId", notification.RelatedEntityId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@RelatedEntityType", notification.RelatedEntityType ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CreatedBy", notification.CreatedBy);
                    command.Parameters.AddWithValue("@IsActive", notification.IsActive);
                    
                    await command.ExecuteNonQueryAsync();
                }
                
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> DeleteOldNotificationsAsync(DateTime cutoffDate)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("UPDATE Notifications SET IsActive = 0 WHERE CreatedAt < @CutoffDate", connection);
            command.Parameters.AddWithValue("@CutoffDate", cutoffDate);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Notification>> GetByConditionAsync(string condition, object? parameter)
        {
            var notifications = new List<Notification>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand($"SELECT * FROM Notifications WHERE {condition} AND IsActive = 1", connection);
            if (parameter != null)
            {
                command.Parameters.AddWithValue("@Parameter", parameter);
            }
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                notifications.Add(MapFromReader(reader));
            }
            
            return notifications;
        }

        public async Task<int> CountAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT COUNT(*) FROM Notifications WHERE IsActive = 1", connection);
            var count = (int)await command.ExecuteScalarAsync();
            return count;
        }

        private Notification MapFromReader(SqlDataReader reader)
        {
            return new Notification
            {
                Id = reader.GetString("Id"),
                UserId = reader.GetString("UserId"),
                Title = reader.GetString("Title"),
                Message = reader.GetString("Message"),
                Type = reader.GetString("Type"),
                IsRead = reader.GetBoolean("IsRead"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                ReadAt = reader.IsDBNull("ReadAt") ? null : reader.GetDateTime("ReadAt"),
                RelatedEntityId = reader.IsDBNull("RelatedEntityId") ? null : reader.GetString("RelatedEntityId"),
                RelatedEntityType = reader.IsDBNull("RelatedEntityType") ? null : reader.GetString("RelatedEntityType"),
                CreatedBy = reader.GetString("CreatedBy"),
                IsActive = reader.GetBoolean("IsActive")
            };
        }
    }
}