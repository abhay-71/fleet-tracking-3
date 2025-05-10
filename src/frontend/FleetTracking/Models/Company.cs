using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FleetTracking.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Address { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        [StringLength(50)]
        public string State { get; set; }

        [StringLength(50)]
        public string Country { get; set; }

        [StringLength(20)]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        [StringLength(20)]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        [StringLength(100)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [StringLength(100)]
        [DataType(DataType.Url)]
        public string Website { get; set; }

        [StringLength(255)]
        [Display(Name = "Logo URL")]
        [DataType(DataType.ImageUrl)]
        public string LogoUrl { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Date Created")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Last Updated")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public ICollection<Driver> Drivers { get; set; } = new List<Driver>();

        // Helper properties
        [Display(Name = "Full Address")]
        public string FullAddress => $"{Address}, {City}, {State} {PostalCode}, {Country}".Trim(' ', ',');
    }
} 