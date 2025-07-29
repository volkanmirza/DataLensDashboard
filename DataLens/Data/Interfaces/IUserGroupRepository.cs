using DataLens.Models;

namespace DataLens.Data.Interfaces
{
    public interface IUserGroupRepository : IRepository<UserGroup>
    {
        Task<IEnumerable<UserGroup>> GetByCreatedByAsync(string createdBy);
        Task<IEnumerable<UserGroup>> GetActiveGroupsAsync();
        Task<bool> IsGroupNameExistsAsync(string groupName);
        Task<IEnumerable<User>> GetGroupMembersAsync(string groupId);
        Task<IEnumerable<UserGroup>> GetUserGroupsAsync(string userId);
        Task<bool> AddUserToGroupAsync(string userId, string groupId, string addedBy);
        Task<bool> RemoveUserFromGroupAsync(string userId, string groupId);
        Task<bool> IsUserInGroupAsync(string userId, string groupId);
    }
}