using System;
using System.ComponentModel.DataAnnotations;

namespace FleetTracking.Models
{
    public class InventoryTransaction
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Item")]
        public int ItemId { get; set; }
        
        [Required]
        [Display(Name = "Transaction Type")]
        [StringLength(20)]
        public string TransactionType { get; set; } // purchase, use, adjustment, transfer
        
        [Required]
        [Display(Name = "Quantity")]
        [Range(-99999, 99999)]
        public decimal Quantity { get; set; }
        
        [Display(Name = "Unit Cost")]
        [DataType(DataType.Currency)]
        public decimal? UnitCost { get; set; }
        
        [Display(Name = "Total Cost")]
        [DataType(DataType.Currency)]
        public decimal? TotalCost { get; set; }
        
        [Display(Name = "Reference ID")]
        [StringLength(50)]
        public string ReferenceId { get; set; } // PO number, etc.
        
        [Display(Name = "Notes")]
        [StringLength(500)]
        public string Notes { get; set; }
        
        [Display(Name = "Vehicle")]
        public int? VehicleId { get; set; }
        
        [Display(Name = "Maintenance Record")]
        public int? MaintenanceRecordId { get; set; }
        
        [Display(Name = "Performed By")]
        [StringLength(100)]
        public string PerformedById { get; set; }
        
        [Required]
        [Display(Name = "Transaction Date")]
        [DataType(DataType.DateTime)]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public InventoryItem Item { get; set; }
        public Vehicle Vehicle { get; set; }
        public MaintenanceRecord MaintenanceRecord { get; set; }
        public ApplicationUser PerformedBy { get; set; }
    }
} 