using DataLens.Models;
using DataLens.Data.Interfaces;
using DataLens.Services.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace DataLens.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IDashboardPermissionRepository _permissionRepository;
        private readonly IUserGroupMembershipRepository _membershipRepository;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IDashboardRepository dashboardRepository,
            IDashboardPermissionRepository permissionRepository,
            IUserGroupMembershipRepository membershipRepository,
            ILogger<DashboardService> logger)
        {
            _dashboardRepository = dashboardRepository;
            _permissionRepository = permissionRepository;
            _membershipRepository = membershipRepository;
            _logger = logger;
        }

        #region Dashboard CRUD Operations

        public async Task<IEnumerable<Dashboard>> GetAllDashboardsAsync()
        {
            try
            {
                return await _dashboardRepository.GetActiveDashboardsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all dashboards");
                throw;
            }
        }

        public async Task<Dashboard?> GetDashboardByIdAsync(string id)
        {
            try
            {
                return await _dashboardRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard by id: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Dashboard>> GetDashboardsByUserAsync(string userId)
        {
            try
            {
                return await _dashboardRepository.GetByCreatedByAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboards by user: {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<Dashboard>> GetDashboardsByCategoryAsync(string category)
        {
            try
            {
                return await _dashboardRepository.GetByCategoryAsync(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboards by category: {Category}", category);
                throw;
            }
        }

        public async Task<IEnumerable<Dashboard>> GetPublicDashboardsAsync()
        {
            try
            {
                return await _dashboardRepository.GetPublicDashboardsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public dashboards");
                throw;
            }
        }

        public async Task<IEnumerable<Dashboard>> GetUserAccessibleDashboardsAsync(string userId)
        {
            try
            {
                var allDashboards = await _dashboardRepository.GetActiveDashboardsAsync();
                var accessibleDashboards = new List<Dashboard>();

                foreach (var dashboard in allDashboards)
                {
                    // Public dashboards are accessible to everyone
                    if (dashboard.IsPublic)
                    {
                        accessibleDashboards.Add(dashboard);
                        continue;
                    }

                    // Dashboards created by the user
                    if (dashboard.CreatedBy == userId)
                    {
                        accessibleDashboards.Add(dashboard);
                        continue;
                    }

                    // Check if user has direct permission
                    if (await CanUserAccessDashboardAsync(userId, dashboard.Id))
                    {
                        accessibleDashboards.Add(dashboard);
                    }
                }

                return accessibleDashboards;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user accessible dashboards: {UserId}", userId);
                throw;
            }
        }

        public async Task<string> CreateDashboardAsync(Dashboard dashboard)
        {
            try
            {
                if (await _dashboardRepository.ExistsAsync(dashboard.Name))
                {
                    throw new InvalidOperationException("Dashboard with this name already exists.");
                }

                dashboard.Id = ObjectId.GenerateNewId().ToString();
                dashboard.CreatedDate = DateTime.UtcNow;
                dashboard.IsActive = true;

                await _dashboardRepository.AddAsync(dashboard);
                _logger.LogInformation("Dashboard created: {DashboardId} by {UserId}", dashboard.Id, dashboard.CreatedBy);

                return dashboard.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating dashboard: {DashboardName}", dashboard.Name);
                throw;
            }
        }

        public async Task<bool> UpdateDashboardAsync(Dashboard dashboard)
        {
            try
            {
                if (await _dashboardRepository.ExistsAsync(dashboard.Name, dashboard.Id))
                {
                    throw new InvalidOperationException("Dashboard with this name already exists.");
                }

                dashboard.LastModifiedDate = DateTime.UtcNow;
                var result = await _dashboardRepository.UpdateAsync(dashboard);
                
                if (result)
                {
                    _logger.LogInformation("Dashboard updated: {DashboardId} by {UserId}", dashboard.Id, dashboard.LastModifiedBy);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dashboard: {DashboardId}", dashboard.Id);
                throw;
            }
        }

        public async Task<bool> DeleteDashboardAsync(string id)
        {
            try
            {
                // First, remove all permissions for this dashboard
                await _permissionRepository.DeleteByDashboardIdAsync(id);

                // Then delete the dashboard
                var result = await _dashboardRepository.DeleteAsync(id);
                
                if (result)
                {
                    _logger.LogInformation("Dashboard deleted: {DashboardId}", id);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting dashboard: {DashboardId}", id);
                throw;
            }
        }

        public async Task<bool> IsDashboardNameExistsAsync(string name, string? excludeId = null)
        {
            try
            {
                return await _dashboardRepository.ExistsAsync(name, excludeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking dashboard name existence: {Name}", name);
                throw;
            }
        }

        public async Task<IEnumerable<Dashboard>> SearchDashboardsAsync(string searchTerm)
        {
            try
            {
                return await _dashboardRepository.SearchAsync(searchTerm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching dashboards: {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<long> GetDashboardCountAsync()
        {
            try
            {
                return await _dashboardRepository.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard count");
                throw;
            }
        }

        public async Task<long> GetUserDashboardCountAsync(string userId)
        {
            try
            {
                return await _dashboardRepository.CountByUserAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user dashboard count: {UserId}", userId);
                throw;
            }
        }

        #endregion

        #region Permission Management

        public async Task<bool> HasPermissionAsync(string userId, string dashboardId, string permissionType)
        {
            try
            {
                return await _permissionRepository.HasPermissionAsync(userId, dashboardId, permissionType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user permission: {UserId}, {DashboardId}, {PermissionType}", userId, dashboardId, permissionType);
                throw;
            }
        }

        public async Task<bool> HasUserPermissionAsync(string dashboardId, string userId, string permissionType)
        {
            try
            {
                return await _permissionRepository.HasPermissionAsync(userId, dashboardId, permissionType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user permission: {UserId}, {DashboardId}, {PermissionType}", userId, dashboardId, permissionType);
                throw;
            }
        }

        public async Task<bool> HasGroupPermissionAsync(string groupId, string dashboardId, string permissionType)
        {
            try
            {
                return await _permissionRepository.HasGroupPermissionAsync(groupId, dashboardId, permissionType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking group permission: {GroupId}, {DashboardId}, {PermissionType}", groupId, dashboardId, permissionType);
                throw;
            }
        }

        public async Task<bool> CanUserAccessDashboardAsync(string userId, string dashboardId, string permissionType = PermissionTypes.View)
        {
            try
            {
                // Check direct user permission
                if (await _permissionRepository.HasPermissionAsync(userId, dashboardId, permissionType))
                {
                    return true;
                }

                // Check group permissions
                var userMemberships = await _membershipRepository.GetByUserIdAsync(userId);
                foreach (var membership in userMemberships)
                {
                    if (await _permissionRepository.HasGroupPermissionAsync(membership.GroupId, dashboardId, permissionType))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user dashboard access: {UserId}, {DashboardId}", userId, dashboardId);
                throw;
            }
        }

        public async Task<IEnumerable<DashboardPermission>> GetDashboardPermissionsAsync(string dashboardId)
        {
            try
            {
                return await _permissionRepository.GetByDashboardIdAsync(dashboardId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard permissions: {DashboardId}", dashboardId);
                throw;
            }
        }

        public async Task<IEnumerable<DashboardPermission>> GetUserPermissionsAsync(string userId)
        {
            try
            {
                return await _permissionRepository.GetByUserIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user permissions: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> GrantPermissionAsync(string dashboardId, string? userId, string? groupId, string permissionType, string grantedBy)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(groupId))
                {
                    throw new ArgumentException("Either userId or groupId must be provided.");
                }

                if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(groupId))
                {
                    throw new ArgumentException("Cannot grant permission to both user and group simultaneously.");
                }

                // Check if permission already exists
                if (!string.IsNullOrEmpty(userId))
                {
                    var existingUserPermission = await _permissionRepository.GetByUserAndDashboardAsync(userId, dashboardId);
                    if (existingUserPermission != null)
                    {
                        existingUserPermission.PermissionType = permissionType;
                        return await _permissionRepository.UpdateAsync(existingUserPermission);
                    }
                }
                else if (!string.IsNullOrEmpty(groupId))
                {
                    var existingGroupPermission = await _permissionRepository.GetByGroupAndDashboardAsync(groupId, dashboardId);
                    if (existingGroupPermission != null)
                    {
                        existingGroupPermission.PermissionType = permissionType;
                        return await _permissionRepository.UpdateAsync(existingGroupPermission);
                    }
                }

                var permission = new DashboardPermission
                {
                    DashboardId = dashboardId,
                    UserId = userId,
                    GroupId = groupId,
                    PermissionType = permissionType,
                    GrantedBy = grantedBy,
                    GrantedDate = DateTime.UtcNow,
                    IsActive = true
                };

                await _permissionRepository.AddAsync(permission);
                _logger.LogInformation("Permission granted: {PermissionType} on {DashboardId} to {Target} by {GrantedBy}", 
                    permissionType, dashboardId, userId ?? groupId, grantedBy);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error granting permission: {DashboardId}, {UserId}, {GroupId}, {PermissionType}", 
                    dashboardId, userId, groupId, permissionType);
                throw;
            }
        }

        public async Task<bool> GrantUserPermissionAsync(string dashboardId, string userId, string permissionType)
        {
            try
            {
                // Check if permission already exists
                var existingUserPermission = await _permissionRepository.GetByUserAndDashboardAsync(userId, dashboardId);
                if (existingUserPermission != null)
                {
                    existingUserPermission.PermissionType = permissionType;
                    return await _permissionRepository.UpdateAsync(existingUserPermission);
                }

                var permission = new DashboardPermission
                {
                    DashboardId = dashboardId,
                    UserId = userId,
                    PermissionType = permissionType,
                    GrantedBy = "System",
                    GrantedDate = DateTime.UtcNow,
                    IsActive = true
                };

                await _permissionRepository.AddAsync(permission);
                _logger.LogInformation("User permission granted: {PermissionType} on {DashboardId} to {UserId}", 
                    permissionType, dashboardId, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error granting user permission: {DashboardId}, {UserId}, {PermissionType}", 
                    dashboardId, userId, permissionType);
                throw;
            }
        }

        public async Task<bool> GrantGroupPermissionAsync(string dashboardId, string groupId, string permissionType)
        {
            try
            {
                // Check if permission already exists
                var existingGroupPermission = await _permissionRepository.GetByGroupAndDashboardAsync(groupId, dashboardId);
                if (existingGroupPermission != null)
                {
                    existingGroupPermission.PermissionType = permissionType;
                    return await _permissionRepository.UpdateAsync(existingGroupPermission);
                }

                var permission = new DashboardPermission
                {
                    DashboardId = dashboardId,
                    GroupId = groupId,
                    PermissionType = permissionType,
                    GrantedBy = "System",
                    GrantedDate = DateTime.UtcNow,
                    IsActive = true
                };

                await _permissionRepository.AddAsync(permission);
                _logger.LogInformation("Group permission granted: {PermissionType} on {DashboardId} to {GroupId}", 
                    permissionType, dashboardId, groupId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error granting group permission: {DashboardId}, {GroupId}, {PermissionType}", 
                    dashboardId, groupId, permissionType);
                throw;
            }
        }

        public async Task<bool> RevokeUserPermissionAsync(string dashboardId, string userId, string permissionType)
        {
            try
            {
                var permission = await _permissionRepository.GetByUserAndDashboardAsync(userId, dashboardId);
                if (permission == null || permission.PermissionType != permissionType)
                {
                    return false;
                }

                permission.IsActive = false;
                return await _permissionRepository.UpdateAsync(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking user permission: {DashboardId}, {UserId}, {PermissionType}", 
                    dashboardId, userId, permissionType);
                throw;
            }
        }

        public async Task<bool> RevokeGroupPermissionAsync(string dashboardId, string groupId, string permissionType)
        {
            try
            {
                var permission = await _permissionRepository.GetByGroupAndDashboardAsync(groupId, dashboardId);
                if (permission == null || permission.PermissionType != permissionType)
                {
                    return false;
                }

                permission.IsActive = false;
                return await _permissionRepository.UpdateAsync(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking group permission: {DashboardId}, {GroupId}, {PermissionType}", 
                    dashboardId, groupId, permissionType);
                throw;
            }
        }

        public async Task<bool> RevokePermissionAsync(string dashboardId, string? userId, string? groupId, string permissionType)
        {
            try
            {
                bool result = false;

                if (!string.IsNullOrEmpty(userId))
                {
                    result = await _permissionRepository.DeleteByUserAndDashboardAsync(userId, dashboardId);
                }
                else if (!string.IsNullOrEmpty(groupId))
                {
                    result = await _permissionRepository.DeleteByGroupAndDashboardAsync(groupId, dashboardId);
                }

                if (result)
                {
                    _logger.LogInformation("Permission revoked: {PermissionType} on {DashboardId} from {Target}", 
                        permissionType, dashboardId, userId ?? groupId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking permission: {DashboardId}, {UserId}, {GroupId}", 
                    dashboardId, userId, groupId);
                throw;
            }
        }

        public async Task<bool> RevokeAllPermissionsAsync(string dashboardId)
        {
            try
            {
                var result = await _permissionRepository.DeleteByDashboardIdAsync(dashboardId);
                
                if (result)
                {
                    _logger.LogInformation("All permissions revoked for dashboard: {DashboardId}", dashboardId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all permissions: {DashboardId}", dashboardId);
                throw;
            }
        }

        public async Task<bool> UpdatePermissionAsync(string permissionId, string newPermissionType)
        {
            try
            {
                var permission = await _permissionRepository.GetByIdAsync(permissionId);
                if (permission == null)
                {
                    return false;
                }

                permission.PermissionType = newPermissionType;
                return await _permissionRepository.UpdateAsync(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating permission: {PermissionId}", permissionId);
                throw;
            }
        }

        #endregion

        #region Dashboard Data Management

        public async Task<bool> SaveDashboardDataAsync(string dashboardId, string dashboardData, string modifiedBy)
        {
            try
            {
                var dashboard = await _dashboardRepository.GetByIdAsync(dashboardId);
                if (dashboard == null)
                {
                    return false;
                }

                dashboard.DashboardData = dashboardData;
                dashboard.LastModifiedBy = modifiedBy;
                dashboard.LastModifiedDate = DateTime.UtcNow;

                return await _dashboardRepository.UpdateAsync(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving dashboard data: {DashboardId}", dashboardId);
                throw;
            }
        }

        public async Task<string?> GetDashboardDataAsync(string dashboardId)
        {
            try
            {
                var dashboard = await _dashboardRepository.GetByIdAsync(dashboardId);
                return dashboard?.DashboardData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard data: {DashboardId}", dashboardId);
                throw;
            }
        }

        public async Task<bool> CloneDashboardAsync(string sourceDashboardId, string newName, string createdBy)
        {
            try
            {
                var sourceDashboard = await _dashboardRepository.GetByIdAsync(sourceDashboardId);
                if (sourceDashboard == null)
                {
                    return false;
                }

                if (await _dashboardRepository.ExistsAsync(newName))
                {
                    throw new InvalidOperationException("Dashboard with this name already exists.");
                }

                var clonedDashboard = new Dashboard
                {
                    Name = newName,
                    Description = sourceDashboard.Description + " (Copy)",
                    DashboardData = sourceDashboard.DashboardData,
                    CreatedBy = createdBy,
                    Category = sourceDashboard.Category,
                    Tags = new List<string>(sourceDashboard.Tags),
                    IsPublic = false, // Cloned dashboards are private by default
                    IsActive = true
                };

                await _dashboardRepository.AddAsync(clonedDashboard);
                _logger.LogInformation("Dashboard cloned: {SourceId} -> {ClonedId} by {UserId}", 
                    sourceDashboardId, clonedDashboard.Id, createdBy);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cloning dashboard: {SourceDashboardId}", sourceDashboardId);
                throw;
            }
        }

        public async Task<bool> ShareDashboardAsync(string dashboardId, bool isPublic)
        {
            try
            {
                var dashboard = await _dashboardRepository.GetByIdAsync(dashboardId);
                if (dashboard == null)
                {
                    return false;
                }

                dashboard.IsPublic = isPublic;
                dashboard.LastModifiedDate = DateTime.UtcNow;

                return await _dashboardRepository.UpdateAsync(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sharing dashboard: {DashboardId}", dashboardId);
                throw;
            }
        }

        #endregion

        #region Category and Tag Management

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            try
            {
                var dashboards = await _dashboardRepository.GetActiveDashboardsAsync();
                return dashboards.Select(d => d.Category).Distinct().OrderBy(c => c);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetTagsAsync()
        {
            try
            {
                var dashboards = await _dashboardRepository.GetActiveDashboardsAsync();
                return dashboards.SelectMany(d => d.Tags).Distinct().OrderBy(t => t);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tags");
                throw;
            }
        }

        public async Task<bool> UpdateDashboardCategoryAsync(string dashboardId, string category)
        {
            try
            {
                var dashboard = await _dashboardRepository.GetByIdAsync(dashboardId);
                if (dashboard == null)
                {
                    return false;
                }

                dashboard.Category = category;
                dashboard.LastModifiedDate = DateTime.UtcNow;

                return await _dashboardRepository.UpdateAsync(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dashboard category: {DashboardId}", dashboardId);
                throw;
            }
        }

        public async Task<bool> UpdateDashboardTagsAsync(string dashboardId, List<string> tags)
        {
            try
            {
                var dashboard = await _dashboardRepository.GetByIdAsync(dashboardId);
                if (dashboard == null)
                {
                    return false;
                }

                dashboard.Tags = tags;
                dashboard.LastModifiedDate = DateTime.UtcNow;

                return await _dashboardRepository.UpdateAsync(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dashboard tags: {DashboardId}", dashboardId);
                throw;
            }
        }

        public async Task<IEnumerable<DashboardPermission>> GetUserPermissionsAsync(string userId, string dashboardId)
        {
            try
            {
                return await _permissionRepository.GetUserPermissionsAsync(userId, dashboardId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user permissions: {UserId}, {DashboardId}", userId, dashboardId);
                throw;
            }
        }

        public async Task<IEnumerable<DashboardPermission>> GetGroupPermissionsAsync(string groupId, string dashboardId)
        {
            try
            {
                return await _permissionRepository.GetGroupPermissionsAsync(groupId, dashboardId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group permissions: {GroupId}, {DashboardId}", groupId, dashboardId);
                throw;
            }
        }

        public async Task<bool> UpdateUserPermissionAsync(string dashboardId, string userId, string permissionType)
        {
            try
            {
                return await _permissionRepository.UpdateUserPermissionAsync(dashboardId, userId, permissionType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user permission: {DashboardId}, {UserId}, {PermissionType}", dashboardId, userId, permissionType);
                throw;
            }
        }

        public async Task<bool> UpdateGroupPermissionAsync(string dashboardId, string groupId, string permissionType)
        {
            try
            {
                return await _permissionRepository.UpdateGroupPermissionAsync(dashboardId, groupId, permissionType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating group permission: {DashboardId}, {GroupId}, {PermissionType}", dashboardId, groupId, permissionType);
                throw;
            }
        }

        #endregion
    }
}