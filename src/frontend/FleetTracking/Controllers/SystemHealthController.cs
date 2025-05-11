using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FleetTracking.Models;
using FleetTracking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FleetTracking.Controllers
{
    /// <summary>
    /// Controller for system health monitoring and diagnostics
    /// </summary>
    [Authorize(Roles = "Administrator")]
    public class SystemHealthController : Controller
    {
        private readonly ISystemHealthService _healthService;
        private readonly ILogger<SystemHealthController> _logger;
        
        /// <summary>
        /// Initializes a new instance of the SystemHealthController class
        /// </summary>
        public SystemHealthController(
            ISystemHealthService healthService,
            ILogger<SystemHealthController> logger)
        {
            _healthService = healthService;
            _logger = logger;
        }
        
        /// <summary>
        /// Displays the system health dashboard
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var systemHealth = await _healthService.GetSystemHealthAsync();
                
                ViewBag.Breadcrumbs = new[]
                {
                    new { Title = "Administration", Url = "/Admin", IsActive = false },
                    new { Title = "System Health", Url = "#", IsActive = true }
                };
                
                return View(systemHealth);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system health information");
                TempData["ErrorMessage"] = "Failed to retrieve system health information.";
                return View(new SystemHealth
                {
                    Status = SystemStatus.Unknown,
                    Timestamp = DateTime.UtcNow,
                    Components = new List<ComponentHealth>(),
                    Resources = new SystemResources(),
                    Performance = new PerformanceMetrics()
                });
            }
        }
        
        /// <summary>
        /// Displays performance metrics
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Performance()
        {
            try
            {
                var metrics = await _healthService.GetPerformanceMetricsAsync();
                
                ViewBag.Breadcrumbs = new[]
                {
                    new { Title = "Administration", Url = "/Admin", IsActive = false },
                    new { Title = "System Health", Url = "/SystemHealth", IsActive = false },
                    new { Title = "Performance", Url = "#", IsActive = true }
                };
                
                return View(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving performance metrics");
                TempData["ErrorMessage"] = "Failed to retrieve performance metrics.";
                return View(new PerformanceMetrics());
            }
        }
        
        /// <summary>
        /// Displays system resources
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Resources()
        {
            try
            {
                var resources = await _healthService.GetSystemResourcesAsync();
                
                ViewBag.Breadcrumbs = new[]
                {
                    new { Title = "Administration", Url = "/Admin", IsActive = false },
                    new { Title = "System Health", Url = "/SystemHealth", IsActive = false },
                    new { Title = "Resources", Url = "#", IsActive = true }
                };
                
                return View(resources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system resources");
                TempData["ErrorMessage"] = "Failed to retrieve system resources.";
                return View(new SystemResources());
            }
        }
        
        /// <summary>
        /// Displays system errors
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Errors()
        {
            try
            {
                var errors = await _healthService.GetRecentErrorsAsync(100);
                
                ViewBag.Breadcrumbs = new[]
                {
                    new { Title = "Administration", Url = "/Admin", IsActive = false },
                    new { Title = "System Health", Url = "/SystemHealth", IsActive = false },
                    new { Title = "Errors", Url = "#", IsActive = true }
                };
                
                return View(errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system errors");
                TempData["ErrorMessage"] = "Failed to retrieve system errors.";
                return View(new List<SystemError>());
            }
        }
        
        /// <summary>
        /// Checks endpoint health
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckEndpoint(string endpoint)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(endpoint))
                {
                    return BadRequest("Endpoint URL is required");
                }
                
                var isHealthy = await _healthService.CheckEndpointHealthAsync(endpoint);
                
                return Json(new { isHealthy });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking endpoint health for {Endpoint}", endpoint);
                return StatusCode(500, new { message = "Failed to check endpoint health" });
            }
        }
        
        /// <summary>
        /// Gets system health data in JSON format for API usage
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSystemHealthData()
        {
            try
            {
                var systemHealth = await _healthService.GetSystemHealthAsync();
                return Json(systemHealth);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system health data for API");
                return StatusCode(500, new { message = "Failed to retrieve system health data" });
            }
        }
        
        /// <summary>
        /// Checks component health
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ComponentHealth(string componentName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(componentName))
                {
                    return BadRequest("Component name is required");
                }
                
                var componentHealth = await _healthService.GetComponentHealthAsync(componentName);
                
                ViewBag.Breadcrumbs = new[]
                {
                    new { Title = "Administration", Url = "/Admin", IsActive = false },
                    new { Title = "System Health", Url = "/SystemHealth", IsActive = false },
                    new { Title = $"{componentName} Health", Url = "#", IsActive = true }
                };
                
                return View(componentHealth);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving component health for {Component}", componentName);
                TempData["ErrorMessage"] = $"Failed to retrieve health information for {componentName}.";
                return View(new ComponentHealth { Name = componentName, Status = SystemStatus.Unknown });
            }
        }
    }
} 