using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetTracking.Models
{
    public class InventoryCategory
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Category Name")]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Display(Name = "Description")]
        [StringLength(500)]
        public string Description { get; set; }
        
        [Display(Name = "Parent Category")]
        public int? ParentCategoryId { get; set; }
        
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("ParentCategoryId")]
        public InventoryCategory ParentCategory { get; set; }
        
        public ICollection<InventoryCategory> ChildCategories { get; set; } = new List<InventoryCategory>();
        
        public ICollection<InventoryItem> Items { get; set; } = new List<InventoryItem>();
    }
} 