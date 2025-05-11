using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FleetTracking.Models;
using FleetTracking.Services;
using FleetTracking.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace FleetTracking.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IApiService _apiService;

        public HomeController(
            ILogger<HomeController> logger,
            IApiService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation($"Home page requested at {DateTime.Now}");
            
            try
            {
                // Get counts from the API for the dashboard
                var vehicles = await _apiService.GetAsync<List<object>>("vehicles");
                var drivers = await _apiService.GetAsync<List<object>>("drivers");
                var trips = await _apiService.GetAsync<List<object>>("trips/active");
                var maintenanceAlerts = await _apiService.GetAsync<List<object>>("maintenance/alerts");

                // Simple dashboard
                var dashboardModel = new DashboardViewModel
                {
                    VehicleCount = vehicles?.Count ?? 0,
                    DriverCount = drivers?.Count ?? 0,
                    TripsInProgress = trips?.Count ?? 0,
                    MaintenanceAlerts = maintenanceAlerts?.Count ?? 0,
                    ServerTime = DateTime.Now
                };
                
                return View(dashboardModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                
                // Fallback to default data if API call fails
                var dashboardModel = new DashboardViewModel
                {
                    VehicleCount = 10,
                    DriverCount = 5,
                    TripsInProgress = 3,
                    MaintenanceAlerts = 2,
                    ServerTime = DateTime.Now
                };
                
                return View(dashboardModel);
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return View(new GlobalSearchViewModel
                {
                    Query = "",
                    Vehicles = new List<VehicleSearchResult>(),
                    Drivers = new List<DriverSearchResult>(),
                    Trips = new List<TripSearchResult>(),
                    Vendors = new List<VendorSearchResult>(),
                    Inventory = new List<InventorySearchResult>(),
                    TotalResults = 0
                });
            }
            
            _logger.LogInformation($"Global search for '{query}' at {DateTime.Now}");
            
            try
            {
                // Call API to search across entities
                var searchModel = new GlobalSearchViewModel { Query = query };
                
                // Make parallel API calls for better performance
                var vehicleTask = _apiService.GetAsync<List<VehicleSearchResult>>($"search/vehicles?query={Uri.EscapeDataString(query)}");
                var driverTask = _apiService.GetAsync<List<DriverSearchResult>>($"search/drivers?query={Uri.EscapeDataString(query)}");
                var tripTask = _apiService.GetAsync<List<TripSearchResult>>($"search/trips?query={Uri.EscapeDataString(query)}");
                var vendorTask = _apiService.GetAsync<List<VendorSearchResult>>($"search/vendors?query={Uri.EscapeDataString(query)}");
                var inventoryTask = _apiService.GetAsync<List<InventorySearchResult>>($"search/inventory?query={Uri.EscapeDataString(query)}");
                
                // Wait for all tasks to complete
                await Task.WhenAll(vehicleTask, driverTask, tripTask, vendorTask, inventoryTask);
                
                // Populate the model with results
                searchModel.Vehicles = vehicleTask.Result ?? new List<VehicleSearchResult>();
                searchModel.Drivers = driverTask.Result ?? new List<DriverSearchResult>();
                searchModel.Trips = tripTask.Result ?? new List<TripSearchResult>();
                searchModel.Vendors = vendorTask.Result ?? new List<VendorSearchResult>();
                searchModel.Inventory = inventoryTask.Result ?? new List<InventorySearchResult>();
                
                // Calculate total results
                searchModel.TotalResults = 
                    searchModel.Vehicles.Count +
                    searchModel.Drivers.Count +
                    searchModel.Trips.Count +
                    searchModel.Vendors.Count +
                    searchModel.Inventory.Count;
                
                return View(searchModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching for '{query}'");
                TempData["ErrorMessage"] = "An error occurred while searching. Please try again.";
                
                // Return empty results
                return View(new GlobalSearchViewModel
                {
                    Query = query,
                    Vehicles = new List<VehicleSearchResult>(),
                    Drivers = new List<DriverSearchResult>(),
                    Trips = new List<TripSearchResult>(),
                    Vendors = new List<VendorSearchResult>(),
                    Inventory = new List<InventorySearchResult>(),
                    TotalResults = 0,
                    Error = "Search service is currently unavailable."
                });
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

    // Simple ViewModel for the dashboard
    public class DashboardViewModel
    {
        public int VehicleCount { get; set; }
        public int DriverCount { get; set; }
        public int TripsInProgress { get; set; }
        public int MaintenanceAlerts { get; set; }
        public DateTime ServerTime { get; set; }
    }

    // Global search view model
    public class GlobalSearchViewModel
    {
        public string Query { get; set; }
        public List<VehicleSearchResult> Vehicles { get; set; }
        public List<DriverSearchResult> Drivers { get; set; }
        public List<TripSearchResult> Trips { get; set; }
        public List<VendorSearchResult> Vendors { get; set; }
        public List<InventorySearchResult> Inventory { get; set; }
        public int TotalResults { get; set; }
        public string Error { get; set; }
    }

    // Search result models
    public class VehicleSearchResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LicensePlate { get; set; }
        public string VehicleType { get; set; }
        public string Status { get; set; }
    }

    public class DriverSearchResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LicenseNumber { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
    }

    public class TripSearchResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime StartTime { get; set; }
        public string Status { get; set; }
    }

    public class VendorSearchResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ContactPerson { get; set; }
        public string Phone { get; set; }
        public string Category { get; set; }
    }

    public class InventorySearchResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int StockLevel { get; set; }
        public string Location { get; set; }
    }
} 