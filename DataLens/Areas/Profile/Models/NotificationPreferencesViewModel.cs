using System.ComponentModel.DataAnnotations;

namespace DataLens.Areas.Profile.Models
{
    public class NotificationPreferencesViewModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Display(Name = "E-posta Bildirimleri")]
        public bool EmailNotifications { get; set; } = true;
        
        [Display(Name = "Tarayıcı Bildirimleri")]
        public bool BrowserNotifications { get; set; } = false;
        
        [Display(Name = "Dashboard Paylaşım Bildirimleri")]
        public bool DashboardShared { get; set; } = true;
        
        [Display(Name = "Dashboard Güncelleme Bildirimleri")]
        public bool DashboardUpdated { get; set; } = true;
        
        [Display(Name = "İzin Değişikliği Bildirimleri")]
        public bool PermissionChanged { get; set; } = true;
        
        [Display(Name = "Sistem Güncellemeleri")]
        public bool SystemUpdates { get; set; } = false;
        
        [Display(Name = "Güvenlik Uyarıları")]
        public bool SecurityAlerts { get; set; } = true;
        
        [Display(Name = "Haftalık Özet")]
        public bool WeeklyDigest { get; set; } = true;
        
        [Display(Name = "Aylık Rapor")]
        public bool MonthlyReport { get; set; } = false;
        
        // Helper properties
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    }
}