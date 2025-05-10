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
        public async Task<IActionResult> Create([Bind("Id,VehicleId,DriverId,StartLocation,EndLocation,StartTime,EndTime,Status,Distance,FuelUsed,AverageSpeed,Notes")] Trip trip)
        {
            if (ModelState.IsValid)
            {
                trip.CreatedAt = DateTime.UtcNow;
                trip.UpdatedAt = DateTime.UtcNow;
                _context.Add(trip);
                await _context.SaveChangesAsync();
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

            var trip = await _context.Trips.FindAsync(id);
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
                
            return View(trip);
        }

        // POST: Trip/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VehicleId,DriverId,StartLocation,EndLocation,StartTime,EndTime,Status,Distance,FuelUsed,AverageSpeed,Notes,CreatedAt")] Trip trip)
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
                waypoint.CreatedAt = DateTime.UtcNow;
                waypoint.UpdatedAt = DateTime.UtcNow;
                _context.Add(waypoint);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = waypoint.TripId });
            }
            
            var trip = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Include(t => t.Driver.User)
                .FirstOrDefaultAsync(m => m.Id == waypoint.TripId);
                
            ViewData["Trip"] = trip;
            
            return View(waypoint);
        }

        private bool TripExists(int id)
        {
            return _context.Trips.Any(e => e.Id == id);
        }
    }
} 