using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataLens.Services.Interfaces;
using DataLens.Data.Interfaces;
using DataLens.Models;
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
                    ProfileVisibility = "private",
                    ShowEmail = false,
                    ShowLastLogin = false,
                    AllowDataExport = true,
                    AllowDataDeletion = true
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
    }

    public class UserSettingsViewModel
    {
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "Dil")]
        public string Language { get; set; } = "tr";

        [Display(Name = "Tema")]
        public string Theme { get; set; } = "light";

        [Display(Name = "Saat Dilimi")]
        public string TimeZone { get; set; } = "Europe/Istanbul";

        [Display(Name = "E-posta Bildirimleri")]
        public bool EmailNotifications { get; set; }

        [Display(Name = "Dashboard Bildirimleri")]
        public bool DashboardNotifications { get; set; }

        [Display(Name = "Sistem Bildirimleri")]
        public bool SystemNotifications { get; set; }
    }

    public class PrivacySettingsViewModel
    {
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "Profil Görünürlüğü")]
        public string ProfileVisibility { get; set; } = "private";

        [Display(Name = "E-posta Adresini Göster")]
        public bool ShowEmail { get; set; }

        [Display(Name = "Son Giriş Tarihini Göster")]
        public bool ShowLastLogin { get; set; }

        [Display(Name = "Veri Dışa Aktarmaya İzin Ver")]
        public bool AllowDataExport { get; set; }

        [Display(Name = "Veri Silmeye İzin Ver")]
        public bool AllowDataDeletion { get; set; }
    }
}