using DataLens.Models;

namespace DataLens.Data.Interfaces
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(string userId);
        Task<IEnumerable<Notification>> GetByTypeAsync(string userId, string type);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> MarkAsReadAsync(string notificationId, string userId);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<bool> DeleteByUserIdAsync(string notificationId, string userId);
        Task<IEnumerable<Notification>> GetActiveNotificationsAsync(string userId);
        Task<IEnumerable<Notification>> GetByRelatedEntityAsync(string relatedEntityId, string relatedEntityType);
        Task<bool> CreateBulkAsync(IEnumerable<Notification> notifications);
        Task<bool> DeleteOldNotificationsAsync(DateTime cutoffDate);
    }
}