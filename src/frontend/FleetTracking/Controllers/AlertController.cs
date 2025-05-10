using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FleetTracking.Models;
using FleetTracking.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FleetTracking.Controllers
{
    [Authorize]
    public class AlertController : Controller
    {
        private readonly ILogger<AlertController> _logger;
        private readonly IApiService _apiService;

        public AlertController(
            ILogger<AlertController> logger,
            IApiService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }

        // GET: Alert
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _apiService.GetAsync<List<Alert>>("alerts");
                return View(response ?? new List<Alert>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving alerts");
                return View(new List<Alert>());
            }
        }

        // GET: Alert/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var alert = await _apiService.GetAsync<Alert>($"alerts/{id}");
                if (alert == null)
                {
                    return NotFound();
                }
                return View(alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving alert details for ID {id}");
                return NotFound();
            }
        }

        // GET: Alert/Create
        public async Task<IActionResult> Create()
        {
            await PopulateViewBags();
            return View(new Alert { Active = true, AutoResolve = false });
        }

        // POST: Alert/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Alert alert, List<string> selectedChannels, List<string> selectedRecipients)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    alert.CreatedAt = DateTime.UtcNow;
                    alert.UpdatedAt = DateTime.UtcNow;
                    alert.CreatedBy = 1; // Replace with actual user ID
                    alert.LastModified = 1; // Replace with actual user ID

                    // Process notification channels and recipients
                    alert.NotificationChannels = string.Join(",", selectedChannels ?? new List<string>());
                    alert.Recipients = string.Join(",", selectedRecipients ?? new List<string>());

                    var response = await _apiService.PostAsync<Alert, Alert>("alerts", alert);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating alert");
                    ModelState.AddModelError("", "Failed to create alert.");
                }
            }

            await PopulateViewBags();
            return View(alert);
        }

        // GET: Alert/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var alert = await _apiService.GetAsync<Alert>($"alerts/{id}");
                if (alert == null)
                {
                    return NotFound();
                }

                await PopulateViewBags();
                return View(alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving alert for edit with ID {id}");
                return NotFound();
            }
        }

        // POST: Alert/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Alert alert, List<string> selectedChannels, List<string> selectedRecipients)
        {
            if (id != alert.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    alert.UpdatedAt = DateTime.UtcNow;
                    alert.LastModified = 1; // Replace with actual user ID

                    // Process notification channels and recipients
                    alert.NotificationChannels = string.Join(",", selectedChannels ?? new List<string>());
                    alert.Recipients = string.Join(",", selectedRecipients ?? new List<string>());

                    await _apiService.PutAsync<Alert, object>($"alerts/{id}", alert);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating alert with ID {id}");
                    ModelState.AddModelError("", "Failed to update alert.");
                }
            }

            await PopulateViewBags();
            return View(alert);
        }

        // GET: Alert/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var alert = await _apiService.GetAsync<Alert>($"alerts/{id}");
                if (alert == null)
                {
                    return NotFound();
                }
                return View(alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving alert for delete with ID {id}");
                return NotFound();
            }
        }

        // POST: Alert/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _apiService.DeleteAsync($"alerts/{id}");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting alert with ID {id}");
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Alert/History
        public async Task<IActionResult> History()
        {
            try
            {
                var response = await _apiService.GetAsync<List<AlertLog>>("alerts/logs");
                return View(response ?? new List<AlertLog>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving alert history");
                return View(new List<AlertLog>());
            }
        }

        // GET: Alert/Acknowledge/5
        public async Task<IActionResult> Acknowledge(int id)
        {
            try
            {
                var alertLog = await _apiService.GetAsync<AlertLog>($"alerts/logs/{id}");
                if (alertLog == null)
                {
                    return NotFound();
                }
                return View(alertLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving alert log for acknowledge with ID {id}");
                return NotFound();
            }
        }

        // POST: Alert/Acknowledge/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Acknowledge(int id, string resolution)
        {
            try
            {
                var data = new
                {
                    Resolution = resolution,
                    AcknowledgedBy = 1 // Replace with actual user ID
                };

                await _apiService.PutAsync<object, object>($"alerts/logs/{id}/acknowledge", data);
                return RedirectToAction(nameof(History));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error acknowledging alert log with ID {id}");
                return RedirectToAction(nameof(History));
            }
        }

        // GET: Alert/Map
        public IActionResult Map()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAlertLogs()
        {
            try
            {
                var response = await _apiService.GetAsync<List<AlertLog>>("alerts/logs");
                return Json(response ?? new List<AlertLog>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving alert logs");
                return Json(new List<AlertLog>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentAlerts()
        {
            try
            {
                var response = await _apiService.GetAsync<List<AlertLog>>("alerts/logs/recent");
                return Json(response ?? new List<AlertLog>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent alerts");
                return Json(new List<AlertLog>());
            }
        }

        private async Task PopulateViewBags()
        {
            try
            {
                // Alert types
                ViewBag.AlertTypes = new List<SelectListItem>
                {
                    new SelectListItem { Value = "geofence_entry", Text = "Geofence Entry" },
                    new SelectListItem { Value = "geofence_exit", Text = "Geofence Exit" },
                    new SelectListItem { Value = "speed", Text = "Speed Alert" },
                    new SelectListItem { Value = "idle_time", Text = "Idle Time" },
                    new SelectListItem { Value = "unauthorized_movement", Text = "Unauthorized Movement" },
                    new SelectListItem { Value = "driver_behavior", Text = "Driver Behavior" },
                    new SelectListItem { Value = "maintenance", Text = "Maintenance Alert" },
                    new SelectListItem { Value = "custom", Text = "Custom Alert" }
                };

                // Alert categories
                ViewBag.AlertCategories = new List<SelectListItem>
                {
                    new SelectListItem { Value = "safety", Text = "Safety" },
                    new SelectListItem { Value = "security", Text = "Security" },
                    new SelectListItem { Value = "compliance", Text = "Compliance" },
                    new SelectListItem { Value = "maintenance", Text = "Maintenance" },
                    new SelectListItem { Value = "operations", Text = "Operations" },
                    new SelectListItem { Value = "custom", Text = "Custom" }
                };

                // Severity levels
                ViewBag.SeverityLevels = new List<SelectListItem>
                {
                    new SelectListItem { Value = "low", Text = "Low" },
                    new SelectListItem { Value = "medium", Text = "Medium" },
                    new SelectListItem { Value = "high", Text = "High" },
                    new SelectListItem { Value = "critical", Text = "Critical" }
                };

                // Notification channels
                ViewBag.NotificationChannels = new List<SelectListItem>
                {
                    new SelectListItem { Value = "email", Text = "Email" },
                    new SelectListItem { Value = "sms", Text = "SMS" },
                    new SelectListItem { Value = "push", Text = "Push Notification" },
                    new SelectListItem { Value = "dashboard", Text = "Dashboard" }
                };

                // Geofences, vehicles, and drivers
                var geofences = await _apiService.GetAsync<List<Geofence>>("geofences");
                var vehicles = await _apiService.GetAsync<List<Vehicle>>("vehicles");
                var drivers = await _apiService.GetAsync<List<Driver>>("drivers");

                ViewBag.Geofences = geofences?.Select(g => new SelectListItem { Value = g.Id.ToString(), Text = g.Name }) ?? new List<SelectListItem>();
                ViewBag.Vehicles = vehicles?.Select(v => new SelectListItem { Value = v.Id.ToString(), Text = v.Name }) ?? new List<SelectListItem>();
                ViewBag.Drivers = drivers?.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = $"{d.FirstName} {d.LastName}" }) ?? new List<SelectListItem>();

                // Users for recipients (simplified for demo)
                ViewBag.Users = new List<SelectListItem>
                {
                    new SelectListItem { Value = "user1@example.com", Text = "John Smith" },
                    new SelectListItem { Value = "user2@example.com", Text = "Jane Doe" },
                    new SelectListItem { Value = "user3@example.com", Text = "Michael Johnson" },
                    new SelectListItem { Value = "all", Text = "All Users" }
                };

                // Condition operators
                ViewBag.Conditions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "greater_than", Text = "Greater Than" },
                    new SelectListItem { Value = "less_than", Text = "Less Than" },
                    new SelectListItem { Value = "equal_to", Text = "Equal To" },
                    new SelectListItem { Value = "not_equal_to", Text = "Not Equal To" }
                };

                // Units
                ViewBag.Units = new List<SelectListItem>
                {
                    new SelectListItem { Value = "km_h", Text = "km/h" },
                    new SelectListItem { Value = "mph", Text = "mph" },
                    new SelectListItem { Value = "minutes", Text = "minutes" },
                    new SelectListItem { Value = "hours", Text = "hours" },
                    new SelectListItem { Value = "days", Text = "days" }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating viewbags for alert form");
            }
        }
    }
} 