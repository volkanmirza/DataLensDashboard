using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataLens.Models
{
    public class UserSettings
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [Required]
        public string UserId { get; set; } = string.Empty;

        // General Settings
        [StringLength(10)]
        public string Language { get; set; } = "tr";

        [StringLength(20)]
        public string Theme { get; set; } = "light";

        [StringLength(50)]
        public string TimeZone { get; set; } = "Europe/Istanbul";

        [StringLength(20)]
        public string DateFormat { get; set; } = "dd/MM/yyyy";

        // Notification Settings
        public bool EmailNotifications { get; set; } = true;
        public bool BrowserNotifications { get; set; } = false;
        public bool PushNotifications { get; set; } = false;
        public bool DashboardNotifications { get; set; } = true;
        public bool SystemNotifications { get; set; } = true;
        public bool DashboardShared { get; set; } = true;
        public bool DashboardUpdated { get; set; } = true;
        public bool PermissionChanged { get; set; } = true;
        public bool SystemUpdates { get; set; } = false;
        public bool SecurityAlerts { get; set; } = true;
        public bool WeeklyDigest { get; set; } = true;
        public bool MonthlyReport { get; set; } = false;

        // Privacy Settings
        [StringLength(20)]
        public string ProfileVisibility { get; set; } = "private";
        public bool ShowEmail { get; set; } = false;
        public bool ShowLastLogin { get; set; } = false;
        public bool AllowDataExport { get; set; } = true;
        public bool AllowDataDeletion { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        public string UpdatedBy { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}