using Dapper;
using DataLens.Data.Interfaces;
using DataLens.Models;
using System.Data;

namespace DataLens.Data.SqlServer
{
    public class SqlUserGroupMembershipRepository : IUserGroupMembershipRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public SqlUserGroupMembershipRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<UserGroupMembership?> GetByIdAsync(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM UserGroupMemberships WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<UserGroupMembership>(sql, new { Id = id });
        }

        public async Task<IEnumerable<UserGroupMembership>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM UserGroupMemberships ORDER BY JoinedDate";
            return await connection.QueryAsync<UserGroupMembership>(sql);
        }

        public async Task<IEnumerable<UserGroupMembership>> GetByConditionAsync(string condition, object? parameters = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = $"SELECT * FROM UserGroupMemberships WHERE {condition}";
            return await connection.QueryAsync<UserGroupMembership>(sql, parameters);
        }

        public async Task<string> AddAsync(UserGroupMembership entity)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                INSERT INTO UserGroupMemberships (Id, UserId, GroupId, JoinedDate, AddedBy, IsActive, Notes)
                VALUES (@Id, @UserId, @GroupId, @JoinedDate, @AddedBy, @IsActive, @Notes)";
            
            await connection.ExecuteAsync(sql, entity);
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(UserGroupMembership entity)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE UserGroupMemberships 
                SET UserId = @UserId, GroupId = @GroupId, JoinedDate = @JoinedDate, 
                    AddedBy = @AddedBy, IsActive = @IsActive, RemovedDate = @RemovedDate, 
                    RemovedBy = @RemovedBy, Notes = @Notes
                WHERE Id = @Id";
            
            var rowsAffected = await connection.ExecuteAsync(sql, entity);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM UserGroupMemberships WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(*) FROM UserGroupMemberships WHERE Id = @Id";
            var count = await connection.QuerySingleAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<int> CountAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(*) FROM UserGroupMemberships";
            return await connection.QuerySingleAsync<int>(sql);
        }

        public async Task<IEnumerable<UserGroupMembership>> GetByUserIdAsync(string userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM UserGroupMemberships WHERE UserId = @UserId ORDER BY JoinedDate";
            return await connection.QueryAsync<UserGroupMembership>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<UserGroupMembership>> GetByGroupIdAsync(string groupId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM UserGroupMemberships WHERE GroupId = @GroupId ORDER BY JoinedDate";
            return await connection.QueryAsync<UserGroupMembership>(sql, new { GroupId = groupId });
        }

        public async Task<UserGroupMembership?> GetByUserAndGroupAsync(string userId, string groupId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM UserGroupMemberships WHERE UserId = @UserId AND GroupId = @GroupId";
            return await connection.QueryFirstOrDefaultAsync<UserGroupMembership>(sql, new { UserId = userId, GroupId = groupId });
        }

        public async Task<IEnumerable<User>> GetGroupMembersAsync(string groupId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT u.* FROM Users u
                INNER JOIN UserGroupMemberships ugm ON u.Id = ugm.UserId
                WHERE ugm.GroupId = @GroupId AND ugm.IsActive = 1
                ORDER BY u.Username";
            return await connection.QueryAsync<User>(sql, new { GroupId = groupId });
        }

        public async Task<IEnumerable<UserGroup>> GetUserGroupsAsync(string userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT ug.* FROM UserGroups ug
                INNER JOIN UserGroupMemberships ugm ON ug.Id = ugm.GroupId
                WHERE ugm.UserId = @UserId AND ugm.IsActive = 1 AND ug.IsActive = 1
                ORDER BY ug.GroupName";
            return await connection.QueryAsync<UserGroup>(sql, new { UserId = userId });
        }

        public async Task<bool> IsUserInGroupAsync(string userId, string groupId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(*) FROM UserGroupMemberships WHERE UserId = @UserId AND GroupId = @GroupId AND IsActive = 1";
            var count = await connection.QuerySingleAsync<int>(sql, new { UserId = userId, GroupId = groupId });
            return count > 0;
        }

        public async Task<int> GetGroupMemberCountAsync(string groupId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(*) FROM UserGroupMemberships WHERE GroupId = @GroupId AND IsActive = 1";
            return await connection.QuerySingleAsync<int>(sql, new { GroupId = groupId });
        }

        public async Task<bool> RemoveUserFromAllGroupsAsync(string userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE UserGroupMemberships 
                SET IsActive = 0, RemovedDate = @RemovedDate, RemovedBy = @RemovedBy
                WHERE UserId = @UserId AND IsActive = 1";
            
            var rowsAffected = await connection.ExecuteAsync(sql, new 
            { 
                UserId = userId, 
                RemovedDate = DateTime.UtcNow, 
                RemovedBy = "System" 
            });
            return rowsAffected > 0;
        }

        public async Task<bool> RemoveAllMembersFromGroupAsync(string groupId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE UserGroupMemberships 
                SET IsActive = 0, RemovedDate = @RemovedDate, RemovedBy = @RemovedBy
                WHERE GroupId = @GroupId AND IsActive = 1";
            
            var rowsAffected = await connection.ExecuteAsync(sql, new 
            { 
                GroupId = groupId, 
                RemovedDate = DateTime.UtcNow, 
                RemovedBy = "System" 
            });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<UserGroupMembership>> GetActiveMembershipsAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM UserGroupMemberships WHERE IsActive = 1 ORDER BY JoinedDate";
            return await connection.QueryAsync<UserGroupMembership>(sql);
        }

        public async Task<IEnumerable<UserGroupMembership>> GetMembershipHistoryAsync(string userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM UserGroupMemberships WHERE UserId = @UserId ORDER BY JoinedDate DESC";
            return await connection.QueryAsync<UserGroupMembership>(sql, new { UserId = userId });
        }

        public async Task<bool> DeactivateMembershipAsync(string userId, string groupId, string removedBy)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE UserGroupMemberships 
                SET IsActive = 0, RemovedDate = @RemovedDate, RemovedBy = @RemovedBy
                WHERE UserId = @UserId AND GroupId = @GroupId AND IsActive = 1";
            
            var rowsAffected = await connection.ExecuteAsync(sql, new 
            { 
                UserId = userId, 
                GroupId = groupId, 
                RemovedDate = DateTime.UtcNow, 
                RemovedBy = removedBy 
            });
            return rowsAffected > 0;
        }

        public async Task<bool> ReactivateMembershipAsync(string userId, string groupId, string addedBy)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE UserGroupMemberships 
                SET IsActive = 1, RemovedDate = NULL, RemovedBy = NULL, AddedBy = @AddedBy
                WHERE UserId = @UserId AND GroupId = @GroupId AND IsActive = 0";
            
            var rowsAffected = await connection.ExecuteAsync(sql, new 
            { 
                UserId = userId, 
                GroupId = groupId, 
                AddedBy = addedBy 
            });
            return rowsAffected > 0;
        }
    }
}