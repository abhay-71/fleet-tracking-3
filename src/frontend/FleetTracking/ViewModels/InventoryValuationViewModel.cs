using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FleetTracking.ViewModels
{
    public class InventoryValuationViewModel
    {
        public InventoryValuationViewModel()
        {
            Items = new List<InventoryItemValuationViewModel>();
        }
        
        [Display(Name = "Category")]
        public string CategoryName { get; set; }
        
        public List<InventoryItemValuationViewModel> Items { get; set; }
        
        // Add properties to match what's used in the controller
        public int ItemId { get; set; }
        
        [Display(Name = "Item Code")]
        public string ItemCode { get; set; }
        
        [Display(Name = "Item Name")]
        public string ItemName { get; set; }
        
        [Display(Name = "Unit of Measure")]
        public string UnitOfMeasure { get; set; }
        
        [Display(Name = "Current Quantity")]
        public decimal CurrentQuantity { get; set; }
        
        [Display(Name = "Average Cost")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal AverageCost { get; set; }
        
        [Display(Name = "Total Value")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal TotalValue { get; set; }
        
        [Display(Name = "Reorder Point")]
        public decimal ReorderPoint { get; set; }
        
        [Display(Name = "Status")]
        public string Status { get; set; }
    }
    
    public class InventoryItemValuationViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "SKU")]
        public string SKU { get; set; }
        
        [Display(Name = "Name")]
        public string Name { get; set; }
        
        [Display(Name = "Current Quantity")]
        public decimal CurrentQuantity { get; set; }
        
        [Display(Name = "Unit Cost")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal UnitCost { get; set; }
        
        [Display(Name = "Total Value")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal TotalValue { get; set; }
        
        [Display(Name = "Status")]
        public string Status { get; set; }
    }
} 