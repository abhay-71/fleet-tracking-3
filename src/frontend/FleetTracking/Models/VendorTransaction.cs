using System;
using System.ComponentModel.DataAnnotations;

namespace FleetTracking.Models
{
    public class VendorTransaction
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Vendor")]
        public int VendorId { get; set; }
        
        [Required]
        [Display(Name = "Transaction Date")]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        [Display(Name = "Transaction Type")]
        [StringLength(50)]
        public string TransactionType { get; set; } = "purchase";  // purchase, service, maintenance, etc.
        
        [Required]
        [Display(Name = "Description")]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;
        
        [Display(Name = "Invoice Number")]
        [StringLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Amount")]
        [DataType(DataType.Currency)]
        [Range(0, 1000000)]
        public decimal Amount { get; set; } = 0;
        
        [Display(Name = "Tax Amount")]
        [DataType(DataType.Currency)]
        [Range(0, 1000000)]
        public decimal? TaxAmount { get; set; }
        
        [Display(Name = "Total Amount")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount => Amount + (TaxAmount ?? 0m);
        
        [Display(Name = "Vehicle")]
        public int? VehicleId { get; set; }
        
        [Display(Name = "Maintenance Record")]
        public int? MaintenanceRecordId { get; set; }
        
        [Display(Name = "Status")]
        [StringLength(50)]
        public string Status { get; set; } = "pending";  // pending, paid, cancelled, disputed
        
        [Display(Name = "Payment Date")]
        [DataType(DataType.Date)]
        public DateTime? PaymentDate { get; set; }
        
        [Display(Name = "Payment Method")]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;
        
        [Display(Name = "Payment Reference")]
        [StringLength(100)]
        public string PaymentReference { get; set; } = string.Empty;
        
        [Display(Name = "Notes")]
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
        
        [Display(Name = "Delivery Time (Days)")]
        public int? DeliveryTimeDays { get; set; }
        
        [Display(Name = "Quality Rating")]
        [Range(1, 5)]
        public int? QualityRating { get; set; }
        
        [Display(Name = "Service Rating")]
        [Range(1, 5)]
        public int? ServiceRating { get; set; }
        
        [Display(Name = "Timeliness Rating")]
        [Range(1, 5)]
        public int? TimelinessRating { get; set; }
        
        [Display(Name = "Price Rating")]
        [Range(1, 5)]
        public int? PriceRating { get; set; }
        
        [Display(Name = "Overall Rating")]
        [Range(1, 5)]
        public int? OverallRating { get; set; }
        
        [Display(Name = "Is On Time")]
        public bool? IsOnTime { get; set; }
        
        [Display(Name = "Is Within Budget")]
        public bool? IsWithinBudget { get; set; }
        
        [Display(Name = "Is Compliant")]
        public bool? IsCompliant { get; set; }
        
        [Required]
        [Display(Name = "Company ID")]
        public int CompanyId { get; set; } = 1; // Default to 1 for demo
        
        [Display(Name = "Created By")]
        public int CreatedBy { get; set; } = 1; // Default to 1 for demo
        
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public Vendor Vendor { get; set; }
        public Vehicle Vehicle { get; set; }
        public MaintenanceRecord MaintenanceRecord { get; set; }
        public Company Company { get; set; }
        
        // Computed properties
        public string StatusDisplayName
        {
            get
            {
                return Status switch
                {
                    "pending" => "Pending",
                    "paid" => "Paid",
                    "cancelled" => "Cancelled",
                    "disputed" => "Disputed",
                    _ => Status,
                };
            }
        }
        
        public string TransactionTypeDisplayName
        {
            get
            {
                return TransactionType switch
                {
                    "purchase" => "Purchase",
                    "service" => "Service",
                    "maintenance" => "Maintenance",
                    "fuel" => "Fuel",
                    "parts" => "Parts",
                    "repair" => "Repair",
                    "other" => "Other",
                    _ => TransactionType,
                };
            }
        }
        
        // Helper methods
        public double GetAverageRating()
        {
            int count = 0;
            int total = 0;
            
            if (QualityRating.HasValue) { total += QualityRating.Value; count++; }
            if (ServiceRating.HasValue) { total += ServiceRating.Value; count++; }
            if (TimelinessRating.HasValue) { total += TimelinessRating.Value; count++; }
            if (PriceRating.HasValue) { total += PriceRating.Value; count++; }
            
            return count > 0 ? (double)total / count : 0;
        }
    }
} 