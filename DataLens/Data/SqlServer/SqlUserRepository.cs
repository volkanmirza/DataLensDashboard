using Dapper;
using DataLens.Data.Interfaces;
using DataLens.Models;
using System.Data;

namespace DataLens.Data.SqlServer
{
    public class SqlUserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public SqlUserRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM Users WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM Users ORDER BY CreatedDate DESC";
            return await connection.QueryAsync<User>(sql);
        }

        public async Task<IEnumerable<User>> GetByConditionAsync(string condition, object? parameters = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = $"SELECT * FROM Users WHERE {condition}";
            return await connection.QueryAsync<User>(sql, parameters);
        }

        public async Task<string> AddAsync(User entity)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                INSERT INTO Users (Id, Username, Email, PasswordHash, Role, CreatedDate, IsActive, FirstName, LastName)
                VALUES (@Id, @Username, @Email, @PasswordHash, @Role, @CreatedDate, @IsActive, @FirstName, @LastName)";
            
            await connection.ExecuteAsync(sql, entity);
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(User entity)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE Users SET 
                    Username = @Username, 
                    Email = @Email, 
                    Role = @Role, 
                    IsActive = @IsActive, 
                    FirstName = @FirstName, 
                    LastName = @LastName
                WHERE Id = @Id";
            
            var rowsAffected = await connection.ExecuteAsync(sql, entity);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM Users WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(1) FROM Users WHERE Id = @Id";
            var count = await connection.QuerySingleAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<int> CountAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(*) FROM Users";
            return await connection.QuerySingleAsync<int>(sql);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM Users WHERE Username = @Username";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM Users WHERE Email = @Email";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(string role)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM Users WHERE Role = @Role ORDER BY Username";
            return await connection.QueryAsync<User>(sql, new { Role = role });
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM Users WHERE IsActive = 1 ORDER BY Username";
            return await connection.QueryAsync<User>(sql);
        }

        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
            var count = await connection.QuerySingleAsync<int>(sql, new { Username = username });
            return count > 0;
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
            var count = await connection.QuerySingleAsync<int>(sql, new { Email = email });
            return count > 0;
        }

        public async Task<bool> UpdatePasswordAsync(string userId, string passwordHash)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "UPDATE Users SET PasswordHash = @PasswordHash WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = userId, PasswordHash = passwordHash });
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateLastLoginAsync(string userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "UPDATE Users SET LastLoginDate = @LastLoginDate WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = userId, LastLoginDate = DateTime.UtcNow });
            return rowsAffected > 0;
        }
    }
}