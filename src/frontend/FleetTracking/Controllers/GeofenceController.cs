using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FleetTracking.Models;
using FleetTracking.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FleetTracking.Controllers
{
    [Authorize]
    public class GeofenceController : Controller
    {
        private readonly ILogger<GeofenceController> _logger;
        private readonly IApiService _apiService;

        public GeofenceController(
            ILogger<GeofenceController> logger,
            IApiService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }

        // GET: Geofence
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _apiService.GetAsync<List<Geofence>>("geofences");
                return View(response ?? new List<Geofence>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving geofences");
                return View(new List<Geofence>());
            }
        }

        // GET: Geofence/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var geofence = await _apiService.GetAsync<Geofence>($"geofences/{id}");
                if (geofence == null)
                {
                    return NotFound();
                }
                return View(geofence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving geofence details for ID {id}");
                return NotFound();
            }
        }

        // GET: Geofence/Create
        public IActionResult Create()
        {
            ViewBag.GeofenceTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "circle", Text = "Circle" },
                new SelectListItem { Value = "polygon", Text = "Polygon" },
                new SelectListItem { Value = "rectangle", Text = "Rectangle" }
            };

            ViewBag.GeofenceCategories = new List<SelectListItem>
            {
                new SelectListItem { Value = "restricted", Text = "Restricted Area" },
                new SelectListItem { Value = "warehouse", Text = "Warehouse" },
                new SelectListItem { Value = "customer", Text = "Customer Site" },
                new SelectListItem { Value = "poi", Text = "Point of Interest" },
                new SelectListItem { Value = "custom", Text = "Custom" }
            };

            return View(new Geofence { Active = true });
        }

        // POST: Geofence/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Geofence geofence)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    geofence.CreatedAt = DateTime.UtcNow;
                    geofence.UpdatedAt = DateTime.UtcNow;
                    geofence.CreatedBy = 1; // Replace with actual user ID
                    geofence.LastModified = 1; // Replace with actual user ID

                    var response = await _apiService.PostAsync<Geofence, Geofence>("geofences", geofence);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating geofence");
                    ModelState.AddModelError("", "Failed to create geofence.");
                }
            }

            ViewBag.GeofenceTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "circle", Text = "Circle" },
                new SelectListItem { Value = "polygon", Text = "Polygon" },
                new SelectListItem { Value = "rectangle", Text = "Rectangle" }
            };

            ViewBag.GeofenceCategories = new List<SelectListItem>
            {
                new SelectListItem { Value = "restricted", Text = "Restricted Area" },
                new SelectListItem { Value = "warehouse", Text = "Warehouse" },
                new SelectListItem { Value = "customer", Text = "Customer Site" },
                new SelectListItem { Value = "poi", Text = "Point of Interest" },
                new SelectListItem { Value = "custom", Text = "Custom" }
            };

            return View(geofence);
        }

        // GET: Geofence/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var geofence = await _apiService.GetAsync<Geofence>($"geofences/{id}");
                if (geofence == null)
                {
                    return NotFound();
                }

                ViewBag.GeofenceTypes = new List<SelectListItem>
                {
                    new SelectListItem { Value = "circle", Text = "Circle" },
                    new SelectListItem { Value = "polygon", Text = "Polygon" },
                    new SelectListItem { Value = "rectangle", Text = "Rectangle" }
                };

                ViewBag.GeofenceCategories = new List<SelectListItem>
                {
                    new SelectListItem { Value = "restricted", Text = "Restricted Area" },
                    new SelectListItem { Value = "warehouse", Text = "Warehouse" },
                    new SelectListItem { Value = "customer", Text = "Customer Site" },
                    new SelectListItem { Value = "poi", Text = "Point of Interest" },
                    new SelectListItem { Value = "custom", Text = "Custom" }
                };

                return View(geofence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving geofence for edit with ID {id}");
                return NotFound();
            }
        }

        // POST: Geofence/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Geofence geofence)
        {
            if (id != geofence.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    geofence.UpdatedAt = DateTime.UtcNow;
                    geofence.LastModified = 1; // Replace with actual user ID

                    await _apiService.PutAsync<Geofence, object>($"geofences/{id}", geofence);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating geofence with ID {id}");
                    ModelState.AddModelError("", "Failed to update geofence.");
                }
            }

            ViewBag.GeofenceTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "circle", Text = "Circle" },
                new SelectListItem { Value = "polygon", Text = "Polygon" },
                new SelectListItem { Value = "rectangle", Text = "Rectangle" }
            };

            ViewBag.GeofenceCategories = new List<SelectListItem>
            {
                new SelectListItem { Value = "restricted", Text = "Restricted Area" },
                new SelectListItem { Value = "warehouse", Text = "Warehouse" },
                new SelectListItem { Value = "customer", Text = "Customer Site" },
                new SelectListItem { Value = "poi", Text = "Point of Interest" },
                new SelectListItem { Value = "custom", Text = "Custom" }
            };

            return View(geofence);
        }

        // GET: Geofence/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var geofence = await _apiService.GetAsync<Geofence>($"geofences/{id}");
                if (geofence == null)
                {
                    return NotFound();
                }
                return View(geofence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving geofence for delete with ID {id}");
                return NotFound();
            }
        }

        // POST: Geofence/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _apiService.DeleteAsync($"geofences/{id}");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting geofence with ID {id}");
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Geofence/Map
        public IActionResult Map()
        {
            return View();
        }

        // POST: Geofence/CheckPoint
        [HttpPost]
        public async Task<IActionResult> CheckPoint([FromBody] GeofenceCheckRequest request)
        {
            try
            {
                var result = await _apiService.PostAsync<GeofenceCheckRequest, GeofenceCheckResult>("geofences/check", request);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking point against geofences");
                return StatusCode(500, "Failed to check point against geofences");
            }
        }

        // API Methods for AJAX calls

        // GET: Geofence/GetAllGeofences
        [HttpGet]
        public async Task<IActionResult> GetAllGeofences()
        {
            try
            {
                var response = await _apiService.GetAsync<List<Geofence>>("geofences");
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all geofences");
                return Json(new List<Geofence>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportGeofences([FromBody] List<Geofence> geofences)
        {
            try
            {
                if (geofences == null || !geofences.Any())
                {
                    return Json(new { success = false, message = "No geofences provided" });
                }

                int successCount = 0;
                foreach (var geofence in geofences)
                {
                    try
                    {
                        // Set required properties
                        geofence.CreatedAt = DateTime.UtcNow;
                        geofence.UpdatedAt = DateTime.UtcNow;
                        geofence.CreatedBy = 1; // Replace with actual user ID
                        geofence.LastModified = 1; // Replace with actual user ID
                        
                        await _apiService.PostAsync<Geofence, Geofence>("geofences", geofence);
                        successCount++;
                    }
                    catch (Exception innerEx)
                    {
                        _logger.LogError(innerEx, $"Error importing geofence: {geofence.Name}");
                    }
                }

                return Json(new { success = true, count = successCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing geofences");
                return Json(new { success = false, message = "Import failed" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BatchOperation(string operation, [FromBody] BatchOperationRequest request)
        {
            try
            {
                if (request == null || request.GeofenceIds == null || !request.GeofenceIds.Any())
                {
                    return Json(new { success = false, message = "No geofences selected" });
                }

                int successCount = 0;
                string endpoint = string.Empty;
                object data = null;

                switch (operation.ToLower())
                {
                    case "updatestatus":
                        endpoint = "geofences/batch/status";
                        data = new 
                        { 
                            geofenceIds = request.GeofenceIds, 
                            active = request.Status 
                        };
                        break;
                    
                    case "updatecategory":
                        endpoint = "geofences/batch/category";
                        data = new 
                        { 
                            geofenceIds = request.GeofenceIds, 
                            category = request.Category 
                        };
                        break;
                    
                    case "delete":
                        endpoint = "geofences/batch/delete";
                        data = new { geofenceIds = request.GeofenceIds };
                        break;
                    
                    default:
                        return Json(new { success = false, message = "Invalid operation" });
                }

                var response = await _apiService.PostAsync<object, BatchOperationResponse>(endpoint, data);
                
                return Json(new { 
                    success = true, 
                    count = response?.SuccessCount ?? request.GeofenceIds.Count 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in batch operation: {operation}");
                return Json(new { success = false, message = "Operation failed" });
            }
        }
    }

    public class GeofenceCheckRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int VehicleId { get; set; }
        public int? TripId { get; set; }
        public string Timestamp { get; set; }
    }

    public class GeofenceCheckResult
    {
        public bool InGeofences { get; set; }
        public List<int> GeofenceMatches { get; set; }
        public int VehicleId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class BatchOperationRequest
    {
        public List<int> GeofenceIds { get; set; }
        public bool Status { get; set; }
        public string Category { get; set; }
    }

    public class BatchOperationResponse
    {
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        public List<string> Errors { get; set; }
    }
} 