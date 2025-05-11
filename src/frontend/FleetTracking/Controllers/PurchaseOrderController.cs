using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FleetTracking.Data;
using FleetTracking.Models;
using Microsoft.AspNetCore.Authorization;
using FleetTracking.ViewModels;

namespace FleetTracking.Controllers
{
    [Authorize]
    public class PurchaseOrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PurchaseOrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PurchaseOrder
        public async Task<IActionResult> Index(string status, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.PurchaseOrders
                .Include(p => p.Vendor)
                .AsQueryable();
                
            // Apply filters if provided
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p => p.Status == status);
            }
            
            if (startDate.HasValue)
            {
                query = query.Where(p => p.OrderDate >= startDate.Value);
            }
            
            if (endDate.HasValue)
            {
                // Add one day to include the end date fully
                query = query.Where(p => p.OrderDate < endDate.Value.AddDays(1));
            }
            
            // Load order statuses for the filter dropdown
            ViewBag.Statuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "All Statuses" },
                new SelectListItem { Value = "draft", Text = "Draft" },
                new SelectListItem { Value = "pending", Text = "Pending Approval" },
                new SelectListItem { Value = "approved", Text = "Approved" },
                new SelectListItem { Value = "ordered", Text = "Ordered" },
                new SelectListItem { Value = "partial", Text = "Partially Received" },
                new SelectListItem { Value = "received", Text = "Received" },
                new SelectListItem { Value = "cancelled", Text = "Cancelled" }
            };
            
            // Set default dates if not provided
            ViewBag.StartDate = startDate ?? DateTime.Now.AddDays(-90);
            ViewBag.EndDate = endDate ?? DateTime.Now;
            ViewBag.Status = status;
            
            // Get results ordered by most recent first
            var purchaseOrders = await query
                .OrderByDescending(p => p.OrderDate)
                .ToListAsync();
                
            return View(purchaseOrders);
        }

        // GET: PurchaseOrder/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseOrder = await _context.PurchaseOrders
                .Include(p => p.Vendor)
                .Include(p => p.CreatedBy)
                .Include(p => p.ApprovedBy)
                .Include(p => p.Items)
                    .ThenInclude(i => i.InventoryItem)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (purchaseOrder == null)
            {
                return NotFound();
            }

            return View(purchaseOrder);
        }

        // GET: PurchaseOrder/Create
        public async Task<IActionResult> Create()
        {
            var vendors = await _context.Vendors.OrderBy(v => v.Name).ToListAsync();
            ViewBag.Vendors = new SelectList(vendors, "Id", "Name");
            
            // Generate a unique PO number
            string poNumber = GeneratePoNumber();
            
            return View(new PurchaseOrder { 
                PurchaseOrderNumber = poNumber,
                OrderDate = DateTime.Now,
                Status = "draft",
                Items = new List<PurchaseOrderItem>()
            });
        }

        // POST: PurchaseOrder/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VendorId,PurchaseOrderNumber,OrderDate,ExpectedDeliveryDate,Notes")] PurchaseOrder purchaseOrder)
        {
            if (ModelState.IsValid)
            {
                // Generate a new PO number
                var poNumber = GeneratePoNumber();
                // Create a new PurchaseOrder with the generated number
                purchaseOrder = new PurchaseOrder
                {
                    VendorId = purchaseOrder.VendorId,
                    PurchaseOrderNumber = poNumber, // Use PurchaseOrderNumber 
                    OrderDate = purchaseOrder.OrderDate,
                    ExpectedDeliveryDate = purchaseOrder.ExpectedDeliveryDate,
                    Notes = purchaseOrder.Notes,
                    Status = "draft",
                    CreatedById = User.Identity?.Name ?? "system",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _context.Add(purchaseOrder);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Purchase order created successfully! Add items to your order.";
                return RedirectToAction(nameof(Edit), new { id = purchaseOrder.Id });
            }
            
            var vendors = await _context.Vendors.OrderBy(v => v.Name).ToListAsync();
            ViewBag.Vendors = new SelectList(vendors, "Id", "Name", purchaseOrder.VendorId);
            
            return View(purchaseOrder);
        }

        // GET: PurchaseOrder/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var purchaseOrder = await _context.PurchaseOrders.FirstOrDefaultAsync(m => m.Id == id);
            if (purchaseOrder == null)
            {
                return NotFound();
            }
            
            // Get vendors for dropdown
            var vendorList = await _context.Vendors
                .Where(v => v.IsActive)
                .OrderBy(v => v.Name)
                .ToListAsync();
                
            ViewBag.Vendors = new SelectList(vendorList, "Id", "Name", purchaseOrder.VendorId);
            
            // Get items categories for add item dropdown
            var categories = await _context.InventoryCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
                
            ViewBag.Categories = categories;
            
            // Get all active inventory items for dropdown
            var allItems = await _context.InventoryItems
                .Include(i => i.Category)
                .Where(i => i.IsActive)
                .OrderBy(i => i.Name)
                .ToListAsync();
                
            ViewBag.AllItems = allItems;
            
            // Load related items if they exist
            var purchaseOrderWithItems = await _context.PurchaseOrders
                .Include(p => p.Vendor)
                .Include(p => p.Items)
                    .ThenInclude(i => i.InventoryItem)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            purchaseOrder.Items = purchaseOrderWithItems.Items;
            
            return View(purchaseOrder);
        }

        // POST: PurchaseOrder/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VendorId,PurchaseOrderNumber,OrderDate,ExpectedDeliveryDate,Notes,Status")] PurchaseOrder purchaseOrder)
        {
            if (id != purchaseOrder.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing PO to merge with
                    var existingPO = await _context.PurchaseOrders
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == id);
                        
                    if (existingPO == null)
                    {
                        return NotFound();
                    }
                    
                    // Only allow editing of draft or pending orders
                    if (existingPO.Status != "draft" && existingPO.Status != "pending")
                    {
                        TempData["ErrorMessage"] = "Only draft or pending purchase orders can be edited.";
                        return RedirectToAction(nameof(Details), new { id = id });
                    }
                    
                    // Preserve values that shouldn't be changed
                    purchaseOrder.CreatedById = existingPO.CreatedById;
                    purchaseOrder.CreatedAt = existingPO.CreatedAt;
                    purchaseOrder.ApprovedById = existingPO.ApprovedById;
                    purchaseOrder.ApprovedDate = existingPO.ApprovedDate;
                    purchaseOrder.PurchaseOrderNumber = existingPO.PurchaseOrderNumber; // Use PurchaseOrderNumber 
                    
                    // Update the timestamp
                    purchaseOrder.UpdatedAt = DateTime.UtcNow;
                    
                    // If changing to pending status, validate that it has items
                    if (purchaseOrder.Status == "pending" && existingPO.Status == "draft")
                    {
                        var hasItems = await _context.PurchaseOrderItems.AnyAsync(i => i.PurchaseOrderId == id);
                        if (!hasItems)
                        {
                            ModelState.AddModelError("", "Cannot submit for approval: This purchase order has no items.");
                            
                            // Reuse the already fetched active vendors
                            var availableVendors = await _context.Vendors
                                .Where(v => v.IsActive)
                                .OrderBy(v => v.Name)
                                .ToListAsync();
                                
                            ViewBag.Vendors = new SelectList(availableVendors, "Id", "Name", purchaseOrder.VendorId);
                            
                            var items = await _context.InventoryItems
                                .Where(i => i.IsActive)
                                .OrderBy(i => i.Name)
                                .ToListAsync();
                                
                            ViewBag.InventoryItems = new SelectList(items, "Id", "Name");
                            
                            // Load the items for display
                            var poWithItems = await _context.PurchaseOrders
                                .Include(p => p.Items)
                                    .ThenInclude(i => i.InventoryItem)
                                .FirstOrDefaultAsync(p => p.Id == id);
                                
                            purchaseOrder.Items = poWithItems.Items;
                            
                            return View(purchaseOrder);
                        }
                    }
                    
                    _context.Update(purchaseOrder);
                    await _context.SaveChangesAsync();
                    
                    if (existingPO.Status == "draft" && purchaseOrder.Status == "pending")
                    {
                        TempData["SuccessMessage"] = "Purchase order submitted for approval!";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Purchase order updated successfully!";
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurchaseOrderExists(purchaseOrder.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = purchaseOrder.Id });
            }

            // If we get this far, something failed, redisplay form
            var vendorList = await _context.Vendors
                .Where(v => v.IsActive)
                .OrderBy(v => v.Name)
                .ToListAsync();
                
            ViewBag.Vendors = new SelectList(vendorList, "Id", "Name", purchaseOrder.VendorId);
            
            // Get inventory items for dropdown
            var inventoryItems = await _context.InventoryItems
                .Where(i => i.IsActive)
                .OrderBy(i => i.Name)
                .ToListAsync();
                
            ViewBag.InventoryItems = new SelectList(inventoryItems, "Id", "Name");
            
            // Load the items for display
            var purchaseOrderWithItems = await _context.PurchaseOrders
                .Include(p => p.Items)
                    .ThenInclude(i => i.InventoryItem)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            purchaseOrder.Items = purchaseOrderWithItems.Items;
            
            return View(purchaseOrder);
        }
        
        // POST: PurchaseOrder/AddItem
        [HttpPost]
        public async Task<IActionResult> AddItem(int purchaseOrderId, int inventoryItemId, string description, 
                                                decimal quantity, decimal unitPrice, decimal? taxRate)
        {
            var purchaseOrder = await _context.PurchaseOrders.FindAsync(purchaseOrderId);
            if (purchaseOrder == null)
            {
                return NotFound();
            }
            
            // Only allow adding items to draft orders
            if (purchaseOrder.Status != "draft")
            {
                TempData["ErrorMessage"] = "Items can only be added to draft purchase orders.";
                return RedirectToAction(nameof(Edit), new { id = purchaseOrderId });
            }
            
            var inventoryItem = await _context.InventoryItems.FindAsync(inventoryItemId);
            if (inventoryItem == null)
            {
                return NotFound();
            }
            
            // Create the item
            var poItem = new PurchaseOrderItem
            {
                PurchaseOrderId = purchaseOrderId,
                InventoryItemId = inventoryItemId,
                Description = description ?? inventoryItem.Name,
                Quantity = quantity,
                UnitPrice = unitPrice,
                TaxRate = taxRate ?? 0m  // Using 0m to ensure decimal type
            };
            
            // Calculate extended price
            poItem.ExtendedPrice = poItem.Quantity * poItem.UnitPrice;
            
            // Add the item
            _context.PurchaseOrderItems.Add(poItem);
            
            // Update the purchase order totals
            await UpdatePurchaseOrderTotals(purchaseOrderId);
            
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Item added to purchase order successfully!";
            return RedirectToAction(nameof(Edit), new { id = purchaseOrderId });
        }
        
        // POST: PurchaseOrder/RemoveItem/5
        [HttpPost]
        public async Task<IActionResult> RemoveItem(int id)
        {
            var poItem = await _context.PurchaseOrderItems
                .Include(i => i.PurchaseOrder)
                .FirstOrDefaultAsync(i => i.Id == id);
                
            if (poItem == null)
            {
                return NotFound();
            }
            
            // Only allow removing items from draft orders
            if (poItem.PurchaseOrder.Status != "draft")
            {
                TempData["ErrorMessage"] = "Items can only be removed from draft purchase orders.";
                return RedirectToAction(nameof(Edit), new { id = poItem.PurchaseOrderId });
            }
            
            int purchaseOrderId = poItem.PurchaseOrderId;
            
            _context.PurchaseOrderItems.Remove(poItem);
            await _context.SaveChangesAsync();
            
            // Update the purchase order totals
            await UpdatePurchaseOrderTotals(purchaseOrderId);
            
            TempData["SuccessMessage"] = "Item removed from purchase order successfully!";
            return RedirectToAction(nameof(Edit), new { id = purchaseOrderId });
        }
        
        // POST: PurchaseOrder/Approve/5
        [HttpPost]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Approve(int id)
        {
            var purchaseOrder = await _context.PurchaseOrders.FindAsync(id);
            if (purchaseOrder == null)
            {
                return NotFound();
            }
            
            // Only allow approving pending orders
            if (purchaseOrder.Status != "pending")
            {
                TempData["ErrorMessage"] = "Only pending purchase orders can be approved.";
                return RedirectToAction(nameof(Details), new { id = id });
            }
            
            // Set approved information
            purchaseOrder.Status = "approved";
            purchaseOrder.ApprovedById = User.Identity?.Name ?? "system";
            purchaseOrder.ApprovedDate = DateTime.UtcNow; // Use ApprovedDate instead of ApprovalDate
            purchaseOrder.UpdatedAt = DateTime.UtcNow;
            
            _context.Update(purchaseOrder);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Purchase order approved successfully!";
            return RedirectToAction(nameof(Details), new { id = id });
        }
        
        // POST: PurchaseOrder/MarkAsOrdered/5
        [HttpPost]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> MarkAsOrdered(int id)
        {
            var purchaseOrder = await _context.PurchaseOrders.FindAsync(id);
            if (purchaseOrder == null)
            {
                return NotFound();
            }
            
            // Only allow marking approved orders as ordered
            if (purchaseOrder.Status != "approved")
            {
                TempData["ErrorMessage"] = "Only approved purchase orders can be marked as ordered.";
                return RedirectToAction(nameof(Details), new { id = id });
            }
            
            // Update status
            purchaseOrder.Status = "ordered";
            purchaseOrder.UpdatedAt = DateTime.UtcNow;
            
            _context.Update(purchaseOrder);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Purchase order marked as ordered successfully!";
            return RedirectToAction(nameof(Details), new { id = id });
        }
        
        // GET: PurchaseOrder/ReceiveItems/5
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> ReceiveItems(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseOrder = await _context.PurchaseOrders
                .Include(p => p.Vendor)
                .Include(p => p.Items)
                    .ThenInclude(i => i.InventoryItem)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (purchaseOrder == null)
            {
                return NotFound();
            }
            
            // Only allow receiving items for ordered, partial or received orders
            if (purchaseOrder.Status != "ordered" && purchaseOrder.Status != "partial" && purchaseOrder.Status != "received")
            {
                TempData["ErrorMessage"] = "Items can only be received for ordered purchase orders.";
                return RedirectToAction(nameof(Details), new { id = id });
            }
            
            return View(purchaseOrder);
        }
        
        // POST: PurchaseOrder/ReceiveItem
        [HttpPost]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> ReceiveItem(int purchaseOrderId, int itemId, decimal quantityReceived, string notes)
        {
            var purchaseOrder = await _context.PurchaseOrders.FindAsync(purchaseOrderId);
            if (purchaseOrder == null)
            {
                return NotFound();
            }
            
            // Only allow receiving items for ordered or partial orders
            if (purchaseOrder.Status != "ordered" && purchaseOrder.Status != "partial")
            {
                TempData["ErrorMessage"] = "Items can only be received for ordered purchase orders.";
                return RedirectToAction(nameof(ReceiveItems), new { id = purchaseOrderId });
            }
            
            var poItem = await _context.PurchaseOrderItems
                .Include(i => i.InventoryItem)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.PurchaseOrderId == purchaseOrderId);
                
            if (poItem == null)
            {
                return NotFound();
            }
            
            // Validate the quantity
            if (quantityReceived <= 0)
            {
                TempData["ErrorMessage"] = "Quantity received must be greater than zero.";
                return RedirectToAction(nameof(ReceiveItems), new { id = purchaseOrderId });
            }
            
            if (poItem.ReceivedQuantity + quantityReceived > poItem.Quantity)
            {
                TempData["ErrorMessage"] = $"Cannot receive more than ordered quantity. Maximum additional quantity: {poItem.Quantity - poItem.ReceivedQuantity}.";
                return RedirectToAction(nameof(ReceiveItems), new { id = purchaseOrderId });
            }
            
            // Update the received quantity
            poItem.ReceivedQuantity += quantityReceived;
            _context.Update(poItem);
            
            // Create new inventory transaction
            var transaction = new InventoryTransaction
            {
                ItemId = poItem.InventoryItemId,
                Quantity = quantityReceived,
                UnitCost = poItem.UnitPrice,
                TransactionType = "purchase",
                ReferenceId = purchaseOrderId.ToString(),
                TransactionDate = DateTime.UtcNow,
                Note = notes,
                PerformedById = User.Identity?.Name ?? "system",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            _context.InventoryTransactions.Add(transaction);
            
            // Update the inventory item quantity
            var inventoryItem = await _context.InventoryItems.FindAsync(poItem.InventoryItemId);
            if (inventoryItem != null)
            {
                inventoryItem.CurrentQuantity += quantityReceived;
                inventoryItem.UpdatedAt = DateTime.UtcNow;
                _context.Update(inventoryItem);
            }
            
            // Check if all items have been fully received
            await _context.SaveChangesAsync();
            
            // Update the purchase order status based on receipt status
            await UpdatePurchaseOrderStatus(purchaseOrderId);
            
            TempData["SuccessMessage"] = "Item received successfully!";
            return RedirectToAction(nameof(ReceiveItems), new { id = purchaseOrderId });
        }
        
        // POST: PurchaseOrder/Cancel/5
        [HttpPost]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Cancel(int id)
        {
            var purchaseOrder = await _context.PurchaseOrders.FindAsync(id);
            if (purchaseOrder == null)
            {
                return NotFound();
            }
            
            // Only allow cancelling orders that are not already received
            if (purchaseOrder.Status == "received")
            {
                TempData["ErrorMessage"] = "Cannot cancel a purchase order that has been fully received.";
                return RedirectToAction(nameof(Details), new { id = id });
            }
            
            // Update status
            purchaseOrder.Status = "cancelled";
            purchaseOrder.UpdatedAt = DateTime.UtcNow;
            
            _context.Update(purchaseOrder);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Purchase order cancelled successfully!";
            return RedirectToAction(nameof(Details), new { id = id });
        }
        
        // GET: PurchaseOrder/Summary
        public async Task<IActionResult> Summary()
        {
            // Get counts by status
            var allOrders = await _context.PurchaseOrders.ToListAsync();
            
            var summary = new PurchaseOrderSummaryViewModel
            {
                TotalOrders = allOrders.Count,
                PendingOrders = allOrders.Count(p => p.Status == "draft" || p.Status == "pending"),
                ApprovedOrders = allOrders.Count(p => p.Status == "approved" || p.Status == "ordered"),
                CompletedOrders = allOrders.Count(p => p.Status == "received"),
                TotalAmount = allOrders.Sum(p => p.Total),
                PendingAmount = allOrders.Where(p => p.Status == "draft" || p.Status == "pending" || p.Status == "approved" || p.Status == "ordered").Sum(p => p.Total)
            };
            
            // Get low stock items
            var lowStockItems = await _context.InventoryItems
                .Include(i => i.Category)
                .Where(i => i.IsActive && i.CurrentQuantity <= i.MinimumQuantity && i.MinimumQuantity > 0)
                .OrderBy(i => i.Name)
                .ToListAsync();
                
            ViewBag.LowStockItems = lowStockItems;
            ViewBag.LowStockCount = lowStockItems.Count;
            
            // Get recent purchase orders
            var recentOrders = await _context.PurchaseOrders
                .Include(p => p.Vendor)
                .OrderByDescending(p => p.OrderDate)
                .Take(5)
                .ToListAsync();
                
            ViewBag.RecentOrders = recentOrders;
            
            return View(summary);
        }
        
        // GET: PurchaseOrder/ValuationReport
        public async Task<IActionResult> ValuationReport(DateTime? startDate, DateTime? endDate, int? categoryId)
        {
            var query = _context.InventoryItems.AsQueryable();
            
            if (categoryId.HasValue)
            {
                query = query.Where(i => i.CategoryId == categoryId);
            }
            
            // Get items with their average cost from transactions
            var items = await query
                .Include(i => i.Category)
                .OrderBy(i => i.Category.Name)
                .ThenBy(i => i.Name)
                .Select(i => new
                {
                    Item = i,
                    AvgCost = _context.InventoryTransactions
                        .Where(t => t.ItemId == i.Id && 
                                   (startDate == null || t.TransactionDate >= startDate) &&
                                   (endDate == null || t.TransactionDate <= endDate) &&
                                   t.TransactionType != "use" && 
                                   t.TransactionType != "adjustment")
                        .Average(t => (decimal?)t.UnitCost) ?? 0m
                })
                .ToListAsync();
            
            // Calculate total values and group by category
            var valuationReport = items
                .GroupBy(i => i.Item.Category?.Name ?? "Uncategorized")
                .Select(g => new InventoryValuationViewModel
                {
                    CategoryName = g.Key,
                    Items = g.Select(i => new InventoryItemValuationViewModel
                    {
                        Id = i.Item.Id,
                        SKU = i.Item.SKU,
                        Name = i.Item.Name,
                        CurrentQuantity = i.Item.CurrentQuantity,
                        UnitCost = i.AvgCost > 0m ? i.AvgCost : i.Item.UnitCost,
                        TotalValue = i.Item.CurrentQuantity * (i.AvgCost > 0m ? i.AvgCost : i.Item.UnitCost),
                        Status = i.Item.CurrentQuantity <= 0m ? "Out of Stock" :
                               i.Item.CurrentQuantity <= i.Item.MinimumQuantity ? "Low Stock" : "In Stock"
                    }).ToList()
                })
                .OrderBy(vr => vr.CategoryName)
                .ToList();
            
            // Calculate summary totals
            var totalItems = valuationReport.Sum(vr => vr.Items.Count);
            var totalQuantity = valuationReport.Sum(vr => vr.Items.Sum(i => i.CurrentQuantity));
            var totalValue = valuationReport.Sum(vr => vr.Items.Sum(i => i.TotalValue));
            
            // Pass the data to the view
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalQuantity = totalQuantity;
            ViewBag.TotalValue = totalValue;
            ViewBag.ReportDate = DateTime.Now;
            ViewBag.DateRange = $"{startDate?.ToShortDateString() ?? "All"} to {endDate?.ToShortDateString() ?? "Present"}";
            
            // Get categories for filter dropdown
            ViewBag.Categories = new SelectList(await _context.InventoryCategories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", categoryId);
            
            return View(valuationReport);
        }
        
        // Helper methods
        private async Task UpdatePurchaseOrderTotals(int purchaseOrderId)
        {
            var purchaseOrder = await _context.PurchaseOrders.FindAsync(purchaseOrderId);
            if (purchaseOrder == null)
            {
                return;
            }
            
            var items = await _context.PurchaseOrderItems
                .Where(i => i.PurchaseOrderId == purchaseOrderId)
                .ToListAsync();
                
            // Set TotalAmount only - Subtotal, TaxAmount, and Total are computed
            purchaseOrder.TotalAmount = items.Sum(i => i.ExtendedPrice);
            purchaseOrder.UpdatedAt = DateTime.UtcNow;
            
            _context.Update(purchaseOrder);
            await _context.SaveChangesAsync();
        }
        
        private async Task UpdatePurchaseOrderStatus(int purchaseOrderId)
        {
            var purchaseOrder = await _context.PurchaseOrders.FindAsync(purchaseOrderId);
            if (purchaseOrder == null)
            {
                return;
            }
            
            // Get all items for this PO
            var items = await _context.PurchaseOrderItems
                .Where(i => i.PurchaseOrderId == purchaseOrderId)
                .ToListAsync();
                
            if (!items.Any())
            {
                return;
            }
            
            // Check if all items are fully received
            bool allReceived = items.All(i => i.ReceivedQuantity >= i.Quantity);
            bool anyReceived = items.Any(i => i.ReceivedQuantity > 0);
            
            // Update status based on receipt status
            if (allReceived)
            {
                purchaseOrder.Status = "received";
            }
            else if (anyReceived)
            {
                purchaseOrder.Status = "partial";
            }
            
            purchaseOrder.UpdatedAt = DateTime.UtcNow;
            
            _context.Update(purchaseOrder);
            await _context.SaveChangesAsync();
        }
        
        private string GeneratePoNumber()
        {
            // Generate a unique PO number in format: PO-YYYYMMDD-####
            string dateComponent = DateTime.Now.ToString("yyyyMMdd");
            
            // Get the highest sequential number used today
            var lastPo = _context.PurchaseOrders
                .Where(p => p.PurchaseOrderNumber.StartsWith($"PO-{dateComponent}"))
                .OrderByDescending(p => p.PurchaseOrderNumber)
                .FirstOrDefault();
                
            int sequence = 1;
            
            if (lastPo != null && lastPo.PurchaseOrderNumber.Length > 14) // PO-YYYYMMDD-####
            {
                string sequenceStr = lastPo.PurchaseOrderNumber.Substring(12);
                if (int.TryParse(sequenceStr, out int lastSequence))
                {
                    sequence = lastSequence + 1;
                }
            }
            
            return $"PO-{dateComponent}-{sequence:D4}";
        }

        private bool PurchaseOrderExists(int id)
        {
            return _context.PurchaseOrders.Any(e => e.Id == id);
        }
    }
} 