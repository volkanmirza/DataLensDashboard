using DataLens.Models;
using DataLens.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DevExpress.DashboardAspNetCore;
using DevExpress.DashboardCommon;
using DevExpress.DashboardWeb;

namespace DataLens.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly IUserService _userService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IDashboardService dashboardService,
            IUserService userService,
            ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _userService = userService;
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

        // GET: Dashboard/Dashboard
        public async Task<IActionResult> Index(string? category = null, string? search = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                IEnumerable<DataLens.Models.Dashboard> dashboards;

                if (userRole == UserRoles.Admin)
                {
                    // Admin can see all dashboards
                    dashboards = string.IsNullOrEmpty(search) 
                        ? await _dashboardService.GetAllDashboardsAsync()
                        : await _dashboardService.SearchDashboardsAsync(search);
                }
                else
                {
                    // Regular users see only accessible dashboards
                    dashboards = await _dashboardService.GetUserAccessibleDashboardsAsync(userId);
                    
                    if (!string.IsNullOrEmpty(search))
                    {
                        dashboards = dashboards.Where(d => 
                            d.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                            (d.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            d.Category.Contains(search, StringComparison.OrdinalIgnoreCase));
                    }
                }

                if (!string.IsNullOrEmpty(category))
                {
                    dashboards = dashboards.Where(d => d.Category == category);
                }

                ViewBag.Categories = await _dashboardService.GetCategoriesAsync();
                ViewBag.CurrentCategory = category;
                ViewBag.SearchTerm = search;
                ViewBag.UserRole = userRole;

                return View(dashboards.OrderBy(d => d.Name));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard index");
                TempData["ErrorMessage"] = "Dashboardlar yüklenirken bir hata oluştu.";
                return View(new List<DataLens.Models.Dashboard>());
            }
        }

        // GET: Dashboard/Dashboard/Details/5
        public async Task<IActionResult> Details(string? id)
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

                // Check access permissions
                if (userRole != UserRoles.Admin && 
                    dashboard.CreatedBy != userId && 
                    !dashboard.IsPublic &&
                    !await _dashboardService.CanUserAccessDashboardAsync(userId, id, PermissionTypes.View))
                {
                    return Forbid();
                }

                ViewBag.CanEdit = userRole == UserRoles.Admin || 
                                 dashboard.CreatedBy == userId ||
                                 await _dashboardService.CanUserAccessDashboardAsync(userId, id, PermissionTypes.Edit);

                ViewBag.CanDelete = userRole == UserRoles.Admin || dashboard.CreatedBy == userId;

                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard details: {Id}", id);
                TempData["ErrorMessage"] = "Dashboard detayları yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Dashboard/Dashboard/Create
        [Authorize(Roles = $"{UserRoles.Designer},{UserRoles.Admin}")]
        public async Task<IActionResult> Create()
        {
            try
            {
                ViewBag.Categories = await _dashboardService.GetCategoriesAsync();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create dashboard page");
                TempData["ErrorMessage"] = "Sayfa yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Dashboard/Dashboard/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{UserRoles.Designer},{UserRoles.Admin}")]
        public async Task<IActionResult> Create(DataLens.Models.Dashboard dashboard)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userId = GetCurrentUserId();
                    dashboard.CreatedBy = userId;
                    dashboard.DashboardData = "{}"; // Empty dashboard data initially

                    if (await _dashboardService.IsDashboardNameExistsAsync(dashboard.Name))
                    {
                        ModelState.AddModelError("Name", "Bu isimde bir dashboard zaten mevcut.");
                        ViewBag.Categories = await _dashboardService.GetCategoriesAsync();
                        return View(dashboard);
                    }

                    var dashboardId = await _dashboardService.CreateDashboardAsync(dashboard);
                    TempData["SuccessMessage"] = "Dashboard başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Edit), new { id = dashboardId });
                }

                ViewBag.Categories = await _dashboardService.GetCategoriesAsync();
                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating dashboard: {Name}", dashboard.Name);
                TempData["ErrorMessage"] = "Dashboard oluşturulurken bir hata oluştu.";
                ViewBag.Categories = await _dashboardService.GetCategoriesAsync();
                return View(dashboard);
            }
        }

        // GET: Dashboard/Dashboard/Edit/5
        public async Task<IActionResult> Edit(string? id)
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

                // Check edit permissions
                if (userRole != UserRoles.Admin && 
                    dashboard.CreatedBy != userId &&
                    !await _dashboardService.CanUserAccessDashboardAsync(userId, id, PermissionTypes.Edit))
                {
                    return Forbid();
                }

                ViewBag.Categories = await _dashboardService.GetCategoriesAsync();
                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit dashboard page: {Id}", id);
                TempData["ErrorMessage"] = "Dashboard düzenleme sayfası yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Dashboard/Dashboard/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, DataLens.Models.Dashboard dashboard)
        {
            if (id != dashboard.Id)
            {
                return NotFound();
            }

            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                var existingDashboard = await _dashboardService.GetDashboardByIdAsync(id);

                if (existingDashboard == null)
                {
                    return NotFound();
                }

                // Check edit permissions
                if (userRole != UserRoles.Admin && 
                    existingDashboard.CreatedBy != userId &&
                    !await _dashboardService.CanUserAccessDashboardAsync(userId, id, PermissionTypes.Edit))
                {
                    return Forbid();
                }

                if (ModelState.IsValid)
                {
                    // Preserve important fields
                    dashboard.CreatedBy = existingDashboard.CreatedBy;
                    dashboard.CreatedDate = existingDashboard.CreatedDate;
                    dashboard.DashboardData = existingDashboard.DashboardData; // Preserve dashboard data
                    dashboard.LastModifiedBy = userId;

                    if (await _dashboardService.UpdateDashboardAsync(dashboard))
                    {
                        TempData["SuccessMessage"] = "Dashboard başarıyla güncellendi.";
                        return RedirectToAction(nameof(Details), new { id = dashboard.Id });
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Dashboard güncellenirken bir hata oluştu.";
                    }
                }

                ViewBag.Categories = await _dashboardService.GetCategoriesAsync();
                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dashboard: {Id}", id);
                TempData["ErrorMessage"] = "Dashboard güncellenirken bir hata oluştu.";
                ViewBag.Categories = await _dashboardService.GetCategoriesAsync();
                return View(dashboard);
            }
        }

        // GET: Dashboard/Dashboard/Delete/5
        public async Task<IActionResult> Delete(string? id)
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

                // Check delete permissions
                if (userRole != UserRoles.Admin && dashboard.CreatedBy != userId)
                {
                    return Forbid();
                }

                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete dashboard page: {Id}", id);
                TempData["ErrorMessage"] = "Sayfa yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Dashboard/Dashboard/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var dashboard = await _dashboardService.GetDashboardByIdAsync(id);
                if (dashboard == null)
                {
                    return NotFound();
                }

                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                // Check delete permissions
                if (userRole != UserRoles.Admin && dashboard.CreatedBy != userId)
                {
                    return Forbid();
                }

                if (await _dashboardService.DeleteDashboardAsync(id))
                {
                    TempData["SuccessMessage"] = "Dashboard başarıyla silindi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Dashboard silinirken bir hata oluştu.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting dashboard: {Id}", id);
                TempData["ErrorMessage"] = "Dashboard silinirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Dashboard/Dashboard/Designer/5
        public async Task<IActionResult> Designer(string? id)
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

                // Check edit permissions for designer
                if (userRole != UserRoles.Admin && 
                    userRole != UserRoles.Designer &&
                    dashboard.CreatedBy != userId &&
                    !await _dashboardService.CanUserAccessDashboardAsync(userId, id, PermissionTypes.Edit))
                {
                    return Forbid();
                }

                ViewBag.DashboardData = dashboard.DashboardData;
                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard designer: {Id}", id);
                TempData["ErrorMessage"] = "Dashboard tasarımcısı yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Dashboard/Dashboard/Viewer/5
        public async Task<IActionResult> Viewer(string? id)
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

                // Check view permissions
                if (userRole != UserRoles.Admin && 
                    dashboard.CreatedBy != userId && 
                    !dashboard.IsPublic &&
                    !await _dashboardService.CanUserAccessDashboardAsync(userId, id, PermissionTypes.View))
                {
                    return Forbid();
                }

                ViewBag.DashboardData = dashboard.DashboardData;
                ViewBag.CanEdit = userRole == UserRoles.Admin || 
                                 userRole == UserRoles.Designer ||
                                 dashboard.CreatedBy == userId ||
                                 await _dashboardService.CanUserAccessDashboardAsync(userId, id, PermissionTypes.Edit);

                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard viewer: {Id}", id);
                TempData["ErrorMessage"] = "Dashboard görüntüleyicisi yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Dashboard/Dashboard/SaveData
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveData(string id, string dashboardData)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                var dashboard = await _dashboardService.GetDashboardByIdAsync(id);

                if (dashboard == null)
                {
                    return Json(new { success = false, message = "Dashboard bulunamadı." });
                }

                // Check edit permissions
                if (userRole != UserRoles.Admin && 
                    dashboard.CreatedBy != userId &&
                    !await _dashboardService.CanUserAccessDashboardAsync(userId, id, PermissionTypes.Edit))
                {
                    return Json(new { success = false, message = "Bu dashboard'u düzenleme yetkiniz yok." });
                }

                if (await _dashboardService.SaveDashboardDataAsync(id, dashboardData, userId))
                {
                    return Json(new { success = true, message = "Dashboard başarıyla kaydedildi." });
                }
                else
                {
                    return Json(new { success = false, message = "Dashboard kaydedilirken bir hata oluştu." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving dashboard data: {Id}", id);
                return Json(new { success = false, message = "Dashboard kaydedilirken bir hata oluştu." });
            }
        }

        // POST: Dashboard/Dashboard/Clone
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clone(string id, string newName)
        {
            try
            {
                var userId = GetCurrentUserId();
                var dashboard = await _dashboardService.GetDashboardByIdAsync(id);

                if (dashboard == null)
                {
                    return Json(new { success = false, message = "Dashboard bulunamadı." });
                }

                // Check if user can access the source dashboard
                if (!dashboard.IsPublic && 
                    dashboard.CreatedBy != userId &&
                    !await _dashboardService.CanUserAccessDashboardAsync(userId, id, PermissionTypes.View))
                {
                    return Json(new { success = false, message = "Bu dashboard'u kopyalama yetkiniz yok." });
                }

                if (await _dashboardService.CloneDashboardAsync(id, newName, userId))
                {
                    return Json(new { success = true, message = "Dashboard başarıyla kopyalandı." });
                }
                else
                {
                    return Json(new { success = false, message = "Dashboard kopyalanırken bir hata oluştu." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cloning dashboard: {Id}", id);
                return Json(new { success = false, message = "Dashboard kopyalanırken bir hata oluştu." });
            }
        }

        // POST: Dashboard/Dashboard/Share
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Share(string id, bool isPublic)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                var dashboard = await _dashboardService.GetDashboardByIdAsync(id);

                if (dashboard == null)
                {
                    return Json(new { success = false, message = "Dashboard bulunamadı." });
                }

                // Only owner or admin can change sharing settings
                if (userRole != UserRoles.Admin && dashboard.CreatedBy != userId)
                {
                    return Json(new { success = false, message = "Bu dashboard'un paylaşım ayarlarını değiştirme yetkiniz yok." });
                }

                if (await _dashboardService.ShareDashboardAsync(id, isPublic))
                {
                    var message = isPublic ? "Dashboard herkese açık hale getirildi." : "Dashboard özel hale getirildi.";
                    return Json(new { success = true, message = message });
                }
                else
                {
                    return Json(new { success = false, message = "Paylaşım ayarları güncellenirken bir hata oluştu." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sharing dashboard: {Id}", id);
                return Json(new { success = false, message = "Paylaşım ayarları güncellenirken bir hata oluştu." });
            }
        }

        // DevExpress Dashboard Export
        [HttpPost]
        [Authorize(Policy = "AllUsers")]
        public async Task<IActionResult> Export(string dashboardId, string format)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                // Check if user has permission to view this dashboard
                var hasPermission = userRole == UserRoles.Admin || 
                                  await _dashboardService.HasUserPermissionAsync(dashboardId, userId, "View");

                if (!hasPermission)
                {
                    return Forbid();
                }

                // Export logic will be implemented here
                // This is a placeholder for DevExpress dashboard export functionality
                return Json(new { success = true, message = "Dashboard exported successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting dashboard: {DashboardId}", dashboardId);
                return Json(new { success = false, message = "Dashboard export failed" });
            }
        }
    }
}