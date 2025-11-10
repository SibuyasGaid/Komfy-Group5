using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    [AllowAnonymous] // Temporarily allow access to all users for development
    public class NotificationController : Controller
    {
        private readonly ILogger<NotificationController> _logger;
        private readonly INotificationService _notificationService;

        // Inject INotificationService
        public NotificationController(ILogger<NotificationController> logger, INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        // GET: /Notification/Index (READ: List user-specific notifications)
        public IActionResult Index()
        {
            // Get current logged-in user's ID
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Show all notifications if Admin, otherwise show only user's notifications
            List<NotificationModel> notifications;
            if (User.IsInRole("Admin"))
            {
                notifications = _notificationService.GetAllNotifications();
            }
            else
            {
                notifications = _notificationService.GetNotificationsByUserId(userId);
            }

            ViewBag.UnreadCount = _notificationService.GetUnreadCountForUser(userId);
            return View(notifications);
        }

        // GET: /Notification/Create (CREATE: Display form)
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Notification/Create (CREATE: Process form submission)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(NotificationModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _notificationService.AddNotification(model);
                    TempData["SuccessMessage"] = "Notification sent successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    // Add model error if business logic fails
                    ModelState.AddModelError(string.Empty, $"Error sending notification: {ex.Message}");
                    _logger.LogError(ex, "Error sending notification.");
                }
            }
            return View(model);
        }

        // GET: /Notification/Edit/{id} (UPDATE: Display form with existing data)
        public IActionResult Edit(int id)
        {
            var notification = _notificationService.GetNotificationDetails(id);
            if (notification == null)
            {
                return NotFound();
            }
            return View(notification);
        }

        // POST: /Notification/Edit/{id} (UPDATE: Process form submission)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, NotificationModel model)
        {
            if (id != model.NotificationID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _notificationService.UpdateNotification(model);
                    TempData["SuccessMessage"] = "Notification updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Error updating notification: {ex.Message}");
                    _logger.LogError(ex, "Error updating notification.");
                }
            }
            return View(model);
        }

        // POST: /Notification/MarkAsRead/{id} (UPDATE: Mark as read)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAsRead(int id)
        {
            try
            {
                _notificationService.MarkAsRead(id);
                TempData["SuccessMessage"] = "Notification marked as read.";
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Error marking notification as read: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Notification/MarkAllAsRead (UPDATE: Mark all as read for a user)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAllAsRead(string userId)
        {
            try
            {
                _notificationService.MarkAllAsReadForUser(userId);
                TempData["SuccessMessage"] = "All notifications marked as read.";
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Error marking all notifications as read: {ex.Message}";
            }
            return RedirectToAction(nameof(UserNotifications), new { userId });
        }

        // POST: /Notification/Delete/{id} (DELETE: Process deletion)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _notificationService.DeleteNotification(id);
                TempData["SuccessMessage"] = "Notification deleted successfully.";
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = "Notification not found or already deleted.";
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting notification: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Notification/Details/{id} (READ: View notification details)
        public IActionResult Details(int id)
        {
            var notification = _notificationService.GetNotificationDetails(id);
            if (notification == null)
            {
                return NotFound();
            }

            // Mark as read when viewing details
            if (!notification.IsRead)
            {
                _notificationService.MarkAsRead(id);
            }

            return View(notification);
        }

        // GET: /Notification/UserNotifications/{userId} (READ: List notifications for a user)
        public IActionResult UserNotifications(string userId)
        {
            var notifications = _notificationService.GetNotificationsByUserId(userId);
            ViewBag.UnreadCount = _notificationService.GetUnreadCountForUser(userId);
            return View(notifications);
        }

        // GET: /Notification/UnreadNotifications/{userId} (READ: List unread notifications)
        public IActionResult UnreadNotifications(string userId)
        {
            var notifications = _notificationService.GetUnreadNotificationsByUserId(userId);
            return View(notifications);
        }

        // GET: /Notification/GetUnreadCount/{userId} (API: Get unread count)
        [HttpGet]
        public IActionResult GetUnreadCount(string userId)
        {
            var count = _notificationService.GetUnreadCountForUser(userId);
            return Json(new { count });
        }

        // GET: /Notification/GetRecentNotifications (API: Get recent notifications for dropdown)
        [HttpGet]
        public IActionResult GetRecentNotifications(string userId, int limit = 5)
        {
            var notifications = _notificationService.GetNotificationsByUserId(userId)
                .OrderByDescending(n => n.Timestamp)
                .Take(limit)
                .Select(n => new
                {
                    n.NotificationID,
                    n.Message,
                    n.Timestamp,
                    n.IsRead,
                    TimeAgo = GetTimeAgo(n.Timestamp)
                })
                .ToList();

            return Json(notifications);
        }

        // POST: /Notification/MarkAllAsReadAjax (API: Mark all as read via AJAX)
        [HttpPost]
        public IActionResult MarkAllAsReadAjax([FromForm] string userId)
        {
            try
            {
                _notificationService.MarkAllAsReadForUser(userId);
                return Json(new { success = true, message = "All notifications marked as read." });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // Helper method to calculate time ago
        private string GetTimeAgo(DateTime timestamp)
        {
            var timeSpan = DateTime.Now - timestamp;

            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)}w ago";

            return timestamp.ToString("MMM dd, yyyy");
        }
    }
}
