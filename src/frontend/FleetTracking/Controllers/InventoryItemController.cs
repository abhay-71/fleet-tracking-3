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

namespace FleetTracking.Controllers
{
    [Authorize]
    public class InventoryItemController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryItemController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: InventoryItem
        public async Task<IActionResult> Index(int? categoryId)
        {
            ViewBag.Categories = new SelectList(await _context.InventoryCategories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", categoryId);
            
            var query = _context.InventoryItems.Include(i => i.Category).AsQueryable();
            
            if (categoryId.HasValue)
            {
                query = query.Where(i => i.CategoryId == categoryId);
                ViewBag.CategoryFilter = await _context.InventoryCategories.FindAsync(categoryId);
            }
            
            var items = await query.OrderBy(i => i.Name).ToListAsync();
            return View(items);
        }

        // GET: InventoryItem/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.InventoryItems
                .Include(i => i.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (item == null)
            {
                return NotFound();
            }
            
            // Get recent transactions for this item
            var recentTransactions = await _context.InventoryTransactions
                .Where(t => t.ItemId == id)
                .OrderByDescending(t => t.TransactionDate)
                .Take(10)
                .ToListAsync();
                
            ViewBag.RecentTransactions = recentTransactions;

            return View(item);
        }

        // GET: InventoryItem/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _context.InventoryCategories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            
            ViewBag.UnitOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "each", Text = "Each" },
                new SelectListItem { Value = "kg", Text = "Kilogram" },
                new SelectListItem { Value = "g", Text = "Gram" },
                new SelectListItem { Value = "l", Text = "Liter" },
                new SelectListItem { Value = "ml", Text = "Milliliter" },
                new SelectListItem { Value = "m", Text = "Meter" },
                new SelectListItem { Value = "cm", Text = "Centimeter" },
                new SelectListItem { Value = "box", Text = "Box" },
                new SelectListItem { Value = "pair", Text = "Pair" },
                new SelectListItem { Value = "set", Text = "Set" }
            };
            
            // Generate a unique SKU
            var sku = "ITM" + DateTime.Now.ToString("yyMMdd") + GetNextSequenceId();
            ViewBag.SuggestedSku = sku;
            
            return View(new InventoryItem { IsActive = true, UnitOfMeasure = "each", SKU = sku });
        }

        // POST: InventoryItem/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,CategoryId,SKU,Barcode,UnitOfMeasure,CurrentQuantity,MinimumQuantity,ReorderQuantity,UnitCost,UnitPrice,Location,IsActive")] InventoryItem item)
        {
            if (ModelState.IsValid)
            {
                // Check if SKU already exists
                if (await _context.InventoryItems.AnyAsync(i => i.SKU == item.SKU))
                {
                    ModelState.AddModelError("SKU", "This SKU already exists. Please use a different one.");
                }
                else
                {
                    item.CreatedAt = DateTime.UtcNow;
                    item.UpdatedAt = DateTime.UtcNow;
                    
                    _context.Add(item);
                    await _context.SaveChangesAsync();
                    
                    // Record the initial inventory if there's any
                    if (item.CurrentQuantity > 0)
                    {
                        var transaction = new InventoryTransaction
                        {
                            ItemId = item.Id,
                            TransactionType = "initial",
                            Quantity = item.CurrentQuantity,
                            UnitCost = item.UnitCost,
                            TotalCost = item.CurrentQuantity * item.UnitCost,
                            Notes = "Initial inventory",
                            TransactionDate = DateTime.UtcNow,
                            PerformedById = User.Identity.Name,
                            CreatedAt = DateTime.UtcNow
                        };
                        
                        _context.InventoryTransactions.Add(transaction);
                        await _context.SaveChangesAsync();
                    }
                    
                    TempData["SuccessMessage"] = "Inventory item created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            
            var categories = await _context.InventoryCategories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", item.CategoryId);
            
            ViewBag.UnitOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "each", Text = "Each" },
                new SelectListItem { Value = "kg", Text = "Kilogram" },
                new SelectListItem { Value = "g", Text = "Gram" },
                new SelectListItem { Value = "l", Text = "Liter" },
                new SelectListItem { Value = "ml", Text = "Milliliter" },
                new SelectListItem { Value = "m", Text = "Meter" },
                new SelectListItem { Value = "cm", Text = "Centimeter" },
                new SelectListItem { Value = "box", Text = "Box" },
                new SelectListItem { Value = "pair", Text = "Pair" },
                new SelectListItem { Value = "set", Text = "Set" }
            };
            
            return View(item);
        }

        // GET: InventoryItem/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            
            var categories = await _context.InventoryCategories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", item.CategoryId);
            
            ViewBag.UnitOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "each", Text = "Each" },
                new SelectListItem { Value = "kg", Text = "Kilogram" },
                new SelectListItem { Value = "g", Text = "Gram" },
                new SelectListItem { Value = "l", Text = "Liter" },
                new SelectListItem { Value = "ml", Text = "Milliliter" },
                new SelectListItem { Value = "m", Text = "Meter" },
                new SelectListItem { Value = "cm", Text = "Centimeter" },
                new SelectListItem { Value = "box", Text = "Box" },
                new SelectListItem { Value = "pair", Text = "Pair" },
                new SelectListItem { Value = "set", Text = "Set" }
            };
            
            return View(item);
        }

        // POST: InventoryItem/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,CategoryId,SKU,Barcode,UnitOfMeasure,CurrentQuantity,MinimumQuantity,ReorderQuantity,UnitCost,UnitPrice,Location,IsActive")] InventoryItem item)
        {
            if (id != item.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing item to check for quantity changes
                    var existingItem = await _context.InventoryItems
                        .AsNoTracking()
                        .FirstOrDefaultAsync(i => i.Id == id);
                        
                    if (existingItem == null)
                    {
                        return NotFound();
                    }
                    
                    // Update only fields that should be updated
                    item.CreatedAt = existingItem.CreatedAt;
                    item.UpdatedAt = DateTime.UtcNow;
                    
                    _context.Update(item);
                    
                    // Check if the quantity has been manually adjusted
                    if (existingItem.CurrentQuantity != item.CurrentQuantity)
                    {
                        // Calculate the adjustment quantity
                        decimal adjustmentQty = item.CurrentQuantity - existingItem.CurrentQuantity;
                        
                        // Create an inventory transaction to record the adjustment
                        var transaction = new InventoryTransaction
                        {
                            ItemId = item.Id,
                            TransactionType = "adjustment",
                            Quantity = adjustmentQty,
                            UnitCost = item.UnitCost,
                            TotalCost = adjustmentQty * (item.UnitCost ?? 0),
                            Notes = "Manual inventory adjustment",
                            TransactionDate = DateTime.UtcNow,
                            PerformedById = User.Identity.Name,
                            CreatedAt = DateTime.UtcNow
                        };
                        
                        _context.InventoryTransactions.Add(transaction);
                    }
                    
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Inventory item updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(item.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            var categories = await _context.InventoryCategories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", item.CategoryId);
            
            ViewBag.UnitOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "each", Text = "Each" },
                new SelectListItem { Value = "kg", Text = "Kilogram" },
                new SelectListItem { Value = "g", Text = "Gram" },
                new SelectListItem { Value = "l", Text = "Liter" },
                new SelectListItem { Value = "ml", Text = "Milliliter" },
                new SelectListItem { Value = "m", Text = "Meter" },
                new SelectListItem { Value = "cm", Text = "Centimeter" },
                new SelectListItem { Value = "box", Text = "Box" },
                new SelectListItem { Value = "pair", Text = "Pair" },
                new SelectListItem { Value = "set", Text = "Set" }
            };
            
            return View(item);
        }

        // GET: InventoryItem/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.InventoryItems
                .Include(i => i.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (item == null)
            {
                return NotFound();
            }
            
            // Check if this item has any transactions
            var hasTransactions = await _context.InventoryTransactions.AnyAsync(t => t.ItemId == id);
            ViewBag.HasTransactions = hasTransactions;

            return View(item);
        }

        // POST: InventoryItem/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Check if this item has any transactions
            var hasTransactions = await _context.InventoryTransactions.AnyAsync(t => t.ItemId == id);
            
            if (hasTransactions)
            {
                TempData["ErrorMessage"] = "Cannot delete this item because it has transaction history. Consider deactivating it instead.";
                return RedirectToAction(nameof(Delete), new { id = id });
            }
            
            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            
            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Inventory item deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        
        // GET: InventoryItem/AdjustStock/5
        public async Task<IActionResult> AdjustStock(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.InventoryItems
                .Include(i => i.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (item == null)
            {
                return NotFound();
            }
            
            ViewBag.Item = item;
            
            ViewBag.TransactionTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "purchase", Text = "Purchase" },
                new SelectListItem { Value = "use", Text = "Use/Consumption" },
                new SelectListItem { Value = "adjustment", Text = "Adjustment" },
                new SelectListItem { Value = "return", Text = "Return" },
                new SelectListItem { Value = "transfer", Text = "Transfer" },
                new SelectListItem { Value = "scrap", Text = "Scrap/Waste" }
            };
            
            var vehicles = await _context.Vehicles.OrderBy(v => v.RegistrationNumber).ToListAsync();
            ViewBag.Vehicles = new SelectList(vehicles, "Id", "RegistrationNumber");
            
            return View(new InventoryTransaction { 
                ItemId = item.Id, 
                UnitCost = item.UnitCost, 
                TransactionDate = DateTime.Now 
            });
        }
        
        // POST: InventoryItem/AdjustStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustStock(InventoryTransaction transaction)
        {
            if (ModelState.IsValid)
            {
                var item = await _context.InventoryItems.FindAsync(transaction.ItemId);
                if (item == null)
                {
                    return NotFound();
                }
                
                // Set created at and performed by
                transaction.CreatedAt = DateTime.UtcNow;
                transaction.PerformedById = User.Identity.Name;
                
                // Calculate total cost if not provided
                if (!transaction.TotalCost.HasValue && transaction.UnitCost.HasValue)
                {
                    transaction.TotalCost = transaction.Quantity * transaction.UnitCost.Value;
                }
                
                // Update the current quantity based on the transaction type
                if (transaction.TransactionType == "purchase" || transaction.TransactionType == "return")
                {
                    // These transaction types increase inventory
                    item.CurrentQuantity += transaction.Quantity;
                }
                else if (transaction.TransactionType == "use" || transaction.TransactionType == "scrap")
                {
                    // These transaction types decrease inventory
                    if (item.CurrentQuantity < transaction.Quantity)
                    {
                        ModelState.AddModelError("Quantity", "Insufficient stock. Available: " + item.CurrentQuantity);
                        
                        ViewBag.Item = item;
                        
                        ViewBag.TransactionTypes = new List<SelectListItem>
                        {
                            new SelectListItem { Value = "purchase", Text = "Purchase" },
                            new SelectListItem { Value = "use", Text = "Use/Consumption" },
                            new SelectListItem { Value = "adjustment", Text = "Adjustment" },
                            new SelectListItem { Value = "return", Text = "Return" },
                            new SelectListItem { Value = "transfer", Text = "Transfer" },
                            new SelectListItem { Value = "scrap", Text = "Scrap/Waste" }
                        };
                        
                        var vehicles = await _context.Vehicles.OrderBy(v => v.RegistrationNumber).ToListAsync();
                        ViewBag.Vehicles = new SelectList(vehicles, "Id", "RegistrationNumber", transaction.VehicleId);
                        
                        return View(transaction);
                    }
                    
                    item.CurrentQuantity -= transaction.Quantity;
                }
                else if (transaction.TransactionType == "adjustment")
                {
                    // Adjustment can be positive or negative
                    // For negative adjustments, ensure there's enough stock
                    if (transaction.Quantity < 0 && item.CurrentQuantity < Math.Abs(transaction.Quantity))
                    {
                        ModelState.AddModelError("Quantity", "Insufficient stock. Available: " + item.CurrentQuantity);
                        
                        ViewBag.Item = item;
                        
                        ViewBag.TransactionTypes = new List<SelectListItem>
                        {
                            new SelectListItem { Value = "purchase", Text = "Purchase" },
                            new SelectListItem { Value = "use", Text = "Use/Consumption" },
                            new SelectListItem { Value = "adjustment", Text = "Adjustment" },
                            new SelectListItem { Value = "return", Text = "Return" },
                            new SelectListItem { Value = "transfer", Text = "Transfer" },
                            new SelectListItem { Value = "scrap", Text = "Scrap/Waste" }
                        };
                        
                        var vehicles = await _context.Vehicles.OrderBy(v => v.RegistrationNumber).ToListAsync();
                        ViewBag.Vehicles = new SelectList(vehicles, "Id", "RegistrationNumber", transaction.VehicleId);
                        
                        return View(transaction);
                    }
                    
                    item.CurrentQuantity += transaction.Quantity;
                }
                
                // Update the updated date of the item
                item.UpdatedAt = DateTime.UtcNow;
                
                // Save the transaction and update the item
                _context.InventoryTransactions.Add(transaction);
                _context.Update(item);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Inventory adjusted successfully!";
                return RedirectToAction(nameof(Details), new { id = item.Id });
            }
            
            var itemForView = await _context.InventoryItems
                .Include(i => i.Category)
                .FirstOrDefaultAsync(m => m.Id == transaction.ItemId);
                
            ViewBag.Item = itemForView;
            
            ViewBag.TransactionTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "purchase", Text = "Purchase" },
                new SelectListItem { Value = "use", Text = "Use/Consumption" },
                new SelectListItem { Value = "adjustment", Text = "Adjustment" },
                new SelectListItem { Value = "return", Text = "Return" },
                new SelectListItem { Value = "transfer", Text = "Transfer" },
                new SelectListItem { Value = "scrap", Text = "Scrap/Waste" }
            };
            
            var vehicles = await _context.Vehicles.OrderBy(v => v.RegistrationNumber).ToListAsync();
            ViewBag.Vehicles = new SelectList(vehicles, "Id", "RegistrationNumber", transaction.VehicleId);
            
            return View(transaction);
        }
        
        // GET: InventoryItem/LowStock
        public async Task<IActionResult> LowStock()
        {
            var lowStockItems = await _context.InventoryItems
                .Include(i => i.Category)
                .Where(i => i.IsActive && i.CurrentQuantity <= i.MinimumQuantity && i.MinimumQuantity > 0)
                .OrderBy(i => i.Name)
                .ToListAsync();
                
            return View(lowStockItems);
        }
        
        // Helper method to generate a unique sequence ID for new items
        private string GetNextSequenceId()
        {
            int nextId = 1;
            
            var lastItem = _context.InventoryItems
                .OrderByDescending(i => i.Id)
                .FirstOrDefault();
                
            if (lastItem != null)
            {
                nextId = lastItem.Id + 1;
            }
            
            return nextId.ToString("D4");
        }

        private bool ItemExists(int id)
        {
            return _context.InventoryItems.Any(e => e.Id == id);
        }
    }
} 