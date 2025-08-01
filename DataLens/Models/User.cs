using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataLens.Models
{
    public class User : IdentityUser
    {
        public override string Id { get; set; } = string.Empty;
        
        public override string? UserName { get; set; }
        
        public override string? Email { get; set; }
        
        public override string? NormalizedUserName { get; set; }
        
        public override string? NormalizedEmail { get; set; }
        
        public override string? PasswordHash { get; set; }
        
        public override string? SecurityStamp { get; set; }
        
        public override string? ConcurrencyStamp { get; set; }
        
        [Required]
        [StringLength(20)]
        [Display(Name = "Rol")]
        public string Role { get; set; } = "Viewer"; // Viewer, Designer, Admin

        [BsonElement("CreatedDate")]
        [Display(Name = "Kayıt Tarihi")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Güncelleme Tarihi")]
        public DateTime? UpdatedDate { get; set; }

        [BsonElement("LastLoginDate")]
        [Display(Name = "Son Giriş Tarihi")]
        public DateTime? LastLoginDate { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        [Required]
        [StringLength(100)]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Ad Soyad")]
        public string FullName => $"{FirstName} {LastName}".Trim();
        
        // Profile picture path
        [StringLength(500)]
        [Display(Name = "Profil Resmi")]
        public string? ProfilePicturePath { get; set; }
        
        // Additional profile information
        [StringLength(500)]
        [Display(Name = "Biyografi")]
        public string? Biography { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Departman")]
        public string? Department { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Pozisyon")]
        public string? Position { get; set; }
        
        [StringLength(20)]
        [Display(Name = "Telefon")]
        public override string? PhoneNumber { get; set; }
        
        // User preferences that should be part of User entity
        [StringLength(10)]
        [Display(Name = "Dil")]
        public string Language { get; set; } = "tr";
        
        [StringLength(20)]
        [Display(Name = "Tema")]
        public string Theme { get; set; } = "light";
        
        [StringLength(50)]
        [Display(Name = "Saat Dilimi")]
        public string TimeZone { get; set; } = "Europe/Istanbul";
        
        // Email verification
        [Display(Name = "E-posta Doğrulandı")]
        public override bool EmailConfirmed { get; set; } = false;
        
        // Two factor authentication
        [Display(Name = "İki Faktörlü Kimlik Doğrulama")]
        public override bool TwoFactorEnabled { get; set; } = false;
        
        // Account lockout
        [Display(Name = "Hesap Kilitli")]
        public override bool LockoutEnabled { get; set; } = false;
        
        [Display(Name = "Kilit Bitiş Tarihi")]
        public override DateTimeOffset? LockoutEnd { get; set; }
        
        [Display(Name = "Başarısız Giriş Sayısı")]
        public override int AccessFailedCount { get; set; } = 0;
        
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
        [StringLength(20)]
        [Display(Name = "Profil Görünürlüğü")]
        public string ProfileVisibility { get; set; } = "private";
        
        [Display(Name = "E-posta Göster")]
        public bool ShowEmail { get; set; } = false;
        
        [Display(Name = "Son Giriş Göster")]
        public bool ShowLastLogin { get; set; } = false;
        
        [Display(Name = "Veri Dışa Aktarımına İzin Ver")]
        public bool AllowDataExport { get; set; } = true;
        
        [Display(Name = "Veri Silmeye İzin Ver")]
        public bool AllowDataDeletion { get; set; } = true;
        
        [Display(Name = "Dashboard Paylaşımına İzin Ver")]
        public bool AllowDashboardSharing { get; set; } = true;
        
        [Display(Name = "Aktivite Takibine İzin Ver")]
        public bool TrackActivity { get; set; } = true;
        
        // Password change properties (not stored in database)
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string? CurrentPassword { get; set; }
        
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string? NewPassword { get; set; }
        
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre Tekrar")]
        public string? ConfirmPassword { get; set; }
    }
}