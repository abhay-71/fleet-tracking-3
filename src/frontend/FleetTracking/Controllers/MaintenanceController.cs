using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using FleetTracking.Models;
using FleetTracking.Services;

namespace FleetTracking.Controllers
{
    [Authorize]
    public class MaintenanceController : Controller
    {
        private readonly ILogger<MaintenanceController> _logger;
        private readonly VehicleService _vehicleService;
        private readonly MaintenanceService _maintenanceService;

        public MaintenanceController(
            ILogger<MaintenanceController> logger,
            VehicleService vehicleService,
            MaintenanceService maintenanceService)
        {
            _logger = logger;
            _vehicleService = vehicleService;
            _maintenanceService = maintenanceService;
        }

        /// <summary>
        /// Displays the maintenance schedule index page
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var maintenanceRecords = await _maintenanceService.GetMaintenanceRecordsAsync();
                return View(maintenanceRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving maintenance records");
                return View(new List<MaintenanceRecord>());
            }
        }

        /// <summary>
        /// Displays the maintenance schedule for a specific vehicle
        /// </summary>
        public async Task<IActionResult> Vehicle(int id)
        {
            try
            {
                var vehicle = await _vehicleService.GetVehicleAsync(id);
                
                if (vehicle == null)
                {
                    _logger.LogWarning($"Vehicle with ID {id} not found");
                    return NotFound();
                }
                
                var maintenanceRecords = await _maintenanceService.GetVehicleMaintenanceRecordsAsync(id);
                
                ViewData["Vehicle"] = vehicle;
                ViewData["MaintenanceRecords"] = maintenanceRecords;
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving maintenance records for vehicle with ID {id}");
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Displays form to create a new maintenance record
        /// </summary>
        public async Task<IActionResult> Create(int? vehicleId = null)
        {
            try
            {
                ViewData["Vehicles"] = new SelectList(
                    await _vehicleService.GetVehiclesAsync(), 
                    "Id", 
                    "DisplayName",
                    vehicleId);
                
                ViewData["MaintenanceTypes"] = new SelectList(
                    await _maintenanceService.GetMaintenanceTypesAsync(),
                    "Id",
                    "Name");
                
                var record = new MaintenanceRecord
                {
                    VehicleId = vehicleId ?? 0,
                    ScheduledDate = DateTime.Now.Date.AddDays(7),
                    CreatedAt = DateTime.Now,
                    Status = "scheduled"
                };
                
                return View(record);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing maintenance creation form");
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Processes the form submission for creating a new maintenance record
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaintenanceRecord record)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    record.CreatedAt = DateTime.Now;
                    record.UpdatedAt = DateTime.Now;
                    
                    await _maintenanceService.CreateMaintenanceRecordAsync(record);
                    
                    TempData["SuccessMessage"] = "Maintenance record created successfully.";
                    return RedirectToAction(nameof(Vehicle), new { id = record.VehicleId });
                }
                
                ViewData["Vehicles"] = new SelectList(
                    await _vehicleService.GetVehiclesAsync(), 
                    "Id", 
                    "DisplayName",
                    record.VehicleId);
                
                ViewData["MaintenanceTypes"] = new SelectList(
                    await _maintenanceService.GetMaintenanceTypesAsync(),
                    "Id",
                    "Name",
                    record.MaintenanceTypeId);
                
                return View(record);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating maintenance record");
                ModelState.AddModelError("", "An error occurred while creating the maintenance record.");
                
                ViewData["Vehicles"] = new SelectList(
                    await _vehicleService.GetVehiclesAsync(), 
                    "Id", 
                    "DisplayName",
                    record.VehicleId);
                
                ViewData["MaintenanceTypes"] = new SelectList(
                    await _maintenanceService.GetMaintenanceTypesAsync(),
                    "Id",
                    "Name",
                    record.MaintenanceTypeId);
                
                return View(record);
            }
        }

        /// <summary>
        /// Displays form to edit a maintenance record
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var record = await _maintenanceService.GetMaintenanceRecordAsync(id);
                
                if (record == null)
                {
                    _logger.LogWarning($"Maintenance record with ID {id} not found");
                    return NotFound();
                }
                
                ViewData["Vehicles"] = new SelectList(
                    await _vehicleService.GetVehiclesAsync(), 
                    "Id", 
                    "DisplayName",
                    record.VehicleId);
                
                ViewData["MaintenanceTypes"] = new SelectList(
                    await _maintenanceService.GetMaintenanceTypesAsync(),
                    "Id",
                    "Name",
                    record.MaintenanceTypeId);
                
                return View(record);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving maintenance record with ID {id}");
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Processes the form submission for editing a maintenance record
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MaintenanceRecord record)
        {
            try
            {
                if (id != record.Id)
                {
                    return NotFound();
                }
                
                if (ModelState.IsValid)
                {
                    record.UpdatedAt = DateTime.Now;
                    
                    await _maintenanceService.UpdateMaintenanceRecordAsync(record);
                    
                    TempData["SuccessMessage"] = "Maintenance record updated successfully.";
                    return RedirectToAction(nameof(Vehicle), new { id = record.VehicleId });
                }
                
                ViewData["Vehicles"] = new SelectList(
                    await _vehicleService.GetVehiclesAsync(), 
                    "Id", 
                    "DisplayName",
                    record.VehicleId);
                
                ViewData["MaintenanceTypes"] = new SelectList(
                    await _maintenanceService.GetMaintenanceTypesAsync(),
                    "Id",
                    "Name",
                    record.MaintenanceTypeId);
                
                return View(record);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating maintenance record with ID {id}");
                ModelState.AddModelError("", "An error occurred while updating the maintenance record.");
                
                ViewData["Vehicles"] = new SelectList(
                    await _vehicleService.GetVehiclesAsync(), 
                    "Id", 
                    "DisplayName",
                    record.VehicleId);
                
                ViewData["MaintenanceTypes"] = new SelectList(
                    await _maintenanceService.GetMaintenanceTypesAsync(),
                    "Id",
                    "Name",
                    record.MaintenanceTypeId);
                
                return View(record);
            }
        }

        /// <summary>
        /// Completes a maintenance task
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id, string notes, decimal cost)
        {
            try
            {
                var record = await _maintenanceService.GetMaintenanceRecordAsync(id);
                
                if (record == null)
                {
                    _logger.LogWarning($"Maintenance record with ID {id} not found");
                    return NotFound();
                }
                
                record.Status = "completed";
                record.CompletedDate = DateTime.Now;
                record.ActualCost = cost;
                record.Notes = notes;
                record.UpdatedAt = DateTime.Now;
                
                await _maintenanceService.UpdateMaintenanceRecordAsync(record);
                
                TempData["SuccessMessage"] = "Maintenance record marked as completed.";
                return RedirectToAction(nameof(Vehicle), new { id = record.VehicleId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing maintenance record with ID {id}");
                TempData["ErrorMessage"] = "An error occurred while completing the maintenance record.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Displays the maintenance forecast page
        /// </summary>
        public async Task<IActionResult> Forecast()
        {
            try
            {
                var forecasts = await _maintenanceService.GetMaintenanceForecastsAsync();
                return View(forecasts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving maintenance forecasts");
                return View(new List<MaintenanceForecast>());
            }
        }

        /// <summary>
        /// API method to get upcoming maintenance alerts
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetUpcomingMaintenance()
        {
            try
            {
                var records = await _maintenanceService.GetUpcomingMaintenanceAsync();
                return Json(records);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming maintenance");
                return Json(new { error = "Error retrieving upcoming maintenance" });
            }
        }
        
        /// <summary>
        /// API method to get maintenance statistics for a vehicle
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetMaintenanceStats(int vehicleId)
        {
            try
            {
                var stats = await _maintenanceService.GetMaintenanceStatsAsync(vehicleId);
                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving maintenance stats for vehicle with ID {vehicleId}");
                return Json(new { error = "Error retrieving maintenance statistics" });
            }
        }
    }
} 