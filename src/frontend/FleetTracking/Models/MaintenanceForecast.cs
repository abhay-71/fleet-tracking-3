using System;
using System.ComponentModel.DataAnnotations;

namespace FleetTracking.Models
{
    public class MaintenanceForecast
    {
        public int Id { get; set; }
        
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; }
        
        [Display(Name = "Maintenance Type")]
        public int MaintenanceTypeId { get; set; }
        
        [Display(Name = "Predicted Due Date")]
        [DataType(DataType.Date)]
        public DateTime PredictedDueDate { get; set; }
        
        [Display(Name = "Confidence Level")]
        [Range(0, 100)]
        public int ConfidenceLevel { get; set; }
        
        [Display(Name = "Estimated Cost")]
        [DataType(DataType.Currency)]
        public decimal EstimatedCost { get; set; }
        
        [Display(Name = "Based On")]
        public string BasedOn { get; set; } // mileage, time, condition, etc.
        
        [Display(Name = "Last Maintenance Date")]
        [DataType(DataType.Date)]
        public DateTime? LastMaintenanceDate { get; set; }
        
        [Display(Name = "Days Until Due")]
        public int DaysUntilDue => (int)(PredictedDueDate - DateTime.Now).TotalDays;
        
        [Display(Name = "Priority")]
        public string Priority { get; set; } // high, medium, low
        
        [Display(Name = "Notes")]
        public string Notes { get; set; }
        
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }
        
        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public Vehicle Vehicle { get; set; }
        public MaintenanceType MaintenanceType { get; set; }
        
        // Computed properties
        [Display(Name = "Priority Level")]
        public string PriorityLevel
        {
            get
            {
                if (DaysUntilDue < 0)
                    return "Overdue";
                else if (DaysUntilDue < 7)
                    return "Critical";
                else if (DaysUntilDue < 30)
                    return "High";
                else if (DaysUntilDue < 90)
                    return "Medium";
                else
                    return "Low";
            }
        }
    }
} 