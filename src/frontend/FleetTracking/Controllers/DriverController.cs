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

        // GET: Driver/Availability
        public async Task<IActionResult> Availability()
        {
            var drivers = await _context.Drivers
                .Include(d => d.User)
                .Include(d => d.AssignedTrips)
                .Where(d => d.Status == "active")
                .ToListAsync();
            
            // Calculate availability for each driver
            foreach (var driver in drivers)
            {
                driver.CurrentTrips = _context.Trips
                    .Where(t => t.DriverId == driver.Id && (t.Status == "scheduled" || t.Status == "in_progress"))
                    .OrderBy(t => t.StartTime)
                    .ToList();
                
                driver.CompletedTrips = _context.Trips
                    .Where(t => t.DriverId == driver.Id && t.Status == "completed")
                    .Count();
            }
            
            return View(drivers);
        }

        // GET: Driver/Schedule/5
        public async Task<IActionResult> Schedule(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driver = await _context.Drivers
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (driver == null)
            {
                return NotFound();
            }
            
            // Get driver trips
            var trips = await _context.Trips
                .Include(t => t.Vehicle)
                .Where(t => t.DriverId == id)
                .OrderBy(t => t.StartTime)
                .ToListAsync();
            
            // Get potential conflicting trips
            var now = DateTime.Now;
            var upcomingTrips = trips
                .Where(t => t.Status == "scheduled" && t.StartTime > now)
                .OrderBy(t => t.StartTime)
                .ToList();
            
            ViewData["DriverName"] = $"{driver.User.FirstName} {driver.User.LastName}";
            ViewData["UpcomingTrips"] = upcomingTrips;
            
            return View(driver);
        }

        // GET: Driver/Skills/5
        public async Task<IActionResult> Skills(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driver = await _context.Drivers
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (driver == null)
            {
                return NotFound();
            }
            
            // Get all skills (dummy data for now)
            var allSkills = new List<string>
            {
                "Heavy Vehicle License", 
                "Hazardous Materials", 
                "First Aid", 
                "Defensive Driving",
                "Mountain Roads",
                "Winter Driving",
                "Load Securing",
                "City Navigation",
                "Off-road Driving",
                "Truck Maintenance"
            };
            
            // For now, we'll randomly assign some skills to the driver
            var random = new Random(driver.Id); // Seed with driver ID for consistency
            var driverSkills = allSkills
                .OrderBy(x => random.Next())
                .Take(random.Next(3, 6))
                .ToList();
            
            ViewData["AllSkills"] = allSkills;
            ViewData["DriverSkills"] = driverSkills;
            
            return View(driver);
        }

        // POST: Driver/Skills/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSkills(int id, List<string> skills)
        {
            var driver = await _context.Drivers.FindAsync(id);
            
            if (driver == null)
            {
                return NotFound();
            }
            
            // In a real implementation, we would save the skills to the database
            // For now, we'll just redirect back to the skills page
            
            return RedirectToAction(nameof(Skills), new { id });
        }

        // GET: Driver/FindForTrip
        public async Task<IActionResult> FindForTrip(DateTime tripDate, string startLocation, string endLocation, int? vehicleTypeId)
        {
            // Get all active drivers
            var drivers = await _context.Drivers
                .Include(d => d.User)
                .Where(d => d.Status == "active")
                .ToListAsync();
            
            // Filter drivers based on availability
            var availableDrivers = new List<Driver>();
            
            foreach (var driver in drivers)
            {
                // Check if driver has conflicting trips
                var hasConflict = await _context.Trips
                    .AnyAsync(t => t.DriverId == driver.Id && 
                                   t.Status != "cancelled" &&
                                   t.Status != "completed" &&
                                   ((t.StartTime <= tripDate && t.EndTime >= tripDate) || 
                                    (t.StartTime.Date == tripDate.Date)));
                                    
                if (!hasConflict)
                {
                    availableDrivers.Add(driver);
                }
            }
            
            // In a real implementation, we would also filter based on skills, location, vehicle type, etc.
            
            return Json(availableDrivers.Select(d => new {
                id = d.Id,
                name = $"{d.User.FirstName} {d.User.LastName}",
                assignedTrips = d.AssignedTrips?.Count ?? 0,
                completedTrips = _context.Trips.Count(t => t.DriverId == d.Id && t.Status == "completed")
            }));
        }

        // GET: Driver/Workload
        public async Task<IActionResult> Workload()
        {
            var drivers = await _context.Drivers
                .Include(d => d.User)
                .Where(d => d.Status == "active")
                .ToListAsync();
            
            // Calculate workload metrics for each driver
            var driverWorkloads = new List<dynamic>();
            
            foreach (var driver in drivers)
            {
                var completedTrips = await _context.Trips
                    .Where(t => t.DriverId == driver.Id && t.Status == "completed")
                    .ToListAsync();
                    
                var upcomingTrips = await _context.Trips
                    .Where(t => t.DriverId == driver.Id && (t.Status == "scheduled" || t.Status == "in_progress"))
                    .OrderBy(t => t.StartTime)
                    .ToListAsync();
                    
                // Calculate metrics
                var totalDistance = completedTrips.Sum(t => t.Distance);
                var totalHours = completedTrips.Sum(t => (t.EndTime - t.StartTime)?.TotalHours ?? 0);
                var averageSpeed = completedTrips.Any() ? completedTrips.Average(t => t.AverageSpeed) : 0;
                
                // Add workload data
                driverWorkloads.Add(new {
                    Driver = driver,
                    CompletedTrips = completedTrips.Count,
                    UpcomingTrips = upcomingTrips.Count,
                    TotalDistance = totalDistance,
                    TotalHours = totalHours,
                    AverageSpeed = averageSpeed,
                    NextTrip = upcomingTrips.FirstOrDefault()
                });
            }
            
            return View(driverWorkloads.OrderByDescending(d => d.UpcomingTrips));
        }

        private bool DriverExists(int id)
        {
            return _context.Drivers.Any(e => e.Id == id);
        }
    }
} 