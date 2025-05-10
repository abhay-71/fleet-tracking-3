using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FleetTracking.Models
{
    public class ScheduledReport
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Report Name")]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        [Display(Name = "Report Type")]
        [StringLength(50)]
        public string ReportType { get; set; }  // vehicle_status, trip_summary, fuel_consumption, etc.
        
        [Required]
        [Display(Name = "Format")]
        [StringLength(10)]
        public string Format { get; set; }  // pdf, excel, csv
        
        [Required]
        [Display(Name = "Recurrence Pattern")]
        [StringLength(20)]
        public string RecurrencePattern { get; set; }  // daily, weekly, monthly, quarterly
        
        [Display(Name = "Description")]
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;
        
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
        
        [Required]
        [Display(Name = "Next Run Date")]
        [DataType(DataType.DateTime)]
        public DateTime NextRunDate { get; set; }
        
        [Display(Name = "Last Run Date")]
        [DataType(DataType.DateTime)]
        public DateTime? LastRunDate { get; set; }
        
        [Required]
        [Display(Name = "Created By")]
        [StringLength(100)]
        public string CreatedByUserId { get; set; }
        
        [Required]
        [Display(Name = "Created Date")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Parameters")]
        public string Parameters { get; set; }  // JSON blob for custom parameters
        
        // Navigation properties
        public ApplicationUser CreatedByUser { get; set; }
        public ICollection<ScheduledReportRecipient> ScheduledReportRecipients { get; set; }
        public ICollection<ScheduledReportVersion> ScheduledReportVersions { get; set; }
    }
} 