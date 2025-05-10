using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using FleetTracking.Models;

namespace FleetTracking.Services
{
    public class VehicleHistoryService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiUrl;

        public VehicleHistoryService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _apiUrl = _configuration["ApiEndpoints:Backend"] ?? "http://localhost:5001/api";
        }

        /// <summary>
        /// Gets vehicle history data for a specific vehicle within a date range
        /// </summary>
        public async Task<List<TripWaypoint>> GetVehicleHistoryDataAsync(int vehicleId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiUrl}/vehicleHistory/{vehicleId}?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<List<TripWaypoint>>(content, options);
                }
                else
                {
                    // Log error
                    Console.WriteLine($"Error retrieving vehicle history: {response.StatusCode}");
                    return new List<TripWaypoint>();
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Exception in GetVehicleHistoryDataAsync: {ex.Message}");
                return new List<TripWaypoint>();
            }
        }

        /// <summary>
        /// Gets trips for a specific vehicle within a date range
        /// </summary>
        public async Task<List<Trip>> GetVehicleTripsAsync(int vehicleId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiUrl}/trips?vehicleId={vehicleId}&startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<List<Trip>>(content, options);
                }
                else
                {
                    // Log error
                    Console.WriteLine($"Error retrieving vehicle trips: {response.StatusCode}");
                    return new List<Trip>();
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Exception in GetVehicleTripsAsync: {ex.Message}");
                return new List<Trip>();
            }
        }

        /// <summary>
        /// Gets a specific trip by ID
        /// </summary>
        public async Task<Trip> GetTripByIdAsync(int tripId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiUrl}/trips/{tripId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<Trip>(content, options);
                }
                else
                {
                    // Log error
                    Console.WriteLine($"Error retrieving trip: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Exception in GetTripByIdAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets waypoints for a specific trip
        /// </summary>
        public async Task<List<TripWaypoint>> GetTripWaypointsAsync(int tripId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiUrl}/trips/{tripId}/waypoints");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<List<TripWaypoint>>(content, options);
                }
                else
                {
                    // Log error
                    Console.WriteLine($"Error retrieving trip waypoints: {response.StatusCode}");
                    return new List<TripWaypoint>();
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Exception in GetTripWaypointsAsync: {ex.Message}");
                return new List<TripWaypoint>();
            }
        }

        /// <summary>
        /// Exports vehicle history data as CSV
        /// </summary>
        public async Task<string> ExportVehicleHistoryAsync(int vehicleId, DateTime startDate, DateTime endDate)
        {
            var historyData = await GetVehicleHistoryDataAsync(vehicleId, startDate, endDate);
            
            if (historyData == null || !historyData.Any())
                return "No data to export";
                
            // Create CSV content
            var csvContent = "Timestamp,Latitude,Longitude,Speed,Heading,FuelLevel,EngineStatus,Location,Event,OdometerReading\n";
            
            foreach (var point in historyData)
            {
                csvContent += $"{point.Timestamp:yyyy-MM-dd HH:mm:ss},{point.Latitude},{point.Longitude},{point.Speed},{point.Heading},{point.FuelLevel}," +
                              $"{point.EngineStatus},{(string.IsNullOrEmpty(point.Location) ? "Unknown" : point.Location)},{point.Event},{point.OdometerReading}\n";
            }
            
            return csvContent;
        }

        /// <summary>
        /// Processes GPS data to identify trips
        /// </summary>
        public List<Trip> ProcessTripsFromGpsData(List<TripWaypoint> gpsData, int vehicleId)
        {
            if (gpsData == null || !gpsData.Any())
                return new List<Trip>();
                
            var trips = new List<Trip>();
            var currentTrip = new Trip { VehicleId = vehicleId };
            bool inTrip = false;
            
            // Sort by timestamp
            var sortedData = gpsData.OrderBy(p => p.Timestamp).ToList();
            
            foreach (var point in sortedData)
            {
                // Trip start detection - engine starts after being off
                if (!inTrip && point.EngineStatus == "running" && point.Speed > 0)
                {
                    inTrip = true;
                    currentTrip = new Trip
                    {
                        VehicleId = vehicleId,
                        StartDate = point.Timestamp,
                        StartLocation = point.Location ?? "Unknown",
                        Waypoints = new List<TripWaypoint> { point },
                        Status = "in_progress"
                    };
                }
                // During a trip
                else if (inTrip)
                {
                    // Add waypoint to current trip
                    currentTrip.Waypoints.Add(point);
                    
                    // Trip end detection - engine stops or extended idle time
                    if (point.EngineStatus != "running" || 
                        (point.Speed < 1 && currentTrip.Waypoints.Count > 1 && 
                         (point.Timestamp - currentTrip.Waypoints[currentTrip.Waypoints.Count - 2].Timestamp).TotalMinutes > 15))
                    {
                        inTrip = false;
                        currentTrip.EndDate = point.Timestamp;
                        currentTrip.EndLocation = point.Location ?? "Unknown";
                        currentTrip.Status = "completed";
                        
                        // Calculate trip metrics
                        CalculateTripMetrics(currentTrip);
                        
                        // Add to trips list
                        trips.Add(currentTrip);
                    }
                }
            }
            
            // Handle an ongoing trip
            if (inTrip)
            {
                var lastPoint = sortedData.Last();
                currentTrip.EndDate = lastPoint.Timestamp;
                currentTrip.EndLocation = lastPoint.Location ?? "Unknown";
                currentTrip.Status = "in_progress";
                
                // Calculate trip metrics
                CalculateTripMetrics(currentTrip);
                
                // Add to trips list
                trips.Add(currentTrip);
            }
            
            return trips;
        }

        /// <summary>
        /// Calculates various metrics for a trip
        /// </summary>
        private void CalculateTripMetrics(Trip trip)
        {
            if (trip.Waypoints.Count < 2)
                return;
                
            // Calculate distance
            trip.Distance = CalculateTripDistance(trip.Waypoints);
            
            // Calculate speeds
            trip.MaxSpeed = trip.Waypoints.Max(w => w.Speed);
            trip.AverageSpeed = trip.Waypoints.Average(w => w.Speed);
            
            // Calculate fuel consumed
            CalculateFuelConsumption(trip);
            
            // Calculate stop count
            trip.StopCount = CountStops(trip.Waypoints);
            
            // Calculate idle time
            trip.IdleTime = CalculateIdleTime(trip.Waypoints);
            
            // Count geofence events
            trip.GeofenceEvents = trip.Waypoints.Count(w => w.Event != null && w.Event.Contains("geofence", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Calculates the total distance of a trip using the Haversine formula
        /// </summary>
        private double CalculateTripDistance(List<TripWaypoint> waypoints)
        {
            double totalDistance = 0;
            
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                var p1 = waypoints[i];
                var p2 = waypoints[i + 1];
                
                // Haversine formula
                var R = 6371; // Earth radius in km
                var dLat = ToRadians(p2.Latitude - p1.Latitude);
                var dLon = ToRadians(p2.Longitude - p1.Longitude);
                
                var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                        Math.Cos(ToRadians(p1.Latitude)) * Math.Cos(ToRadians(p2.Latitude)) *
                        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                        
                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                var distance = R * c;
                
                totalDistance += distance;
            }
            
            return totalDistance;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        /// <summary>
        /// Estimates fuel consumption for a trip
        /// </summary>
        private void CalculateFuelConsumption(Trip trip)
        {
            if (trip.Waypoints.Count < 2)
                return;
                
            // Basic estimation based on first and last fuel level readings
            var startFuel = trip.Waypoints.First().FuelLevel;
            var endFuel = trip.Waypoints.Last().FuelLevel;
            
            // If fuel level decreased, calculate consumption
            if (endFuel < startFuel)
            {
                // Assuming fuel tank capacity (example: 60 liters)
                double fuelTankCapacity = 60; 
                trip.FuelConsumed = (startFuel - endFuel) * fuelTankCapacity / 100;
            }
            // If fuel level increased (refueling occurred), use a basic estimate based on distance
            else
            {
                // Assume average consumption of 10 km/L
                trip.FuelConsumed = trip.Distance / 10;
            }
        }

        /// <summary>
        /// Counts the number of stops in a trip
        /// </summary>
        private int CountStops(List<TripWaypoint> waypoints)
        {
            int stopCount = 0;
            bool inStop = false;
            
            foreach (var point in waypoints)
            {
                if (!inStop && point.Speed < 2)
                {
                    inStop = true;
                    stopCount++;
                }
                else if (inStop && point.Speed >= 2)
                {
                    inStop = false;
                }
            }
            
            return stopCount;
        }

        /// <summary>
        /// Calculates the total idle time during a trip
        /// </summary>
        private TimeSpan CalculateIdleTime(List<TripWaypoint> waypoints)
        {
            TimeSpan totalIdleTime = TimeSpan.Zero;
            DateTime? idleStartTime = null;
            
            foreach (var point in waypoints)
            {
                // Start idle period
                if (idleStartTime == null && point.Speed < 2 && point.EngineStatus == "running")
                {
                    idleStartTime = point.Timestamp;
                }
                // End idle period
                else if (idleStartTime != null && (point.Speed >= 2 || point.EngineStatus != "running"))
                {
                    totalIdleTime += point.Timestamp - idleStartTime.Value;
                    idleStartTime = null;
                }
            }
            
            // If still in an idle period at the end of the trip
            if (idleStartTime != null)
            {
                totalIdleTime += waypoints.Last().Timestamp - idleStartTime.Value;
            }
            
            return totalIdleTime;
        }

        /// <summary>
        /// Generate sample vehicle history data for testing
        /// </summary>
        public List<TripWaypoint> GenerateSampleHistoryData(int vehicleId, DateTime startDate, DateTime endDate)
        {
            var random = new Random();
            var data = new List<TripWaypoint>();
            
            // Create sample data points every 5 minutes
            for (var time = startDate; time <= endDate; time = time.AddMinutes(5))
            {
                // Base coordinates (Los Angeles)
                double baseLat = 34.0522;
                double baseLng = -118.2437;
                
                // Add some randomness to coordinates
                double lat = baseLat + (random.NextDouble() * 0.1 - 0.05);
                double lng = baseLng + (random.NextDouble() * 0.1 - 0.05);
                
                // Generate speed (0-120 km/h)
                double speed = random.Next(0, 120);
                
                // Engine status
                string engineStatus = speed > 0 ? "running" : (random.Next(0, 10) > 7 ? "off" : "idle");
                
                // Fuel level (50-100%)
                double fuelLevel = 50 + random.Next(0, 50);
                
                // Odometer reading (starting at 10000 km)
                double odometer = 10000 + (time - startDate).TotalHours * 50 * random.NextDouble();
                
                // Random events
                string event_ = null;
                if (random.Next(0, 20) == 0) // 5% chance of event
                {
                    var events = new[] { "Stop", "Speeding", "Geofence entry", "Geofence exit", "Idle", "Harsh braking" };
                    event_ = events[random.Next(0, events.Length)];
                }
                
                // Create waypoint
                var waypoint = new TripWaypoint
                {
                    TripId = 1, // Placeholder trip ID
                    Timestamp = time,
                    Latitude = lat,
                    Longitude = lng,
                    Speed = speed,
                    Heading = random.Next(0, 360),
                    FuelLevel = fuelLevel,
                    EngineStatus = engineStatus,
                    Location = GetSampleLocation(lat, lng),
                    Event = event_,
                    OdometerReading = odometer,
                    EngineTemperature = 80 + random.Next(-10, 20),
                    EngineRpm = engineStatus == "running" ? 800 + random.Next(0, 2000) : 0
                };
                
                data.Add(waypoint);
            }
            
            return data;
        }

        /// <summary>
        /// Generate a sample location name for demo purposes
        /// </summary>
        private string GetSampleLocation(double lat, double lng)
        {
            // In a real application, this would use reverse geocoding
            // For demo, return a random location name
            var locations = new[]
            {
                "Downtown",
                "Financial District",
                "Hollywood",
                "Santa Monica",
                "Beverly Hills",
                "Warehouse District",
                "Industrial Zone",
                "Shopping Center",
                "Office Park",
                "Distribution Center"
            };
            
            // Use coordinates to deterministically select a location
            int index = (int)((lat * 100 + lng * 100) % locations.Length);
            if (index < 0) index = -index;
            
            return locations[index];
        }
    }
} 