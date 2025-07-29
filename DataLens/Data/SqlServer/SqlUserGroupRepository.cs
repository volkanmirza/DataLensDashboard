using Dapper;
using DataLens.Data.Interfaces;
using DataLens.Models;
using System.Data;

namespace DataLens.Data.SqlServer
{
    public class SqlUserGroupRepository : IUserGroupRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public SqlUserGroupRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<UserGroup?> GetByIdAsync(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM UserGroups WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<UserGroup>(sql, new { Id = id });
        }

        public async Task<IEnumerable<UserGroup>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM UserGroups ORDER BY GroupName";
            return await connection.QueryAsync<UserGroup>(sql);
        }

        public async Task<IEnumerable<UserGroup>> GetByConditionAsync(string condition, object? parameters = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = $"SELECT * FROM UserGroups WHERE {condition}";
            return await connection.QueryAsync<UserGroup>(sql, parameters);
        }

        public async Task<string> AddAsync(UserGroup entity)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                INSERT INTO UserGroups (Id, GroupName, Description, CreatedDate, IsActive, CreatedBy)
                VALUES (@Id, @GroupName, @Description, @CreatedDate, @IsActive, @CreatedBy)";
            
            await connection.ExecuteAsync(sql, entity);
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(UserGroup entity)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE UserGroups 
                SET GroupName = @GroupName, Description = @Description, IsActive = @IsActive
                WHERE Id = @Id";
            
            var rowsAffected = await connection.ExecuteAsync(sql, entity);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM UserGroups WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(1) FROM UserGroups WHERE Id = @Id";
            var count = await connection.QuerySingleAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<int> CountAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(*) FROM UserGroups";
            return await connection.QuerySingleAsync<int>(sql);
        }

        public async Task<IEnumerable<UserGroup>> GetByCreatedByAsync(string createdBy)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM UserGroups WHERE CreatedBy = @CreatedBy ORDER BY GroupName";
            return await connection.QueryAsync<UserGroup>(sql, new { CreatedBy = createdBy });
        }

        public async Task<IEnumerable<UserGroup>> GetActiveGroupsAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM UserGroups WHERE IsActive = 1 ORDER BY GroupName";
            return await connection.QueryAsync<UserGroup>(sql);
        }

        public async Task<bool> IsGroupNameExistsAsync(string groupName)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(1) FROM UserGroups WHERE GroupName = @GroupName";
            var count = await connection.QuerySingleAsync<int>(sql, new { GroupName = groupName });
            return count > 0;
        }

        public async Task<IEnumerable<User>> GetGroupMembersAsync(string groupId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT u.* FROM Users u
                INNER JOIN UserGroupMembers ugm ON u.Id = ugm.UserId
                WHERE ugm.GroupId = @GroupId
                ORDER BY u.Username";
            return await connection.QueryAsync<User>(sql, new { GroupId = groupId });
        }

        public async Task<IEnumerable<UserGroup>> GetUserGroupsAsync(string userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT ug.* FROM UserGroups ug
                INNER JOIN UserGroupMembers ugm ON ug.Id = ugm.GroupId
                WHERE ugm.UserId = @UserId AND ug.IsActive = 1
                ORDER BY ug.GroupName";
            return await connection.QueryAsync<UserGroup>(sql, new { UserId = userId });
        }

        public async Task<bool> AddUserToGroupAsync(string userId, string groupId, string addedBy)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                INSERT INTO UserGroupMembers (Id, UserId, GroupId, JoinedDate, AddedBy)
                VALUES (@Id, @UserId, @GroupId, @JoinedDate, @AddedBy)";
            
            var member = new
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                GroupId = groupId,
                JoinedDate = DateTime.UtcNow,
                AddedBy = addedBy
            };
            
            var rowsAffected = await connection.ExecuteAsync(sql, member);
            return rowsAffected > 0;
        }

        public async Task<bool> RemoveUserFromGroupAsync(string userId, string groupId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM UserGroupMembers WHERE UserId = @UserId AND GroupId = @GroupId";
            var rowsAffected = await connection.ExecuteAsync(sql, new { UserId = userId, GroupId = groupId });
            return rowsAffected > 0;
        }

        public async Task<bool> IsUserInGroupAsync(string userId, string groupId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(1) FROM UserGroupMembers WHERE UserId = @UserId AND GroupId = @GroupId";
            var count = await connection.QuerySingleAsync<int>(sql, new { UserId = userId, GroupId = groupId });
            return count > 0;
        }
    }
}