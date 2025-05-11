using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FleetTracking.Models
{
    public class Geofence
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Company")]
        public int CompanyId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Type")]
        public string Type { get; set; } = "polygon";

        [Required]
        [Display(Name = "Category")]
        public string Category { get; set; } = "custom";

        [StringLength(20)]
        [Display(Name = "Color")]
        public string Color { get; set; } = "#3388ff";

        [Required]
        [Display(Name = "Coordinates")]
        public string Coordinates { get; set; } = "[]";

        [Display(Name = "Radius (km)")]
        public double? Radius { get; set; }

        [Display(Name = "Active")]
        public bool Active { get; set; } = true;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Created By")]
        public int CreatedBy { get; set; }

        [Display(Name = "Last Modified By")]
        public int LastModified { get; set; }

        [Display(Name = "Metadata")]
        public string Metadata { get; set; }

        // Helper properties for UI not sent to API
        [JsonIgnore]
        public double? CenterLatitude { get; set; }

        [JsonIgnore]
        public double? CenterLongitude { get; set; }

        [JsonIgnore]
        public double? NorthEastLatitude { get; set; }

        [JsonIgnore]
        public double? NorthEastLongitude { get; set; }

        [JsonIgnore]
        public double? SouthWestLatitude { get; set; }

        [JsonIgnore]
        public double? SouthWestLongitude { get; set; }

        [JsonIgnore]
        public List<Vertex> Vertices { get; set; }

        public string TypeDisplayName
        {
            get
            {
                return Type switch
                {
                    "circle" => "Circle",
                    "polygon" => "Polygon",
                    "rectangle" => "Rectangle",
                    _ => Type,
                };
            }
        }

        public string CategoryDisplayName
        {
            get
            {
                return Category switch
                {
                    "restricted" => "Restricted Area",
                    "warehouse" => "Warehouse",
                    "customer" => "Customer Site",
                    "poi" => "Point of Interest",
                    "custom" => "Custom",
                    _ => Category,
                };
            }
        }

        public string StatusDisplayName => Active ? "Active" : "Inactive";

        public List<int> AssignedVehicleIds { get; set; } = new List<int>();
    }

    public class Vertex
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
} 