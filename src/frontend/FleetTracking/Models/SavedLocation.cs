using System;
using System.ComponentModel.DataAnnotations;

namespace FleetTracking.Models
{
    public class SavedLocation
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [StringLength(250)]
        public string Address { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

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
        public string OriginalQuery { get; set; }
        public string FormattedAddress { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string PlaceId { get; set; }
        
        // Additional properties
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public double? Confidence { get; set; }
    }
} 