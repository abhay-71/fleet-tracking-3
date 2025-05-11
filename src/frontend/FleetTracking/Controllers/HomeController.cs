using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FleetTracking.Models;
using FleetTracking.Services;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;

namespace FleetTracking.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly VehicleService _vehicleService;
    private readonly DriverService _driverService;
    private readonly MaintenanceService _maintenanceService;

    public HomeController(
        ILogger<HomeController> logger,
        VehicleService vehicleService,
        DriverService driverService,
        MaintenanceService maintenanceService)
    {
        _logger = logger;
        _vehicleService = vehicleService;
        _driverService = driverService;
        _maintenanceService = maintenanceService;
    }

    public IActionResult Index()
    {
        // If the user is authenticated, redirect to dashboard
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Dashboard");
        }

        return View();
    }

    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            // Fetch summary data for the dashboard
            var vehicles = await _vehicleService.GetAllVehiclesAsync();
            
            int activeVehicles = 0;
            int maintenanceVehicles = 0;
            int inactiveVehicles = 0;
            
            if (vehicles != null)
            {
                activeVehicles = vehicles.Count(v => v.Status == "active");
                maintenanceVehicles = vehicles.Count(v => v.Status == "maintenance");
                inactiveVehicles = vehicles.Count - activeVehicles - maintenanceVehicles;
            }

            var maintenanceRecords = await _maintenanceService.GetMaintenanceRecordsAsync();
            List<MaintenanceRecord> upcomingMaintenance = new List<MaintenanceRecord>();
            List<MaintenanceRecord> overdueMaintenance = new List<MaintenanceRecord>();
            List<MaintenanceRecord> inProgressMaintenance = new List<MaintenanceRecord>();
            
            if (maintenanceRecords != null)
            {
                upcomingMaintenance = maintenanceRecords.Where(m => m.Status == "scheduled" && !m.IsOverdue).ToList();
                overdueMaintenance = maintenanceRecords.Where(m => m.Status == "scheduled" && m.IsOverdue).ToList();
                inProgressMaintenance = maintenanceRecords.Where(m => m.Status == "in_progress").ToList();
            }

            // Populate ViewData with summary data
            ViewData["VehicleCount"] = vehicles?.Count ?? 0;
            ViewData["ActiveVehicleCount"] = activeVehicles;
            ViewData["MaintenanceVehicleCount"] = maintenanceVehicles;
            ViewData["InactiveVehicleCount"] = inactiveVehicles;
            
            ViewData["UpcomingMaintenanceCount"] = upcomingMaintenance.Count;
            ViewData["OverdueMaintenanceCount"] = overdueMaintenance.Count;
            ViewData["InProgressMaintenanceCount"] = inProgressMaintenance.Count;
            
            ViewData["RecentVehicles"] = vehicles != null 
                ? vehicles.OrderByDescending(v => v.LastActivityDate).Take(5).ToList() 
                : new List<Vehicle>();
                
            ViewData["RecentMaintenance"] = maintenanceRecords != null
                ? maintenanceRecords
                    .Where(m => m.Status != "cancelled")
                    .OrderByDescending(m => m.UpdatedAt ?? m.CreatedAt)
                    .Take(5)
                    .ToList()
                : new List<MaintenanceRecord>();

            // Get maintenance forecasts
            var maintenanceForecasts = await _maintenanceService.GetMaintenanceForecastsAsync();
            ViewData["MaintenanceForecasts"] = maintenanceForecasts != null
                ? maintenanceForecasts
                    .OrderBy(f => f.PredictedDueDate)
                    .Take(5)
                    .ToList()
                : new List<MaintenanceForecast>();

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data");
            return View();
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
} 