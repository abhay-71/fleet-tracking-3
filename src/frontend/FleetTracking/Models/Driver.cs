using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FleetTracking.Data;

namespace FleetTracking.Models
{
    public class Driver
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "User")]
        public string UserId { get; set; }

        [Required]
        [Display(Name = "Company")]
        public int CompanyId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "License Number")]
        public string LicenseNumber { get; set; }

        [StringLength(20)]
        [Display(Name = "License Class")]
        public string LicenseClass { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "License Expiry Date")]
        public DateTime LicenseExpiryDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "inactive";

        [Display(Name = "Date Created")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Last Updated")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }

        public ICollection<Trip> Trips { get; set; } = new List<Trip>();

        // Helper properties
        [NotMapped]
        [Display(Name = "License Status")]
        public string LicenseStatus => DateTime.Now > LicenseExpiryDate ? "Expired" : "Valid";

        [NotMapped]
        [Display(Name = "Days Until Expiry")]
        public int DaysUntilExpiry => (LicenseExpiryDate - DateTime.Now).Days;
    }
} 