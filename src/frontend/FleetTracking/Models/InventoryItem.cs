using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetTracking.Models
{
    public class InventoryItem
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int CategoryId { get; set; }
        
        [Required]
        public string ItemCode { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string UnitOfMeasure { get; set; } = string.Empty;
        
        public decimal CurrentQuantity { get; set; }
        
        public decimal UnitPrice { get; set; }
        
        [NotMapped]
        public decimal UnitCost => UnitPrice; // Alias for backward compatibility
        
        public decimal ReorderPoint { get; set; }
        
        [NotMapped]
        public decimal MinimumQuantity => ReorderPoint; // Alias for backward compatibility
        
        public decimal ReorderQuantity { get; set; }
        
        [NotMapped]
        public decimal TotalValue => CurrentQuantity * UnitCost;
        
        public string Location { get; set; } = string.Empty;
        
        public string Barcode { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("CategoryId")]
        public InventoryCategory Category { get; set; }
    }
} 