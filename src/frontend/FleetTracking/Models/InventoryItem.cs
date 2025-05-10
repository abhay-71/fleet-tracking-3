using System;
using System.ComponentModel.DataAnnotations;

namespace FleetTracking.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Item Name")]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Display(Name = "Description")]
        [StringLength(500)]
        public string Description { get; set; }
        
        [Display(Name = "Category")]
        public int? CategoryId { get; set; }
        
        [Required]
        [Display(Name = "SKU")]
        [StringLength(50)]
        public string SKU { get; set; }
        
        [Display(Name = "Barcode")]
        [StringLength(50)]
        public string Barcode { get; set; }
        
        [Required]
        [Display(Name = "Unit of Measure")]
        [StringLength(20)]
        public string UnitOfMeasure { get; set; } = "each";
        
        [Required]
        [Display(Name = "Current Quantity")]
        [Range(0, 99999)]
        public decimal CurrentQuantity { get; set; } = 0;
        
        [Display(Name = "Minimum Quantity")]
        [Range(0, 99999)]
        public decimal MinimumQuantity { get; set; } = 0;
        
        [Display(Name = "Reorder Quantity")]
        [Range(0, 99999)]
        public decimal ReorderQuantity { get; set; } = 0;
        
        [Display(Name = "Unit Cost")]
        [DataType(DataType.Currency)]
        public decimal? UnitCost { get; set; }
        
        [Display(Name = "Unit Price")]
        [DataType(DataType.Currency)]
        public decimal? UnitPrice { get; set; }
        
        [Display(Name = "Storage Location")]
        [StringLength(50)]
        public string Location { get; set; }
        
        [Required]
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Computed properties
        public decimal TotalValue => CurrentQuantity * (UnitCost ?? 0);
        
        [Display(Name = "Needs Reorder")]
        public bool NeedsReorder => CurrentQuantity <= MinimumQuantity && MinimumQuantity > 0;
        
        // Navigation properties
        public InventoryCategory Category { get; set; }
    }
} 