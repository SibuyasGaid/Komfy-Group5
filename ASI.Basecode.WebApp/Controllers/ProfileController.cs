using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;

        public ProfileController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: /Profile/Index
        public IActionResult Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var userModel = _userService.GetUserDetails(userId);

            if (userModel == null)
            {
                return NotFound();
            }

            // Get theme preference from cookie
            var theme = Request.Cookies["theme"] ?? "light";
            ViewBag.CurrentTheme = theme;

            return View(userModel);
        }

        // POST: /Profile/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(string name, string email)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                _userService.UpdateUserProfile(userId, name, email);
                TempData["SuccessMessage"] = "Profile updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Validate passwords match
            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "New password and confirmation password do not match.";

                // Re-render the profile page and show the Change Password section
                var userModel = _userService.GetUserDetails(userId);
                ViewBag.ActiveSection = "change-password";
                return View("Index", userModel);
            }

            try
            {
                _userService.ChangePassword(userId, currentPassword, newPassword);
                TempData["SuccessMessage"] = "Password changed successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                // On error re-render and keep the change-password tab open
                var userModel = _userService.GetUserDetails(userId);
                ViewBag.ActiveSection = "change-password";
                return View("Index", userModel);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Profile/SetTheme
        [HttpPost]
        public IActionResult SetTheme([FromForm] string theme)
        {
            if (string.IsNullOrEmpty(theme))
            {
                return Json(new { success = false, message = "Theme value is required" });
            }

            // Store theme preference in cookie (valid for 1 year)
            Response.Cookies.Append("theme", theme, new Microsoft.AspNetCore.Http.CookieOptions
            {
                Expires = DateTimeOffset.Now.AddYears(1),
                HttpOnly = false, // Allow JavaScript to read for immediate theme switch
                Secure = false,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax
            });

            return Json(new { success = true });
        }
    }
}
