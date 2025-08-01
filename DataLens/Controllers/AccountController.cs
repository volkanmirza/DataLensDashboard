using DataLens.Models;
using DataLens.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataLens.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<MongoRole> _roleManager;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<MongoRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, bool rememberMe = false)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Kullanıcı adı ve şifre gereklidir.";
                return View();
            }

            // Find user by username or email
            var user = await _userManager.FindByNameAsync(username) ?? await _userManager.FindByEmailAsync(username);
            
            if (user == null || !user.IsActive)
            {
                ViewBag.Error = "Geçersiz kullanıcı adı veya şifre.";
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: true);
            
            if (result.Succeeded)
            {
                // Update last login date
                user.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // For API usage, return JSON response
                if (Request.Headers.Accept.ToString().Contains("application/json"))
                {
                    return Json(new { 
                        success = true, 
                        user = new { 
                            user.Id, 
                            user.UserName, 
                            user.Email, 
                            user.Role,
                            user.FirstName,
                            user.LastName
                        } 
                    });
                }

                return RedirectToAction("Index", "Home");
            }
            
            if (result.IsLockedOut)
            {
                ViewBag.Error = "Hesabınız geçici olarak kilitlenmiştir. Lütfen daha sonra tekrar deneyin.";
            }
            else
            {
                ViewBag.Error = "Geçersiz kullanıcı adı veya şifre.";
            }

            return View();
        }

        [HttpPost]
        [Route("api/auth/login")]
        public async Task<IActionResult> ApiLogin([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { error = "Kullanıcı adı ve şifre gereklidir." });
            }

            var user = await _userManager.FindByNameAsync(request.Username) ?? await _userManager.FindByEmailAsync(request.Username);
            
            if (user == null || !user.IsActive)
            {
                return Unauthorized(new { error = "Geçersiz kullanıcı adı veya şifre." });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    return Unauthorized(new { error = "Hesabınız geçici olarak kilitlenmiştir." });
                }
                return Unauthorized(new { error = "Geçersiz kullanıcı adı veya şifre." });
            }

            // Update last login date
            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                success = true,
                user = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.Role,
                    user.FirstName,
                    user.LastName
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpPost]
        [Route("api/auth/logout")]
        [Authorize]
        public async Task<IActionResult> ApiLogout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Başarıyla çıkış yapıldı." });
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Role = "Viewer",
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    // Ensure Viewer role exists
                    if (!await _roleManager.RoleExistsAsync("Viewer"))
                    {
                        await _roleManager.CreateAsync(new MongoRole("Viewer"));
                    }
                    
                    // Add user to Viewer role
                    await _userManager.AddToRoleAsync(user, "Viewer");
                    
                    TempData["Success"] = "Hesabınız başarıyla oluşturuldu. Giriş yapabilirsiniz.";
                    return RedirectToAction(nameof(Login));
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Profile()
        {
            // Redirect to Profile area Dashboard
            return RedirectToAction("Dashboard", "Profile", new { area = "Profile" });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(User model)
        {
            // Redirect to Profile area Dashboard
            return RedirectToAction("Dashboard", "Profile", new { area = "Profile" });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
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
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Yeni şifre ve şifre onayı eşleşmiyor.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
                    if (result.Succeeded)
                    {
                        // Sign in the user again to refresh the security stamp
                        await _signInManager.RefreshSignInAsync(user);
                        
                        TempData["Success"] = "Şifreniz başarıyla değiştirildi.";
                        return RedirectToAction(nameof(Profile));
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Şifre değiştirilirken bir hata oluştu.");
                }
            }

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
        public async Task<IActionResult> ValidateToken()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(new
            {
                valid = true,
                user = new
                {
                    id = user.Id,
                    username = user.UserName,
                    email = user.Email,
                    role = user.Role,
                    firstName = user.FirstName,
                    lastName = user.LastName
                }
            });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}