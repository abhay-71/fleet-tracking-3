using System;
using System.ComponentModel.DataAnnotations;

namespace FleetTracking.Models
{
    public class VehicleType
    {
        // Default constructor to initialize required properties
        public VehicleType()
        {
            Name = string.Empty;
            Description = string.Empty;
        }
        
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Range(0, 100000)]
        [Display(Name = "Maximum Load (kg)")]
        public decimal? MaximumLoad { get; set; }

        [Range(0, 100)]
        [Display(Name = "Number of Seats")]
        public int? NumberOfSeats { get; set; }

        [Display(Name = "Requires Special License")]
        public bool RequiresSpecialLicense { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
} 