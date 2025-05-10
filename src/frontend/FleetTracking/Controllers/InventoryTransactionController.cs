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
    public class InventoryTransactionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryTransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: InventoryTransaction
        public async Task<IActionResult> Index(string transactionType, DateTime? startDate, DateTime? endDate, int? itemId)
        {
            var query = _context.InventoryTransactions
                .Include(t => t.Item)
                .Include(t => t.Vehicle)
                .Include(t => t.MaintenanceRecord)
                .AsQueryable();
                
            // Apply filters if provided
            if (!string.IsNullOrWhiteSpace(transactionType))
            {
                query = query.Where(t => t.TransactionType == transactionType);
            }
            
            if (startDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= startDate.Value);
            }
            
            if (endDate.HasValue)
            {
                // Add one day to include the end date fully
                query = query.Where(t => t.TransactionDate < endDate.Value.AddDays(1));
            }
            
            if (itemId.HasValue)
            {
                query = query.Where(t => t.ItemId == itemId);
            }
            
            // Load items for the filter dropdown
            ViewBag.Items = new SelectList(await _context.InventoryItems.OrderBy(i => i.Name).ToListAsync(), "Id", "Name", itemId);
            
            // Load transaction types for the filter dropdown
            ViewBag.TransactionTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "All Types" },
                new SelectListItem { Value = "initial", Text = "Initial" },
                new SelectListItem { Value = "purchase", Text = "Purchase" },
                new SelectListItem { Value = "use", Text = "Use/Consumption" },
                new SelectListItem { Value = "adjustment", Text = "Adjustment" },
                new SelectListItem { Value = "return", Text = "Return" },
                new SelectListItem { Value = "transfer", Text = "Transfer" },
                new SelectListItem { Value = "scrap", Text = "Scrap/Waste" }
            };
            
            // Set default dates if not provided
            ViewBag.StartDate = startDate ?? DateTime.Now.AddDays(-30);
            ViewBag.EndDate = endDate ?? DateTime.Now;
            ViewBag.TransactionType = transactionType;
            
            // Get results ordered by most recent first
            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
                
            return View(transactions);
        }

        // GET: InventoryTransaction/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.InventoryTransactions
                .Include(t => t.Item)
                .Include(t => t.Vehicle)
                .Include(t => t.MaintenanceRecord)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }
        
        // GET: InventoryTransaction/VehicleHistory/5
        public async Task<IActionResult> VehicleHistory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }
            
            ViewBag.Vehicle = vehicle;
            
            var transactions = await _context.InventoryTransactions
                .Include(t => t.Item)
                .Where(t => t.VehicleId == id)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
                
            return View(transactions);
        }
        
        // GET: InventoryTransaction/ItemHistory/5
        public async Task<IActionResult> ItemHistory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var item = await _context.InventoryItems
                .Include(i => i.Category)
                .FirstOrDefaultAsync(i => i.Id == id);
                
            if (item == null)
            {
                return NotFound();
            }
            
            ViewBag.Item = item;
            
            var transactions = await _context.InventoryTransactions
                .Include(t => t.Vehicle)
                .Include(t => t.MaintenanceRecord)
                .Where(t => t.ItemId == id)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
                
            return View(transactions);
        }
        
        // GET: InventoryTransaction/Usage
        public async Task<IActionResult> Usage(DateTime? startDate, DateTime? endDate)
        {
            // Set default date range to last 30 days if not specified
            startDate ??= DateTime.Now.AddDays(-30).Date;
            endDate ??= DateTime.Now.Date;
            
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            
            // Query for transactions during the date range that are consumptions (use, scrap)
            var consumptionTransactions = await _context.InventoryTransactions
                .Include(t => t.Item)
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate.Value.AddDays(1))
                .Where(t => t.TransactionType == "use" || t.TransactionType == "scrap")
                .ToListAsync();
                
            // Group by item and calculate total consumption
            var usageReport = consumptionTransactions
                .GroupBy(t => t.ItemId)
                .Select(g => new ItemUsageViewModel
                {
                    ItemId = g.Key,
                    ItemName = g.First().Item.Name,
                    UnitOfMeasure = g.First().Item.UnitOfMeasure,
                    Category = g.First().Item.Category?.Name ?? "Uncategorized",
                    QuantityUsed = g.Sum(t => t.Quantity),
                    TotalCost = g.Sum(t => t.TotalCost ?? 0),
                    UsageCount = g.Count()
                })
                .OrderByDescending(i => i.TotalCost)
                .ToList();
                
            return View(usageReport);
        }
        
        // GET: InventoryTransaction/ValuationReport
        public async Task<IActionResult> ValuationReport()
        {
            // Get all active inventory items with their current quantities
            var inventoryItems = await _context.InventoryItems
                .Include(i => i.Category)
                .Where(i => i.IsActive)
                .OrderBy(i => i.Category.Name)
                .ThenBy(i => i.Name)
                .ToListAsync();
                
            // Group by category and calculate value
            var valuationReport = inventoryItems
                .GroupBy(i => i.CategoryId)
                .Select(g => new CategoryValuationViewModel
                {
                    CategoryId = g.Key,
                    CategoryName = g.First().Category?.Name ?? "Uncategorized",
                    ItemCount = g.Count(),
                    Items = g.ToList(),
                    TotalValue = g.Sum(i => i.TotalValue)
                })
                .OrderBy(c => c.CategoryName)
                .ToList();
                
            // Calculate overall totals
            ViewBag.TotalItems = inventoryItems.Count;
            ViewBag.TotalCategories = valuationReport.Count;
            ViewBag.TotalValue = inventoryItems.Sum(i => i.TotalValue);
            ViewBag.LowStockItems = inventoryItems.Count(i => i.NeedsReorder);
            
            return View(valuationReport);
        }
        
        // GET: InventoryTransaction/Delete/5
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.InventoryTransactions
                .Include(t => t.Item)
                .FirstOrDefaultAsync(t => t.Id == id);
                
            if (transaction == null)
            {
                return NotFound();
            }
            
            return View(transaction);
        }

        // POST: InventoryTransaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.InventoryTransactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            
            // Get the item to update its quantity
            var item = await _context.InventoryItems.FindAsync(transaction.ItemId);
            if (item == null)
            {
                return NotFound();
            }
            
            // Reverse the effect of this transaction
            if (transaction.TransactionType == "purchase" || transaction.TransactionType == "return")
            {
                // These added inventory, so removing them should decrease
                item.CurrentQuantity -= transaction.Quantity;
            }
            else if (transaction.TransactionType == "use" || transaction.TransactionType == "scrap")
            {
                // These decreased inventory, so removing them should increase
                item.CurrentQuantity += transaction.Quantity;
            }
            else if (transaction.TransactionType == "adjustment")
            {
                // Simply reverse the adjustment
                item.CurrentQuantity -= transaction.Quantity;
            }
            
            // Update timestamps
            item.UpdatedAt = DateTime.UtcNow;
            
            // Create a note in the transaction history about this correction
            var correctionTransaction = new InventoryTransaction
            {
                ItemId = item.Id,
                TransactionType = "adjustment",
                Quantity = -transaction.Quantity, // Opposite of the deleted transaction
                UnitCost = transaction.UnitCost,
                TotalCost = -(transaction.TotalCost ?? 0), // Negative of the original total
                Notes = $"Correction due to deletion of transaction {id}",
                TransactionDate = DateTime.UtcNow,
                PerformedById = User.Identity.Name,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.InventoryTransactions.Add(correctionTransaction);
            _context.InventoryTransactions.Remove(transaction);
            _context.Update(item);
            
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Transaction deleted and inventory corrected successfully!";
            return RedirectToAction(nameof(Index));
        }
        
        // Helper methods
        private bool TransactionExists(int id)
        {
            return _context.InventoryTransactions.Any(e => e.Id == id);
        }
    }
} 