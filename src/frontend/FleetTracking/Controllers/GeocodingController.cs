using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FleetTracking.Models;
using FleetTracking.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FleetTracking.Controllers
{
    [Authorize]
    public class GeocodingController : Controller
    {
        private readonly ILogger<GeocodingController> _logger;
        private readonly IApiService _apiService;

        public GeocodingController(
            ILogger<GeocodingController> logger,
            IApiService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }

        // GET: Geocoding
        public IActionResult Index()
        {
            return View();
        }

        // GET: Geocoding/Search
        [HttpGet]
        public async Task<IActionResult> Search(string query, string type = "forward")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Json(new List<GeocodingResult>());
                }

                var endpoint = type == "reverse" ? 
                    $"geocoding/reverse?coordinates={query}" : 
                    $"geocoding/forward?address={Uri.EscapeDataString(query)}";

                var results = await _apiService.GetAsync<List<GeocodingResult>>(endpoint);
                return Json(results ?? new List<GeocodingResult>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error performing {type} geocoding for query: {query}");
                return Json(new { error = "Failed to perform geocoding" });
            }
        }

        // GET: Geocoding/Batch
        public IActionResult Batch()
        {
            return View();
        }

        // POST: Geocoding/Batch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Batch(string addresses, string type)
        {
            try
            {
                // Process the batch of addresses
                var addressList = JsonSerializer.Deserialize<List<string>>(addresses);
                var request = new
                {
                    Addresses = addressList,
                    Type = type
                };

                var results = await _apiService.PostAsync<List<GeocodingResult>>("geocoding/batch", request);
                return View("BatchResults", results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing batch geocoding");
                ModelState.AddModelError("", "Failed to process batch geocoding request");
                return View();
            }
        }

        // GET: Geocoding/Suggest
        [HttpGet]
        public async Task<IActionResult> Suggest(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Length < 3)
                {
                    return Json(new List<string>());
                }

                var endpoint = $"geocoding/suggest?query={Uri.EscapeDataString(query)}";
                var suggestions = await _apiService.GetAsync<List<string>>(endpoint);
                return Json(suggestions ?? new List<string>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving address suggestions for query: {query}");
                return Json(new List<string>());
            }
        }

        // GET: Geocoding/SavedLocations
        public async Task<IActionResult> SavedLocations()
        {
            try
            {
                var locations = await _apiService.GetAsync<List<SavedLocation>>("locations/saved");
                return View(locations ?? new List<SavedLocation>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving saved locations");
                return View(new List<SavedLocation>());
            }
        }

        // GET: Geocoding/CreateLocation
        public IActionResult CreateLocation()
        {
            return View(new SavedLocation { IsActive = true });
        }

        // POST: Geocoding/CreateLocation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLocation(SavedLocation location)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    location.CreatedAt = DateTime.UtcNow;
                    location.UpdatedAt = DateTime.UtcNow;
                    location.CreatedBy = 1; // Replace with actual user ID

                    await _apiService.PostAsync<SavedLocation>("locations/saved", location);
                    return RedirectToAction(nameof(SavedLocations));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating saved location");
                    ModelState.AddModelError("", "Failed to create saved location");
                }
            }

            return View(location);
        }

        // GET: Geocoding/EditLocation/5
        public async Task<IActionResult> EditLocation(int id)
        {
            try
            {
                var location = await _apiService.GetAsync<SavedLocation>($"locations/saved/{id}");
                if (location == null)
                {
                    return NotFound();
                }
                return View(location);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving saved location with ID {id}");
                return NotFound();
            }
        }

        // POST: Geocoding/EditLocation/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLocation(int id, SavedLocation location)
        {
            if (id != location.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    location.UpdatedAt = DateTime.UtcNow;
                    await _apiService.PutAsync<SavedLocation>($"locations/saved/{id}", location);
                    return RedirectToAction(nameof(SavedLocations));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating saved location with ID {id}");
                    ModelState.AddModelError("", "Failed to update saved location");
                }
            }

            return View(location);
        }

        // GET: Geocoding/DeleteLocation/5
        public async Task<IActionResult> DeleteLocation(int id)
        {
            try
            {
                var location = await _apiService.GetAsync<SavedLocation>($"locations/saved/{id}");
                if (location == null)
                {
                    return NotFound();
                }
                return View(location);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving saved location for delete with ID {id}");
                return NotFound();
            }
        }

        // POST: Geocoding/DeleteLocation/5
        [HttpPost, ActionName("DeleteLocation")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLocationConfirmed(int id)
        {
            try
            {
                await _apiService.DeleteAsync($"locations/saved/{id}");
                return RedirectToAction(nameof(SavedLocations));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting saved location with ID {id}");
                return RedirectToAction(nameof(SavedLocations));
            }
        }
    }
} 