using DataLens.Models;

namespace DataLens.Data.Interfaces
{
    public interface IDashboardRepository : IRepository<Dashboard>
    {
        Task<IEnumerable<Dashboard>> GetByCreatedByAsync(string createdBy);
        Task<IEnumerable<Dashboard>> GetActiveDashboardsAsync();
        Task<IEnumerable<Dashboard>> GetPublicDashboardsAsync();
        Task<IEnumerable<Dashboard>> GetByCategoryAsync(string category);
        Task<IEnumerable<Dashboard>> GetByTagAsync(string tag);
        Task<IEnumerable<Dashboard>> GetUserAccessibleDashboardsAsync(string userId);
        Task<IEnumerable<Dashboard>> GetGroupAccessibleDashboardsAsync(string groupId);
        Task<bool> UpdateLastModifiedAsync(string dashboardId, string modifiedBy);
        Task<IEnumerable<Dashboard>> SearchDashboardsAsync(string searchTerm);
        Task<bool> ExistsAsync(string name, string? excludeId = null);
        Task<IEnumerable<Dashboard>> SearchAsync(string searchTerm);
        Task<long> CountByUserAsync(string userId);
    }
}