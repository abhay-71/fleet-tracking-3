using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace FleetTracking.Models
{
    public class ScheduledReportVersion
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ScheduledReportId { get; set; }
        
        [Required]
        public DateTime GeneratedDate { get; set; }
        
        [Required]
        public string FilePath { get; set; }
        
        public string FileSize { get; set; }
        
        public string Format { get; set; }
        
        public string Parameters { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Add missing properties needed by views
        [NotMapped]
        [Display(Name = "File Name")]
        public string FileName => Path.GetFileName(FilePath);
        
        [NotMapped]
        [Display(Name = "File Size (KB)")]
        public decimal FileSizeKb => !string.IsNullOrEmpty(FileSize) && decimal.TryParse(FileSize, out var size) ? Math.Round(size / 1024, 2) : 0;
        
        [NotMapped]
        [Display(Name = "Parameters Used")]
        public string ParametersUsed => Parameters;
        
        [NotMapped]
        [Display(Name = "Version Notes")]
        public string VersionNotes { get; set; } = string.Empty;
        
        // Navigation properties
        [ForeignKey("ScheduledReportId")]
        public ScheduledReport ScheduledReport { get; set; }
    }
} 