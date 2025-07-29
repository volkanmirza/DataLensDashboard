using DataLens.Data.Interfaces;
using DataLens.Models;
using DataLens.Services;
using DataLens.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace DataLens.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;

        public AccountController(IUserService userService, IJwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Kullanıcı adı ve şifre gereklidir.";
                return View();
            }

            var user = await _userService.ValidateUserAsync(username, password);
            if (user == null)
            {
                ViewBag.Error = "Geçersiz kullanıcı adı veya şifre.";
                return View();
            }

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);

            // Store token in cookie for web app usage
            Response.Cookies.Append("jwt_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(60)
            });

            // For API usage, return token in response
            if (Request.Headers.Accept.ToString().Contains("application/json"))
            {
                return Json(new { success = true, token = token, user = new { user.Id, user.Username, user.Email, user.Role } });
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Route("api/auth/login")]
        public async Task<IActionResult> ApiLogin([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { error = "Kullanıcı adı ve şifre gereklidir." });
            }

            var user = await _userService.ValidateUserAsync(request.Username, request.Password);
            if (user == null)
            {
                return Unauthorized(new { error = "Geçersiz kullanıcı adı veya şifre." });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new { error = "Hesabınız aktif değil." });
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                token = token,
                user = new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.Role,
                    user.FirstName,
                    user.LastName
                },
                expiresAt = DateTime.UtcNow.AddMinutes(60)
            });
        }

        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt_token");
            return RedirectToAction("Login");
        }

        [HttpPost]
        [Route("api/auth/logout")]
        [Authorize]
        public IActionResult ApiLogout()
        {
            // For JWT, logout is handled client-side by removing the token
            return Ok(new { message = "Başarıyla çıkış yapıldı." });
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user, string password, string confirmPassword)
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

            // Set default role for registration
            user.Role = "Viewer";

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = await _userService.CreateUserAsync(user, password);
                    TempData["Success"] = "Hesabınız başarıyla oluşturuldu. Giriş yapabilirsiniz.";
                    return RedirectToAction(nameof(Login));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Hesap oluşturulurken bir hata oluştu.");
                }
            }

            return View(user);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction(nameof(Login));
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return RedirectToAction(nameof(Login));
                }

                return View(user);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Profil bilgileri yüklenirken bir hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(User user)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || userId != user.Id)
            {
                return RedirectToAction(nameof(Login));
            }

            // Preserve certain fields that shouldn't be changed via profile
            var existingUser = await _userService.GetUserByIdAsync(userId);
            if (existingUser != null)
            {
                user.Role = existingUser.Role;
                user.CreatedDate = existingUser.CreatedDate;
                user.PasswordHash = existingUser.PasswordHash;
                user.IsActive = existingUser.IsActive;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _userService.UpdateUserAsync(user);
                    if (result)
                    {
                        TempData["Success"] = "Profil bilgileriniz başarıyla güncellendi.";
                        return RedirectToAction(nameof(Profile));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Profil güncellenemedi.");
                    }
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Profil güncellenirken bir hata oluştu.");
                }
            }

            return View(user);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangePassword()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            ViewBag.User = user;
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(nameof(Login));
            }

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
                    var result = await _userService.ChangePasswordAsync(userId!, currentPassword, newPassword);
                    if (result)
                    {
                        TempData["Success"] = "Şifreniz başarıyla değiştirildi.";
                        return RedirectToAction(nameof(Profile));
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
                catch (Exception)
                {
                    ModelState.AddModelError("", "Şifre değiştirilirken bir hata oluştu.");
                }
            }

            var user = await _userService.GetUserByIdAsync(userId);
            ViewBag.User = user;
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Route("api/auth/validate")]
        [Authorize]
        public IActionResult ValidateToken()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            return Ok(new
            {
                valid = true,
                user = new
                {
                    id = userId,
                    username = username,
                    role = role
                }
            });
        }

        private bool VerifyPassword(string password, string hash)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var hashedPassword = Convert.ToBase64String(hashedBytes);
                return hashedPassword == hash;
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}