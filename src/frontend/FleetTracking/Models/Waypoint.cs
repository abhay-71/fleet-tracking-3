using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetTracking.Models
{
    public class Waypoint
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Trip")]
        public int TripId { get; set; }

        [Required]
        [Range(-90, 90)]
        public decimal Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public decimal Longitude { get; set; }

        [Range(-1000, 10000)]
        public decimal? Altitude { get; set; }

        [Range(0, 300)]
        [Display(Name = "Speed (km/h)")]
        public decimal? Speed { get; set; }

        [Range(0, 360)]
        [Display(Name = "Heading (degrees)")]
        public decimal? Heading { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Timestamp { get; set; }

        [Display(Name = "Date Created")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("TripId")]
        public Trip Trip { get; set; }

        // Helper properties
        [NotMapped]
        [Display(Name = "Coordinates")]
        public string Coordinates => $"{Latitude}, {Longitude}";

        [NotMapped]
        [Display(Name = "Time")]
        public string FormattedTimestamp => Timestamp.ToString("MM/dd/yyyy HH:mm:ss");
    }
} 