using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetTracking.Models
{
    public class Maintenance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; }

        [Required]
        [Display(Name = "Service Type")]
        public string ServiceType { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Service Date")]
        [DataType(DataType.Date)]
        public DateTime ServiceDate { get; set; }

        [Display(Name = "Next Service Date")]
        [DataType(DataType.Date)]
        public DateTime? NextServiceDate { get; set; }

        [Display(Name = "Odometer Reading")]
        public decimal OdometerReading { get; set; }

        [Display(Name = "Cost")]
        [DataType(DataType.Currency)]
        public decimal Cost { get; set; }

        [Display(Name = "Performed By")]
        public string PerformedBy { get; set; }

        [Display(Name = "Service Location")]
        public string ServiceLocation { get; set; }

        [Display(Name = "Parts Replaced")]
        public string PartsReplaced { get; set; }

        [Display(Name = "Notes")]
        public string Notes { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = "completed";

        [Display(Name = "Date Created")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Last Updated")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("VehicleId")]
        public Vehicle Vehicle { get; set; }

        // Helper properties
        [NotMapped]
        [Display(Name = "Maintenance Record")]
        public string DisplayName => $"{ServiceType} - {ServiceDate.ToShortDateString()} - {Vehicle?.RegistrationNumber ?? "N/A"}";

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
        [Display(Name = "Days Until Next Service")]
        public int? DaysUntilNextService => NextServiceDate.HasValue ? (NextServiceDate.Value - DateTime.Today).Days : null;
    }
} 