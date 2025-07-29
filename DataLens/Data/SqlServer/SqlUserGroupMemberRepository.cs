using Dapper;
using DataLens.Data.Interfaces;
using DataLens.Models;
using System.Data;

namespace DataLens.Data.SqlServer
{
    public class SqlUserGroupMemberRepository : IUserGroupMemberRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public SqlUserGroupMemberRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<UserGroupMember?> GetByIdAsync(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM UserGroupMembers WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<UserGroupMember>(sql, new { Id = id });
        }

        public async Task<IEnumerable<UserGroupMember>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM UserGroupMembers ORDER BY JoinedDate";
            return await connection.QueryAsync<UserGroupMember>(sql);
        }

        public async Task<IEnumerable<UserGroupMember>> GetByConditionAsync(string condition, object? parameters = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = $"SELECT * FROM UserGroupMembers WHERE {condition}";
            return await connection.QueryAsync<UserGroupMember>(sql, parameters);
        }

        public async Task<string> AddAsync(UserGroupMember entity)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                INSERT INTO UserGroupMembers (Id, UserId, GroupId, JoinedDate, AddedBy)
                VALUES (@Id, @UserId, @GroupId, @JoinedDate, @AddedBy)";
            
            await connection.ExecuteAsync(sql, entity);
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(UserGroupMember entity)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE UserGroupMembers 
                SET UserId = @UserId, GroupId = @GroupId, JoinedDate = @JoinedDate, AddedBy = @AddedBy
                WHERE Id = @Id";
            
            var rowsAffected = await connection.ExecuteAsync(sql, entity);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM UserGroupMembers WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(1) FROM UserGroupMembers WHERE Id = @Id";
            var count = await connection.QuerySingleAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<int> CountAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(*) FROM UserGroupMembers";
            return await connection.QuerySingleAsync<int>(sql);
        }

        public async Task<IEnumerable<UserGroupMember>> GetByUserIdAsync(string userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM UserGroupMembers WHERE UserId = @UserId ORDER BY JoinedDate";
            return await connection.QueryAsync<UserGroupMember>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<UserGroupMember>> GetByGroupIdAsync(string groupId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM UserGroupMembers WHERE GroupId = @GroupId ORDER BY JoinedDate";
            return await connection.QueryAsync<UserGroupMember>(sql, new { GroupId = groupId });
        }

        public async Task<UserGroupMember?> GetByUserAndGroupAsync(string userId, string groupId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM UserGroupMembers WHERE UserId = @UserId AND GroupId = @GroupId";
            return await connection.QueryFirstOrDefaultAsync<UserGroupMember>(sql, new { UserId = userId, GroupId = groupId });
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

        public async Task<bool> IsUserInGroupAsync(string userId, string groupId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(1) FROM UserGroupMembers WHERE UserId = @UserId AND GroupId = @GroupId";
            var count = await connection.QuerySingleAsync<int>(sql, new { UserId = userId, GroupId = groupId });
            return count > 0;
        }

        public async Task<int> GetGroupMemberCountAsync(string groupId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(*) FROM UserGroupMembers WHERE GroupId = @GroupId";
            return await connection.QuerySingleAsync<int>(sql, new { GroupId = groupId });
        }

        public async Task<bool> RemoveUserFromAllGroupsAsync(string userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM UserGroupMembers WHERE UserId = @UserId";
            var rowsAffected = await connection.ExecuteAsync(sql, new { UserId = userId });
            return rowsAffected > 0;
        }

        public async Task<bool> RemoveAllMembersFromGroupAsync(string groupId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM UserGroupMembers WHERE GroupId = @GroupId";
            var rowsAffected = await connection.ExecuteAsync(sql, new { GroupId = groupId });
            return rowsAffected > 0;
        }
    }
}