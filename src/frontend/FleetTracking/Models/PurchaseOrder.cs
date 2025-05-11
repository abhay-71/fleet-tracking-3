using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using FleetTracking.Data;

namespace FleetTracking.Models
{
    public class PurchaseOrder
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string PurchaseOrderNumber { get; set; }
        
        [NotMapped]
        public string PONumber => PurchaseOrderNumber; // Alias for backward compatibility
        
        [Required]
        public int VendorId { get; set; }
        
        [Required]
        public DateTime OrderDate { get; set; }
        
        public DateTime? ExpectedDeliveryDate { get; set; }
        
        public DateTime? ActualDeliveryDate { get; set; }
        
        [Required]
        public string Status { get; set; } // draft, pending, approved, received, cancelled, etc.
        
        public decimal TotalAmount { get; set; }
        
        public string Notes { get; set; }
        
        public string ShippingAddress { get; set; }
        
        public string BillingAddress { get; set; }
        
        public string PaymentTerms { get; set; }
        
        public string CreatedById { get; set; }
        
        public string ApprovedById { get; set; }
        
        public DateTime? ApprovedDate { get; set; }
        
        [NotMapped]
        public DateTime? ApprovalDate => ApprovedDate; // Alias for backward compatibility
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("VendorId")]
        public Vendor Vendor { get; set; }
        
        [ForeignKey("CreatedById")]
        public ApplicationUser CreatedBy { get; set; }
        
        [ForeignKey("ApprovedById")]
        public ApplicationUser ApprovedBy { get; set; }
        
        public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
        
        // Computed properties
        [NotMapped]
        public decimal Subtotal => Items?.Sum(i => i.ExtendedPrice) ?? 0m;
        
        [NotMapped]
        public decimal TaxAmount => Items?.Sum(i => i.TaxAmount) ?? 0m;
        
        [NotMapped]
        public decimal Total => Items?.Sum(i => i.TotalPrice) ?? TotalAmount;
    }
} 