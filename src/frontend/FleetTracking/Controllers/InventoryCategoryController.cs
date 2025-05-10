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
    public class InventoryCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryCategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: InventoryCategory
        public async Task<IActionResult> Index()
        {
            var categories = await _context.InventoryCategories
                .Include(c => c.ParentCategory)
                .OrderBy(c => c.Name)
                .ToListAsync();
                
            return View(categories);
        }

        // GET: InventoryCategory/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.InventoryCategories
                .Include(c => c.ParentCategory)
                .Include(c => c.ChildCategories)
                .Include(c => c.Items)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: InventoryCategory/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _context.InventoryCategories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.ParentCategories = new SelectList(categories, "Id", "Name");
            
            return View();
        }

        // POST: InventoryCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,ParentCategoryId")] InventoryCategory category)
        {
            if (ModelState.IsValid)
            {
                category.CreatedAt = DateTime.UtcNow;
                category.UpdatedAt = DateTime.UtcNow;
                
                _context.Add(category);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Category created successfully!";
                return RedirectToAction(nameof(Index));
            }
            
            var categories = await _context.InventoryCategories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.ParentCategories = new SelectList(categories, "Id", "Name", category.ParentCategoryId);
            
            return View(category);
        }

        // GET: InventoryCategory/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.InventoryCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            
            // Exclude the current category and its children from parent category selection
            // to prevent circular references
            var availableCategories = await _context.InventoryCategories
                .Where(c => c.Id != id)
                .OrderBy(c => c.Name)
                .ToListAsync();
                
            ViewBag.ParentCategories = new SelectList(availableCategories, "Id", "Name", category.ParentCategoryId);
            
            return View(category);
        }

        // POST: InventoryCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ParentCategoryId")] InventoryCategory category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing record to preserve created date
                    var existingCategory = await _context.InventoryCategories
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.Id == id);
                        
                    if (existingCategory == null)
                    {
                        return NotFound();
                    }
                    
                    // Update only fields that should be updated
                    category.CreatedAt = existingCategory.CreatedAt;
                    category.UpdatedAt = DateTime.UtcNow;
                    
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Category updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            
            var availableCategories = await _context.InventoryCategories
                .Where(c => c.Id != id)
                .OrderBy(c => c.Name)
                .ToListAsync();
                
            ViewBag.ParentCategories = new SelectList(availableCategories, "Id", "Name", category.ParentCategoryId);
            
            return View(category);
        }

        // GET: InventoryCategory/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.InventoryCategories
                .Include(c => c.ParentCategory)
                .Include(c => c.ChildCategories)
                .Include(c => c.Items)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: InventoryCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.InventoryCategories
                .Include(c => c.ChildCategories)
                .Include(c => c.Items)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (category == null)
            {
                return NotFound();
            }
            
            // Check if category has child categories or items
            if (category.ChildCategories.Any() || category.Items.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete category. It has child categories or items associated with it.";
                return RedirectToAction(nameof(Delete), new { id = id });
            }
            
            _context.InventoryCategories.Remove(category);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Category deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.InventoryCategories.Any(e => e.Id == id);
        }
    }
} 