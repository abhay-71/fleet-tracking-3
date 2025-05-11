using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetTracking.Models;
using Microsoft.Extensions.Logging;

namespace FleetTracking.Services
{
    public interface INotificationService
    {
        Task<List<Notification>> GetNotificationsAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> MarkAsReadAsync(int notificationId, string userId);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<bool> DeleteNotificationAsync(int notificationId, string userId);
        Task<bool> SendNotificationAsync(Notification notification);
        Task<bool> SendSystemNotificationAsync(string title, string message, string linkUrl = null, NotificationType type = NotificationType.Info);
    }

    public class NotificationService : INotificationService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IApiService apiService, ILogger<NotificationService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<List<Notification>> GetNotificationsAsync(string userId)
        {
            try
            {
                var notifications = await _apiService.GetAsync<List<Notification>>($"notifications/user/{userId}");
                return notifications ?? new List<Notification>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get notifications for user {UserId}", userId);
                // Return empty list instead of throwing to not break the UI
                return new List<Notification>();
            }
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            try
            {
                var count = await _apiService.GetAsync<int>($"notifications/user/{userId}/unread/count");
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get unread notification count for user {UserId}", userId);
                return 0;
            }
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
        {
            try
            {
                var result = await _apiService.PutAsync<bool>($"notifications/{notificationId}/read", new { UserId = userId });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark notification {NotificationId} as read for user {UserId}", notificationId, userId);
                return false;
            }
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            try
            {
                var result = await _apiService.PutAsync<bool>($"notifications/user/{userId}/markallread", new { });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark all notifications as read for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId, string userId)
        {
            try
            {
                var result = await _apiService.DeleteAsync<bool>($"notifications/{notificationId}?userId={userId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete notification {NotificationId} for user {UserId}", notificationId, userId);
                return false;
            }
        }

        public async Task<bool> SendNotificationAsync(Notification notification)
        {
            try
            {
                var result = await _apiService.PostAsync<bool>("notifications", notification);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification");
                return false;
            }
        }

        public async Task<bool> SendSystemNotificationAsync(string title, string message, string linkUrl = null, NotificationType type = NotificationType.Info)
        {
            try
            {
                var notification = new Notification
                {
                    Title = title,
                    Message = message,
                    LinkUrl = linkUrl,
                    Type = type,
                    IsSystem = true,
                    Timestamp = DateTime.UtcNow
                };

                var result = await _apiService.PostAsync<bool>("notifications/system", notification);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send system notification");
                return false;
            }
        }
    }
} 