using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetTracking.Models
{
    public class Vehicle
    {
        // Default constructor to initialize required properties
        public Vehicle()
        {
            RegistrationNumber = string.Empty;
            Make = string.Empty;
            Model = string.Empty;
            FuelType = string.Empty;
            Status = "inactive";
            CurrentLocation = string.Empty;
            Trips = new List<Trip>();
            Company = new Company();
            VehicleType = new VehicleType();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Company")]
        public int CompanyId { get; set; }

        [Required]
        [Display(Name = "Vehicle Type")]
        public int VehicleTypeId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string Make { get; set; }

        [Required]
        [StringLength(50)]
        public string Model { get; set; }

        [Required]
        [Range(1900, 2100)]
        public int Year { get; set; }

        [StringLength(30)]
        public string? Color { get; set; }

        [StringLength(50)]
        [Display(Name = "VIN")]
        public string? VIN { get; set; }

        [StringLength(20)]
        [Display(Name = "License Plate")]
        public string? LicensePlate { get; set; }

        [StringLength(20)]
        [Display(Name = "Fuel Type")]
        public string FuelType { get; set; }

        [Range(0, 10000)]
        [Display(Name = "Fuel Capacity (L)")]
        public decimal FuelCapacity { get; set; }

        [Range(0, 10000)]
        [Display(Name = "Current Fuel Level (L)")]
        public decimal CurrentFuelLevel { get; set; }

        [Range(0, 10000000)]
        [Display(Name = "Odometer Reading (km)")]
        public decimal OdometerReading { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        [Display(Name = "Last Service Date")]
        [DataType(DataType.Date)]
        public DateTime? LastServiceDate { get; set; }

        [Display(Name = "Next Service Date")]
        [DataType(DataType.Date)]
        public DateTime? NextServiceDate { get; set; }

        [Display(Name = "Date Created")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Last Updated")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Range(0, 10000000)]
        [Display(Name = "Odometer Reading (km)")]
        public decimal Odometer { get; set; }

        [Range(0, 10000000)]
        [Display(Name = "Current Latitude")]
        public double? CurrentLatitude { get; set; }

        [Range(0, 10000000)]
        [Display(Name = "Current Longitude")]
        public double? CurrentLongitude { get; set; }

        [Range(0, 10000000)]
        [Display(Name = "Driver ID")]
        public int? DriverId { get; set; }

        [Display(Name = "Last Activity Date")]
        public DateTime? LastActivityDate { get; set; }

        [StringLength(255)]
        [Display(Name = "Current Location")]
        public string CurrentLocation { get; set; }

        // Navigation properties
        [ForeignKey("CompanyId")]
        public Company Company { get; set; }

        [ForeignKey("VehicleTypeId")]
        public VehicleType VehicleType { get; set; }

        public ICollection<Trip> Trips { get; set; }

        // Helper properties
        [NotMapped]
        [Display(Name = "Vehicle")]
        public string DisplayName => $"{Year} {Make} {Model} - {RegistrationNumber}";

        [NotMapped]
        [Display(Name = "Fuel Level")]
        public decimal FuelLevelPercentage => FuelCapacity > 0 ? Math.Round((CurrentFuelLevel / FuelCapacity) * 100, 1) : 0;
    }
} 