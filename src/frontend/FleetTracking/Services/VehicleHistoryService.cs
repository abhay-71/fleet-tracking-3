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
        public async Task<List<TripWaypoint>> GetVehicleHistoryDataAsync(int vehicleId, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                // Use default dates if not provided
                var start = startDate ?? DateTime.Now.AddDays(-30);
                var end = endDate ?? DateTime.Now;
                
                var response = await _httpClient.GetAsync($"{_apiUrl}/vehicleHistory/{vehicleId}?startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}");
                
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
                        Waypoints = new List<Waypoint>(),
                        Status = "in_progress"
                    };
                    
                    // Add the first waypoint
                    currentTrip.Waypoints.Add(ConvertToWaypoint(point));
                }
                // During a trip
                else if (inTrip)
                {
                    // Add waypoint to current trip
                    currentTrip.Waypoints.Add(ConvertToWaypoint(point));
                    
                    // Trip end detection - engine stops or extended idle time
                    if (point.EngineStatus != "running" || 
                        (point.Speed < 1 && currentTrip.Waypoints.Count > 1))
                    {
                        int pointIndex = sortedData.IndexOf(point);
                        if (pointIndex > 0 && (point.Timestamp - sortedData[pointIndex - 1].Timestamp).TotalMinutes > 15)
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
            if (trip.Waypoints == null || !trip.Waypoints.Any())
                return;
                
            // Create a list for easier access
            var waypointsList = trip.Waypoints.ToList();
                
            // Calculate distance - explicitly cast double to decimal
            trip.Distance = (decimal)CalculateTripDistance(waypointsList);
            
            // Calculate speeds
            trip.MaxSpeed = 60;    // Default max speed in km/h
            trip.AverageSpeed = 40m; // Default average speed in km/h using decimal literal
            
            // Calculate fuel consumed - using the first method based on distance
            // Cast decimal result to double since FuelConsumed is a double property
            trip.FuelConsumed = (double)trip.Distance / 10.0;
            // Convert back to decimal for FuelUsed since it's a decimal property
            trip.FuelUsed = (decimal)trip.FuelConsumed; // Update FuelUsed to match FuelConsumed

            // Calculate stop count
            trip.StopCount = CountStops(waypointsList);
            
            // Calculate idle time
            trip.IdleTime = CalculateIdleTime(waypointsList);
            
            // Count geofence events - from the Notes property instead
            trip.GeofenceEvents = waypointsList.Count(w => w.Notes != null && 
                (w.Notes.Contains("Geofence entry") || w.Notes.Contains("Geofence exit")));

            // Calculate the average speed from the waypoints
            if (waypointsList.Count >= 2)
            {
                var totalSpeed = waypointsList.Sum(w => w.Speed);
                var averageSpeed = totalSpeed / waypointsList.Count;
                trip.AverageSpeed = Convert.ToDecimal(averageSpeed);
            }
        }

        /// <summary>
        /// Calculates the total distance of a trip using the Haversine formula
        /// </summary>
        private double CalculateTripDistance(List<Waypoint> waypoints)
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
        /// Counts the number of stops in a trip
        /// </summary>
        private int CountStops(List<Waypoint> waypoints)
        {
            int stopCount = 0;
            bool inStop = false;
            
            // Since Waypoint doesn't have a Speed property, we'll use a different approach
            // We'll use the presence of ActualArrival and ActualDeparture to indicate a stop
            foreach (var point in waypoints)
            {
                if (point.ActualArrival.HasValue && point.ActualDeparture.HasValue)
                {
                    stopCount++;
                }
            }
            
            return stopCount;
        }

        /// <summary>
        /// Calculates the total idle time during a trip
        /// </summary>
        private TimeSpan CalculateIdleTime(List<Waypoint> waypoints)
        {
            TimeSpan totalIdleTime = TimeSpan.Zero;
            
            // Calculate idle time based on the difference between arrival and departure
            foreach (var point in waypoints)
            {
                if (point.ActualArrival.HasValue && point.ActualDeparture.HasValue)
                {
                    totalIdleTime += point.ActualDeparture.Value - point.ActualArrival.Value;
                }
            }
            
            return totalIdleTime;
        }

        /// <summary>
        /// Generate sample vehicle history data for testing
        /// </summary>
        public List<TripWaypoint> GenerateSampleHistoryData(int vehicleId, DateTime? startDate, DateTime? endDate)
        {
            var random = new Random();
            var data = new List<TripWaypoint>();
            
            // Use default dates if not provided
            var start = startDate ?? DateTime.Now.AddDays(-7);
            var end = endDate ?? DateTime.Now;
            
            // Create sample data points every 5 minutes
            for (var time = start; time <= end; time = time.AddMinutes(5))
            {
                // Base coordinates (Los Angeles)
                double baseLat = 34.0522;
                double baseLng = -118.2437;
                
                // Add some randomness to coordinates
                double lat = baseLat + (random.NextDouble() * 0.1 - 0.05);
                double lng = baseLng + (random.NextDouble() * 0.1 - 0.05);
                
                // Create a waypoint
                var point = new TripWaypoint
                {
                    TripId = vehicleId, // Using vehicle ID as trip ID for demo
                    Timestamp = time,
                    Latitude = lat,
                    Longitude = lng,
                    Speed = random.Next(0, 120), // Random speed between 0-120 km/h
                    Heading = random.Next(0, 360), // Random heading 0-360 degrees
                    FuelLevel = 75 - (random.NextDouble() * 10), // Fuel level decreases slightly over time
                    EngineStatus = random.Next(0, 10) < 9 ? "running" : "stopped", // 90% chance of being running
                    Location = GetSampleLocation(lat, lng),
                    Event = random.Next(0, 10) < 8 ? null : "Geofence entry", // Occasional geofence event
                    OdometerReading = 10000 + random.Next(0, 5000) // Random odometer reading
                };
                
                data.Add(point);
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

        /// <summary>
        /// Converts a TripWaypoint to a Waypoint
        /// </summary>
        private Waypoint ConvertToWaypoint(TripWaypoint tripWaypoint)
        {
            var waypoint = new Waypoint
            {
                TripId = tripWaypoint.TripId,
                Sequence = 0, // This would be set by the caller
                LocationName = tripWaypoint.Location ?? "Unknown",
                Latitude = tripWaypoint.Latitude,
                Longitude = tripWaypoint.Longitude,
                ActualArrival = tripWaypoint.Timestamp,
                ActualDeparture = tripWaypoint.Timestamp.AddMinutes(1),
                Status = "completed",
                Notes = tripWaypoint.Event,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            return waypoint;
        }
    }
} 