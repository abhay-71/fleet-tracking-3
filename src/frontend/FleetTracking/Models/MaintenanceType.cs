using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FleetTracking.Models
{
    public class MaintenanceType
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Name")]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Display(Name = "Description")]
        [StringLength(500)]
        public string Description { get; set; }
        
        [Display(Name = "Default Interval Days")]
        public int? DefaultIntervalDays { get; set; }
        
        [Display(Name = "Default Interval Distance")]
        public int? DefaultIntervalDistance { get; set; }
        
        [Display(Name = "Is Required")]
        public bool IsRequired { get; set; }
        
        [Display(Name = "Estimated Duration Hours")]
        public decimal? EstimatedDurationHours { get; set; }
        
        [Display(Name = "Estimated Cost")]
        [DataType(DataType.Currency)]
        public decimal? EstimatedCost { get; set; }
        
        [Display(Name = "Category")]
        public string Category { get; set; } // routine, repair, inspection, etc.
        
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }
        
        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }
        
        // Computed properties
        [Display(Name = "Interval Display")]
        public string IntervalDisplay
        {
            get
            {
                var parts = new List<string>();
                
                if (DefaultIntervalDays.HasValue && DefaultIntervalDays.Value > 0)
                {
                    parts.Add($"Every {DefaultIntervalDays} days");
                }
                
                if (DefaultIntervalDistance.HasValue && DefaultIntervalDistance.Value > 0)
                {
                    parts.Add($"Every {DefaultIntervalDistance} km");
                }
                
                return parts.Count > 0 ? string.Join(" or ", parts) : "No interval set";
            }
        }
    }
} 