using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FleetTracking.Data;
using System.Linq;

namespace FleetTracking.Models
{
    public class Driver
    {
        // Default constructor to initialize required properties
        public Driver()
        {
            UserId = string.Empty;
            LicenseNumber = string.Empty;
            LicenseClass = string.Empty;
            Status = "inactive";
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            Address = string.Empty;
            City = string.Empty;
            State = string.Empty;
            ZipCode = string.Empty;
            EmergencyContactName = string.Empty;
            EmergencyContactPhone = string.Empty;
            Trips = new List<Trip>();
        }

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
        public string Status { get; set; }

        [Display(Name = "Date Created")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Last Updated")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string FirstName { get; set; }
        
        [Required]
        public string LastName { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Phone]
        public string PhoneNumber { get; set; }
        
        public DateTime DateOfBirth { get; set; }
        
        public DateTime HireDate { get; set; }
        
        public string Address { get; set; }
        
        public string City { get; set; }
        
        public string State { get; set; }
        
        public string ZipCode { get; set; }
        
        public string EmergencyContactName { get; set; }
        
        public string EmergencyContactPhone { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }

        public ICollection<Trip> Trips { get; set; }

        // Helper properties
        [NotMapped]
        [Display(Name = "License Status")]
        public string LicenseStatus => DateTime.Now > LicenseExpiryDate ? "Expired" : "Valid";

        [NotMapped]
        [Display(Name = "Days Until Expiry")]
        public int DaysUntilExpiry => (LicenseExpiryDate - DateTime.Now).Days;
        
        // Added properties to fix build errors
        [NotMapped]
        public ICollection<Trip> AssignedTrips => Trips?.Where(t => t.Status == "scheduled" || t.Status == "in_progress").ToList() ?? new List<Trip>();
        
        [NotMapped]
        public ICollection<Trip> CurrentTrips => Trips?.Where(t => t.Status == "scheduled" || t.Status == "in_progress").ToList() ?? new List<Trip>();
        
        [NotMapped]
        public int CompletedTrips => Trips?.Count(t => t.Status == "completed") ?? 0;
        
        [NotMapped]
        public DateTime? LicenseExpiry => LicenseExpiryDate;
    }
} 