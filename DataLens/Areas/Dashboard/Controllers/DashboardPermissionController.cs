using DataLens.Models;
using DataLens.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataLens.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class DashboardPermissionController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly IUserService _userService;
        private readonly IUserGroupService _userGroupService;
        private readonly ILogger<DashboardPermissionController> _logger;

        public DashboardPermissionController(
            IDashboardService dashboardService,
            IUserService userService,
            IUserGroupService userGroupService,
            ILogger<DashboardPermissionController> logger)
        {
            _dashboardService = dashboardService;
            _userService = userService;
            _userGroupService = userGroupService;
            _logger = logger;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        // GET: Dashboard/DashboardPermission/Manage/5
        public async Task<IActionResult> Manage(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var dashboard = await _dashboardService.GetDashboardByIdAsync(id);
                if (dashboard == null)
                {
                    return NotFound();
                }

                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                // Only owner or admin can manage permissions
                if (userRole != UserRoles.Admin && dashboard.CreatedBy != userId)
                {
                    return Forbid();
                }

                var permissions = await _dashboardService.GetDashboardPermissionsAsync(id);
                var users = await _userService.GetAllUsersAsync();
                var groups = await _userGroupService.GetAllGroupsAsync();

                ViewBag.Dashboard = dashboard;
                ViewBag.Users = users.Where(u => u.Id != dashboard.CreatedBy); // Exclude owner
                ViewBag.Groups = groups;
                ViewBag.PermissionTypes = new[] { PermissionTypes.View, PermissionTypes.Edit };

                return View(permissions);
            }
            catch (Exception ex)
            {
            _logger.LogError(ex, "Error loading dashboard permissions management: {Id}", id);
                TempData["ErrorMessage"] = "İzin yönetimi sayfası yüklenirken bir hata oluştu.";
                return RedirectToAction("Details", "Dashboard", new { id });
            }
        }

        // POST: Dashboard/DashboardPermission/GrantUserPermission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GrantUserPermission(string dashboardId, string userId, string permissionType)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                var dashboard = await _dashboardService.GetDashboardByIdAsync(dashboardId);

                if (dashboard == null)
                {
                    return Json(new { success = false, message = "Dashboard bulunamadı." });
                }

                // Only owner or admin can grant permissions
                if (userRole != UserRoles.Admin && dashboard.CreatedBy != currentUserId)
                {
                    return Json(new { success = false, message = "İzin verme yetkiniz yok." });
                }

                // Validate permission type
                if (permissionType != PermissionTypes.View && permissionType != PermissionTypes.Edit)
                {
                    return Json(new { success = false, message = "Geçersiz izin türü." });
                }

                // Check if user exists
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "Kullanıcı bulunamadı." });
                }

                // Don't allow granting permission to owner
                if (userId == dashboard.CreatedBy)
                {
                    return Json(new { success = false, message = "Dashboard sahibine izin verilemez." });
                }

                // Check if permission already exists
                if (await _dashboardService.HasUserPermissionAsync(dashboardId, userId, permissionType))
                {
                    return Json(new { success = false, message = "Bu kullanıcı zaten bu izne sahip." });
                }

                if (await _dashboardService.GrantUserPermissionAsync(dashboardId, userId, permissionType))
                {
                    return Json(new { success = true, message = "İzin başarıyla verildi." });
                }
                else
                {
                    return Json(new { success = false, message = "İzin verilirken bir hata oluştu." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error granting user permission: Dashboard={DashboardId}, User={UserId}, Permission={PermissionType}", 
                    dashboardId, userId, permissionType);
                return Json(new { success = false, message = "İzin verilirken bir hata oluştu." });
            }
        }

        // POST: Dashboard/DashboardPermission/GrantGroupPermission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GrantGroupPermission(string dashboardId, string groupId, string permissionType)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                var dashboard = await _dashboardService.GetDashboardByIdAsync(dashboardId);

                if (dashboard == null)
                {
                    return Json(new { success = false, message = "Dashboard bulunamadı." });
                }

                // Only owner or admin can grant permissions
                if (userRole != UserRoles.Admin && dashboard.CreatedBy != currentUserId)
                {
                    return Json(new { success = false, message = "İzin verme yetkiniz yok." });
                }

                // Validate permission type
                if (permissionType != PermissionTypes.View && permissionType != PermissionTypes.Edit)
                {
                    return Json(new { success = false, message = "Geçersiz izin türü." });
                }

                // Check if group exists
                var group = await _userGroupService.GetGroupByIdAsync(groupId);
                if (group == null)
                {
                    return Json(new { success = false, message = "Grup bulunamadı." });
                }

                // Check if permission already exists
                if (await _dashboardService.HasGroupPermissionAsync(dashboardId, groupId, permissionType))
                {
                    return Json(new { success = false, message = "Bu grup zaten bu izne sahip." });
                }

                if (await _dashboardService.GrantGroupPermissionAsync(dashboardId, groupId, permissionType))
                {
                    return Json(new { success = true, message = "Grup izni başarıyla verildi." });
                }
                else
                {
                    return Json(new { success = false, message = "Grup izni verilirken bir hata oluştu." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error granting group permission: Dashboard={DashboardId}, Group={GroupId}, Permission={PermissionType}", 
                    dashboardId, groupId, permissionType);
                return Json(new { success = false, message = "Grup izni verilirken bir hata oluştu." });
            }
        }

        // POST: Dashboard/DashboardPermission/RevokeUserPermission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeUserPermission(string dashboardId, string userId, string permissionType)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                var dashboard = await _dashboardService.GetDashboardByIdAsync(dashboardId);

                if (dashboard == null)
                {
                    return Json(new { success = false, message = "Dashboard bulunamadı." });
                }

                // Only owner or admin can revoke permissions
                if (userRole != UserRoles.Admin && dashboard.CreatedBy != currentUserId)
                {
                    return Json(new { success = false, message = "İzin iptal etme yetkiniz yok." });
                }

                if (await _dashboardService.RevokeUserPermissionAsync(dashboardId, userId, permissionType))
                {
                    return Json(new { success = true, message = "İzin başarıyla iptal edildi." });
                }
                else
                {
                    return Json(new { success = false, message = "İzin iptal edilirken bir hata oluştu." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking user permission: Dashboard={DashboardId}, User={UserId}, Permission={PermissionType}", 
                    dashboardId, userId, permissionType);
                return Json(new { success = false, message = "İzin iptal edilirken bir hata oluştu." });
            }
        }

        // POST: Dashboard/DashboardPermission/RevokeGroupPermission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeGroupPermission(string dashboardId, string groupId, string permissionType)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                var dashboard = await _dashboardService.GetDashboardByIdAsync(dashboardId);

                if (dashboard == null)
                {
                    return Json(new { success = false, message = "Dashboard bulunamadı." });
                }

                // Only owner or admin can revoke permissions
                if (userRole != UserRoles.Admin && dashboard.CreatedBy != currentUserId)
                {
                    return Json(new { success = false, message = "İzin iptal etme yetkiniz yok." });
                }

                if (await _dashboardService.RevokeGroupPermissionAsync(dashboardId, groupId, permissionType))
                {
                    return Json(new { success = true, message = "Grup izni başarıyla iptal edildi." });
                }
                else
                {
                    return Json(new { success = false, message = "Grup izni iptal edilirken bir hata oluştu." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking group permission: Dashboard={DashboardId}, Group={GroupId}, Permission={PermissionType}", 
                    dashboardId, groupId, permissionType);
                return Json(new { success = false, message = "Grup izni iptal edilirken bir hata oluştu." });
            }
        }

        // POST: Dashboard/DashboardPermission/UpdateUserPermission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserPermission(string dashboardId, string userId, string oldPermissionType, string newPermissionType)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                var dashboard = await _dashboardService.GetDashboardByIdAsync(dashboardId);

                if (dashboard == null)
                {
                    return Json(new { success = false, message = "Dashboard bulunamadı." });
                }

                // Only owner or admin can update permissions
                if (userRole != UserRoles.Admin && dashboard.CreatedBy != currentUserId)
                {
                    return Json(new { success = false, message = "İzin güncelleme yetkiniz yok." });
                }

                // Validate permission types
                if ((oldPermissionType != PermissionTypes.View && oldPermissionType != PermissionTypes.Edit) ||
                    (newPermissionType != PermissionTypes.View && newPermissionType != PermissionTypes.Edit))
                {
                    return Json(new { success = false, message = "Geçersiz izin türü." });
                }

                if (await _dashboardService.UpdateUserPermissionAsync(dashboardId, userId, newPermissionType))
                {
                    return Json(new { success = true, message = "İzin başarıyla güncellendi." });
                }
                else
                {
                    return Json(new { success = false, message = "İzin güncellenirken bir hata oluştu." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user permission: Dashboard={DashboardId}, User={UserId}, OldPermission={OldPermissionType}, NewPermission={NewPermissionType}", 
                    dashboardId, userId, oldPermissionType, newPermissionType);
                return Json(new { success = false, message = "İzin güncellenirken bir hata oluştu." });
            }
        }

        // POST: Dashboard/DashboardPermission/UpdateGroupPermission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGroupPermission(string dashboardId, string groupId, string oldPermissionType, string newPermissionType)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                var dashboard = await _dashboardService.GetDashboardByIdAsync(dashboardId);

                if (dashboard == null)
                {
                    return Json(new { success = false, message = "Dashboard bulunamadı." });
                }

                // Only owner or admin can update permissions
                if (userRole != UserRoles.Admin && dashboard.CreatedBy != currentUserId)
                {
                    return Json(new { success = false, message = "İzin güncelleme yetkiniz yok." });
                }

                // Validate permission types
                if ((oldPermissionType != PermissionTypes.View && oldPermissionType != PermissionTypes.Edit) ||
                    (newPermissionType != PermissionTypes.View && newPermissionType != PermissionTypes.Edit))
                {
                    return Json(new { success = false, message = "Geçersiz izin türü." });
                }

                if (await _dashboardService.UpdateGroupPermissionAsync(dashboardId, groupId, newPermissionType))
                {
                    return Json(new { success = true, message = "Grup izni başarıyla güncellendi." });
                }
                else
                {
                    return Json(new { success = false, message = "Grup izni güncellenirken bir hata oluştu." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating group permission: Dashboard={DashboardId}, Group={GroupId}, OldPermission={OldPermissionType}, NewPermission={NewPermissionType}", 
                    dashboardId, groupId, oldPermissionType, newPermissionType);
                return Json(new { success = false, message = "Grup izni güncellenirken bir hata oluştu." });
            }
        }

        // GET: Dashboard/DashboardPermission/GetUserPermissions
        public async Task<IActionResult> GetUserPermissions(string dashboardId, string userId)
        {
            try
            {
                var permissions = await _dashboardService.GetUserPermissionsAsync(dashboardId, userId);
                return Json(new { success = true, permissions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user permissions: Dashboard={DashboardId}, User={UserId}", dashboardId, userId);
                return Json(new { success = false, message = "Kullanıcı izinleri alınırken bir hata oluştu." });
            }
        }

        // GET: Dashboard/DashboardPermission/GetGroupPermissions
        public async Task<IActionResult> GetGroupPermissions(string dashboardId, string groupId)
        {
            try
            {
                var permissions = await _dashboardService.GetGroupPermissionsAsync(dashboardId, groupId);
                return Json(new { success = true, permissions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group permissions: Dashboard={DashboardId}, Group={GroupId}", dashboardId, groupId);
                return Json(new { success = false, message = "Grup izinleri alınırken bir hata oluştu." });
            }
        }
    }
}