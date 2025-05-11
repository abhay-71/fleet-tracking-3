using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FleetTracking.Models
{
    public class Alert
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Company")]
        public int CompanyId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Type")]
        public string Type { get; set; } = "custom";

        [Required]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        [Display(Name = "Geofence")]
        public int? GeofenceId { get; set; }

        [Display(Name = "Vehicle")]
        public int? VehicleId { get; set; }

        [Display(Name = "Driver")]
        public int? DriverId { get; set; }

        [Display(Name = "Severity")]
        public string Severity { get; set; } = "medium";

        [Display(Name = "Notification Channels")]
        public string NotificationChannels { get; set; } = string.Empty;

        [Display(Name = "Recipients")]
        public string Recipients { get; set; } = string.Empty;

        [Display(Name = "Condition")]
        public string Condition { get; set; } = string.Empty;

        [Display(Name = "Threshold")]
        public double? Threshold { get; set; }

        [Display(Name = "Unit")]
        public string Unit { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool Active { get; set; } = true;

        [Display(Name = "Auto Resolve")]
        public bool AutoResolve { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Created By")]
        public int CreatedBy { get; set; }

        [Display(Name = "Last Modified By")]
        public int LastModified { get; set; }

        public string TypeDisplayName
        {
            get
            {
                return Type switch
                {
                    "geofence_entry" => "Geofence Entry",
                    "geofence_exit" => "Geofence Exit",
                    "speed" => "Speed Alert",
                    "idle_time" => "Idle Time",
                    "unauthorized_movement" => "Unauthorized Movement",
                    "driver_behavior" => "Driver Behavior",
                    "maintenance" => "Maintenance Alert",
                    "custom" => "Custom Alert",
                    _ => Type,
                };
            }
        }

        public string SeverityDisplayName
        {
            get
            {
                return Severity switch
                {
                    "low" => "Low",
                    "medium" => "Medium",
                    "high" => "High",
                    "critical" => "Critical",
                    _ => Severity,
                };
            }
        }

        public string SeverityBadgeClass
        {
            get
            {
                return Severity switch
                {
                    "low" => "bg-info",
                    "medium" => "bg-warning",
                    "high" => "bg-danger",
                    "critical" => "bg-danger text-white",
                    _ => "bg-secondary",
                };
            }
        }

        public string StatusDisplayName => Active ? "Active" : "Inactive";

        [JsonIgnore]
        public bool IsLocationBased => Type == "geofence_entry" || Type == "geofence_exit" || Type == "speed";

        public List<string> GetNotificationChannelsList()
        {
            if (string.IsNullOrEmpty(NotificationChannels))
                return new List<string>();

            return new List<string>(NotificationChannels.Split(','));
        }

        public List<string> GetRecipientsList()
        {
            if (string.IsNullOrEmpty(Recipients))
                return new List<string>();

            return new List<string>(Recipients.Split(','));
        }
    }

    public class AlertLog
    {
        public int Id { get; set; }
        public int AlertId { get; set; }
        public int? VehicleId { get; set; }
        public int? DriverId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Details { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool Acknowledged { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public int? AcknowledgedBy { get; set; }
        public string Resolution { get; set; } = string.Empty;
        public DateTime? ResolvedAt { get; set; }
        public string AlertType { get; set; } = string.Empty;
        public string AlertName { get; set; } = string.Empty;
        public string Severity { get; set; } = "medium";

        public string SeverityBadgeClass
        {
            get
            {
                return Severity switch
                {
                    "low" => "bg-info",
                    "medium" => "bg-warning",
                    "high" => "bg-danger",
                    "critical" => "bg-danger text-white",
                    _ => "bg-secondary",
                };
            }
        }

        public string StatusDisplayName => Acknowledged ? "Acknowledged" : "Pending";
    }
} 