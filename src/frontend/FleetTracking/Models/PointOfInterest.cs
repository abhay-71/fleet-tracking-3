using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FleetTracking.Models
{
    public class PointOfInterest
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Name")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Display(Name = "Description")]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Latitude")]
        [Range(-90, 90)]
        public double Latitude { get; set; }
        
        [Required]
        [Display(Name = "Longitude")]
        [Range(-180, 180)]
        public double Longitude { get; set; }
        
        [Required]
        [Display(Name = "Category")]
        [StringLength(50)]
        public string Category { get; set; } = "custom"; // fuel_station, restaurant, rest_area, customer, warehouse, custom, etc.
        
        [Display(Name = "Icon")]
        [StringLength(50)]
        public string Icon { get; set; } = string.Empty; // CSS icon class or image path
        
        [Display(Name = "Address")]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;
        
        [Display(Name = "City")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;
        
        [Display(Name = "State")]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;
        
        [Display(Name = "Zip Code")]
        [StringLength(20)]
        public string ZipCode { get; set; } = string.Empty;
        
        [Display(Name = "Country")]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;
        
        [Display(Name = "Icon URL")]
        [StringLength(200)]
        public string IconUrl { get; set; } = string.Empty;
        
        [Display(Name = "Phone")]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;
        
        [Display(Name = "Website")]
        [StringLength(100)]
        public string Website { get; set; } = string.Empty;
        
        [Display(Name = "Operating Hours")]
        [StringLength(100)]
        public string OperatingHours { get; set; } = string.Empty;
        
        [Display(Name = "Tags")]
        [StringLength(200)]
        public string Tags { get; set; } = string.Empty; // Comma-separated tags for searching
        
        [Required]
        [Display(Name = "Company ID")]
        public int CompanyId { get; set; } = 1; // Default to 1 for demo
        
        [Display(Name = "Is Public")]
        public bool IsPublic { get; set; } = false;
        
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Created By")]
        public int CreatedBy { get; set; } = 1; // Default to 1 for demo
        
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Computed properties
        [JsonIgnore]
        public string CategoryDisplayName
        {
            get
            {
                return Category switch
                {
                    "fuel_station" => "Fuel Station",
                    "restaurant" => "Restaurant",
                    "rest_area" => "Rest Area",
                    "customer" => "Customer Location",
                    "warehouse" => "Warehouse",
                    "service_center" => "Service Center",
                    "parking" => "Parking",
                    "hotel" => "Hotel",
                    "custom" => "Custom",
                    _ => Category,
                };
            }
        }
        
        [JsonIgnore]
        public string StatusDisplayName => IsActive ? "Active" : "Inactive";
    }
} 