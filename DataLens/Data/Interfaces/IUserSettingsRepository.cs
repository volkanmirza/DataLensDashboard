using DataLens.Models;

namespace DataLens.Data.Interfaces
{
    public interface IUserSettingsRepository : IRepository<UserSettings>
    {
        Task<UserSettings?> GetByUserIdAsync(string userId);
        Task<bool> UpdateNotificationSettingsAsync(string userId, Dictionary<string, bool> settings);
        Task<bool> UpdatePrivacySettingsAsync(string userId, Dictionary<string, object> settings);
        Task<bool> UpdateGeneralSettingsAsync(string userId, Dictionary<string, string> settings);
        Task<UserSettings> CreateDefaultSettingsAsync(string userId);
        Task<bool> ResetToDefaultAsync(string userId);
        Task<IEnumerable<UserSettings>> GetByLanguageAsync(string language);
        Task<IEnumerable<UserSettings>> GetByThemeAsync(string theme);
        Task<bool> BulkUpdateSettingAsync(string settingName, object value);
    }
}