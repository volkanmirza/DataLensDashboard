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
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IUserService userService, INotificationRepository notificationRepository, ILogger<ProfileController> logger)
        {
            _userService = userService;
            _notificationRepository = notificationRepository;
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
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in claims for dashboard access");
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for ID: {UserId} during dashboard access", userId);
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Inactive user {UserId} attempted to access dashboard", userId);
                    return RedirectToAction("AccessDenied", "Account", new { area = "" });
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                TempData["Error"] = "Dashboard yüklenirken bir hata oluştu.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        // GET: Profile/ChangePassword
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
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
                    ModelState.AddModelError("", "Mevcut şifre yanlış.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                ModelState.AddModelError("", "Şifre değiştirme işlemi sırasında bir hata oluştu.");
                return View(model);
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