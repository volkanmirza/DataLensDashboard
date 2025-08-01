using System.ComponentModel.DataAnnotations;

namespace DataLens.Areas.Profile.Models
{
    public class PrivacySettingsViewModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Display(Name = "Profil Görünürlüğü")]
        public ProfileVisibility ProfileVisibility { get; set; } = ProfileVisibility.Private;
        
        [Display(Name = "Dashboard Paylaşım İzni")]
        public bool AllowDashboardSharing { get; set; } = true;
        
        [Display(Name = "Aktivite Takibi")]
        public bool AllowActivityTracking { get; set; } = true;
        
        [Display(Name = "Kullanım Analizi")]
        public bool AllowUsageAnalytics { get; set; } = false;
        
        [Display(Name = "Üçüncü Taraf Entegrasyonları")]
        public bool AllowThirdPartyIntegrations { get; set; } = false;
        
        [Display(Name = "Veri Saklama Süresi (Gün)")]
        [Range(30, 365, ErrorMessage = "Veri saklama süresi 30-365 gün arasında olmalıdır.")]
        public int DataRetentionDays { get; set; } = 90;
        
        [Display(Name = "Otomatik Veri Silme")]
        public bool AutoDeleteData { get; set; } = false;
        
        [Display(Name = "İki Faktörlü Kimlik Doğrulama")]
        public bool TwoFactorEnabled { get; set; } = false;
        
        [Display(Name = "Oturum Zaman Aşımı (Dakika)")]
        [Range(15, 480, ErrorMessage = "Oturum zaman aşımı 15-480 dakika arasında olmalıdır.")]
        public int SessionTimeoutMinutes { get; set; } = 60;
        
        // Helper properties
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        
        // Data export related
        public bool DataExportRequested { get; set; } = false;
        public DateTime? LastDataExportDate { get; set; }
        
        // Account deletion related
        public bool AccountDeletionRequested { get; set; } = false;
        public DateTime? AccountDeletionRequestDate { get; set; }
    }
    
    public enum ProfileVisibility
    {
        [Display(Name = "Özel")]
        Private = 0,
        
        [Display(Name = "Takım")]
        Team = 1,
        
        [Display(Name = "Organizasyon")]
        Organization = 2,
        
        [Display(Name = "Herkese Açık")]
        Public = 3
    }
}