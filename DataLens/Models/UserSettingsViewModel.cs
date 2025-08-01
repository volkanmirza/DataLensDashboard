using System.ComponentModel.DataAnnotations;

namespace DataLens.Models
{
    public class UserSettingsViewModel
    {
        public string Id { get; set; } = string.Empty;
        
        public string UserId { get; set; } = string.Empty;
        
        // General Settings
        [Display(Name = "Dil")]
        [StringLength(10)]
        public string Language { get; set; } = "tr";
        
        [Display(Name = "Tema")]
        [StringLength(20)]
        public string Theme { get; set; } = "light";
        
        [Display(Name = "Saat Dilimi")]
        [StringLength(50)]
        public string TimeZone { get; set; } = "Europe/Istanbul";
        
        [Display(Name = "Tarih Formatı")]
        [StringLength(20)]
        public string DateFormat { get; set; } = "dd/MM/yyyy";
        
        // Notification Settings
        [Display(Name = "E-posta Bildirimleri")]
        public bool EmailNotifications { get; set; } = true;
        
        [Display(Name = "Tarayıcı Bildirimleri")]
        public bool BrowserNotifications { get; set; } = false;
        
        [Display(Name = "Push Bildirimleri")]
        public bool PushNotifications { get; set; } = false;
        
        [Display(Name = "Dashboard Bildirimleri")]
        public bool DashboardNotifications { get; set; } = true;
        
        [Display(Name = "Sistem Bildirimleri")]
        public bool SystemNotifications { get; set; } = true;
        
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
        
        // Privacy Settings
        [Display(Name = "Profil Görünürlüğü")]
        [StringLength(20)]
        public string ProfileVisibility { get; set; } = "private";
        
        [Display(Name = "E-posta Adresini Göster")]
        public bool ShowEmail { get; set; } = false;
        
        [Display(Name = "Son Giriş Tarihini Göster")]
        public bool ShowLastLogin { get; set; } = false;
        
        [Display(Name = "Veri Dışa Aktarımına İzin Ver")]
        public bool AllowDataExport { get; set; } = true;
        
        [Display(Name = "Veri Silmeye İzin Ver")]
        public bool AllowDataDeletion { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        public string UpdatedBy { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        
        // Helper properties for display
        public List<SelectListItem> LanguageOptions => new List<SelectListItem>
        {
            new SelectListItem { Value = "tr", Text = "Türkçe" },
            new SelectListItem { Value = "en", Text = "English" }
        };
        
        public List<SelectListItem> ThemeOptions => new List<SelectListItem>
        {
            new SelectListItem { Value = "light", Text = "Açık Tema" },
            new SelectListItem { Value = "dark", Text = "Koyu Tema" },
            new SelectListItem { Value = "auto", Text = "Otomatik" }
        };
        
        public List<SelectListItem> TimeZoneOptions => new List<SelectListItem>
        {
            new SelectListItem { Value = "Europe/Istanbul", Text = "İstanbul (UTC+3)" },
            new SelectListItem { Value = "UTC", Text = "UTC (UTC+0)" },
            new SelectListItem { Value = "Europe/London", Text = "Londra (UTC+0)" },
            new SelectListItem { Value = "America/New_York", Text = "New York (UTC-5)" }
        };
        
        public List<SelectListItem> DateFormatOptions => new List<SelectListItem>
        {
            new SelectListItem { Value = "dd/MM/yyyy", Text = "31/12/2023" },
            new SelectListItem { Value = "MM/dd/yyyy", Text = "12/31/2023" },
            new SelectListItem { Value = "yyyy-MM-dd", Text = "2023-12-31" },
            new SelectListItem { Value = "dd.MM.yyyy", Text = "31.12.2023" }
        };
        
        public List<SelectListItem> ProfileVisibilityOptions => new List<SelectListItem>
        {
            new SelectListItem { Value = "private", Text = "Özel" },
            new SelectListItem { Value = "public", Text = "Genel" },
            new SelectListItem { Value = "friends", Text = "Arkadaşlar" }
        };
    }
    
    public class SelectListItem
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}