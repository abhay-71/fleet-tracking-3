using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FleetTracking.Data;

namespace FleetTracking.Models
{
    public class InventoryTransaction
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ItemId { get; set; }
        
        public int? VehicleId { get; set; }
        
        public int? MaintenanceRecordId { get; set; }
        
        [Required]
        public string TransactionType { get; set; }
        
        [Required]
        public decimal Quantity { get; set; }
        
        public decimal UnitCost { get; set; }
        
        [NotMapped]
        public decimal TotalCost => Quantity * UnitCost;
        
        public string Note { get; set; }
        
        [NotMapped]
        public string Notes => Note; // Alias for backward compatibility
        
        public string ReferenceId { get; set; }
        
        [Required]
        public string PerformedById { get; set; }
        
        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("ItemId")]
        public InventoryItem Item { get; set; }
        
        [ForeignKey("VehicleId")]
        public Vehicle Vehicle { get; set; }
        
        [ForeignKey("MaintenanceRecordId")]
        public MaintenanceRecord MaintenanceRecord { get; set; }
        
        [ForeignKey("PerformedById")]
        public ApplicationUser PerformedBy { get; set; }
    }
} 