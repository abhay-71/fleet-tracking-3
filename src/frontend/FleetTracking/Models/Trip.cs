using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetTracking.Models
{
    public class Trip
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Vehicle")]
        public int? VehicleId { get; set; }

        [Required]
        [Display(Name = "Driver")]
        public int? DriverId { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Start Location")]
        public string StartLocation { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Display(Name = "End Location")]
        public string EndLocation { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        [Display(Name = "End Time")]
        public DateTime? EndTime { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "scheduled";

        [Range(0, 100000)]
        [Display(Name = "Distance (km)")]
        public decimal Distance { get; set; }

        [Range(0, 10000)]
        [Display(Name = "Fuel Used (L)")]
        public decimal FuelUsed { get; set; }

        [Range(0, 250)]
        [Display(Name = "Average Speed (km/h)")]
        public decimal AverageSpeed { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;

        [Display(Name = "Date Created")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Last Updated")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("VehicleId")]
        public Vehicle Vehicle { get; set; }

        [ForeignKey("DriverId")]
        public Driver Driver { get; set; }

        // Add Waypoints collection
        public ICollection<Waypoint> Waypoints { get; set; } = new List<Waypoint>();

        // Helper properties
        [NotMapped]
        [Display(Name = "Duration")]
        public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;

        [NotMapped]
        [Display(Name = "Efficiency")]
        public decimal? FuelEfficiency => Distance > 0 && FuelUsed > 0 ? Math.Round(Distance / FuelUsed, 2) : null;

        [NotMapped]
        [Display(Name = "Status")]
        public string StatusDisplay
        {
            get
            {
                switch (Status.ToLower())
                {
                    case "scheduled": return "Scheduled";
                    case "in_progress": return "In Progress";
                    case "completed": return "Completed";
                    case "cancelled": return "Cancelled";
                    default: return Status;
                }
            }
        }

        [NotMapped]
        [Display(Name = "Scheduled Start")]
        public string FormattedStartTime => StartTime.ToString("MM/dd/yyyy HH:mm");

        [NotMapped]
        [Display(Name = "Scheduled End")]
        public string FormattedEndTime => EndTime.HasValue ? EndTime.Value.ToString("MM/dd/yyyy HH:mm") : "-";

        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Max Speed (km/h)")]
        public double MaxSpeed { get; set; }

        [Display(Name = "Fuel Consumed (L)")]
        public double FuelConsumed { get; set; }

        [Display(Name = "Driver")]
        public string DriverName { get; set; } = string.Empty;

        [Display(Name = "Stop Count")]
        public int StopCount { get; set; }

        [Display(Name = "Idle Time")]
        public TimeSpan IdleTime { get; set; } = TimeSpan.Zero;

        [Display(Name = "Geofence Events")]
        public int GeofenceEvents { get; set; }

        [Display(Name = "Current Location")]
        public string CurrentLocation { get; set; } = string.Empty;

        // Calculate fuel economy (km/L)
        public double GetFuelEconomy()
        {
            if (FuelConsumed <= 0)
                return 0;
                
            return (double)Distance / FuelConsumed;
        }

        // Calculate average trip speed excluding stops
        public double GetMovingAverageSpeed()
        {
            if (!EndDate.HasValue)
                return 0;
                
            TimeSpan movingTime = EndDate.Value - StartDate;
            
            if (movingTime.TotalHours <= 0)
                return 0;
                
            return (double)Distance / movingTime.TotalHours;
        }
    }
} 