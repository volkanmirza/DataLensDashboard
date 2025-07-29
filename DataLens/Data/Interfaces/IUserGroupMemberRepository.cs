using DataLens.Models;

namespace DataLens.Data.Interfaces
{
    public interface IUserGroupMemberRepository : IRepository<UserGroupMember>
    {
        Task<IEnumerable<UserGroupMember>> GetByUserIdAsync(string userId);
        Task<IEnumerable<UserGroupMember>> GetByGroupIdAsync(string groupId);
        Task<UserGroupMember?> GetByUserAndGroupAsync(string userId, string groupId);
        Task<IEnumerable<User>> GetGroupMembersAsync(string groupId);
        Task<IEnumerable<UserGroup>> GetUserGroupsAsync(string userId);
        Task<bool> IsUserInGroupAsync(string userId, string groupId);
        Task<int> GetGroupMemberCountAsync(string groupId);
        Task<bool> RemoveUserFromAllGroupsAsync(string userId);
        Task<bool> RemoveAllMembersFromGroupAsync(string groupId);
    }
}