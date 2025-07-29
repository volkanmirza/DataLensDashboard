using DataLens.Data.Interfaces;
using DataLens.Models;
using DataLens.Services.Interfaces;

namespace DataLens.Services
{
    public class UserGroupService : IUserGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserGroupService> _logger;

        public UserGroupService(IUnitOfWork unitOfWork, ILogger<UserGroupService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<UserGroup>> GetAllGroupsAsync()
        {
            try
            {
                return await _unitOfWork.UserGroups.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all user groups");
                throw;
            }
        }

        public async Task<UserGroup?> GetGroupByIdAsync(string id)
        {
            try
            {
                return await _unitOfWork.UserGroups.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user group by id: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<UserGroup>> GetActiveGroupsAsync()
        {
            try
            {
                return await _unitOfWork.UserGroups.GetActiveGroupsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active user groups");
                throw;
            }
        }

        public async Task<IEnumerable<UserGroup>> GetGroupsByCreatedByAsync(string createdBy)
        {
            try
            {
                return await _unitOfWork.UserGroups.GetByCreatedByAsync(createdBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user groups by created by: {CreatedBy}", createdBy);
                throw;
            }
        }

        public async Task<string> CreateGroupAsync(UserGroup group)
        {
            try
            {
                // Check if group name already exists
                if (await _unitOfWork.UserGroups.IsGroupNameExistsAsync(group.GroupName))
                {
                    throw new InvalidOperationException($"Group name '{group.GroupName}' already exists");
                }

                // Set default values
                group.Id = Guid.NewGuid().ToString();
                group.CreatedDate = DateTime.UtcNow;
                group.IsActive = true;

                await _unitOfWork.BeginTransactionAsync();
                var result = await _unitOfWork.UserGroups.AddAsync(group);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("User group created successfully: {GroupName} by {CreatedBy}", group.GroupName, group.CreatedBy);
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error creating user group: {GroupName}", group.GroupName);
                throw;
            }
        }

        public async Task<bool> UpdateGroupAsync(UserGroup group)
        {
            try
            {
                // Check if another group with the same name exists (excluding current group)
                var existingGroups = await _unitOfWork.UserGroups.GetAllAsync();
                if (existingGroups.Any(g => g.GroupName == group.GroupName && g.Id != group.Id))
                {
                    throw new InvalidOperationException($"Group name '{group.GroupName}' already exists");
                }

                await _unitOfWork.BeginTransactionAsync();
                var result = await _unitOfWork.UserGroups.UpdateAsync(group);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("User group updated successfully: {GroupName}", group.GroupName);
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error updating user group: {GroupId}", group.Id);
                throw;
            }
        }

        public async Task<bool> DeleteGroupAsync(string id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                // Remove all members from the group first
                await _unitOfWork.UserGroupMembers.RemoveAllMembersFromGroupAsync(id);
                
                // Then delete the group
                var result = await _unitOfWork.UserGroups.DeleteAsync(id);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("User group deleted successfully: {GroupId}", id);
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error deleting user group: {GroupId}", id);
                throw;
            }
        }

        public async Task<bool> IsGroupNameExistsAsync(string groupName)
        {
            try
            {
                return await _unitOfWork.UserGroups.IsGroupNameExistsAsync(groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if group name exists: {GroupName}", groupName);
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetGroupMembersAsync(string groupId)
        {
            try
            {
                return await _unitOfWork.UserGroupMembers.GetGroupMembersAsync(groupId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group members: {GroupId}", groupId);
                throw;
            }
        }

        public async Task<IEnumerable<UserGroup>> GetUserGroupsAsync(string userId)
        {
            try
            {
                return await _unitOfWork.UserGroupMembers.GetUserGroupsAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user groups: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> AddUserToGroupAsync(string userId, string groupId, string addedBy)
        {
            try
            {
                // Check if user is already in the group
                if (await _unitOfWork.UserGroupMembers.IsUserInGroupAsync(userId, groupId))
                {
                    throw new InvalidOperationException("User is already a member of this group");
                }

                // Check if user and group exist
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ArgumentException("User not found", nameof(userId));
                }

                var group = await _unitOfWork.UserGroups.GetByIdAsync(groupId);
                if (group == null)
                {
                    throw new ArgumentException("Group not found", nameof(groupId));
                }

                var member = new UserGroupMember
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    GroupId = groupId,
                    JoinedDate = DateTime.UtcNow,
                    AddedBy = addedBy
                };

                await _unitOfWork.BeginTransactionAsync();
                await _unitOfWork.UserGroupMembers.AddAsync(member);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("User added to group successfully: {UserId} to {GroupId} by {AddedBy}", userId, groupId, addedBy);
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error adding user to group: {UserId} to {GroupId}", userId, groupId);
                throw;
            }
        }

        public async Task<bool> RemoveUserFromGroupAsync(string userId, string groupId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                // Find and delete the specific membership
                var member = await _unitOfWork.UserGroupMembers.GetByUserAndGroupAsync(userId, groupId);
                if (member == null)
                {
                    throw new InvalidOperationException("User is not a member of this group");
                }
                
                var result = await _unitOfWork.UserGroupMembers.DeleteAsync(member.Id);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("User removed from group successfully: {UserId} from {GroupId}", userId, groupId);
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error removing user from group: {UserId} from {GroupId}", userId, groupId);
                throw;
            }
        }

        public async Task<bool> IsUserInGroupAsync(string userId, string groupId)
        {
            try
            {
                return await _unitOfWork.UserGroupMembers.IsUserInGroupAsync(userId, groupId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user is in group: {UserId} in {GroupId}", userId, groupId);
                throw;
            }
        }

        public async Task<int> GetGroupMemberCountAsync(string groupId)
        {
            try
            {
                return await _unitOfWork.UserGroupMembers.GetGroupMemberCountAsync(groupId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group member count: {GroupId}", groupId);
                throw;
            }
        }

        public async Task<bool> RemoveUserFromAllGroupsAsync(string userId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var result = await _unitOfWork.UserGroupMembers.RemoveUserFromAllGroupsAsync(userId);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("User removed from all groups successfully: {UserId}", userId);
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error removing user from all groups: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> RemoveAllMembersFromGroupAsync(string groupId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var result = await _unitOfWork.UserGroupMembers.RemoveAllMembersFromGroupAsync(groupId);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("All members removed from group successfully: {GroupId}", groupId);
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error removing all members from group: {GroupId}", groupId);
                throw;
            }
        }
    }
}