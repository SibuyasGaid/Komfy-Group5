using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Controllers
{
    [AllowAnonymous] // Temporarily allow access to all users for development
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;

        // Inject IUserService
        public UserController(ILogger<UserController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        // GET: /User/Index (READ: List all users)
        public IActionResult Index()
        {
            var users = _userService.GetAllUsers();
            return View(users);
        }

        // GET: /User/Create (CREATE: Display form)
        public IActionResult Create()
        {
            return View();
        }

        // POST: /User/Create (CREATE: Process form submission)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UserModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _userService.AddUser(model);
                    TempData["SuccessMessage"] = "User added successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    // Add model error if business logic fails (e.g., duplicate UserId or Email)
                    ModelState.AddModelError(string.Empty, $"Error adding user: {ex.Message}");
                    _logger.LogError(ex, "Error adding user.");
                }
            }
            return View(model);
        }

        // GET: /User/Edit/{id} (UPDATE: Display form with existing data)
        public IActionResult Edit(string id)
        {
            var user = _userService.GetUserDetails(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: /User/Edit/{id} (UPDATE: Process form submission)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, UserModel model)
        {
            if (id != model.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _userService.UpdateUser(model);
                    TempData["SuccessMessage"] = "User updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Error updating user: {ex.Message}");
                    _logger.LogError(ex, "Error updating user.");
                }
            }
            return View(model);
        }

        // POST: /User/Delete/{id} (DELETE: Process deletion)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            try
            {
                _userService.DeleteUser(id);
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = "User not found or already deleted.";
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting user: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /User/Details/{id} (READ: View user details)
        public IActionResult Details(string id)
        {
            var user = _userService.GetUserDetails(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // QUICK WIN #4: POST: /User/GrantAdmin/{id} (UPDATE: Grant admin access)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GrantAdmin(string id)
        {
            try
            {
                _userService.GrantAdminAccess(id);
                TempData["SuccessMessage"] = "Admin access granted successfully.";
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = "User not found.";
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Error granting admin access: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // QUICK WIN #4: POST: /User/RevokeAdmin/{id} (UPDATE: Revoke admin access)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RevokeAdmin(string id)
        {
            try
            {
                _userService.RevokeAdminAccess(id);
                TempData["SuccessMessage"] = "Admin access revoked successfully.";
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = "User not found.";
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Error revoking admin access: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
