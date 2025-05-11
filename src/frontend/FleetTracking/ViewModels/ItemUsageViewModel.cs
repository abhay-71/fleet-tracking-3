using System;
using System.ComponentModel.DataAnnotations;

namespace FleetTracking.ViewModels
{
    public class ItemUsageViewModel
    {
        public int ItemId { get; set; }
        
        [Display(Name = "Item Name")]
        public string ItemName { get; set; }
        
        [Display(Name = "Category")]
        public string Category { get; set; }
        
        [Display(Name = "Unit")]
        public string UnitOfMeasure { get; set; }
        
        [Display(Name = "Quantity Used")]
        public decimal QuantityUsed { get; set; }
        
        [Display(Name = "Total Cost")]
        [DataType(DataType.Currency)]
        public decimal TotalCost { get; set; }
        
        [Display(Name = "Usage Count")]
        public int UsageCount { get; set; }
        
        [Display(Name = "Average Cost Per Use")]
        [DataType(DataType.Currency)]
        public decimal AverageCostPerUse => UsageCount > 0 ? TotalCost / UsageCount : 0;
    }
} 