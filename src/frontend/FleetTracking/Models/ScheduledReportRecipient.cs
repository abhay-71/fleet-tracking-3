using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetTracking.Models
{
    public class ScheduledReportRecipient
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ScheduledReportId { get; set; }
        
        [Required]
        public string Email { get; set; }
        
        public string Name { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("ScheduledReportId")]
        public ScheduledReport ScheduledReport { get; set; }
    }
} 