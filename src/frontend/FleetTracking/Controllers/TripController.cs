using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using FleetTracking.Data;
using FleetTracking.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace FleetTracking.Controllers
{
    [Authorize]
    public class TripController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TripController> _logger;

        public TripController(ApplicationDbContext context, ILogger<TripController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Trip
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Retrieving list of all trips");

                var trips = await _context.Trips
                    .Include(t => t.Vehicle)
                    .Include(t => t.Driver)
                    .Include(t => t.Driver.User)
                    .OrderByDescending(t => t.StartTime)
                    .ToListAsync();

                _logger.LogInformation("Successfully retrieved {Count} trips", trips.Count);
                return View(trips);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving trips list");
                TempData["ErrorMessage"] = "An error occurred while retrieving the trips. Please try again.";
                return View(new List<Trip>());
            }
        }

        // GET: Trip/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Trip Details accessed with null ID");
                return NotFound();
            }

            try
            {
                _logger.LogInformation("Retrieving details for trip with ID: {Id}", id);

                var trip = await _context.Trips
                    .Include(t => t.Vehicle)
                    .Include(t => t.Driver)
                    .Include(t => t.Driver.User)
                    .Include(t => t.Waypoints.OrderBy(w => w.SequenceNumber))
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (trip == null)
                {
                    _logger.LogWarning("Trip with ID: {Id} not found", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully retrieved details for trip with ID: {Id}", id);
                return View(trip);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving details for trip with ID: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving trip details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Trip/Create
        [Authorize(Roles = "Administrator,Manager,Dispatcher")]
        public IActionResult Create()
        {
            try
            {
                _logger.LogInformation("Accessing trip creation form");

                ViewData["VehicleId"] = new SelectList(_context.Vehicles.Where(v => v.Status == "active"), "Id", "DisplayName");
                
                // Modify the driver query to avoid null-propagating operator in LINQ expression
                var driversWithUsers = _context.Drivers
                    .Include(d => d.User)
                    .Where(d => d.Status == "active")
                    .ToList();
                    
                var driverSelectList = driversWithUsers
                    .Select(d => new 
                    {
                        d.Id,
                        DriverName = GetDriverName(d.User)
                    });
                    
                ViewData["DriverId"] = new SelectList(
                    _context.Drivers
                        .Include(d => d.User)
                        .Where(d => d.Status == "active")
                        .Select(d => new { 
                            d.Id, 
                            DriverName = d.User != null ? GetDriverFullName(d.User) : "Unknown Driver"
                        }),
                    "Id", 
                    "DriverName");
                    
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing trip creation form");
                TempData["ErrorMessage"] = "An error occurred while loading the creation form. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Trip/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager,Dispatcher")]
        public async Task<IActionResult> Create([Bind("Id,VehicleId,DriverId,StartLocation,EndLocation,StartTime,EndTime,Status,Distance,FuelUsed,AverageSpeed,Notes")] Trip trip, string WaypointsJson, 
            double StartLatitude, double StartLongitude, double EndLatitude, double EndLongitude)
        {
            if (ModelState.IsValid)
            {
                trip.CreatedAt = DateTime.UtcNow;
                trip.UpdatedAt = DateTime.UtcNow;
                
                // Save the trip first to get an ID
                _context.Add(trip);
                await _context.SaveChangesAsync();
                
                // Process waypoints if they exist
                if (!string.IsNullOrEmpty(WaypointsJson))
                {
                    try
                    {
                        var waypointList = JsonSerializer.Deserialize<List<WaypointDto>>(WaypointsJson);
                        
                        // Add start waypoint
                        var startWaypoint = new Waypoint
                        {
                            TripId = trip.Id,
                            LocationName = trip.StartLocation,
                            Latitude = StartLatitude,
                            Longitude = StartLongitude,
                            Sequence = 0, // Start is always 0
                            Status = "start",
                            ActualArrival = trip.StartTime,
                            ActualDeparture = trip.StartTime.AddMinutes(5) // Assuming 5 minutes at start
                        };
                        _context.Waypoints.Add(startWaypoint);
                        
                        // Add intermediate waypoints
                        foreach (var wpDto in waypointList)
                        {
                            var waypoint = new Waypoint
                            {
                                TripId = trip.Id,
                                LocationName = $"Waypoint {wpDto.SequenceNumber}",
                                Latitude = wpDto.Latitude,
                                Longitude = wpDto.Longitude,
                                Sequence = wpDto.SequenceNumber,
                                Status = "scheduled",
                                // Estimate arrival time based on sequence
                                ActualArrival = trip.StartTime.AddMinutes(30 * wpDto.SequenceNumber),
                                ActualDeparture = trip.StartTime.AddMinutes((30 * wpDto.SequenceNumber) + 15)
                            };
                            _context.Waypoints.Add(waypoint);
                        }
                        
                        // Add end waypoint
                        var endWaypoint = new Waypoint
                        {
                            TripId = trip.Id,
                            LocationName = trip.EndLocation,
                            Latitude = EndLatitude,
                            Longitude = EndLongitude,
                            Sequence = waypointList.Count + 1, // End is after all waypoints
                            Status = "end",
                            ActualArrival = trip.EndTime ?? trip.StartTime.AddHours(2), // Default to 2 hours later if no end time
                            ActualDeparture = null // No departure from end
                        };
                        _context.Waypoints.Add(endWaypoint);
                        
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        // Log the error but continue
                        Console.WriteLine($"Error processing waypoints: {ex.Message}");
                    }
                }
                
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "DisplayName", trip.VehicleId);
            
            // Fix the same issue in the POST method
            ViewData["DriverId"] = new SelectList(
                _context.Drivers
                    .Include(d => d.User)
                    .Where(d => d.Status == "active")
                    .Select(d => new { 
                        d.Id, 
                        DriverName = d.User != null ? GetDriverFullName(d.User) : "Unknown Driver"
                    }),
                "Id", 
                "DriverName");
            
            return View(trip);
        }

        // GET: Trip/Edit/5
        [Authorize(Roles = "Administrator,Manager,Dispatcher")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trips
                .Include(t => t.Waypoints)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (trip == null)
            {
                return NotFound();
            }
            
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "DisplayName", trip.VehicleId);
            
            // Fix the same issue here
            ViewData["DriverId"] = new SelectList(
                _context.Drivers
                    .Include(d => d.User)
                    .Select(d => new { 
                        d.Id, 
                        DriverName = d.User != null ? GetDriverFullName(d.User) : "Unknown Driver"
                    }),
                "Id", 
                "DriverName", 
                trip.DriverId);
            
            // Get waypoints for map initialization
            var waypoints = await _context.Waypoints
                .Where(w => w.TripId == id)
                .OrderBy(w => w.SequenceNumber)
                .ToListAsync();
                
            ViewData["Waypoints"] = waypoints;
                
            return View(trip);
        }

        // POST: Trip/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager,Dispatcher")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VehicleId,DriverId,StartLocation,EndLocation,StartTime,EndTime,Status,Distance,FuelUsed,AverageSpeed,Notes,CreatedAt")] Trip trip,
            string WaypointsJson, double StartLatitude, double StartLongitude, double EndLatitude, double EndLongitude)
        {
            if (id != trip.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    trip.UpdatedAt = DateTime.UtcNow;
                    _context.Update(trip);
                    
                    // Process waypoints if they exist
                    if (!string.IsNullOrEmpty(WaypointsJson))
                    {
                        // Remove existing waypoints for this trip
                        var existingWaypoints = await _context.Waypoints
                            .Where(w => w.TripId == trip.Id)
                            .ToListAsync();
                            
                        _context.Waypoints.RemoveRange(existingWaypoints);
                        
                        // Add new waypoints
                        var waypointList = JsonSerializer.Deserialize<List<WaypointDto>>(WaypointsJson);
                        
                        // Add start waypoint
                        var startWaypoint = new Waypoint
                        {
                            TripId = trip.Id,
                            LocationName = trip.StartLocation,
                            Latitude = StartLatitude,
                            Longitude = StartLongitude,
                            Sequence = 0, // Start is always 0
                            Status = "start",
                            ActualArrival = trip.StartTime,
                            ActualDeparture = trip.StartTime.AddMinutes(5) // Assuming 5 minutes at start
                        };
                        _context.Waypoints.Add(startWaypoint);
                        
                        // Add intermediate waypoints
                        foreach (var wpDto in waypointList)
                        {
                            var waypoint = new Waypoint
                            {
                                TripId = trip.Id,
                                LocationName = $"Waypoint {wpDto.SequenceNumber}",
                                Latitude = wpDto.Latitude,
                                Longitude = wpDto.Longitude,
                                Sequence = wpDto.SequenceNumber,
                                Status = "scheduled",
                                // Estimate arrival time based on sequence
                                ActualArrival = trip.StartTime.AddMinutes(30 * wpDto.SequenceNumber),
                                ActualDeparture = trip.StartTime.AddMinutes((30 * wpDto.SequenceNumber) + 15)
                            };
                            _context.Waypoints.Add(waypoint);
                        }
                        
                        // Add end waypoint
                        var endWaypoint = new Waypoint
                        {
                            TripId = trip.Id,
                            LocationName = trip.EndLocation,
                            Latitude = EndLatitude,
                            Longitude = EndLongitude,
                            Sequence = waypointList.Count + 1, // End is after all waypoints
                            Status = "end",
                            ActualArrival = trip.EndTime ?? trip.StartTime.AddHours(2), // Default to 2 hours later if no end time
                            ActualDeparture = null // No departure from end
                        };
                        _context.Waypoints.Add(endWaypoint);
                    }
                    
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TripExists(trip.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "DisplayName", trip.VehicleId);
            ViewData["DriverId"] = new SelectList(
                _context.Drivers
                    .Include(d => d.User)
                    .Select(d => new { 
                        d.Id, 
                        DriverName = d.User != null ? GetDriverFullName(d.User) : "Unknown Driver"
                    }),
                "Id", 
                "DriverName", 
                trip.DriverId);
                
            return View(trip);
        }

        // GET: Trip/Delete/5
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Include(t => t.Driver.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trip == null)
            {
                return NotFound();
            }

            return View(trip);
        }

        // POST: Trip/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip != null)
            {
                // Delete associated waypoints first
                var waypoints = await _context.Waypoints.Where(w => w.TripId == id).ToListAsync();
                _context.Waypoints.RemoveRange(waypoints);
                
                // Then delete the trip
                _context.Trips.Remove(trip);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Trip/AddWaypoint/5
        [Authorize(Roles = "Administrator,Manager,Dispatcher")]
        public async Task<IActionResult> AddWaypoint(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Include(t => t.Driver.User)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (trip == null)
            {
                return NotFound();
            }

            var waypoint = new Waypoint
            {
                TripId = trip.Id,
                Sequence = await _context.Waypoints.Where(w => w.TripId == trip.Id).CountAsync() + 1
            };
            
            ViewData["Trip"] = trip;
            
            return View(waypoint);
        }

        // POST: Trip/AddWaypoint
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager,Dispatcher")]
        public async Task<IActionResult> AddWaypoint([Bind("Id,TripId,LocationName,Latitude,Longitude,Sequence,ActualArrival,ActualDeparture,Status,Notes")] Waypoint waypoint)
        {
            if (ModelState.IsValid)
            {
                waypoint.CreatedAt = DateTime.UtcNow;
                waypoint.UpdatedAt = DateTime.UtcNow;
                _context.Add(waypoint);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Edit), new { id = waypoint.TripId });
            }
            
            ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Id", waypoint.TripId);
            return View(waypoint);
        }
        
        // GET: Trip/History
        public async Task<IActionResult> History()
        {
            var completedTrips = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Include(t => t.Driver.User)
                .Where(t => t.Status == "completed")
                .OrderByDescending(t => t.EndTime)
                .ToListAsync();
                
            return View(completedTrips);
        }
        
        // GET: Trip/Analytics
        [Authorize(Roles = "Administrator,Manager,Dispatcher")]
        public async Task<IActionResult> Analytics()
        {
            // Get all completed trips for analytics
            var completedTrips = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Include(t => t.Driver.User)
                .Where(t => t.Status == "completed")
                .OrderByDescending(t => t.EndTime)
                .ToListAsync();
                
            // Calculate analytics data
            ViewData["TotalTrips"] = completedTrips.Count;
            ViewData["TotalDistance"] = completedTrips.Sum(t => t.Distance);
            ViewData["AverageFuelEfficiency"] = completedTrips.Any(t => t.FuelUsed > 0) ? 
                completedTrips.Where(t => t.FuelUsed > 0).Average(t => t.Distance / t.FuelUsed) : 0;
            
            // Driver performance data
            var driverPerformance = completedTrips
                .GroupBy(t => t.DriverId)
                .Select(g => new {
                    DriverId = g.Key,
                    DriverName = g.FirstOrDefault() != null ? GetDriverNameFromTrip(g.FirstOrDefault()) : "Unknown",
                    TripCount = g.Count(),
                    TotalDistance = g.Sum(t => t.Distance),
                    AverageSpeed = g.Average(t => t.AverageSpeed)
                })
                .OrderByDescending(d => d.TripCount)
                .ToList();
                
            ViewData["DriverPerformance"] = driverPerformance;
            
            // Vehicle performance data
            var vehiclePerformance = completedTrips
                .GroupBy(t => t.VehicleId)
                .Select(g => new {
                    VehicleId = g.Key,
                    VehicleName = g.FirstOrDefault() != null ? GetVehicleName(g.FirstOrDefault()) : "Unknown",
                    TripCount = g.Count(),
                    TotalDistance = g.Sum(t => t.Distance),
                    FuelEfficiency = g.Any(t => t.FuelUsed > 0) ? 
                        g.Where(t => t.FuelUsed > 0).Average(t => t.Distance / t.FuelUsed) : 0
                })
                .OrderByDescending(v => v.TripCount)
                .ToList();
                
            ViewData["VehiclePerformance"] = vehiclePerformance;
                
            return View();
        }
        
        // GET: Trip/Playback/5
        public async Task<IActionResult> Playback(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Include(t => t.Driver.User)
                .Include(t => t.Waypoints.OrderBy(w => w.SequenceNumber))
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (trip == null)
            {
                return NotFound();
            }

            return View(trip);
        }

        private bool TripExists(int id)
        {
            return _context.Trips.Any(e => e.Id == id);
        }
        
        // Helper method to safely get driver name from user
        private string GetDriverName(ApplicationUser user)
        {
            if (user == null) return "Unknown";
            return $"{user.FirstName} {user.LastName}";
        }
        
        private string GetDriverFullName(ApplicationUser user)
        {
            if (user == null) return string.Empty;
            
            string firstName = user.FirstName;
            if (firstName == null) firstName = string.Empty;
            
            string lastName = user.LastName;
            if (lastName == null) lastName = string.Empty;
            
            return $"{firstName} {lastName}";
        }
        
        // Helper method to safely get vehicle name
        private string GetVehicleName(Trip trip)
        {
            if (trip == null || trip.Vehicle == null)
                return "Unknown";
                
            return trip.Vehicle.DisplayName ?? "Unknown";
        }

        // Helper method to get driver name from trip
        private string GetDriverNameFromTrip(Trip trip)
        {
            if (trip == null || trip.Driver == null || trip.Driver.User == null)
                return "Unknown";
                
            return GetDriverFullName(trip.Driver.User);
        }
    }
    
    // DTO for waypoint data from JSON
    public class WaypointDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int SequenceNumber { get; set; }
    }
} 