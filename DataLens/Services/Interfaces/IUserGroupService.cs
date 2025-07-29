using DataLens.Models;

namespace DataLens.Services.Interfaces
{
    public interface IUserGroupService
    {
        Task<IEnumerable<UserGroup>> GetAllGroupsAsync();
        Task<UserGroup?> GetGroupByIdAsync(string id);
        Task<IEnumerable<UserGroup>> GetActiveGroupsAsync();
        Task<IEnumerable<UserGroup>> GetGroupsByCreatedByAsync(string createdBy);
        Task<string> CreateGroupAsync(UserGroup group);
        Task<bool> UpdateGroupAsync(UserGroup group);
        Task<bool> DeleteGroupAsync(string id);
        Task<bool> IsGroupNameExistsAsync(string groupName);
        
        // Member management
        Task<IEnumerable<User>> GetGroupMembersAsync(string groupId);
        Task<IEnumerable<UserGroup>> GetUserGroupsAsync(string userId);
        Task<bool> AddUserToGroupAsync(string userId, string groupId, string addedBy);
        Task<bool> RemoveUserFromGroupAsync(string userId, string groupId);
        Task<bool> IsUserInGroupAsync(string userId, string groupId);
        Task<int> GetGroupMemberCountAsync(string groupId);
        Task<bool> RemoveUserFromAllGroupsAsync(string userId);
        Task<bool> RemoveAllMembersFromGroupAsync(string groupId);
    }
}