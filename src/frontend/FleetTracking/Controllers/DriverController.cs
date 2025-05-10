using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using FleetTracking.Data;
using FleetTracking.Models;
using Microsoft.AspNetCore.Identity;

namespace FleetTracking.Controllers
{
    [Authorize]
    public class DriverController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DriverController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Driver
        public async Task<IActionResult> Index()
        {
            var drivers = await _context.Drivers
                .Include(d => d.User)
                .Include(d => d.Company)
                .ToListAsync();
            return View(drivers);
        }

        // GET: Driver/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driver = await _context.Drivers
                .Include(d => d.User)
                .Include(d => d.Company)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (driver == null)
            {
                return NotFound();
            }

            return View(driver);
        }

        // GET: Driver/Create
        public IActionResult Create()
        {
            // Get all users that are not already assigned as drivers
            var existingDriverUserIds = _context.Drivers.Select(d => d.UserId).ToList();
            var availableUsers = _userManager.Users
                .Where(u => !existingDriverUserIds.Contains(u.Id))
                .Select(u => new { Id = u.Id, Name = $"{u.FirstName} {u.LastName} ({u.Email})" })
                .ToList();

            ViewData["UserId"] = new SelectList(availableUsers, "Id", "Name");
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name");
            return View();
        }

        // POST: Driver/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,CompanyId,LicenseNumber,LicenseClass,LicenseExpiryDate,Status")] Driver driver)
        {
            if (ModelState.IsValid)
            {
                driver.CreatedAt = DateTime.UtcNow;
                driver.UpdatedAt = DateTime.UtcNow;
                _context.Add(driver);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            var existingDriverUserIds = _context.Drivers.Select(d => d.UserId).ToList();
            var availableUsers = _userManager.Users
                .Where(u => !existingDriverUserIds.Contains(u.Id) || u.Id == driver.UserId)
                .Select(u => new { Id = u.Id, Name = $"{u.FirstName} {u.LastName} ({u.Email})" })
                .ToList();

            ViewData["UserId"] = new SelectList(availableUsers, "Id", "Name", driver.UserId);
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", driver.CompanyId);
            return View(driver);
        }

        // GET: Driver/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driver = await _context.Drivers.FindAsync(id);
            if (driver == null)
            {
                return NotFound();
            }
            
            // Get all users that are not already assigned as drivers or are the current driver
            var existingDriverUserIds = _context.Drivers.Where(d => d.Id != id).Select(d => d.UserId).ToList();
            var availableUsers = _userManager.Users
                .Where(u => !existingDriverUserIds.Contains(u.Id))
                .Select(u => new { Id = u.Id, Name = $"{u.FirstName} {u.LastName} ({u.Email})" })
                .ToList();

            ViewData["UserId"] = new SelectList(availableUsers, "Id", "Name", driver.UserId);
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", driver.CompanyId);
            return View(driver);
        }

        // POST: Driver/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,CompanyId,LicenseNumber,LicenseClass,LicenseExpiryDate,Status,CreatedAt")] Driver driver)
        {
            if (id != driver.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    driver.UpdatedAt = DateTime.UtcNow;
                    _context.Update(driver);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DriverExists(driver.Id))
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
            
            var existingDriverUserIds = _context.Drivers.Where(d => d.Id != id).Select(d => d.UserId).ToList();
            var availableUsers = _userManager.Users
                .Where(u => !existingDriverUserIds.Contains(u.Id))
                .Select(u => new { Id = u.Id, Name = $"{u.FirstName} {u.LastName} ({u.Email})" })
                .ToList();

            ViewData["UserId"] = new SelectList(availableUsers, "Id", "Name", driver.UserId);
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", driver.CompanyId);
            return View(driver);
        }

        // GET: Driver/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driver = await _context.Drivers
                .Include(d => d.User)
                .Include(d => d.Company)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (driver == null)
            {
                return NotFound();
            }

            return View(driver);
        }

        // POST: Driver/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var driver = await _context.Drivers.FindAsync(id);
            if (driver != null)
            {
                _context.Drivers.Remove(driver);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool DriverExists(int id)
        {
            return _context.Drivers.Any(e => e.Id == id);
        }
    }
} 