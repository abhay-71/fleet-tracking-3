using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetTracking.Models
{
    public class Vendor
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int CompanyId { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string ContactName { get; set; } = string.Empty;
        
        [NotMapped]
        public string ContactPerson => ContactName; // Alias for backward compatibility
        
        public string Phone { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
        
        public string Address { get; set; } = string.Empty;
        
        public string City { get; set; } = string.Empty;
        
        public string State { get; set; } = string.Empty;
        
        public string ZipCode { get; set; } = string.Empty;
        
        public string Country { get; set; } = string.Empty;
        
        public string VendorType { get; set; } = "other";
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("CompanyId")]
        public Company Company { get; set; }
        
        // Computed properties
        public string StatusDisplayName => IsActive ? "Active" : "Inactive";
        
        public string VendorTypeDisplayName
        {
            get
            {
                return VendorType switch
                {
                    "maintenance" => "Maintenance",
                    "fuel" => "Fuel",
                    "parts" => "Parts",
                    "service" => "Service",
                    "tires" => "Tires",
                    "insurance" => "Insurance",
                    "other" => "Other",
                    _ => VendorType,
                };
            }
        }
    }
} 