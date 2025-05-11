using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<DriverController> _logger;

        public DriverController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<DriverController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Driver
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Retrieving list of all drivers");

                var drivers = await _context.Drivers
                    .Include(d => d.User)
                    .Include(d => d.Company)
                    .ToListAsync();

                _logger.LogInformation("Successfully retrieved {Count} drivers", drivers.Count);
                return View(drivers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving drivers list");
                TempData["ErrorMessage"] = "An error occurred while retrieving the drivers. Please try again.";
                return View(new List<Driver>());
            }
        }

        // GET: Driver/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Driver Details accessed with null ID");
                return NotFound();
            }

            try
            {
                _logger.LogInformation("Retrieving details for driver with ID: {Id}", id);

                var driver = await _context.Drivers
                    .Include(d => d.User)
                    .Include(d => d.Company)
                    .Include(d => d.Trips)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (driver == null)
                {
                    _logger.LogWarning("Driver with ID: {Id} not found", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully retrieved details for driver with ID: {Id}", id);
                return View(driver);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving details for driver with ID: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving the driver details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Driver/Create
        [Authorize(Roles = "Administrator,Manager,Dispatcher")]
        public IActionResult Create()
        {
            try
            {
                _logger.LogInformation("Accessing driver creation form");

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing driver creation form");
                TempData["ErrorMessage"] = "An error occurred while loading the creation form. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Driver/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager,Dispatcher")]
        public async Task<IActionResult> Create([Bind("Id,UserId,CompanyId,LicenseNumber,LicenseClass,LicenseExpiryDate,Status")] Driver driver)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _logger.LogInformation("Creating new driver for user ID: {UserId}", driver.UserId);
                    
                    driver.CreatedAt = DateTime.UtcNow;
                    driver.UpdatedAt = DateTime.UtcNow;
                    _context.Add(driver);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Successfully created driver with ID: {Id}", driver.Id);
                    TempData["SuccessMessage"] = "Driver was successfully created.";
                    return RedirectToAction(nameof(Index));
                }
                
                _logger.LogWarning("Driver creation failed due to validation errors");
                
                var existingDriverUserIds = _context.Drivers.Select(d => d.UserId).ToList();
                var availableUsers = _userManager.Users
                    .Where(u => !existingDriverUserIds.Contains(u.Id) || u.Id == driver.UserId)
                    .Select(u => new { Id = u.Id, Name = $"{u.FirstName} {u.LastName} ({u.Email})" })
                    .ToList();

                ViewData["UserId"] = new SelectList(availableUsers, "Id", "Name", driver.UserId);
                ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", driver.CompanyId);
                return View(driver);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating driver for user ID: {UserId}", driver.UserId);
                
                ModelState.AddModelError("", "An error occurred while creating the driver. Please try again.");
                
                var existingDriverUserIds = _context.Drivers.Select(d => d.UserId).ToList();
                var availableUsers = _userManager.Users
                    .Where(u => !existingDriverUserIds.Contains(u.Id) || u.Id == driver.UserId)
                    .Select(u => new { Id = u.Id, Name = $"{u.FirstName} {u.LastName} ({u.Email})" })
                    .ToList();

                ViewData["UserId"] = new SelectList(availableUsers, "Id", "Name", driver.UserId);
                ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", driver.CompanyId);
                return View(driver);
            }
        }

        // GET: Driver/Edit/5
        [Authorize(Roles = "Administrator,Manager,Dispatcher")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Driver Edit accessed with null ID");
                return NotFound();
            }

            try
            {
                _logger.LogInformation("Retrieving driver with ID: {Id} for editing", id);
                
                var driver = await _context.Drivers.FindAsync(id);
                if (driver == null)
                {
                    _logger.LogWarning("Driver with ID: {Id} not found for editing", id);
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
                
                _logger.LogInformation("Successfully retrieved driver with ID: {Id} for editing", id);
                return View(driver);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing to edit driver with ID: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the edit form. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Driver/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager,Dispatcher")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,CompanyId,LicenseNumber,LicenseClass,LicenseExpiryDate,Status,CreatedAt")] Driver driver)
        {
            if (id != driver.Id)
            {
                _logger.LogWarning("Driver Edit POST accessed with mismatched ID. Route ID: {RouteId}, Model ID: {ModelId}", id, driver.Id);
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        _logger.LogInformation("Updating driver with ID: {Id}", driver.Id);
                        
                        driver.UpdatedAt = DateTime.UtcNow;
                        _context.Update(driver);
                        await _context.SaveChangesAsync();
                        
                        _logger.LogInformation("Successfully updated driver with ID: {Id}", driver.Id);
                        TempData["SuccessMessage"] = "Driver was successfully updated.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        if (!DriverExists(driver.Id))
                        {
                            _logger.LogWarning("Concurrency error - Driver with ID: {Id} no longer exists", driver.Id);
                            return NotFound();
                        }
                        else
                        {
                            _logger.LogError(ex, "Concurrency error occurred while updating driver with ID: {Id}", driver.Id);
                            throw;
                        }
                    }
                }
                
                _logger.LogWarning("Driver update failed due to validation errors");
                
                var existingDriverUserIds = _context.Drivers.Where(d => d.Id != id).Select(d => d.UserId).ToList();
                var availableUsers = _userManager.Users
                    .Where(u => !existingDriverUserIds.Contains(u.Id))
                    .Select(u => new { Id = u.Id, Name = $"{u.FirstName} {u.LastName} ({u.Email})" })
                    .ToList();

                ViewData["UserId"] = new SelectList(availableUsers, "Id", "Name", driver.UserId);
                ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", driver.CompanyId);
                return View(driver);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating driver with ID: {Id}", id);
                ModelState.AddModelError("", "An error occurred while updating the driver. Please try again.");
                
                var existingDriverUserIds = _context.Drivers.Where(d => d.Id != id).Select(d => d.UserId).ToList();
                var availableUsers = _userManager.Users
                    .Where(u => !existingDriverUserIds.Contains(u.Id))
                    .Select(u => new { Id = u.Id, Name = $"{u.FirstName} {u.LastName} ({u.Email})" })
                    .ToList();

                ViewData["UserId"] = new SelectList(availableUsers, "Id", "Name", driver.UserId);
                ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", driver.CompanyId);
                return View(driver);
            }
        }

        // GET: Driver/Delete/5
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Driver Delete accessed with null ID");
                return NotFound();
            }

            try
            {
                _logger.LogInformation("Retrieving driver with ID: {Id} for deletion", id);
                
                var driver = await _context.Drivers
                    .Include(d => d.User)
                    .Include(d => d.Company)
                    .FirstOrDefaultAsync(m => m.Id == id);
                
                if (driver == null)
                {
                    _logger.LogWarning("Driver with ID: {Id} not found for deletion", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully retrieved driver with ID: {Id} for deletion", id);
                return View(driver);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing to delete driver with ID: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the deletion page. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Driver/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                _logger.LogInformation("Deleting driver with ID: {Id}", id);
                
                var driver = await _context.Drivers.FindAsync(id);
                if (driver != null)
                {
                    _context.Drivers.Remove(driver);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully deleted driver with ID: {Id}", id);
                    TempData["SuccessMessage"] = "Driver was successfully deleted.";
                }
                else
                {
                    _logger.LogWarning("Driver with ID: {Id} not found during deletion confirmation", id);
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting driver with ID: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the driver. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Driver/Availability
        public async Task<IActionResult> Availability()
        {
            try
            {
                _logger.LogInformation("Retrieving driver availability data");
                
                var drivers = await _context.Drivers
                    .Include(d => d.User)
                    .Include(d => d.Trips)
                    .ToListAsync();
                
                _logger.LogInformation("Successfully retrieved availability data for {Count} drivers", drivers.Count);
                return View(drivers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving driver availability data");
                TempData["ErrorMessage"] = "An error occurred while retrieving the driver availability data. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Driver/Schedule/5
        public async Task<IActionResult> Schedule(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Driver Schedule accessed with null ID");
                return NotFound();
            }

            try
            {
                _logger.LogInformation("Retrieving schedule for driver with ID: {Id}", id);
                
                var driver = await _context.Drivers
                    .Include(d => d.User)
                    .Include(d => d.Trips)
                    .FirstOrDefaultAsync(m => m.Id == id);
                
                if (driver == null)
                {
                    _logger.LogWarning("Driver with ID: {Id} not found for schedule view", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully retrieved schedule for driver with ID: {Id}", id);
                return View(driver);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving schedule for driver with ID: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving the driver schedule. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Driver/Skills/5
        public async Task<IActionResult> Skills(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Driver Skills accessed with null ID");
                return NotFound();
            }

            try
            {
                _logger.LogInformation("Retrieving skills for driver with ID: {Id}", id);
                
                var driver = await _context.Drivers
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(m => m.Id == id);
                
                if (driver == null)
                {
                    _logger.LogWarning("Driver with ID: {Id} not found for skills view", id);
                    return NotFound();
                }

                // For this example, skills are stored as a comma-separated string
                // In a real application, you might have a many-to-many relationship
                var driverSkills = !string.IsNullOrEmpty(driver.Skills) 
                    ? driver.Skills.Split(',').ToList() 
                    : new List<string>();

                // Create a list of all available skills
                var allSkills = new List<string> {
                    "CDL-A", "CDL-B", "Hazmat", "Tanker", "Doubles/Triples", 
                    "Passenger", "Air Brakes", "Mountain Driving", "Winter Driving",
                    "Equipment Operation", "Forklift", "First Aid", "Defensive Driving"
                };

                ViewBag.AllSkills = allSkills;
                ViewBag.DriverSkills = driverSkills;
                
                _logger.LogInformation("Successfully retrieved skills for driver with ID: {Id}", id);
                return View(driver);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving skills for driver with ID: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving the driver skills. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Driver/Skills/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager,Dispatcher")]
        public async Task<IActionResult> UpdateSkills(int id, List<string> skills)
        {
            try
            {
                _logger.LogInformation("Updating skills for driver with ID: {Id}", id);
                
                var driver = await _context.Drivers.FindAsync(id);
                if (driver == null)
                {
                    _logger.LogWarning("Driver with ID: {Id} not found when updating skills", id);
                    return NotFound();
                }

                driver.Skills = skills != null && skills.Count > 0 ? string.Join(",", skills) : null;
                driver.UpdatedAt = DateTime.UtcNow;
                
                _context.Update(driver);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Successfully updated skills for driver with ID: {Id}", id);
                TempData["SuccessMessage"] = "Driver skills were successfully updated.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating skills for driver with ID: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while updating the driver skills. Please try again.";
                return RedirectToAction(nameof(Skills), new { id });
            }
        }

        // GET: Driver/FindForTrip
        [Authorize(Roles = "Administrator,Manager,Dispatcher")]
        public async Task<IActionResult> FindForTrip(DateTime tripDate, string startLocation, string endLocation, int? vehicleTypeId)
        {
            try
            {
                _logger.LogInformation("Finding available drivers for trip on {TripDate}", tripDate);
                
                // Get all drivers
                var drivers = await _context.Drivers
                    .Include(d => d.User)
                    .Include(d => d.Trips)
                    .Where(d => d.Status == "active")
                    .ToListAsync();

                // Filter out drivers who are already assigned to trips on the given date
                var availableDrivers = drivers.Where(d => !d.Trips.Any(t => 
                    (t.StartTime.Date <= tripDate.Date && (t.EndTime == null || t.EndTime.Value.Date >= tripDate.Date)) &&
                    (t.Status == "scheduled" || t.Status == "in-progress")
                )).ToList();

                // If a vehicle type is specified, filter drivers who have that skill
                if (vehicleTypeId.HasValue)
                {
                    var vehicleType = await _context.VehicleTypes.FindAsync(vehicleTypeId.Value);
                    if (vehicleType != null && !string.IsNullOrEmpty(vehicleType.RequiredLicense))
                    {
                        availableDrivers = availableDrivers.Where(d => 
                            !string.IsNullOrEmpty(d.Skills) && 
                            d.Skills.Contains(vehicleType.RequiredLicense)
                        ).ToList();
                    }
                }

                // Further filtering based on location proximity could be added here
                // This would require geocoding the start/end locations and comparing with driver's home base

                ViewBag.TripDate = tripDate;
                ViewBag.StartLocation = startLocation;
                ViewBag.EndLocation = endLocation;
                ViewBag.VehicleTypeId = vehicleTypeId;
                
                _logger.LogInformation("Found {Count} available drivers for trip on {TripDate}", availableDrivers.Count, tripDate);
                return View(availableDrivers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while finding available drivers for trip on {TripDate}", tripDate);
                TempData["ErrorMessage"] = "An error occurred while finding available drivers. Please try again.";
                return RedirectToAction("Create", "Trip");
            }
        }

        // GET: Driver/Workload
        public async Task<IActionResult> Workload()
        {
            try
            {
                _logger.LogInformation("Retrieving driver workload data");
                
                var drivers = await _context.Drivers
                    .Include(d => d.User)
                    .Include(d => d.Trips)
                    .Where(d => d.Status == "active")
                    .ToListAsync();

                // Calculate workload metrics
                var today = DateTime.UtcNow.Date;
                var startOfMonth = new DateTime(today.Year, today.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                foreach (var driver in drivers)
                {
                    // Trips this month
                    driver.CurrentMonthTrips = driver.Trips.Count(t => 
                        t.StartTime >= startOfMonth && t.StartTime <= endOfMonth);
                    
                    // Hours this month (using trip duration)
                    driver.CurrentMonthHours = driver.Trips
                        .Where(t => t.StartTime >= startOfMonth && t.StartTime <= endOfMonth)
                        .Sum(t => t.EndTime.HasValue ? 
                            (t.EndTime.Value - t.StartTime).TotalHours : 
                            (today > t.StartTime ? (today - t.StartTime).TotalHours : 0));
                    
                    // Distance this month
                    driver.CurrentMonthDistance = driver.Trips
                        .Where(t => t.StartTime >= startOfMonth && t.StartTime <= endOfMonth)
                        .Sum(t => t.Distance);
                }

                _logger.LogInformation("Successfully retrieved workload data for {Count} drivers", drivers.Count);
                return View(drivers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving driver workload data");
                TempData["ErrorMessage"] = "An error occurred while retrieving the driver workload data. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool DriverExists(int id)
        {
            return _context.Drivers.Any(e => e.Id == id);
        }
    }
} 