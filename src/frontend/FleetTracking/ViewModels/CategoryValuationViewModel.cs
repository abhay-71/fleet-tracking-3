using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FleetTracking.Models;

namespace FleetTracking.ViewModels
{
    public class CategoryValuationViewModel
    {
        public int? CategoryId { get; set; }
        
        [Display(Name = "Category")]
        public string CategoryName { get; set; }
        
        [Display(Name = "Item Count")]
        public int ItemCount { get; set; }
        
        public List<InventoryItem> Items { get; set; } = new List<InventoryItem>();
        
        [Display(Name = "Total Value")]
        [DataType(DataType.Currency)]
        public decimal TotalValue { get; set; }
    }
} 