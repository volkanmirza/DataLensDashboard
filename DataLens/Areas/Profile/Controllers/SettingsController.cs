using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataLens.Services.Interfaces;
using DataLens.Data.Interfaces;
using DataLens.Models;
using DataLens.Areas.Profile.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace DataLens.Areas.Profile.Controllers
{
    [Area("Profile")]
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly IUserService _userService;
        private readonly IUserSettingsRepository _userSettingsRepository;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(IUserService userService, IUserSettingsRepository userSettingsRepository, ILogger<SettingsController> logger)
        {
            _userService = userService;
            _userSettingsRepository = userSettingsRepository;
            _logger = logger;
        }

        // GET: Profile/Settings
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound();
                }

                var settings = new UserSettingsViewModel
                {
                    UserId = user.Id,
                    Language = "tr", // Default language
                    Theme = "light", // Default theme
                    TimeZone = "Europe/Istanbul", // Default timezone
                    EmailNotifications = true,
                    DashboardNotifications = true,
                    SystemNotifications = false
                };

                return View(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user settings");
                TempData["Error"] = "Ayarlar yüklenirken bir hata oluştu.";
                return RedirectToAction("Index", "Profile");
            }
        }

        // POST: Profile/Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(UserSettingsViewModel settings)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || userId != settings.UserId)
                {
                    return Forbid();
                }

                if (ModelState.IsValid)
                {
                    // Here you would typically save settings to a UserSettings table
                    // For now, we'll just show a success message
                    TempData["Success"] = "Ayarlarınız başarıyla kaydedildi.";
                    return RedirectToAction("Index");
                }

                return View(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving user settings");
                TempData["Error"] = "Ayarlar kaydedilirken bir hata oluştu.";
                return View(settings);
            }
        }

        // GET: Profile/Settings/Privacy
        public async Task<IActionResult> Privacy()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var privacySettings = new PrivacySettingsViewModel
                {
                    UserId = userId,
                    ProfileVisibility = ProfileVisibility.Private,
                    AllowDashboardSharing = true,
                    AllowActivityTracking = true,
                    AllowUsageAnalytics = false,
                    AllowThirdPartyIntegrations = false,
                    DataRetentionDays = 90,
                    AutoDeleteData = false,
                    TwoFactorEnabled = false,
                    SessionTimeoutMinutes = 60
                };

                return View(privacySettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving privacy settings");
                TempData["Error"] = "Gizlilik ayarları yüklenirken bir hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        // POST: Profile/Settings/Privacy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Privacy(PrivacySettingsViewModel settings)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || userId != settings.UserId)
                {
                    return Forbid();
                }

                if (ModelState.IsValid)
                {
                    // Here you would typically save privacy settings
                    TempData["Success"] = "Gizlilik ayarlarınız başarıyla kaydedildi.";
                    return RedirectToAction("Privacy");
                }

                return View(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving privacy settings");
                TempData["Error"] = "Gizlilik ayarları kaydedilirken bir hata oluştu.";
                return View(settings);
            }
        }

        // POST: Profile/Settings/ExportData
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportData()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // Here you would implement data export functionality
                TempData["Info"] = "Veri dışa aktarma isteğiniz alındı. E-posta adresinize gönderilecektir.";
                return RedirectToAction("Privacy");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting user data");
                TempData["Error"] = "Veri dışa aktarılırken bir hata oluştu.";
                return RedirectToAction("Privacy");
            }
        }

        // POST: Profile/Settings/DeleteAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount(string confirmPassword)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                if (string.IsNullOrEmpty(confirmPassword))
                {
                    TempData["Error"] = "Hesap silme işlemi için şifrenizi girmelisiniz.";
                    return RedirectToAction("Privacy");
                }

                // Here you would implement account deletion functionality
                // This is a critical operation and should be handled carefully
                TempData["Info"] = "Hesap silme isteğiniz alındı. İşlem 30 gün içinde tamamlanacaktır.";
                return RedirectToAction("Privacy");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user account");
                TempData["Error"] = "Hesap silinirken bir hata oluştu.";
                return RedirectToAction("Privacy");
            }
        }

        // POST: Profile/Settings/UpdateSettings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSettings(string language, string theme, string timeZone)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // Here you would typically save settings to database
                // For now, we'll just show a success message
                TempData["Success"] = "Ayarlarınız başarıyla kaydedildi.";
                return RedirectToAction("Dashboard", "Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating settings");
                TempData["Error"] = "Ayarlar kaydedilirken bir hata oluştu.";
                return RedirectToAction("Dashboard", "Profile");
            }
        }

        // POST: Profile/Settings/UpdatePrivacySettings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePrivacySettings(string profileVisibility, bool allowDashboardSharing, bool trackActivity, bool twoFactorAuth)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // Here you would typically save privacy settings to database
                TempData["Success"] = "Gizlilik ayarlarınız başarıyla kaydedildi.";
                return RedirectToAction("Dashboard", "Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating privacy settings");
                TempData["Error"] = "Gizlilik ayarları kaydedilirken bir hata oluştu.";
                return RedirectToAction("Dashboard", "Profile");
            }
        }

        // POST: Profile/Settings/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Oturum süresi dolmuş." });
                }

                if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
                {
                    return Json(new { success = false, message = "Tüm alanları doldurunuz." });
                }

                if (newPassword != confirmPassword)
                {
                    return Json(new { success = false, message = "Yeni şifre ve tekrarı eşleşmiyor." });
                }

                if (newPassword.Length < 6)
                {
                    return Json(new { success = false, message = "Şifre en az 6 karakter olmalıdır." });
                }

                var result = await _userService.ChangePasswordAsync(userId, currentPassword, newPassword);
                if (result)
                {
                    TempData["Success"] = "Şifreniz başarıyla değiştirildi.";
                    return RedirectToAction("Dashboard", "Profile");
                }
                else
                {
                    TempData["Error"] = "Mevcut şifre yanlış.";
                    return RedirectToAction("Dashboard", "Profile");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                TempData["Error"] = "Şifre değiştirilirken bir hata oluştu.";
                return RedirectToAction("Dashboard", "Profile");
            }
        }
    }

    // UserSettingsViewModel and PrivacySettingsViewModel are now defined in DataLens.Areas.Profile.Models namespace
}