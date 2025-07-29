using DataLens.Data.Interfaces;
using DataLens.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DataLens.Data.SqlServer
{
    public class SqlDashboardRepository : IDashboardRepository
    {
        private readonly string _connectionString;

        public SqlDashboardRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Dashboard>> GetAllAsync()
        {
            var dashboards = new List<Dashboard>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM Dashboards WHERE IsActive = 1", connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                dashboards.Add(MapFromReader(reader));
            }
            
            return dashboards;
        }

        public async Task<Dashboard?> GetByIdAsync(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM Dashboards WHERE Id = @Id AND IsActive = 1", connection);
            command.Parameters.AddWithValue("@Id", id);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapFromReader(reader);
            }
            
            return null;
        }

        public async Task<string> AddAsync(Dashboard dashboard)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(@"
                INSERT INTO Dashboards (Id, Name, Title, Description, DashboardData, CreatedBy, CreatedDate, IsActive, IsPublic, Category, ViewCount)
                VALUES (@Id, @Name, @Title, @Description, @DashboardData, @CreatedBy, @CreatedDate, @IsActive, @IsPublic, @Category, @ViewCount)", connection);
            
            command.Parameters.AddWithValue("@Id", dashboard.Id);
            command.Parameters.AddWithValue("@Name", dashboard.Name);
            command.Parameters.AddWithValue("@Title", dashboard.Title);
            command.Parameters.AddWithValue("@Description", dashboard.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DashboardData", dashboard.DashboardData);
            command.Parameters.AddWithValue("@CreatedBy", dashboard.CreatedBy);
            command.Parameters.AddWithValue("@CreatedDate", dashboard.CreatedDate);
            command.Parameters.AddWithValue("@IsActive", dashboard.IsActive);
            command.Parameters.AddWithValue("@IsPublic", dashboard.IsPublic);
            command.Parameters.AddWithValue("@Category", dashboard.Category);
            command.Parameters.AddWithValue("@ViewCount", dashboard.ViewCount);
            
            await command.ExecuteNonQueryAsync();
            return dashboard.Id;
        }

        public async Task<bool> UpdateAsync(Dashboard dashboard)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(@"
                UPDATE Dashboards SET 
                    Name = @Name, Title = @Title, Description = @Description, DashboardData = @DashboardData,
                    LastModifiedDate = @LastModifiedDate, LastModifiedBy = @LastModifiedBy, LastModifiedAt = @LastModifiedAt,
                    IsActive = @IsActive, IsPublic = @IsPublic, Category = @Category, ViewCount = @ViewCount
                WHERE Id = @Id", connection);
            
            command.Parameters.AddWithValue("@Id", dashboard.Id);
            command.Parameters.AddWithValue("@Name", dashboard.Name);
            command.Parameters.AddWithValue("@Title", dashboard.Title);
            command.Parameters.AddWithValue("@Description", dashboard.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DashboardData", dashboard.DashboardData);
            command.Parameters.AddWithValue("@LastModifiedDate", dashboard.LastModifiedDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@LastModifiedBy", dashboard.LastModifiedBy ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@LastModifiedAt", dashboard.LastModifiedAt ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsActive", dashboard.IsActive);
            command.Parameters.AddWithValue("@IsPublic", dashboard.IsPublic);
            command.Parameters.AddWithValue("@Category", dashboard.Category);
            command.Parameters.AddWithValue("@ViewCount", dashboard.ViewCount);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("UPDATE Dashboards SET IsActive = 0 WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Dashboard>> GetByUserIdAsync(string userId)
        {
            var dashboards = new List<Dashboard>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM Dashboards WHERE CreatedBy = @UserId AND IsActive = 1", connection);
            command.Parameters.AddWithValue("@UserId", userId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                dashboards.Add(MapFromReader(reader));
            }
            
            return dashboards;
        }

        public async Task<IEnumerable<Dashboard>> SearchAsync(string searchTerm)
        {
            return await SearchDashboardsAsync(searchTerm);
        }

        public async Task<bool> ExistsAsync(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT COUNT(*) FROM Dashboards WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            
            var result = await command.ExecuteScalarAsync();
            var count = result != null ? (int)result : 0;
            return count > 0;
        }

        public async Task<IEnumerable<Dashboard>> GetPublicAsync()
        {
            var dashboards = new List<Dashboard>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM Dashboards WHERE IsPublic = 1 AND IsActive = 1", connection);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                dashboards.Add(MapFromReader(reader));
            }
            
            return dashboards;
        }



        public async Task<int> CountAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT COUNT(*) FROM Dashboards WHERE IsActive = 1", connection);
            var result = await command.ExecuteScalarAsync();
            var count = result != null ? (int)result : 0;
            return count;
        }

        public async Task<IEnumerable<Dashboard>> SearchDashboardsAsync(string searchTerm)
        {
            var dashboards = new List<Dashboard>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(@"
                SELECT * FROM Dashboards 
                WHERE (Title LIKE @SearchTerm OR Name LIKE @SearchTerm OR Description LIKE @SearchTerm) 
                AND IsActive = 1", connection);
            command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                dashboards.Add(MapFromReader(reader));
            }
            
            return dashboards;
        }

        public async Task<long> CountByUserAsync(string userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT COUNT(*) FROM Dashboards WHERE CreatedBy = @UserId AND IsActive = 1", connection);
            command.Parameters.AddWithValue("@UserId", userId);
            
            var result = await command.ExecuteScalarAsync();
            var count = result != null ? (long)result : 0L;
            return count;
        }

        public async Task<IEnumerable<Dashboard>> GetByConditionAsync(string condition, object? parameter)
        {
            var dashboards = new List<Dashboard>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand($"SELECT * FROM Dashboards WHERE {condition} AND IsActive = 1", connection);
            if (parameter != null)
            {
                command.Parameters.AddWithValue("@Parameter", parameter);
            }
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                dashboards.Add(MapFromReader(reader));
            }
            
            return dashboards;
        }

        public async Task<bool> ExistsAsync(string name, string? excludeId = null)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var sql = "SELECT COUNT(*) FROM Dashboards WHERE Name = @Name AND IsActive = 1";
            if (!string.IsNullOrEmpty(excludeId))
            {
                sql += " AND Id != @ExcludeId";
            }
            
            var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Name", name);
            if (!string.IsNullOrEmpty(excludeId))
            {
                command.Parameters.AddWithValue("@ExcludeId", excludeId);
            }
            
            var result = await command.ExecuteScalarAsync();
            var count = result != null ? (int)result : 0;
            return count > 0;
        }

        public async Task<IEnumerable<Dashboard>> GetByCreatedByAsync(string createdBy)
        {
            var dashboards = new List<Dashboard>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM Dashboards WHERE CreatedBy = @CreatedBy AND IsActive = 1", connection);
            command.Parameters.AddWithValue("@CreatedBy", createdBy);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                dashboards.Add(MapFromReader(reader));
            }
            
            return dashboards;
        }

        public async Task<IEnumerable<Dashboard>> GetActiveDashboardsAsync()
        {
            return await GetAllAsync();
        }

        public async Task<IEnumerable<Dashboard>> GetPublicDashboardsAsync()
        {
            var dashboards = new List<Dashboard>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM Dashboards WHERE IsPublic = 1 AND IsActive = 1", connection);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                dashboards.Add(MapFromReader(reader));
            }
            
            return dashboards;
        }

        public async Task<IEnumerable<Dashboard>> GetByCategoryAsync(string category)
        {
            var dashboards = new List<Dashboard>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT * FROM Dashboards WHERE Category = @Category AND IsActive = 1", connection);
            command.Parameters.AddWithValue("@Category", category);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                dashboards.Add(MapFromReader(reader));
            }
            
            return dashboards;
        }

        public Task<IEnumerable<Dashboard>> GetByTagAsync(string tag)
        {
            // SQL Server implementation would need a separate Tags table
            // For now, return empty list
            return Task.FromResult<IEnumerable<Dashboard>>(new List<Dashboard>());
        }

        public async Task<IEnumerable<Dashboard>> GetUserAccessibleDashboardsAsync(string userId)
        {
            var dashboards = new List<Dashboard>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(@"
                SELECT DISTINCT d.* FROM Dashboards d
                LEFT JOIN DashboardPermissions dp ON d.Id = dp.DashboardId
                WHERE (d.CreatedBy = @UserId OR d.IsPublic = 1 OR dp.UserId = @UserId)
                AND d.IsActive = 1", connection);
            command.Parameters.AddWithValue("@UserId", userId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                dashboards.Add(MapFromReader(reader));
            }
            
            return dashboards;
        }

        public async Task<IEnumerable<Dashboard>> GetGroupAccessibleDashboardsAsync(string groupId)
        {
            var dashboards = new List<Dashboard>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(@"
                SELECT DISTINCT d.* FROM Dashboards d
                INNER JOIN DashboardPermissions dp ON d.Id = dp.DashboardId
                WHERE dp.GroupId = @GroupId AND d.IsActive = 1", connection);
            command.Parameters.AddWithValue("@GroupId", groupId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                dashboards.Add(MapFromReader(reader));
            }
            
            return dashboards;
        }

        public async Task<bool> UpdateLastModifiedAsync(string dashboardId, string modifiedBy)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(@"
                UPDATE Dashboards 
                SET LastModifiedDate = @LastModifiedDate, 
                    LastModifiedBy = @ModifiedBy,
                    LastModifiedAt = @LastModifiedAt
                WHERE Id = @Id", connection);
            
            command.Parameters.AddWithValue("@Id", dashboardId);
            command.Parameters.AddWithValue("@ModifiedBy", modifiedBy);
            command.Parameters.AddWithValue("@LastModifiedDate", DateTime.UtcNow);
            command.Parameters.AddWithValue("@LastModifiedAt", DateTime.UtcNow);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Dashboard>> GetRecentAsync(int count)
        {
            var dashboards = new List<Dashboard>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand($"SELECT TOP {count} * FROM Dashboards WHERE IsActive = 1 ORDER BY CreatedDate DESC", connection);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                dashboards.Add(MapFromReader(reader));
            }
            
            return dashboards;
        }

        public async Task<IEnumerable<Dashboard>> GetMostViewedAsync(int count)
        {
            var dashboards = new List<Dashboard>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand($"SELECT TOP {count} * FROM Dashboards WHERE IsActive = 1 ORDER BY ViewCount DESC", connection);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                dashboards.Add(MapFromReader(reader));
            }
            
            return dashboards;
        }

        public async Task<bool> IncrementViewCountAsync(string dashboardId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("UPDATE Dashboards SET ViewCount = ViewCount + 1 WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", dashboardId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            var categories = new List<string>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT DISTINCT Category FROM Dashboards WHERE IsActive = 1", connection);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                categories.Add(reader.GetString("Category"));
            }
            
            return categories;
        }

        public Task<IEnumerable<string>> GetTagsAsync()
        {
            // SQL Server implementation would need a separate Tags table
            // For now, return empty list
            return Task.FromResult<IEnumerable<string>>(new List<string>());
        }

        private Dashboard MapFromReader(SqlDataReader reader)
        {
            return new Dashboard
            {
                Id = reader.GetString("Id"),
                Name = reader.GetString("Name"),
                Title = reader.GetString("Title"),
                Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                DashboardData = reader.GetString("DashboardData"),
                CreatedBy = reader.GetString("CreatedBy"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                LastModifiedDate = reader.IsDBNull("LastModifiedDate") ? null : reader.GetDateTime("LastModifiedDate"),
                LastModifiedBy = reader.IsDBNull("LastModifiedBy") ? null : reader.GetString("LastModifiedBy"),
                LastModifiedAt = reader.IsDBNull("LastModifiedAt") ? null : reader.GetDateTime("LastModifiedAt"),
                IsActive = reader.GetBoolean("IsActive"),
                IsPublic = reader.GetBoolean("IsPublic"),
                Category = reader.GetString("Category"),
                ViewCount = reader.GetInt32("ViewCount")
            };
        }
    }
}