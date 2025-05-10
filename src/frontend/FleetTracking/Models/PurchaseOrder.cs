using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FleetTracking.Models
{
    public class PurchaseOrder
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Vendor")]
        public int VendorId { get; set; }
        
        [Required]
        [Display(Name = "PO Number")]
        [StringLength(50)]
        public string PONumber { get; set; }
        
        [Required]
        [Display(Name = "Order Date")]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; } = DateTime.Today;
        
        [Display(Name = "Expected Delivery")]
        [DataType(DataType.Date)]
        public DateTime? ExpectedDeliveryDate { get; set; }
        
        [Display(Name = "Notes")]
        [StringLength(500)]
        public string Notes { get; set; }
        
        [Required]
        [Display(Name = "Status")]
        [StringLength(20)]
        public string Status { get; set; } = "draft"; // draft, submitted, approved, received, cancelled
        
        [Display(Name = "Subtotal")]
        [DataType(DataType.Currency)]
        public decimal Subtotal { get; set; }
        
        [Display(Name = "Tax Amount")]
        [DataType(DataType.Currency)]
        public decimal TaxAmount { get; set; }
        
        [Display(Name = "Total")]
        [DataType(DataType.Currency)]
        public decimal Total => Subtotal + TaxAmount;
        
        [Display(Name = "Created By")]
        [StringLength(100)]
        public string CreatedById { get; set; }
        
        [Display(Name = "Approved By")]
        [StringLength(100)]
        public string ApprovedById { get; set; }
        
        [Display(Name = "Approval Date")]
        [DataType(DataType.DateTime)]
        public DateTime? ApprovalDate { get; set; }
        
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public Vendor Vendor { get; set; }
        public ApplicationUser CreatedBy { get; set; }
        public ApplicationUser ApprovedBy { get; set; }
        public ICollection<PurchaseOrderItem> Items { get; set; }
    }
} 