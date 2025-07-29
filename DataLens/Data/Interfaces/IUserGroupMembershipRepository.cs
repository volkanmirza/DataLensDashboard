using DataLens.Models;

namespace DataLens.Data.Interfaces
{
    public interface IUserGroupMembershipRepository : IRepository<UserGroupMembership>
    {
        Task<IEnumerable<UserGroupMembership>> GetByUserIdAsync(string userId);
        Task<IEnumerable<UserGroupMembership>> GetByGroupIdAsync(string groupId);
        Task<UserGroupMembership?> GetByUserAndGroupAsync(string userId, string groupId);
        Task<IEnumerable<User>> GetGroupMembersAsync(string groupId);
        Task<IEnumerable<UserGroup>> GetUserGroupsAsync(string userId);
        Task<bool> IsUserInGroupAsync(string userId, string groupId);
        Task<int> GetGroupMemberCountAsync(string groupId);
        Task<bool> RemoveUserFromAllGroupsAsync(string userId);
        Task<bool> RemoveAllMembersFromGroupAsync(string groupId);
        Task<IEnumerable<UserGroupMembership>> GetActiveMembershipsAsync();
        Task<IEnumerable<UserGroupMembership>> GetMembershipHistoryAsync(string userId);
        Task<bool> DeactivateMembershipAsync(string userId, string groupId, string removedBy);
        Task<bool> ReactivateMembershipAsync(string userId, string groupId, string addedBy);
    }
}