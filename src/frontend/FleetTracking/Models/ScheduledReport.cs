using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FleetTracking.Data;

namespace FleetTracking.Models
{
    public class ScheduledReport
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Report Name")]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        [Display(Name = "Report Type")]
        [StringLength(50)]
        public string ReportType { get; set; }  // vehicle_status, trip_summary, fuel_consumption, etc.
        
        [Display(Name = "Description")]
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        [Display(Name = "Frequency")]
        [StringLength(20)]
        public string Frequency { get; set; }
        
        // Add missing RecurrencePattern property - alias for Frequency
        [NotMapped]
        [Display(Name = "Recurrence Pattern")]
        public string RecurrencePattern 
        { 
            get { return Frequency; }
            set { Frequency = value; }
        }
        
        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;
        
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
        
        [Required]
        [Display(Name = "Time")]
        [DataType(DataType.DateTime)]
        public DateTime Time { get; set; }
        
        [Required]
        [Display(Name = "File Format")]
        [StringLength(10)]
        public string FileFormat { get; set; }
        
        // Add missing Format property - alias for FileFormat
        [NotMapped]
        [Display(Name = "Format")]
        public string Format
        {
            get { return FileFormat; }
            set { FileFormat = value; }
        }
        
        [Required]
        [Display(Name = "Delivery Method")]
        [StringLength(50)]
        public string DeliveryMethod { get; set; }
        
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
        
        [Required]
        [Display(Name = "Created By")]
        [StringLength(100)]
        public string CreatedByUserId { get; set; }
        
        [Display(Name = "Last Run Date")]
        [DataType(DataType.DateTime)]
        public DateTime? LastRunDate { get; set; }
        
        [Required]
        [Display(Name = "Next Run Date")]
        [DataType(DataType.DateTime)]
        public DateTime NextRunDate { get; set; }
        
        [Required]
        [Display(Name = "Created Date")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        [Display(Name = "Updated Date")]
        [DataType(DataType.DateTime)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("CreatedByUserId")]
        public ApplicationUser CreatedByUser { get; set; }
        
        public ICollection<ScheduledReportRecipient> ScheduledReportRecipients { get; set; } = new List<ScheduledReportRecipient>();
        
        public ICollection<ScheduledReportVersion> ScheduledReportVersions { get; set; } = new List<ScheduledReportVersion>();
    }
} 