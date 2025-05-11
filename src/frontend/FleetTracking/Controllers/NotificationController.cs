using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FleetTracking.Models;
using FleetTracking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FleetTracking.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationService notificationService,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notifications = await _notificationService.GetNotificationsAsync(userId);
            
            ViewBag.Breadcrumbs = new[]
            {
                new { Title = "Notifications", Url = "#", IsActive = true }
            };
            
            return View(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notifications = await _notificationService.GetNotificationsAsync(userId);
            return Json(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Json(new { count });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _notificationService.MarkAsReadAsync(id, userId);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _notificationService.MarkAllAsReadAsync(userId);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _notificationService.DeleteNotificationAsync(id, userId);
            return Json(new { success = result });
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> SendSystemNotification(string title, string message, string linkUrl, NotificationType type)
        {
            var result = await _notificationService.SendSystemNotificationAsync(title, message, linkUrl, type);
            
            if (result)
            {
                TempData["SuccessMessage"] = "System notification sent successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to send system notification.";
            }
            
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            ViewBag.Breadcrumbs = new[]
            {
                new { Title = "Notifications", Url = "/Notification", IsActive = false },
                new { Title = "Create Notification", Url = "#", IsActive = true }
            };
            
            return View(new Notification
            {
                Timestamp = DateTime.UtcNow,
                Type = NotificationType.Info
            });
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(Notification notification)
        {
            if (!ModelState.IsValid)
            {
                return View(notification);
            }

            notification.Timestamp = DateTime.UtcNow;
            
            var result = await _notificationService.SendNotificationAsync(notification);
            
            if (result)
            {
                TempData["SuccessMessage"] = "Notification created successfully.";
                return RedirectToAction(nameof(Index));
            }
            
            TempData["ErrorMessage"] = "Failed to create notification.";
            return View(notification);
        }
    }
} 