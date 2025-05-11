using System;
using System.ComponentModel.DataAnnotations;

namespace FleetTracking.Models
{
    public class SavedLocation
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = "other";

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int CreatedBy { get; set; }

        public string TypeDisplayName
        {
            get
            {
                return Type switch
                {
                    "customer" => "Customer",
                    "warehouse" => "Warehouse",
                    "office" => "Office",
                    "rest_area" => "Rest Area",
                    "service_center" => "Service Center",
                    _ => "Other",
                };
            }
        }
    }
    
    public class GeocodingResult
    {
        public string OriginalQuery { get; set; } = string.Empty;
        public string FormattedAddress { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string PlaceId { get; set; } = string.Empty;
        
        // Additional properties
        public string Country { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public double? Confidence { get; set; }
    }
} 