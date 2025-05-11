using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FleetTracking.Models;

namespace FleetTracking.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogInformation($"Home page requested at {DateTime.Now}");
            
            // Simple dashboard
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
} 