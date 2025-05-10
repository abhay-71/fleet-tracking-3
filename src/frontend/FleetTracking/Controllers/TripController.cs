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

namespace FleetTracking.Controllers
{
    [Authorize]
    public class TripController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TripController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Trip
        public async Task<IActionResult> Index()
        {
            var trips = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Include(t => t.Driver.User)
                .OrderByDescending(t => t.StartTime)
                .ToListAsync();
            return View(trips);
        }

        // GET: Trip/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: Trip/Create
        public IActionResult Create()
        {
            ViewData["VehicleId"] = new SelectList(_context.Vehicles.Where(v => v.Status == "active"), "Id", "DisplayName");
            ViewData["DriverId"] = new SelectList(
                _context.Drivers
                    .Include(d => d.User)
                    .Where(d => d.Status == "active")
                    .Select(d => new { 
                        d.Id, 
                        DriverName = $"{d.User.FirstName} {d.User.LastName}" 
                    }), 
                "Id", 
                "DriverName");
                
            return View();
        }

        // POST: Trip/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                            Location = trip.StartLocation,
                            Latitude = StartLatitude,
                            Longitude = StartLongitude,
                            SequenceNumber = 0, // Start is always 0
                            Status = "start",
                            ArrivalTime = trip.StartTime,
                            DepartureTime = trip.StartTime.AddMinutes(5) // Assuming 5 minutes at start
                        };
                        _context.Waypoints.Add(startWaypoint);
                        
                        // Add intermediate waypoints
                        foreach (var wpDto in waypointList)
                        {
                            var waypoint = new Waypoint
                            {
                                TripId = trip.Id,
                                Location = $"Waypoint {wpDto.SequenceNumber}",
                                Latitude = wpDto.Latitude,
                                Longitude = wpDto.Longitude,
                                SequenceNumber = wpDto.SequenceNumber,
                                Status = "scheduled",
                                // Estimate arrival time based on sequence
                                ArrivalTime = trip.StartTime.AddMinutes(30 * wpDto.SequenceNumber),
                                DepartureTime = trip.StartTime.AddMinutes((30 * wpDto.SequenceNumber) + 15)
                            };
                            _context.Waypoints.Add(waypoint);
                        }
                        
                        // Add end waypoint
                        var endWaypoint = new Waypoint
                        {
                            TripId = trip.Id,
                            Location = trip.EndLocation,
                            Latitude = EndLatitude,
                            Longitude = EndLongitude,
                            SequenceNumber = waypointList.Count + 1, // End is after all waypoints
                            Status = "end",
                            ArrivalTime = trip.EndTime ?? trip.StartTime.AddHours(2), // Default to 2 hours later if no end time
                            DepartureTime = null // No departure from end
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
            ViewData["DriverId"] = new SelectList(
                _context.Drivers
                    .Include(d => d.User)
                    .Select(d => new { 
                        d.Id, 
                        DriverName = $"{d.User.FirstName} {d.User.LastName}" 
                    }), 
                "Id", 
                "DriverName", 
                trip.DriverId);
                
            return View(trip);
        }

        // GET: Trip/Edit/5
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
            ViewData["DriverId"] = new SelectList(
                _context.Drivers
                    .Include(d => d.User)
                    .Select(d => new { 
                        d.Id, 
                        DriverName = $"{d.User.FirstName} {d.User.LastName}" 
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
                            Location = trip.StartLocation,
                            Latitude = StartLatitude,
                            Longitude = StartLongitude,
                            SequenceNumber = 0, // Start is always 0
                            Status = "start",
                            ArrivalTime = trip.StartTime,
                            DepartureTime = trip.StartTime.AddMinutes(5) // Assuming 5 minutes at start
                        };
                        _context.Waypoints.Add(startWaypoint);
                        
                        // Add intermediate waypoints
                        foreach (var wpDto in waypointList)
                        {
                            var waypoint = new Waypoint
                            {
                                TripId = trip.Id,
                                Location = $"Waypoint {wpDto.SequenceNumber}",
                                Latitude = wpDto.Latitude,
                                Longitude = wpDto.Longitude,
                                SequenceNumber = wpDto.SequenceNumber,
                                Status = "scheduled",
                                // Estimate arrival time based on sequence
                                ArrivalTime = trip.StartTime.AddMinutes(30 * wpDto.SequenceNumber),
                                DepartureTime = trip.StartTime.AddMinutes((30 * wpDto.SequenceNumber) + 15)
                            };
                            _context.Waypoints.Add(waypoint);
                        }
                        
                        // Add end waypoint
                        var endWaypoint = new Waypoint
                        {
                            TripId = trip.Id,
                            Location = trip.EndLocation,
                            Latitude = EndLatitude,
                            Longitude = EndLongitude,
                            SequenceNumber = waypointList.Count + 1, // End is after all waypoints
                            Status = "end",
                            ArrivalTime = trip.EndTime ?? trip.StartTime.AddHours(2), // Default to 2 hours later if no end time
                            DepartureTime = null // No departure from end
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
                        DriverName = $"{d.User.FirstName} {d.User.LastName}" 
                    }), 
                "Id", 
                "DriverName", 
                trip.DriverId);
                
            return View(trip);
        }

        // GET: Trip/Delete/5
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
                SequenceNumber = await _context.Waypoints.Where(w => w.TripId == trip.Id).CountAsync() + 1
            };
            
            ViewData["Trip"] = trip;
            
            return View(waypoint);
        }

        // POST: Trip/AddWaypoint
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWaypoint([Bind("Id,TripId,Location,Latitude,Longitude,SequenceNumber,ArrivalTime,DepartureTime,Status,Notes")] Waypoint waypoint)
        {
            if (ModelState.IsValid)
            {
                _context.Add(waypoint);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = waypoint.TripId });
            }
            
            var trip = await _context.Trips.FindAsync(waypoint.TripId);
            if (trip == null)
            {
                return NotFound();
            }
            
            ViewData["Trip"] = trip;
            
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
                    DriverName = g.FirstOrDefault()?.Driver?.User?.FirstName + " " + g.FirstOrDefault()?.Driver?.User?.LastName,
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
                    VehicleName = g.FirstOrDefault()?.Vehicle?.DisplayName,
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
    }
    
    // DTO for waypoint data from JSON
    public class WaypointDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int SequenceNumber { get; set; }
    }
} 