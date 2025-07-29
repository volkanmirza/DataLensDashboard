using DataLens.Models;
using DataLens.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DataLens.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserGroupController : Controller
    {
        private readonly IUserGroupService _userGroupService;
        private readonly IUserService _userService;
        private readonly ILogger<UserGroupController> _logger;

        public UserGroupController(IUserGroupService userGroupService, IUserService userService, ILogger<UserGroupController> logger)
        {
            _userGroupService = userGroupService;
            _userService = userService;
            _logger = logger;
        }

        // GET: Admin/UserGroup
        public async Task<IActionResult> Index()
        {
            try
            {
                var userGroups = await _userGroupService.GetAllGroupsAsync();
                return View(userGroups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user groups");
                TempData["Error"] = "Kullanıcı grupları yüklenirken bir hata oluştu.";
                return View(new List<UserGroup>());
            }
        }

        // GET: Admin/UserGroup/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var userGroup = await _userGroupService.GetGroupByIdAsync(id);
                if (userGroup == null)
                {
                    return NotFound();
                }

                // Get group members
                var members = await _userGroupService.GetGroupMembersAsync(id);
                ViewBag.Members = members;

                return View(userGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user group details for ID: {GroupId}", id);
                TempData["Error"] = "Grup detayları yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/UserGroup/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/UserGroup/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserGroup userGroup)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    userGroup.CreatedBy = User.Identity?.Name ?? "System";

                    await _userGroupService.CreateGroupAsync(userGroup);
                    TempData["SuccessMessage"] = "User group created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating user group: {GroupName}", userGroup.GroupName);
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View(userGroup);
        }

        // GET: Admin/UserGroup/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var userGroup = await _userGroupService.GetGroupByIdAsync(id);
                if (userGroup == null)
                {
                    return NotFound();
                }

                return View(userGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user group for edit: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the group.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/UserGroup/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserGroup userGroup)
        {
            if (id != userGroup.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _userGroupService.UpdateGroupAsync(userGroup);
                    TempData["SuccessMessage"] = "User group updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating user group: {Id}", id);
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View(userGroup);
        }

        // GET: Admin/UserGroup/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var userGroup = await _userGroupService.GetGroupByIdAsync(id);
                if (userGroup == null)
                {
                    return NotFound();
                }

                // Get member count
                var members = await _userGroupService.GetGroupMembersAsync(id);
                ViewBag.MemberCount = members.Count();

                return View(userGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user group for delete: {GroupId}", id);
                TempData["Error"] = "Grup bilgileri yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/UserGroup/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _userGroupService.DeleteGroupAsync(id);
                TempData["Success"] = "Kullanıcı grubu başarıyla silindi.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user group: {GroupId}", id);
                TempData["Error"] = "Kullanıcı grubu silinirken bir hata oluştu.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/UserGroup/ManageMembers/5
        public async Task<IActionResult> ManageMembers(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var userGroup = await _userGroupService.GetGroupByIdAsync(id);
                if (userGroup == null)
                {
                    return NotFound();
                }

                // Get all users
                var allUsers = await _userService.GetAllUsersAsync();
                
                // Get current group members
                var currentMembers = await _userGroupService.GetGroupMembersAsync(id);
                var currentMemberIds = currentMembers.Select(m => m.Id).ToList();

                // Create view model
                var availableUsers = allUsers.Where(u => !currentMemberIds.Contains(u.Id)).ToList();
                
                ViewBag.UserGroup = userGroup;
                ViewBag.CurrentMembers = currentMembers;
                ViewBag.AvailableUsers = new SelectList(availableUsers, "Id", "Username");

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading manage members page for group: {GroupId}", id);
                TempData["Error"] = "Üye yönetimi sayfası yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/UserGroup/AddMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(string groupId, string userId)
        {
            if (string.IsNullOrEmpty(groupId) || string.IsNullOrEmpty(userId))
            {
                return BadRequest();
            }

            try
            {
                var addedBy = User.Identity?.Name ?? "System";
                await _userGroupService.AddUserToGroupAsync(userId, groupId, addedBy);
                TempData["SuccessMessage"] = "User added to group successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user {UserId} to group {GroupId}", userId, groupId);
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(ManageMembers), new { id = groupId });
        }

        // POST: Admin/UserGroup/RemoveMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(string groupId, string userId)
        {
            if (string.IsNullOrEmpty(groupId) || string.IsNullOrEmpty(userId))
            {
                return BadRequest();
            }

            try
            {
                await _userGroupService.RemoveUserFromGroupAsync(userId, groupId);
                TempData["SuccessMessage"] = "User removed from group successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user {UserId} from group {GroupId}", userId, groupId);
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(ManageMembers), new { id = groupId });
        }
    }
}