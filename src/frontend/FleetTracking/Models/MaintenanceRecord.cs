using System;
using System.ComponentModel.DataAnnotations;

namespace FleetTracking.Models
{
    public class MaintenanceRecord
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; }
        
        [Required]
        [Display(Name = "Maintenance Type")]
        public int MaintenanceTypeId { get; set; }
        
        [Required]
        [Display(Name = "Description")]
        [StringLength(200)]
        public string Description { get; set; }
        
        [Required]
        [Display(Name = "Scheduled Date")]
        [DataType(DataType.Date)]
        public DateTime ScheduledDate { get; set; }
        
        [Display(Name = "Completed Date")]
        [DataType(DataType.Date)]
        public DateTime? CompletedDate { get; set; }
        
        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } // scheduled, in_progress, completed, cancelled
        
        [Display(Name = "Odometer Reading")]
        public int? OdometerReading { get; set; }
        
        [Display(Name = "Estimated Cost")]
        [DataType(DataType.Currency)]
        public decimal? EstimatedCost { get; set; }
        
        [Display(Name = "Actual Cost")]
        [DataType(DataType.Currency)]
        public decimal? ActualCost { get; set; }
        
        [Display(Name = "Performed By")]
        [StringLength(100)]
        public string PerformedBy { get; set; }
        
        [Display(Name = "Notes")]
        [StringLength(500)]
        public string Notes { get; set; }
        
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }
        
        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public Vehicle Vehicle { get; set; }
        public MaintenanceType MaintenanceType { get; set; }
        
        // Computed properties
        [Display(Name = "Days Until Due")]
        public int DaysUntilDue => Status != "completed" && Status != "cancelled" 
            ? (int)(ScheduledDate - DateTime.Now).TotalDays 
            : 0;
            
        [Display(Name = "Is Overdue")]
        public bool IsOverdue => Status != "completed" && Status != "cancelled" && ScheduledDate < DateTime.Now;
        
        [Display(Name = "Status Display")]
        public string StatusDisplay
        {
            get
            {
                return Status switch
                {
                    "scheduled" => IsOverdue ? "Overdue" : "Scheduled",
                    "in_progress" => "In Progress",
                    "completed" => "Completed",
                    "cancelled" => "Cancelled",
                    _ => Status
                };
            }
        }
    }
} 