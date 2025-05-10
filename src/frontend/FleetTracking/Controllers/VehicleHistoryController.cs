using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FleetTracking.Models;
using FleetTracking.Services;
using System.Text;

namespace FleetTracking.Controllers
{
    public class VehicleHistoryController : Controller
    {
        private readonly ILogger<VehicleHistoryController> _logger;
        private readonly VehicleService _vehicleService;
        private readonly VehicleHistoryService _historyService;

        public VehicleHistoryController(
            ILogger<VehicleHistoryController> logger,
            VehicleService vehicleService,
            VehicleHistoryService historyService)
        {
            _logger = logger;
            _vehicleService = vehicleService;
            _historyService = historyService;
        }

        /// <summary>
        /// Displays the vehicle history index page
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Get all vehicles to display in the list
            var vehicles = await _vehicleService.GetVehiclesAsync();
            
            ViewData["Vehicles"] = vehicles;
            return View();
        }

        /// <summary>
        /// Displays the history page for a specific vehicle
        /// </summary>
        public async Task<IActionResult> Vehicle(int id)
        {
            try
            {
                // Get vehicle details
                var vehicle = await _vehicleService.GetVehicleAsync(id);
                
                if (vehicle == null)
                {
                    _logger.LogWarning($"Vehicle with ID {id} not found");
                    return NotFound();
                }
                
                ViewData["Vehicle"] = vehicle;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving vehicle with ID {id}");
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Displays a specific trip route
        /// </summary>
        public async Task<IActionResult> Route(int id)
        {
            try
            {
                // Get trip details
                var trip = await _historyService.GetTripByIdAsync(id);
                
                if (trip == null)
                {
                    _logger.LogWarning($"Trip with ID {id} not found");
                    return NotFound();
                }
                
                // Get vehicle details
                var vehicle = await _vehicleService.GetVehicleAsync(trip.VehicleId);
                
                if (vehicle == null)
                {
                    _logger.LogWarning($"Vehicle with ID {trip.VehicleId} not found");
                    return NotFound();
                }
                
                ViewData["Trip"] = trip;
                ViewData["Vehicle"] = vehicle;
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving trip with ID {id}");
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Exports vehicle history data as CSV
        /// </summary>
        public async Task<IActionResult> Export(int id, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Use default dates if not provided
                var start = startDate ?? DateTime.Now.AddDays(-30);
                var end = endDate ?? DateTime.Now;
                
                // Get vehicle details for the filename
                var vehicle = await _vehicleService.GetVehicleAsync(id);
                
                if (vehicle == null)
                {
                    _logger.LogWarning($"Vehicle with ID {id} not found");
                    return NotFound();
                }
                
                // Export data
                var csvContent = await _historyService.ExportVehicleHistoryAsync(id, start, end);
                
                // Generate file name based on vehicle and date range
                var fileName = $"vehicle_history_{vehicle.RegistrationNumber}_{start:yyyyMMdd}_to_{end:yyyyMMdd}.csv";
                
                // Return file
                var bytes = Encoding.UTF8.GetBytes(csvContent);
                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error exporting history for vehicle with ID {id}");
                return RedirectToAction(nameof(Vehicle), new { id });
            }
        }

        /// <summary>
        /// API method to get vehicle history data for AJAX calls
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetVehicleHistoryData(int id, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Use default dates if not provided
                var start = startDate ?? DateTime.Now.AddDays(-7);
                var end = endDate ?? DateTime.Now;
                
                // For demo purposes, use sample data if no real data exists
                var historyData = await _historyService.GetVehicleHistoryDataAsync(id, start, end);
                
                if (historyData == null || !historyData.Any())
                {
                    historyData = _historyService.GenerateSampleHistoryData(id, start, end);
                }
                
                return Json(historyData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving history data for vehicle with ID {id}");
                return Json(new { error = "Error retrieving history data" });
            }
        }

        /// <summary>
        /// API method to get trip waypoints for AJAX calls
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetTripWaypoints(int id)
        {
            try
            {
                // Get waypoints for the trip
                var waypoints = await _historyService.GetTripWaypointsAsync(id);
                
                if (waypoints == null || !waypoints.Any())
                {
                    // For demo purposes, generate sample waypoints if none exist
                    var trip = await _historyService.GetTripByIdAsync(id);
                    if (trip != null)
                    {
                        waypoints = _historyService.GenerateSampleHistoryData(trip.VehicleId, trip.StartDate, trip.EndDate);
                    }
                }
                
                return Json(waypoints);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving waypoints for trip with ID {id}");
                return Json(new { error = "Error retrieving trip waypoints" });
            }
        }
    }
} 