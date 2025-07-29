using DataLens.Models;

namespace DataLens.Data.Interfaces
{
    public interface IDashboardPermissionRepository : IRepository<DashboardPermission>
    {
        Task<IEnumerable<DashboardPermission>> GetByDashboardIdAsync(string dashboardId);
        Task<IEnumerable<DashboardPermission>> GetByUserIdAsync(string userId);
        Task<IEnumerable<DashboardPermission>> GetByGroupIdAsync(string groupId);
        Task<DashboardPermission?> GetUserPermissionAsync(string dashboardId, string userId);
        Task<DashboardPermission?> GetGroupPermissionAsync(string dashboardId, string groupId);
        Task<bool> HasPermissionAsync(string dashboardId, string userId, string permissionType);
        Task<bool> HasGroupPermissionAsync(string dashboardId, string groupId, string permissionType);
        Task<IEnumerable<DashboardPermission>> GetActivePermissionsAsync();
        Task<bool> RevokeUserPermissionAsync(string dashboardId, string userId);
        Task<bool> RevokeGroupPermissionAsync(string dashboardId, string groupId);
        Task<bool> RevokeAllDashboardPermissionsAsync(string dashboardId);
        Task<bool> DeleteByDashboardIdAsync(string dashboardId);
        Task<bool> DeleteByUserAndDashboardAsync(string userId, string dashboardId);
        Task<bool> DeleteByGroupAndDashboardAsync(string groupId, string dashboardId);
        Task<DashboardPermission?> GetByUserAndDashboardAsync(string userId, string dashboardId);
        Task<DashboardPermission?> GetByGroupAndDashboardAsync(string groupId, string dashboardId);
        Task<IEnumerable<DashboardPermission>> GetUserPermissionsAsync(string userId, string dashboardId);
        Task<IEnumerable<DashboardPermission>> GetGroupPermissionsAsync(string groupId, string dashboardId);
        Task<bool> UpdateUserPermissionAsync(string dashboardId, string userId, string permissionType);
        Task<bool> UpdateGroupPermissionAsync(string dashboardId, string groupId, string permissionType);
    }
}