using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FleetTracking.Models;

namespace FleetTracking.ViewModels
{
    // View model for item usage reporting
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
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal QuantityUsed { get; set; }
        
        [Display(Name = "Total Cost")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal TotalCost { get; set; }
        
        [Display(Name = "Usage Frequency")]
        public int UsageCount { get; set; }
        
        [Display(Name = "Average Cost Per Use")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal AverageCostPerUse => UsageCount > 0 ? TotalCost / UsageCount : 0;
    }
    
    // View model for inventory valuation by category
    public class CategoryValuationViewModel
    {
        public int? CategoryId { get; set; }
        
        [Display(Name = "Category")]
        public string CategoryName { get; set; }
        
        [Display(Name = "Item Count")]
        public int ItemCount { get; set; }
        
        public List<InventoryItem> Items { get; set; }
        
        [Display(Name = "Total Value")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal TotalValue { get; set; }
        
        [Display(Name = "Average Item Value")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal AverageItemValue => ItemCount > 0 ? TotalValue / ItemCount : 0;
    }
    
    // View model for inventory transaction filters
    public class InventoryTransactionFilterViewModel
    {
        [Display(Name = "Transaction Type")]
        public string TransactionType { get; set; }
        
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }
        
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
        
        [Display(Name = "Item")]
        public int? ItemId { get; set; }
    }
    
    // View model for purchase order overview
    public class PurchaseOrderSummaryViewModel
    {
        [Display(Name = "Total Orders")]
        public int TotalOrders { get; set; }
        
        [Display(Name = "Pending Orders")]
        public int PendingOrders { get; set; }
        
        [Display(Name = "Approved Orders")]
        public int ApprovedOrders { get; set; }
        
        [Display(Name = "Completed Orders")]
        public int CompletedOrders { get; set; }
        
        [Display(Name = "Total Amount")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal TotalAmount { get; set; }
        
        [Display(Name = "Pending Amount")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal PendingAmount { get; set; }
    }
    
    // View model for low stock items
    public class LowStockViewModel
    {
        public List<InventoryItem> LowStockItems { get; set; }
        
        [Display(Name = "Total Items")]
        public int TotalItems { get; set; }
        
        [Display(Name = "Total Value")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal TotalValue { get; set; }
        
        [Display(Name = "Estimated Reorder Cost")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal EstimatedReorderCost { get; set; }
    }
} 