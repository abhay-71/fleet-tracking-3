using System;
using System.ComponentModel.DataAnnotations;

namespace FleetTracking.Models
{
    public class Vendor
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Name")]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Display(Name = "Contact Person")]
        [StringLength(100)]
        public string ContactPerson { get; set; }
        
        [Display(Name = "Phone")]
        [StringLength(20)]
        public string Phone { get; set; }
        
        [Display(Name = "Email")]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }
        
        [Display(Name = "Address")]
        [StringLength(255)]
        public string Address { get; set; }
        
        [Display(Name = "Payment Terms")]
        [StringLength(50)]
        public string PaymentTerms { get; set; }
        
        [Display(Name = "Vendor Type")]
        [StringLength(50)]
        public string VendorType { get; set; }  // maintenance, fuel, parts, etc.
        
        [Required]
        [Display(Name = "Company ID")]
        public int CompanyId { get; set; } = 1; // Default to 1 for demo
        
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
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