using System.ComponentModel.DataAnnotations;

namespace FleetTracking.Models
{
    public class ScheduledReportRecipient
    {
        public int Id { get; set; }
        
        [Required]
        public int ScheduledReportId { get; set; }
        
        [Required]
        [Display(Name = "Recipient Email")]
        [EmailAddress]
        [StringLength(100)]
        public string RecipientEmail { get; set; }
        
        // Navigation property
        public ScheduledReport ScheduledReport { get; set; }
    }
} 