using DataLens.Models;

namespace DataLens.Services.Interfaces
{
    public interface IDashboardService
    {
        // Dashboard CRUD Operations
        Task<IEnumerable<Dashboard>> GetAllDashboardsAsync();
        Task<Dashboard?> GetDashboardByIdAsync(string id);
        Task<IEnumerable<Dashboard>> GetDashboardsByUserAsync(string userId);
        Task<IEnumerable<Dashboard>> GetDashboardsByCategoryAsync(string category);
        Task<IEnumerable<Dashboard>> GetPublicDashboardsAsync();
        Task<IEnumerable<Dashboard>> GetUserAccessibleDashboardsAsync(string userId);
        Task<string> CreateDashboardAsync(Dashboard dashboard);
        Task<bool> UpdateDashboardAsync(Dashboard dashboard);
        Task<bool> DeleteDashboardAsync(string id);
        Task<bool> IsDashboardNameExistsAsync(string name, string? excludeId = null);
        Task<IEnumerable<Dashboard>> SearchDashboardsAsync(string searchTerm);
        Task<long> GetDashboardCountAsync();
        Task<long> GetUserDashboardCountAsync(string userId);

        // Permission Management
        Task<bool> HasPermissionAsync(string userId, string dashboardId, string permissionType);
        Task<bool> HasUserPermissionAsync(string dashboardId, string userId, string permissionType);
        Task<bool> HasGroupPermissionAsync(string groupId, string dashboardId, string permissionType);
        Task<bool> CanUserAccessDashboardAsync(string userId, string dashboardId, string permissionType = PermissionTypes.View);
        Task<IEnumerable<DashboardPermission>> GetDashboardPermissionsAsync(string dashboardId);
        Task<IEnumerable<DashboardPermission>> GetUserPermissionsAsync(string userId);
        Task<IEnumerable<DashboardPermission>> GetUserPermissionsAsync(string userId, string dashboardId);
        Task<IEnumerable<DashboardPermission>> GetGroupPermissionsAsync(string groupId, string dashboardId);
        Task<bool> GrantPermissionAsync(string dashboardId, string? userId, string? groupId, string permissionType, string grantedBy);
        Task<bool> GrantUserPermissionAsync(string dashboardId, string userId, string permissionType);
        Task<bool> GrantGroupPermissionAsync(string dashboardId, string groupId, string permissionType);
        Task<bool> RevokeUserPermissionAsync(string dashboardId, string userId, string permissionType);
        Task<bool> RevokeGroupPermissionAsync(string dashboardId, string groupId, string permissionType);
        Task<bool> RevokePermissionAsync(string dashboardId, string? userId, string? groupId, string permissionType);
        Task<bool> RevokeAllPermissionsAsync(string dashboardId);
        Task<bool> UpdatePermissionAsync(string permissionId, string newPermissionType);
        Task<bool> UpdateUserPermissionAsync(string dashboardId, string userId, string permissionType);
        Task<bool> UpdateGroupPermissionAsync(string dashboardId, string groupId, string permissionType);

        // Dashboard Data Management
        Task<bool> SaveDashboardDataAsync(string dashboardId, string dashboardData, string modifiedBy);
        Task<string?> GetDashboardDataAsync(string dashboardId);
        Task<bool> CloneDashboardAsync(string sourceDashboardId, string newName, string createdBy);
        Task<bool> ShareDashboardAsync(string dashboardId, bool isPublic);

        // Category and Tag Management
        Task<IEnumerable<string>> GetCategoriesAsync();
        Task<IEnumerable<string>> GetTagsAsync();
        Task<bool> UpdateDashboardCategoryAsync(string dashboardId, string category);
        Task<bool> UpdateDashboardTagsAsync(string dashboardId, List<string> tags);
    }
}