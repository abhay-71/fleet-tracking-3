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
        public int VehicleId { get; set; }

        [Required]
        [Display(Name = "Driver")]
        public int DriverId { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Start Location")]
        public string StartLocation { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "End Location")]
        public string EndLocation { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

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
        public string Notes { get; set; }

        [Display(Name = "Date Created")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Last Updated")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("VehicleId")]
        public Vehicle Vehicle { get; set; }

        [ForeignKey("DriverId")]
        public Driver Driver { get; set; }

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
    }
} 