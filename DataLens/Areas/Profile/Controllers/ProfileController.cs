using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataLens.Services.Interfaces;
using DataLens.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using DataLens.Data.Interfaces;

namespace DataLens.Areas.Profile.Controllers
{
    [Area("Profile")]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserSettingsRepository _userSettingsRepository;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IUserService userService, INotificationRepository notificationRepository, IUserSettingsRepository userSettingsRepository, ILogger<ProfileController> logger)
        {
            _userService = userService;
            _notificationRepository = notificationRepository;
            _userSettingsRepository = userSettingsRepository;
            _logger = logger;
        }

        // GET: Profile
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in claims for profile index");
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for ID: {UserId}", userId);
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Inactive user {UserId} attempted to access profile", userId);
                    return RedirectToAction("AccessDenied", "Account", new { area = "" });
                }

                // Get notifications for the user
                var notifications = await _notificationRepository.GetByUserIdAsync(userId);
                var notificationViewModels = notifications.Select(n => new DataLens.Models.NotificationViewModel
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedDate = n.CreatedDate
                }).ToList();

                ViewBag.Notifications = notificationViewModels;

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                TempData["Error"] = "Profil bilgileri alınırken bir hata oluştu.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        // GET: Profile/Edit
        public async Task<IActionResult> Edit()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in claims for profile edit");
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for ID: {UserId} during edit", userId);
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Inactive user {UserId} attempted to edit profile", userId);
                    return RedirectToAction("AccessDenied", "Account", new { area = "" });
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user for edit, user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                TempData["Error"] = "Profil düzenleme sayfası yüklenirken bir hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User model)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in claims for profile edit");
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // Get the current user from database
                var currentUser = await _userService.GetByIdAsync(userId);
                if (currentUser == null)
                {
                    _logger.LogWarning("Current user not found for ID: {UserId}", userId);
                    return NotFound();
                }

                // Security check: Ensure user can only edit their own profile
                if (model.Id != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to edit profile of user {TargetUserId}", userId, model.Id);
                    return RedirectToAction("AccessDenied", "Account", new { area = "" });
                }

                // Validate the model
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Check if email is already taken by another user
                var existingUser = await _userService.GetByEmailAsync(model.Email);
                if (existingUser != null && existingUser.Id != userId)
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
                    return View(model);
                }

                // Update only the allowed fields
                currentUser.FirstName = model.FirstName;
                currentUser.LastName = model.LastName;
                currentUser.Email = model.Email;
                currentUser.Department = model.Department;
                currentUser.Position = model.Position;
                currentUser.PhoneNumber = model.PhoneNumber;
                currentUser.Biography = model.Biography;
                currentUser.UpdatedDate = DateTime.UtcNow;

                var updateResult = await _userService.UpdateUserAsync(currentUser);
                if (updateResult)
                {
                    TempData["Success"] = "Profil bilgileriniz başarıyla güncellendi.";
                    _logger.LogInformation("User {UserId} successfully updated their profile", userId);
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Profil güncellenirken bir hata oluştu.";
                    _logger.LogError("Failed to update profile for user {UserId}", userId);
                    ModelState.AddModelError("", "Profil güncellenirken bir hata oluştu.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                TempData["Error"] = "Profil güncellenirken bir hata oluştu.";
                ModelState.AddModelError("", "Profil güncellenirken bir hata oluştu.");
                return View(model);
            }
        }

        // GET: Profile/Dashboard
        //public async Task<IActionResult> Dashboard()
        //{
        //    try
        //    {
        //        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(userId))
        //        {
        //            _logger.LogWarning("User ID not found in claims for dashboard access");
        //            return RedirectToAction("Login", "Account", new { area = "" });
        //        }

        //        var user = await _userService.GetByIdAsync(userId);
        //        if (user == null)
        //        {
        //            _logger.LogWarning("User not found for ID: {UserId} during dashboard access", userId);
        //            return RedirectToAction("Login", "Account", new { area = "" });
        //        }

        //        // Check if user is active
        //        if (!user.IsActive)
        //        {
        //            _logger.LogWarning("Inactive user {UserId} attempted to access dashboard", userId);
        //            return RedirectToAction("AccessDenied", "Account", new { area = "" });
        //        }

        //        return View(user);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error loading dashboard for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        //        TempData["Error"] = "Dashboard yüklenirken bir hata oluştu.";
        //        return RedirectToAction("Index", "Home", new { area = "" });
        //    }
        //}

        // GET: Profile/ChangePassword
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(User model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.CurrentPassword) || string.IsNullOrEmpty(model.NewPassword) || string.IsNullOrEmpty(model.ConfirmPassword))
                {
                    TempData["Error"] = "Tüm şifre alanları doldurulmalıdır.";
                    return RedirectToAction("Index");
                }

                if (model.NewPassword != model.ConfirmPassword)
                {
                    TempData["Error"] = "Yeni şifre ve tekrarı eşleşmiyor.";
                    return RedirectToAction("Index");
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in claims for password change");
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // Verify user exists and is active
                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for ID: {UserId} during password change", userId);
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Inactive user {UserId} attempted to change password", userId);
                    return RedirectToAction("AccessDenied", "Account", new { area = "" });
                }

                var result = await _userService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
                if (result)
                {
                    _logger.LogInformation("Password changed successfully for user {UserId}", userId);
                    TempData["Success"] = "Şifreniz başarıyla değiştirildi.";
                    return RedirectToAction("Index");
                }
                else
                {
                    _logger.LogWarning("Password change failed for user {UserId} - incorrect current password", userId);
                    TempData["Error"] = "Mevcut şifre yanlış.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                ModelState.AddModelError("", "Şifre değiştirme işlemi sırasında bir hata oluştu.");
                return View(model);
            }
        }

        // ===== NOTIFICATION METHODS =====

        // POST: Profile/UpdateNotificationPreferences
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNotificationPreferences(User model)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var currentUser = await _userService.GetByIdAsync(userId);
                if (currentUser == null)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("Index");
                }

                // Update notification preferences
                currentUser.EmailNotifications = model.EmailNotifications;
                currentUser.BrowserNotifications = model.BrowserNotifications;
                currentUser.DashboardShared = model.DashboardShared;
                currentUser.SecurityAlerts = model.SecurityAlerts;
                currentUser.UpdatedDate = DateTime.UtcNow;

                var updateResult = await _userService.UpdateUserAsync(currentUser);
                if (updateResult)
                {
                    _logger.LogInformation("Notification preferences updated successfully for user {UserId}", userId);
                    TempData["Success"] = "Bildirim tercihleriniz başarıyla kaydedildi.";
                }
                else
                {
                    TempData["Error"] = "Bildirim tercihleri kaydedilirken bir hata oluştu.";
                }
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification preferences for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                TempData["Error"] = "Bildirim tercihleri kaydedilirken bir hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        // POST: Profile/MarkAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> MarkAsRead(string notificationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Task.FromResult<IActionResult>(Json(new { success = false, message = "Kullanıcı bulunamadı." }));
                }

                if (string.IsNullOrEmpty(notificationId))
                {
                    return Task.FromResult<IActionResult>(Json(new { success = false, message = "Bildirim ID'si gereklidir." }));
                }

                // Here you would typically mark notification as read in database
                return Task.FromResult<IActionResult>(Json(new { success = true, message = "Bildirim okundu olarak işaretlendi." }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return Task.FromResult<IActionResult>(Json(new { success = false, message = "Bildirim işaretlenirken bir hata oluştu." }));
            }
        }

        // POST: Profile/MarkAllAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Task.FromResult<IActionResult>(Json(new { success = false, message = "Kullanıcı bulunamadı." }));
                }

                // Here you would typically mark all notifications as read in database
                return Task.FromResult<IActionResult>(Json(new { success = true, message = "Tüm bildirimler okundu olarak işaretlendi." }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return Task.FromResult<IActionResult>(Json(new { success = false, message = "Bildirimler işaretlenirken bir hata oluştu." }));
            }
        }

        // POST: Profile/DeleteNotification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> DeleteNotification(string notificationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Task.FromResult<IActionResult>(Json(new { success = false, message = "Kullanıcı bulunamadı." }));
                }

                if (string.IsNullOrEmpty(notificationId))
                {
                    return Task.FromResult<IActionResult>(Json(new { success = false, message = "Bildirim ID'si gereklidir." }));
                }

                // Here you would typically delete notification from database
                return Task.FromResult<IActionResult>(Json(new { success = true, message = "Bildirim silindi." }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                return Task.FromResult<IActionResult>(Json(new { success = false, message = "Bildirim silinirken bir hata oluştu." }));
            }
        }

        // GET: Profile/GetUnreadCount
        public Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Task.FromResult<IActionResult>(Json(new { count = 0 }));
                }

                // Here you would typically get unread count from database
                var unreadCount = 2; // Mock data
                return Task.FromResult<IActionResult>(Json(new { count = unreadCount }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notification count");
                return Task.FromResult<IActionResult>(Json(new { count = 0 }));
            }
        }

        // ===== SETTINGS METHODS =====

        // POST: Profile/UpdateSettings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSettings(User model)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var currentUser = await _userService.GetByIdAsync(userId);
                if (currentUser == null)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("Index");
                }

                // Update settings
                currentUser.Language = model.Language;
                currentUser.Theme = model.Theme;
                currentUser.TimeZone = model.TimeZone;
                currentUser.UpdatedDate = DateTime.UtcNow;

                var updateResult = await _userService.UpdateUserAsync(currentUser);
                if (updateResult)
                {
                    _logger.LogInformation("Settings updated successfully for user {UserId}", userId);
                    TempData["Success"] = "Ayarlarınız başarıyla kaydedildi.";
                }
                else
                {
                    TempData["Error"] = "Ayarlar kaydedilirken bir hata oluştu.";
                }
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating settings for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                TempData["Error"] = "Ayarlar kaydedilirken bir hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        // POST: Profile/UpdatePrivacySettings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePrivacySettings(User model)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var currentUser = await _userService.GetByIdAsync(userId);
                if (currentUser == null)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("Index");
                }

                // Update privacy settings
                currentUser.ProfileVisibility = model.ProfileVisibility;
                currentUser.AllowDashboardSharing = model.AllowDashboardSharing;
                currentUser.TrackActivity = model.TrackActivity;
                currentUser.TwoFactorEnabled = model.TwoFactorEnabled;
                currentUser.UpdatedDate = DateTime.UtcNow;

                var updateResult = await _userService.UpdateUserAsync(currentUser);
                if (updateResult)
                {
                    _logger.LogInformation("Privacy settings updated successfully for user {UserId}", userId);
                    TempData["Success"] = "Gizlilik ayarlarınız başarıyla kaydedildi.";
                }
                else
                {
                    TempData["Error"] = "Gizlilik ayarları kaydedilirken bir hata oluştu.";
                }
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating privacy settings for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                TempData["Error"] = "Gizlilik ayarları kaydedilirken bir hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        // POST: Profile/ExportData
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
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting user data");
                TempData["Error"] = "Veri dışa aktarılırken bir hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        // POST: Profile/DeleteAccount
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
                    return RedirectToAction("Index");
                }

                // Here you would implement account deletion functionality
                // This is a critical operation and should be handled carefully
                TempData["Info"] = "Hesap silme isteğiniz alındı. İşlem 30 gün içinde tamamlanacaktır.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user account");
                TempData["Error"] = "Hesap silinirken bir hata oluştu.";
                return RedirectToAction("Index");
            }
        }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mevcut şifre gereklidir.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifre gereklidir.")]
        [StringLength(100, ErrorMessage = "Şifre en az {2} karakter olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre Tekrar")]
        [Compare("NewPassword", ErrorMessage = "Yeni şifre ve tekrarı eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}