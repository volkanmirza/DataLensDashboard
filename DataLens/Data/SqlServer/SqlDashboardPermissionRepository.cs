using DataLens.Data.Interfaces;
using DataLens.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DataLens.Data.SqlServer
{
    public class SqlDashboardPermissionRepository : IDashboardPermissionRepository
    {
        private readonly string _connectionString;

        public SqlDashboardPermissionRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<DashboardPermission>> GetAllAsync()
        {
            var permissions = new List<DashboardPermission>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM DashboardPermissions WHERE IsActive = 1", connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                permissions.Add(MapFromReader(reader));
            }
            
            return permissions;
        }

        public async Task<DashboardPermission?> GetByIdAsync(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM DashboardPermissions WHERE Id = @Id AND IsActive = 1", connection);
            command.Parameters.AddWithValue("@Id", id);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapFromReader(reader);
            }
            
            return null;
        }

        public async Task<string> AddAsync(DashboardPermission permission)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(@"
                INSERT INTO DashboardPermissions (Id, DashboardId, UserId, GroupId, PermissionType, GrantedBy, GrantedDate, ExpiryDate, IsActive)
                VALUES (@Id, @DashboardId, @UserId, @GroupId, @PermissionType, @GrantedBy, @GrantedDate, @ExpiryDate, @IsActive)", connection);
            
            command.Parameters.AddWithValue("@Id", permission.Id);
            command.Parameters.AddWithValue("@DashboardId", permission.DashboardId);
            command.Parameters.AddWithValue("@UserId", permission.UserId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@GroupId", permission.GroupId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PermissionType", permission.PermissionType);
            command.Parameters.AddWithValue("@GrantedBy", permission.GrantedBy);
            command.Parameters.AddWithValue("@GrantedDate", permission.GrantedDate);
            command.Parameters.AddWithValue("@ExpiryDate", permission.ExpiryDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsActive", permission.IsActive);
            
            await command.ExecuteNonQueryAsync();
            return permission.Id;
        }

        public async Task<bool> UpdateAsync(DashboardPermission permission)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(@"
                UPDATE DashboardPermissions SET 
                    DashboardId = @DashboardId, UserId = @UserId, GroupId = @GroupId, PermissionType = @PermissionType,
                    GrantedBy = @GrantedBy, GrantedDate = @GrantedDate, ExpiryDate = @ExpiryDate, IsActive = @IsActive
                WHERE Id = @Id", connection);
            
            command.Parameters.AddWithValue("@Id", permission.Id);
            command.Parameters.AddWithValue("@DashboardId", permission.DashboardId);
            command.Parameters.AddWithValue("@UserId", permission.UserId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@GroupId", permission.GroupId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PermissionType", permission.PermissionType);
            command.Parameters.AddWithValue("@GrantedBy", permission.GrantedBy);
            command.Parameters.AddWithValue("@GrantedDate", permission.GrantedDate);
            command.Parameters.AddWithValue("@ExpiryDate", permission.ExpiryDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsActive", permission.IsActive);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("DELETE FROM DashboardPermissions WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<DashboardPermission>> GetByConditionAsync(string condition, object? parameter)
        {
            var permissions = new List<DashboardPermission>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand($"SELECT * FROM DashboardPermissions WHERE {condition}", connection);
            if (parameter != null)
            {
                command.Parameters.AddWithValue("@Parameter", parameter);
            }
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                permissions.Add(MapFromReader(reader));
            }
            
            return permissions;
        }

        public async Task<int> CountAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT COUNT(*) FROM DashboardPermissions", connection);
            var count = (int)await command.ExecuteScalarAsync();
            return count;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT COUNT(*) FROM DashboardPermissions WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            
            var count = (int)await command.ExecuteScalarAsync();
            return count > 0;
        }

        public async Task<IEnumerable<DashboardPermission>> GetByDashboardIdAsync(string dashboardId)
        {
            var permissions = new List<DashboardPermission>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM DashboardPermissions WHERE DashboardId = @DashboardId AND IsActive = 1", connection);
            command.Parameters.AddWithValue("@DashboardId", dashboardId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                permissions.Add(MapFromReader(reader));
            }
            
            return permissions;
        }

        public async Task<IEnumerable<DashboardPermission>> GetByUserIdAsync(string userId)
        {
            var permissions = new List<DashboardPermission>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM DashboardPermissions WHERE UserId = @UserId AND IsActive = 1", connection);
            command.Parameters.AddWithValue("@UserId", userId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                permissions.Add(MapFromReader(reader));
            }
            
            return permissions;
        }

        public async Task<IEnumerable<DashboardPermission>> GetByGroupIdAsync(string groupId)
        {
            var permissions = new List<DashboardPermission>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM DashboardPermissions WHERE GroupId = @GroupId AND IsActive = 1", connection);
            command.Parameters.AddWithValue("@GroupId", groupId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                permissions.Add(MapFromReader(reader));
            }
            
            return permissions;
        }

        public async Task<DashboardPermission?> GetByUserAndDashboardAsync(string userId, string dashboardId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM DashboardPermissions WHERE UserId = @UserId AND DashboardId = @DashboardId AND IsActive = 1", connection);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@DashboardId", dashboardId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapFromReader(reader);
            }
            
            return null;
        }

        public async Task<DashboardPermission?> GetByGroupAndDashboardAsync(string groupId, string dashboardId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM DashboardPermissions WHERE GroupId = @GroupId AND DashboardId = @DashboardId AND IsActive = 1", connection);
            command.Parameters.AddWithValue("@GroupId", groupId);
            command.Parameters.AddWithValue("@DashboardId", dashboardId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapFromReader(reader);
            }
            
            return null;
        }

        public async Task<bool> HasPermissionAsync(string userId, string dashboardId, string permissionType)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(@"
                SELECT COUNT(*) FROM DashboardPermissions 
                WHERE UserId = @UserId AND DashboardId = @DashboardId AND PermissionType = @PermissionType 
                AND IsActive = 1 AND (ExpiryDate IS NULL OR ExpiryDate > GETDATE())", connection);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@DashboardId", dashboardId);
            command.Parameters.AddWithValue("@PermissionType", permissionType);
            
            var count = (int)await command.ExecuteScalarAsync();
            return count > 0;
        }

        public async Task<bool> HasGroupPermissionAsync(string groupId, string dashboardId, string permissionType)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(@"
                SELECT COUNT(*) FROM DashboardPermissions 
                WHERE GroupId = @GroupId AND DashboardId = @DashboardId AND PermissionType = @PermissionType 
                AND IsActive = 1 AND (ExpiryDate IS NULL OR ExpiryDate > GETDATE())", connection);
            command.Parameters.AddWithValue("@GroupId", groupId);
            command.Parameters.AddWithValue("@DashboardId", dashboardId);
            command.Parameters.AddWithValue("@PermissionType", permissionType);
            
            var count = (int)await command.ExecuteScalarAsync();
            return count > 0;
        }

        public async Task CreateAsync(DashboardPermission permission)
        {
            await AddAsync(permission);
        }

        public async Task<IEnumerable<DashboardPermission>> GetUserPermissionsAsync(string userId, string dashboardId)
        {
            var permissions = new List<DashboardPermission>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM DashboardPermissions WHERE UserId = @UserId AND DashboardId = @DashboardId AND IsActive = 1", connection);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@DashboardId", dashboardId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                permissions.Add(MapFromReader(reader));
            }
            
            return permissions;
        }

        public async Task<IEnumerable<DashboardPermission>> GetGroupPermissionsAsync(string groupId, string dashboardId)
        {
            var permissions = new List<DashboardPermission>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM DashboardPermissions WHERE GroupId = @GroupId AND DashboardId = @DashboardId AND IsActive = 1", connection);
            command.Parameters.AddWithValue("@GroupId", groupId);
            command.Parameters.AddWithValue("@DashboardId", dashboardId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                permissions.Add(MapFromReader(reader));
            }
            
            return permissions;
        }

        public async Task<DashboardPermission?> GetUserPermissionAsync(string dashboardId, string userId)
        {
            return await GetByUserAndDashboardAsync(userId, dashboardId);
        }

        public async Task<DashboardPermission?> GetGroupPermissionAsync(string dashboardId, string groupId)
        {
            return await GetByGroupAndDashboardAsync(groupId, dashboardId);
        }

        public async Task<bool> DeleteByUserAndDashboardAsync(string userId, string dashboardId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("DELETE FROM DashboardPermissions WHERE UserId = @UserId AND DashboardId = @DashboardId", connection);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@DashboardId", dashboardId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteByGroupAndDashboardAsync(string groupId, string dashboardId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("DELETE FROM DashboardPermissions WHERE GroupId = @GroupId AND DashboardId = @DashboardId", connection);
            command.Parameters.AddWithValue("@GroupId", groupId);
            command.Parameters.AddWithValue("@DashboardId", dashboardId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> RevokeAllDashboardPermissionsAsync(string dashboardId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("DELETE FROM DashboardPermissions WHERE DashboardId = @DashboardId", connection);
            command.Parameters.AddWithValue("@DashboardId", dashboardId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteByDashboardIdAsync(string dashboardId)
        {
            return await RevokeAllDashboardPermissionsAsync(dashboardId);
        }

        public async Task<IEnumerable<DashboardPermission>> GetActivePermissionsAsync()
        {
            var permissions = new List<DashboardPermission>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM DashboardPermissions WHERE IsActive = 1", connection);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                permissions.Add(MapFromReader(reader));
            }
            
            return permissions;
        }

        public async Task<bool> RevokeUserPermissionAsync(string dashboardId, string userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("DELETE FROM DashboardPermissions WHERE DashboardId = @DashboardId AND UserId = @UserId", connection);
            command.Parameters.AddWithValue("@DashboardId", dashboardId);
            command.Parameters.AddWithValue("@UserId", userId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> RevokeGroupPermissionAsync(string dashboardId, string groupId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("DELETE FROM DashboardPermissions WHERE DashboardId = @DashboardId AND GroupId = @GroupId", connection);
            command.Parameters.AddWithValue("@DashboardId", dashboardId);
            command.Parameters.AddWithValue("@GroupId", groupId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateUserPermissionAsync(string dashboardId, string userId, string permissionType)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("UPDATE DashboardPermissions SET PermissionType = @PermissionType WHERE DashboardId = @DashboardId AND UserId = @UserId AND IsActive = 1", connection);
            command.Parameters.AddWithValue("@PermissionType", permissionType);
            command.Parameters.AddWithValue("@DashboardId", dashboardId);
            command.Parameters.AddWithValue("@UserId", userId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateGroupPermissionAsync(string dashboardId, string groupId, string permissionType)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("UPDATE DashboardPermissions SET PermissionType = @PermissionType WHERE DashboardId = @DashboardId AND GroupId = @GroupId AND IsActive = 1", connection);
            command.Parameters.AddWithValue("@PermissionType", permissionType);
            command.Parameters.AddWithValue("@DashboardId", dashboardId);
            command.Parameters.AddWithValue("@GroupId", groupId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        private DashboardPermission MapFromReader(SqlDataReader reader)
        {
            return new DashboardPermission
            {
                Id = reader.GetString("Id"),
                DashboardId = reader.GetString("DashboardId"),
                UserId = reader.IsDBNull("UserId") ? null : reader.GetString("UserId"),
                GroupId = reader.IsDBNull("GroupId") ? null : reader.GetString("GroupId"),
                PermissionType = reader.GetString("PermissionType"),
                GrantedBy = reader.GetString("GrantedBy"),
                GrantedDate = reader.GetDateTime("GrantedDate"),
                ExpiryDate = reader.IsDBNull("ExpiryDate") ? null : reader.GetDateTime("ExpiryDate"),
                IsActive = reader.GetBoolean("IsActive")
            };
        }
    }
}