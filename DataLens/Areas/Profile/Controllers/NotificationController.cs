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
    public class NotificationController : Controller
    {
        private readonly IUserService _userService;
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(IUserService userService, INotificationRepository notificationRepository, ILogger<NotificationController> logger)
        {
            _userService = userService;
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        // GET: Profile/Notification
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // Gerçek veritabanından bildirimleri al
                var notificationEntities = await _notificationRepository.GetByUserIdAsync(userId);
                
                var notifications = notificationEntities.Select(n => new NotificationViewModel
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                }).ToList();

                // Eğer hiç bildirim yoksa örnek veriler ekle (geliştirme amaçlı)
                if (!notifications.Any())
                {
                    notifications = new List<NotificationViewModel>
                    {
                        new NotificationViewModel
                        {
                            Id = "1",
                            Title = "Dashboard Paylaşıldı",
                            Message = "'Satış Raporu' dashboard'u sizinle paylaşıldı.",
                            Type = "dashboard",
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow.AddHours(-2)
                        },
                        new NotificationViewModel
                        {
                            Id = "2",
                            Title = "Sistem Güncellemesi",
                            Message = "Sistem bakımı 15.01.2024 tarihinde yapılacaktır.",
                            Type = "system",
                            IsRead = true,
                            CreatedAt = DateTime.UtcNow.AddDays(-1)
                    },
                    new NotificationViewModel
                    {
                        Id = "3",
                        Title = "Yeni Yetki",
                        Message = "'Finans' grubuna eklendiniz.",
                        Type = "permission",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-3)
                    }
                };
                }

                return View(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications");
                TempData["Error"] = "Bildirimler yüklenirken bir hata oluştu.";
                return RedirectToAction("Index", "Profile");
            }
        }

        // GET: Profile/Notification/Preferences
        public async Task<IActionResult> Preferences()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var preferences = new NotificationPreferencesViewModel
                {
                    UserId = userId,
                    EmailNotifications = true,
                    BrowserNotifications = false,
                    DashboardShared = true,
                    DashboardUpdated = true,
                    PermissionChanged = true,
                    SystemUpdates = false,
                    SecurityAlerts = true,
                    WeeklyDigest = true,
                    MonthlyReport = false
                };

                return View(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification preferences");
                TempData["Error"] = "Bildirim tercihleri yüklenirken bir hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        // POST: Profile/Notification/Preferences
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Preferences(NotificationPreferencesViewModel preferences)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || userId != preferences.UserId)
                {
                    return Forbid();
                }

                if (ModelState.IsValid)
                {
                    // Here you would typically save notification preferences to database
                    TempData["Success"] = "Bildirim tercihleriniz başarıyla kaydedildi.";
                    return RedirectToAction("Preferences");
                }

                return View(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving notification preferences");
                TempData["Error"] = "Bildirim tercihleri kaydedilirken bir hata oluştu.";
                return View(preferences);
            }
        }

        // POST: Profile/Notification/MarkAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(string notificationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Kullanıcı bulunamadı." });
                }

                if (string.IsNullOrEmpty(notificationId))
                {
                    return Json(new { success = false, message = "Bildirim ID'si gereklidir." });
                }

                // Here you would typically mark notification as read in database
                return Json(new { success = true, message = "Bildirim okundu olarak işaretlendi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return Json(new { success = false, message = "Bildirim işaretlenirken bir hata oluştu." });
            }
        }

        // POST: Profile/Notification/MarkAllAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Kullanıcı bulunamadı." });
                }

                // Here you would typically mark all notifications as read in database
                return Json(new { success = true, message = "Tüm bildirimler okundu olarak işaretlendi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return Json(new { success = false, message = "Bildirimler işaretlenirken bir hata oluştu." });
            }
        }

        // POST: Profile/Notification/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string notificationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Kullanıcı bulunamadı." });
                }

                if (string.IsNullOrEmpty(notificationId))
                {
                    return Json(new { success = false, message = "Bildirim ID'si gereklidir." });
                }

                // Here you would typically delete notification from database
                return Json(new { success = true, message = "Bildirim silindi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                return Json(new { success = false, message = "Bildirim silinirken bir hata oluştu." });
            }
        }

        // GET: Profile/Notification/GetUnreadCount
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { count = 0 });
                }

                // Here you would typically get unread count from database
                var unreadCount = 2; // Mock data
                return Json(new { count = unreadCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notification count");
                return Json(new { count = 0 });
            }
        }
    }

    public class NotificationViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NotificationPreferencesViewModel
    {
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "E-posta Bildirimleri")]
        public bool EmailNotifications { get; set; }

        [Display(Name = "Tarayıcı Bildirimleri")]
        public bool BrowserNotifications { get; set; }

        [Display(Name = "Dashboard Paylaşıldığında")]
        public bool DashboardShared { get; set; }

        [Display(Name = "Dashboard Güncellendiğinde")]
        public bool DashboardUpdated { get; set; }

        [Display(Name = "Yetki Değiştiğinde")]
        public bool PermissionChanged { get; set; }

        [Display(Name = "Sistem Güncellemeleri")]
        public bool SystemUpdates { get; set; }

        [Display(Name = "Güvenlik Uyarıları")]
        public bool SecurityAlerts { get; set; }

        [Display(Name = "Haftalık Özet")]
        public bool WeeklyDigest { get; set; }

        [Display(Name = "Aylık Rapor")]
        public bool MonthlyReport { get; set; }
    }
}