using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetTracking.Models
{
    public class PurchaseOrderItem
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int PurchaseOrderId { get; set; }
        
        [Required]
        public int InventoryItemId { get; set; }
        
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public decimal Quantity { get; set; } = 1;
        
        public decimal ReceivedQuantity { get; set; } = 0;
        
        public decimal UnitPrice { get; set; } = 0;
        
        public decimal ExtendedPrice { get; set; } = 0;
        
        public decimal TaxRate { get; set; } = 0;
        
        [NotMapped]
        public decimal TaxAmount => Math.Round(ExtendedPrice * (TaxRate / 100), 2);
        
        [NotMapped]
        public decimal TotalPrice => ExtendedPrice + TaxAmount;
        
        public string Notes { get; set; } = string.Empty;
        
        // Navigation properties
        [ForeignKey("PurchaseOrderId")]
        public PurchaseOrder PurchaseOrder { get; set; }
        
        [ForeignKey("InventoryItemId")]
        public InventoryItem InventoryItem { get; set; }
    }
} 