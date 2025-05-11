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

namespace FleetTracking.Controllers
{
    [Authorize]
    public class VehicleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VehicleController> _logger;

        public VehicleController(ApplicationDbContext context, ILogger<VehicleController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Vehicle
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Retrieving list of all vehicles");
                
                var vehicles = await _context.Vehicles
                    .Include(v => v.Company)
                    .Include(v => v.VehicleType)
                    .ToListAsync();
                
                _logger.LogInformation("Successfully retrieved {Count} vehicles", vehicles.Count);
                return View(vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving vehicles list");
                TempData["ErrorMessage"] = "An error occurred while retrieving the vehicles. Please try again.";
                return View(new List<Vehicle>());
            }
        }

        // GET: Vehicle/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Vehicle Details accessed with null ID");
                return NotFound();
            }

            try
            {
                _logger.LogInformation("Retrieving details for vehicle with ID: {Id}", id);
                
                var vehicle = await _context.Vehicles
                    .Include(v => v.Company)
                    .Include(v => v.VehicleType)
                    .Include(v => v.Trips)
                        .ThenInclude(t => t.Driver)
                    .FirstOrDefaultAsync(m => m.Id == id);
                
                if (vehicle == null)
                {
                    _logger.LogWarning("Vehicle with ID: {Id} not found", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully retrieved details for vehicle: {Registration}", vehicle.RegistrationNumber);
                return View(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving details for vehicle with ID: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving the vehicle details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Vehicle/Create
        public IActionResult Create()
        {
            try
            {
                _logger.LogInformation("Accessing vehicle creation form");
                
                ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name");
                ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name");
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing vehicle creation form");
                TempData["ErrorMessage"] = "An error occurred while loading the creation form. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Vehicle/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompanyId,VehicleTypeId,RegistrationNumber,Make,Model,Year,Color,VIN,LicensePlate,FuelType,FuelCapacity,CurrentFuelLevel,OdometerReading,Status,LastServiceDate,NextServiceDate")] Vehicle vehicle)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _logger.LogInformation("Creating new vehicle: {Registration}", vehicle.RegistrationNumber);
                    
                    vehicle.CreatedAt = DateTime.UtcNow;
                    vehicle.UpdatedAt = DateTime.UtcNow;
                    _context.Add(vehicle);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Successfully created vehicle with ID: {Id}", vehicle.Id);
                    TempData["SuccessMessage"] = "Vehicle was successfully created.";
                    return RedirectToAction(nameof(Index));
                }
                
                _logger.LogWarning("Vehicle creation failed due to validation errors");
                ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", vehicle.CompanyId);
                ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name", vehicle.VehicleTypeId);
                return View(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating vehicle: {Registration}", vehicle.RegistrationNumber);
                ModelState.AddModelError("", "An error occurred while creating the vehicle. Please try again.");
                ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", vehicle.CompanyId);
                ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name", vehicle.VehicleTypeId);
                return View(vehicle);
            }
        }

        // GET: Vehicle/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Vehicle Edit accessed with null ID");
                return NotFound();
            }

            try
            {
                _logger.LogInformation("Retrieving vehicle with ID: {Id} for editing", id);
                
                var vehicle = await _context.Vehicles.FindAsync(id);
                if (vehicle == null)
                {
                    _logger.LogWarning("Vehicle with ID: {Id} not found for editing", id);
                    return NotFound();
                }
                
                ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", vehicle.CompanyId);
                ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name", vehicle.VehicleTypeId);
                
                _logger.LogInformation("Successfully retrieved vehicle: {Registration} for editing", vehicle.RegistrationNumber);
                return View(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing to edit vehicle with ID: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the edit form. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Vehicle/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,VehicleTypeId,RegistrationNumber,Make,Model,Year,Color,VIN,LicensePlate,FuelType,FuelCapacity,CurrentFuelLevel,OdometerReading,Status,LastServiceDate,NextServiceDate,CreatedAt,CurrentLocation,CurrentLatitude,CurrentLongitude")] Vehicle vehicle)
        {
            if (id != vehicle.Id)
            {
                _logger.LogWarning("Vehicle Edit POST accessed with mismatched ID. Route ID: {RouteId}, Model ID: {ModelId}", id, vehicle.Id);
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        _logger.LogInformation("Updating vehicle with ID: {Id}", vehicle.Id);
                        
                        vehicle.UpdatedAt = DateTime.UtcNow;
                        _context.Update(vehicle);
                        await _context.SaveChangesAsync();
                        
                        _logger.LogInformation("Successfully updated vehicle: {Registration}", vehicle.RegistrationNumber);
                        TempData["SuccessMessage"] = "Vehicle was successfully updated.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        if (!VehicleExists(vehicle.Id))
                        {
                            _logger.LogWarning("Concurrency error - Vehicle with ID: {Id} no longer exists", vehicle.Id);
                            return NotFound();
                        }
                        else
                        {
                            _logger.LogError(ex, "Concurrency error occurred while updating vehicle with ID: {Id}", vehicle.Id);
                            throw;
                        }
                    }
                }
                
                _logger.LogWarning("Vehicle update failed due to validation errors");
                ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", vehicle.CompanyId);
                ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name", vehicle.VehicleTypeId);
                return View(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating vehicle with ID: {Id}", id);
                ModelState.AddModelError("", "An error occurred while updating the vehicle. Please try again.");
                ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", vehicle.CompanyId);
                ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name", vehicle.VehicleTypeId);
                return View(vehicle);
            }
        }

        // GET: Vehicle/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Vehicle Delete accessed with null ID");
                return NotFound();
            }

            try
            {
                _logger.LogInformation("Retrieving vehicle with ID: {Id} for deletion", id);
                
                var vehicle = await _context.Vehicles
                    .Include(v => v.Company)
                    .Include(v => v.VehicleType)
                    .FirstOrDefaultAsync(m => m.Id == id);
                
                if (vehicle == null)
                {
                    _logger.LogWarning("Vehicle with ID: {Id} not found for deletion", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully retrieved vehicle: {Registration} for deletion", vehicle.RegistrationNumber);
                return View(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing to delete vehicle with ID: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the deletion page. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Vehicle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                _logger.LogInformation("Deleting vehicle with ID: {Id}", id);
                
                var vehicle = await _context.Vehicles.FindAsync(id);
                if (vehicle != null)
                {
                    _context.Vehicles.Remove(vehicle);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully deleted vehicle with ID: {Id}", id);
                    TempData["SuccessMessage"] = "Vehicle was successfully deleted.";
                }
                else
                {
                    _logger.LogWarning("Vehicle with ID: {Id} not found during deletion confirmation", id);
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting vehicle with ID: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the vehicle. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool VehicleExists(int id)
        {
            return _context.Vehicles.Any(e => e.Id == id);
        }
    }
} 