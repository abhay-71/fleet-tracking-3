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
        [Display(Name = "Sequence")]
        public int Sequence { get; set; }

        [Required]
        [Display(Name = "Location Name")]
        public string LocationName { get; set; }

        [Required]
        [Display(Name = "Latitude")]
        public double Latitude { get; set; }

        [Required]
        [Display(Name = "Longitude")]
        public double Longitude { get; set; }

        [Display(Name = "Scheduled Arrival")]
        public DateTime? ScheduledArrival { get; set; }

        [Display(Name = "Actual Arrival")]
        public DateTime? ActualArrival { get; set; }

        [Display(Name = "Scheduled Departure")]
        public DateTime? ScheduledDeparture { get; set; }

        [Display(Name = "Actual Departure")]
        public DateTime? ActualDeparture { get; set; }

        [Display(Name = "Notes")]
        public string Notes { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = "pending";

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("TripId")]
        public Trip Trip { get; set; }

        // Helper properties
        [NotMapped]
        [Display(Name = "Status")]
        public string StatusDisplay
        {
            get
            {
                switch (Status.ToLower())
                {
                    case "pending": return "Pending";
                    case "arrived": return "Arrived";
                    case "departed": return "Departed";
                    case "skipped": return "Skipped";
                    case "delayed": return "Delayed";
                    default: return Status;
                }
            }
        }

        [NotMapped]
        public int SequenceNumber => Sequence;

        [NotMapped]
        public string Location => LocationName;
        
        [NotMapped]
        public DateTime? ArrivalTime => ActualArrival;
        
        [NotMapped]
        public DateTime? DepartureTime => ActualDeparture;
        
        [NotMapped]
        public string Event { get; set; }
        
        [NotMapped]
        public double Speed { get; set; }
        
        [NotMapped]
        public decimal FuelLevel { get; set; }

        [NotMapped]
        [Display(Name = "Delay")]
        public TimeSpan? ArrivalDelay
        {
            get
            {
                if (ScheduledArrival.HasValue && ActualArrival.HasValue)
                {
                    return ActualArrival.Value - ScheduledArrival.Value;
                }
                return null;
            }
        }

        [NotMapped]
        [Display(Name = "Stay Duration")]
        public TimeSpan? StayDuration
        {
            get
            {
                if (ActualArrival.HasValue && ActualDeparture.HasValue)
                {
                    return ActualDeparture.Value - ActualArrival.Value;
                }
                return null;
            }
        }
    }
} 