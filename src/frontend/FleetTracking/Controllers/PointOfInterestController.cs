using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using FleetTracking.Models;
using FleetTracking.Services;
using Microsoft.EntityFrameworkCore;
using FleetTracking.Data;

namespace FleetTracking.Controllers
{
    public class PointOfInterestController : Controller
    {
        private readonly ILogger<PointOfInterestController> _logger;
        private readonly ApiService _apiService;
        private readonly ApplicationDbContext _context;

        public PointOfInterestController(
            ILogger<PointOfInterestController> logger,
            ApiService apiService,
            ApplicationDbContext context)
        {
            _logger = logger;
            _apiService = apiService;
            _context = context;
        }

        // GET: PointOfInterest
        public async Task<IActionResult> Index()
        {
            try
            {
                // Try to get POIs from API first
                var pois = await _apiService.GetAsync<List<PointOfInterest>>("pois");
                return View(pois);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve POIs from API, falling back to database");
                
                // Fallback to database
                var pois = await _context.PointsOfInterest.ToListAsync();
                return View(pois);
            }
        }

        // GET: PointOfInterest/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var poi = await _apiService.GetAsync<PointOfInterest>($"pois/{id}");
                if (poi == null)
                {
                    return NotFound();
                }
                return View(poi);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to retrieve POI {id} from API, falling back to database");
                
                // Fallback to database
                var poi = await _context.PointsOfInterest.FirstOrDefaultAsync(p => p.Id == id);
                if (poi == null)
                {
                    return NotFound();
                }
                return View(poi);
            }
        }

        // GET: PointOfInterest/Create
        public IActionResult Create()
        {
            ViewBag.Categories = new List<SelectListItem>
            {
                new SelectListItem { Value = "fuel_station", Text = "Fuel Station" },
                new SelectListItem { Value = "restaurant", Text = "Restaurant" },
                new SelectListItem { Value = "rest_area", Text = "Rest Area" },
                new SelectListItem { Value = "customer", Text = "Customer Location" },
                new SelectListItem { Value = "warehouse", Text = "Warehouse" },
                new SelectListItem { Value = "service_center", Text = "Service Center" },
                new SelectListItem { Value = "parking", Text = "Parking" },
                new SelectListItem { Value = "hotel", Text = "Hotel" },
                new SelectListItem { Value = "custom", Text = "Custom" }
            };
            
            return View(new PointOfInterest { IsActive = true });
        }

        // POST: PointOfInterest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PointOfInterest poi)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    poi.CreatedAt = DateTime.UtcNow;
                    poi.UpdatedAt = DateTime.UtcNow;
                    poi.CreatedBy = 1; // Replace with actual user ID

                    var response = await _apiService.PostAsync<PointOfInterest, PointOfInterest>("pois", poi);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating POI");
                    ModelState.AddModelError("", "Failed to create POI.");
                    
                    // Fallback to database
                    try
                    {
                        _context.PointsOfInterest.Add(poi);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception dbEx)
                    {
                        _logger.LogError(dbEx, "Error creating POI in database");
                        ModelState.AddModelError("", "Failed to create POI in database.");
                    }
                }
            }

            ViewBag.Categories = new List<SelectListItem>
            {
                new SelectListItem { Value = "fuel_station", Text = "Fuel Station" },
                new SelectListItem { Value = "restaurant", Text = "Restaurant" },
                new SelectListItem { Value = "rest_area", Text = "Rest Area" },
                new SelectListItem { Value = "customer", Text = "Customer Location" },
                new SelectListItem { Value = "warehouse", Text = "Warehouse" },
                new SelectListItem { Value = "service_center", Text = "Service Center" },
                new SelectListItem { Value = "parking", Text = "Parking" },
                new SelectListItem { Value = "hotel", Text = "Hotel" },
                new SelectListItem { Value = "custom", Text = "Custom" }
            };

            return View(poi);
        }

        // GET: PointOfInterest/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var poi = await _apiService.GetAsync<PointOfInterest>($"pois/{id}");
                if (poi == null)
                {
                    return NotFound();
                }

                ViewBag.Categories = new List<SelectListItem>
                {
                    new SelectListItem { Value = "fuel_station", Text = "Fuel Station" },
                    new SelectListItem { Value = "restaurant", Text = "Restaurant" },
                    new SelectListItem { Value = "rest_area", Text = "Rest Area" },
                    new SelectListItem { Value = "customer", Text = "Customer Location" },
                    new SelectListItem { Value = "warehouse", Text = "Warehouse" },
                    new SelectListItem { Value = "service_center", Text = "Service Center" },
                    new SelectListItem { Value = "parking", Text = "Parking" },
                    new SelectListItem { Value = "hotel", Text = "Hotel" },
                    new SelectListItem { Value = "custom", Text = "Custom" }
                };

                return View(poi);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to retrieve POI {id} from API for edit, falling back to database");
                
                // Fallback to database
                var poi = await _context.PointsOfInterest.FirstOrDefaultAsync(p => p.Id == id);
                if (poi == null)
                {
                    return NotFound();
                }
                
                ViewBag.Categories = new List<SelectListItem>
                {
                    new SelectListItem { Value = "fuel_station", Text = "Fuel Station" },
                    new SelectListItem { Value = "restaurant", Text = "Restaurant" },
                    new SelectListItem { Value = "rest_area", Text = "Rest Area" },
                    new SelectListItem { Value = "customer", Text = "Customer Location" },
                    new SelectListItem { Value = "warehouse", Text = "Warehouse" },
                    new SelectListItem { Value = "service_center", Text = "Service Center" },
                    new SelectListItem { Value = "parking", Text = "Parking" },
                    new SelectListItem { Value = "hotel", Text = "Hotel" },
                    new SelectListItem { Value = "custom", Text = "Custom" }
                };

                return View(poi);
            }
        }

        // POST: PointOfInterest/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PointOfInterest poi)
        {
            if (id != poi.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    poi.UpdatedAt = DateTime.UtcNow;

                    await _apiService.PutAsync<PointOfInterest, object>($"pois/{id}", poi);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating POI with ID {id}");
                    ModelState.AddModelError("", "Failed to update POI.");
                    
                    // Fallback to database
                    try
                    {
                        _context.Update(poi);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!POIExists(poi.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    catch (Exception dbEx)
                    {
                        _logger.LogError(dbEx, $"Error updating POI with ID {id} in database");
                        ModelState.AddModelError("", "Failed to update POI in database.");
                    }
                }
            }

            ViewBag.Categories = new List<SelectListItem>
            {
                new SelectListItem { Value = "fuel_station", Text = "Fuel Station" },
                new SelectListItem { Value = "restaurant", Text = "Restaurant" },
                new SelectListItem { Value = "rest_area", Text = "Rest Area" },
                new SelectListItem { Value = "customer", Text = "Customer Location" },
                new SelectListItem { Value = "warehouse", Text = "Warehouse" },
                new SelectListItem { Value = "service_center", Text = "Service Center" },
                new SelectListItem { Value = "parking", Text = "Parking" },
                new SelectListItem { Value = "hotel", Text = "Hotel" },
                new SelectListItem { Value = "custom", Text = "Custom" }
            };

            return View(poi);
        }

        // GET: PointOfInterest/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var poi = await _apiService.GetAsync<PointOfInterest>($"pois/{id}");
                if (poi == null)
                {
                    return NotFound();
                }
                return View(poi);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to retrieve POI {id} from API for delete, falling back to database");
                
                // Fallback to database
                var poi = await _context.PointsOfInterest.FirstOrDefaultAsync(p => p.Id == id);
                if (poi == null)
                {
                    return NotFound();
                }
                return View(poi);
            }
        }

        // POST: PointOfInterest/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _apiService.DeleteAsync($"pois/{id}");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting POI with ID {id}");
                
                // Fallback to database
                try
                {
                    var poi = await _context.PointsOfInterest.FindAsync(id);
                    if (poi != null)
                    {
                        _context.PointsOfInterest.Remove(poi);
                        await _context.SaveChangesAsync();
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, $"Error deleting POI with ID {id} from database");
                    return RedirectToAction(nameof(Index));
                }
            }
        }
        
        // GET: PointOfInterest/Map
        public async Task<IActionResult> Map()
        {
            try
            {
                var pois = await _apiService.GetAsync<List<PointOfInterest>>("pois");
                return View(pois);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve POIs from API for map, falling back to database");
                
                // Fallback to database
                var pois = await _context.PointsOfInterest.ToListAsync();
                return View(pois);
            }
        }
        
        // Import/Export functionality
        
        // GET: PointOfInterest/Import
        public IActionResult Import()
        {
            return View();
        }
        
        // POST: PointOfInterest/Import
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(string jsonData)
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                ModelState.AddModelError("", "No data provided for import.");
                return View();
            }
            
            try
            {
                // This would typically be handled by an API endpoint
                // For demo purposes, we'll add a simple implementation here
                var pois = System.Text.Json.JsonSerializer.Deserialize<List<PointOfInterest>>(jsonData);
                
                foreach (var poi in pois)
                {
                    poi.Id = 0; // Clear ID to allow insertion
                    poi.CreatedAt = DateTime.UtcNow;
                    poi.UpdatedAt = DateTime.UtcNow;
                    poi.CreatedBy = 1; // Replace with actual user ID
                    
                    _context.PointsOfInterest.Add(poi);
                }
                
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Successfully imported {pois.Count} points of interest.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing POIs");
                ModelState.AddModelError("", "Failed to import POIs. Please check the format of your data.");
                return View();
            }
        }
        
        // GET: PointOfInterest/Export
        public async Task<IActionResult> Export()
        {
            try
            {
                var pois = await _context.PointsOfInterest.ToListAsync();
                var jsonData = System.Text.Json.JsonSerializer.Serialize(pois, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                return File(System.Text.Encoding.UTF8.GetBytes(jsonData), "application/json", "pois_export.json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting POIs");
                TempData["ErrorMessage"] = "Failed to export POIs.";
                return RedirectToAction(nameof(Index));
            }
        }
        
        // GET: PointOfInterest/Search
        public async Task<IActionResult> Search(string query, string category)
        {
            IQueryable<PointOfInterest> poisQuery = _context.PointsOfInterest;
            
            if (!string.IsNullOrEmpty(query))
            {
                poisQuery = poisQuery.Where(p => 
                    p.Name.Contains(query) || 
                    p.Description.Contains(query) || 
                    p.Address.Contains(query) ||
                    p.Tags.Contains(query));
            }
            
            if (!string.IsNullOrEmpty(category))
            {
                poisQuery = poisQuery.Where(p => p.Category == category);
            }
            
            var pois = await poisQuery.ToListAsync();
            
            return PartialView("_POIList", pois);
        }
        
        // GET: PointOfInterest/Nearby
        [HttpGet]
        public async Task<IActionResult> Nearby(double lat, double lng, double radius = 5.0)
        {
            try
            {
                // This would typically be handled by a spatial query in the database or API
                // For demo purposes, we'll use a simple distance calculation
                var pois = await _context.PointsOfInterest.ToListAsync();
                
                // Filter POIs based on distance
                var nearbyPois = pois.Where(p => CalculateDistance(lat, lng, p.Latitude, p.Longitude) <= radius).ToList();
                
                return Json(nearbyPois);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding nearby POIs");
                return Json(new { error = "Failed to find nearby POIs." });
            }
        }
        
        // Helper method to calculate distance between two points using Haversine formula
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in km
            
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                    
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = R * c;
            
            return distance;
        }
        
        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        private bool POIExists(int id)
        {
            return _context.PointsOfInterest.Any(e => e.Id == id);
        }
    }
} 