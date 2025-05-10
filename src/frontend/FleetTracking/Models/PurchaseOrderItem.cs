using System.ComponentModel.DataAnnotations;

namespace FleetTracking.Models
{
    public class PurchaseOrderItem
    {
        public int Id { get; set; }
        
        [Required]
        public int PurchaseOrderId { get; set; }
        
        [Required]
        [Display(Name = "Item")]
        public int? InventoryItemId { get; set; }
        
        [Required]
        [Display(Name = "Description")]
        [StringLength(200)]
        public string Description { get; set; }
        
        [Required]
        [Display(Name = "Quantity")]
        [Range(0.01, 9999)]
        public decimal Quantity { get; set; }
        
        [Required]
        [Display(Name = "Unit Price")]
        [DataType(DataType.Currency)]
        [Range(0, 999999)]
        public decimal UnitPrice { get; set; }
        
        [Required]
        [Display(Name = "Extended Price")]
        [DataType(DataType.Currency)]
        [Range(0, 999999)]
        public decimal ExtendedPrice => Quantity * UnitPrice;
        
        [Display(Name = "Tax Rate %")]
        [Range(0, 100)]
        public decimal TaxRate { get; set; }
        
        [Display(Name = "Tax Amount")]
        [DataType(DataType.Currency)]
        public decimal TaxAmount => ExtendedPrice * (TaxRate / 100);
        
        [Display(Name = "Total Price")]
        [DataType(DataType.Currency)]
        public decimal TotalPrice => ExtendedPrice + TaxAmount;
        
        [Display(Name = "Received Quantity")]
        [Range(0, 9999)]
        public decimal ReceivedQuantity { get; set; } = 0;
        
        // Navigation properties
        public PurchaseOrder PurchaseOrder { get; set; }
        public InventoryItem InventoryItem { get; set; }
    }
} 