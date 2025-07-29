using DataLens.Models;
using DataLens.Services;
using DataLens.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataLens.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // GET: Admin/User
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                TempData["Error"] = "Kullanıcılar yüklenirken bir hata oluştu.";
                return View(new List<User>());
            }
        }

        // GET: Admin/User/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user details for ID: {UserId}", id);
                TempData["Error"] = "Kullanıcı detayları yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/User/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(password) || password != confirmPassword)
            {
                ModelState.AddModelError("", "Şifre ve şifre onayı eşleşmiyor.");
                return View(user);
            }

            if (password.Length < 6)
            {
                ModelState.AddModelError("", "Şifre en az 6 karakter olmalıdır.");
                return View(user);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = await _userService.CreateUserAsync(user, password);
                    TempData["Success"] = "Kullanıcı başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating user");
                    ModelState.AddModelError("", "Kullanıcı oluşturulurken bir hata oluştu.");
                }
            }

            return View(user);
        }

        // GET: Admin/User/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user for edit: {UserId}", id);
                TempData["Error"] = "Kullanıcı bilgileri yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _userService.UpdateUserAsync(user);
                    if (result)
                    {
                        TempData["Success"] = "Kullanıcı başarıyla güncellendi.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Kullanıcı güncellenemedi.");
                    }
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating user: {UserId}", id);
                    ModelState.AddModelError("", "Kullanıcı güncellenirken bir hata oluştu.");
                }
            }

            return View(user);
        }

        // GET: Admin/User/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user for delete: {UserId}", id);
                TempData["Error"] = "Kullanıcı bilgileri yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);
                if (result)
                {
                    TempData["Success"] = "Kullanıcı başarıyla silindi.";
                }
                else
                {
                    TempData["Error"] = "Kullanıcı silinemedi.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", id);
                TempData["Error"] = "Kullanıcı silinirken bir hata oluştu.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/User/ChangePassword/5
        public async Task<IActionResult> ChangePassword(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                ViewBag.User = user;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user for password change: {UserId}", id);
                TempData["Error"] = "Kullanıcı bilgileri yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/User/ChangePassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string id, string currentPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Yeni şifre ve şifre onayı eşleşmiyor.");
            }

            if (newPassword?.Length < 6)
            {
                ModelState.AddModelError("", "Şifre en az 6 karakter olmalıdır.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _userService.ChangePasswordAsync(id, currentPassword, newPassword);
                    if (result)
                    {
                        TempData["Success"] = "Şifre başarıyla değiştirildi.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Şifre değiştirilemedi.");
                    }
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error changing password for user: {UserId}", id);
                    ModelState.AddModelError("", "Şifre değiştirilirken bir hata oluştu.");
                }
            }

            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                ViewBag.User = user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user for password change view: {UserId}", id);
            }

            return View();
        }
    }
}