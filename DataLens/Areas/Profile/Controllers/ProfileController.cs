using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataLens.Services.Interfaces;
using DataLens.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace DataLens.Areas.Profile.Controllers
{
    [Area("Profile")]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IUserService userService, ILogger<ProfileController> logger)
        {
            _userService = userService;
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
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound();
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile");
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
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound();
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user for edit");
                TempData["Error"] = "Profil düzenleme sayfası yüklenirken bir hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User user)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || userId != user.Id)
                {
                    return Forbid();
                }

                // Remove password and sensitive fields from model state
                ModelState.Remove("Password");
                ModelState.Remove("CreatedAt");
                ModelState.Remove("UpdatedAt");
                ModelState.Remove("IsActive");
                ModelState.Remove("Role");

                if (ModelState.IsValid)
                {
                    // Check if email is already taken by another user
                    var existingUser = await _userService.GetByEmailAsync(user.Email);
                    if (existingUser != null && existingUser.Id != user.Id)
                    {
                        ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
                        return View(user);
                    }

                    // Get current user to preserve sensitive fields
                    var currentUser = await _userService.GetByIdAsync(userId);
                    if (currentUser == null)
                    {
                        return NotFound();
                    }

                    // Update only allowed fields
                    currentUser.FirstName = user.FirstName;
                    currentUser.LastName = user.LastName;
                    currentUser.Email = user.Email;
                    currentUser.UpdatedAt = DateTime.UtcNow;

                    await _userService.UpdateAsync(currentUser);
                    TempData["Success"] = "Profil bilgileriniz başarıyla güncellendi.";
                    return RedirectToAction("Index");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                TempData["Error"] = "Profil güncellenirken bir hata oluştu.";
                return View(user);
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
                if (ModelState.IsValid)
                {
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userId))
                    {
                        return RedirectToAction("Login", "Account", new { area = "" });
                    }

                    var result = await _userService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
                    if (result)
                    {
                        TempData["Success"] = "Şifreniz başarıyla değiştirildi.";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("CurrentPassword", "Mevcut şifre yanlış.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                TempData["Error"] = "Şifre değiştirilirken bir hata oluştu.";
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