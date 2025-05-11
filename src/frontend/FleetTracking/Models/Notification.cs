using System;
using System.ComponentModel.DataAnnotations;

namespace FleetTracking.Models
{
    /// <summary>
    /// Represents a notification in the system
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Gets or sets the notification ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets the notification title
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        /// <summary>
        /// Gets or sets the notification message
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Message { get; set; }
        
        /// <summary>
        /// Gets or sets the notification timestamp (in UTC)
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Gets or sets the notification type
        /// </summary>
        public NotificationType Type { get; set; }
        
        /// <summary>
        /// Gets or sets the user ID this notification is for.
        /// If null, this is a broadcast notification.
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Gets or sets whether this notification has been read
        /// </summary>
        public bool IsRead { get; set; }
        
        /// <summary>
        /// Gets or sets whether this is a system notification
        /// </summary>
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the entity type this notification relates to (e.g., "Vehicle", "Driver", "Trip")
        /// </summary>
        public string EntityType { get; set; }
        
        /// <summary>
        /// Gets or sets the entity ID this notification relates to
        /// </summary>
        public int? EntityId { get; set; }
        
        /// <summary>
        /// Gets or sets a link URL for this notification
        /// </summary>
        public string LinkUrl { get; set; }
        
        /// <summary>
        /// Gets or sets additional data for this notification (JSON)
        /// </summary>
        public string Data { get; set; }
    }
    
    /// <summary>
    /// Notification type enum
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Information notification
        /// </summary>
        Info = 0,
        
        /// <summary>
        /// Success notification
        /// </summary>
        Success = 1,
        
        /// <summary>
        /// Warning notification
        /// </summary>
        Warning = 2,
        
        /// <summary>
        /// Error notification
        /// </summary>
        Error = 3,
        
        /// <summary>
        /// System notification
        /// </summary>
        System = 4,
        
        /// <summary>
        /// Alert notification
        /// </summary>
        Alert = 5
    }
} 