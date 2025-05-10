using System;
using System.ComponentModel.DataAnnotations;

namespace FleetTracking.Models
{
    public class ScheduledReportVersion
    {
        public int Id { get; set; }
        
        [Required]
        public int ScheduledReportId { get; set; }
        
        [Required]
        [Display(Name = "File Name")]
        [StringLength(255)]
        public string FileName { get; set; }
        
        [Required]
        [Display(Name = "File Path")]
        [StringLength(1000)]
        public string FilePath { get; set; }
        
        [Required]
        [Display(Name = "File Size (KB)")]
        public long FileSizeKb { get; set; }
        
        [Required]
        [Display(Name = "Format")]
        [StringLength(10)]
        public string Format { get; set; }  // pdf, excel, csv
        
        [Required]
        [Display(Name = "Generated Date")]
        [DataType(DataType.DateTime)]
        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Parameters Used")]
        public string ParametersUsed { get; set; }  // JSON blob of parameters used
        
        [Display(Name = "Version Notes")]
        [StringLength(500)]
        public string VersionNotes { get; set; }
        
        // Navigation property
        public ScheduledReport ScheduledReport { get; set; }
    }
} 