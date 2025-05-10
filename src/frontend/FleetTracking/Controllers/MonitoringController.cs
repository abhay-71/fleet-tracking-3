using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FleetTracking.Data;
using FleetTracking.Models;
using FleetTracking.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FleetTracking.Controllers
{
    [Authorize]
    public class MonitoringController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly VehicleStatusService _vehicleStatusService;

        public MonitoringController(
            ApplicationDbContext context,
            VehicleStatusService vehicleStatusService)
        {
            _context = context;
            _vehicleStatusService = vehicleStatusService;
        }

        // Main monitoring dashboard
        public IActionResult Index()
        {
            return View();
        }

        // Real-time vehicle status dashboard
        public IActionResult VehicleStatus()
        {
            // Get the latest vehicle statuses from the service
            var vehicleStatuses = _vehicleStatusService.GetAllVehicleStatuses().ToList();
            
            // For vehicles without real-time data, add placeholder statuses from database
            var vehicleIds = vehicleStatuses.Select(v => v.VehicleId).ToHashSet();
            var missingVehicles = _context.Vehicles
                .Where(v => !vehicleIds.Contains(v.Id) && v.Status == "active")
                .ToList();
                
            foreach (var vehicle in missingVehicles)
            {
                vehicleStatuses.Add(new VehicleStatus
                {
                    VehicleId = vehicle.Id,
                    VehicleRegistration = vehicle.RegistrationNumber,
                    VehicleName = $"{vehicle.Year} {vehicle.Make} {vehicle.Model}",
                    Status = "offline",
                    IsOnline = false,
                    LastUpdated = vehicle.UpdatedAt
                });
            }
            
            return View(vehicleStatuses);
        }

        // Vehicle details with real-time status
        public IActionResult VehicleDetails(int id)
        {
            var vehicle = _context.Vehicles
                .FirstOrDefault(v => v.Id == id);
                
            if (vehicle == null)
            {
                return NotFound();
            }
            
            // Get real-time status if available
            var status = _vehicleStatusService.GetVehicleStatus(id);
            
            // If no real-time status, create a placeholder
            if (status == null)
            {
                status = new VehicleStatus
                {
                    VehicleId = vehicle.Id,
                    VehicleRegistration = vehicle.RegistrationNumber,
                    VehicleName = $"{vehicle.Year} {vehicle.Make} {vehicle.Model}",
                    Status = vehicle.Status,
                    IsOnline = false,
                    FuelLevelPercentage = vehicle.FuelLevelPercentage,
                    OdometerReading = vehicle.OdometerReading,
                    LastUpdated = vehicle.UpdatedAt
                };
            }
            
            ViewData["Vehicle"] = vehicle;
            return View(status);
        }

        // Map view showing all active vehicles
        public IActionResult Map()
        {
            return View();
        }

        // Alert dashboard
        public IActionResult Alerts()
        {
            return View();
        }

        // API endpoint to get all vehicle statuses (for AJAX refreshes)
        [HttpGet]
        public IActionResult GetVehicleStatuses()
        {
            var statuses = _vehicleStatusService.GetAllVehicleStatuses();
            return Json(statuses);
        }

        // API endpoint to get a single vehicle's status
        [HttpGet]
        public IActionResult GetVehicleStatus(int id)
        {
            var status = _vehicleStatusService.GetVehicleStatus(id);
            if (status == null)
            {
                return NotFound();
            }
            return Json(status);
        }
    }
} 