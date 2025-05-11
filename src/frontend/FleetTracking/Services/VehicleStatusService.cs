using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FleetTracking.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FleetTracking.Hubs;

namespace FleetTracking.Services
{
    public class VehicleStatusService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VehicleStatusService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private Timer _timer;
        private Timer _idleTimer;
        private readonly Dictionary<int, VehicleStatus> _vehicleStatuses = new();
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        };

        public VehicleStatusService(
            IServiceProvider serviceProvider,
            ILogger<VehicleStatusService> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient("FleetTrackingApi");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Vehicle Status Service is starting.");

            // Temporarily commented out the timer initializations to prevent connection errors
            // _timer = new Timer(FetchVehicleStatusUpdates, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            // _idleTimer = new Timer(ProcessIdleVehicles, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));

            // Initialize with some mock data instead
            InitializeMockData();

            return Task.CompletedTask;
        }

        private void InitializeMockData()
        {
            // Add some mock vehicle statuses for demonstration
            var mockVehicles = new List<VehicleStatus>
            {
                new VehicleStatus 
                { 
                    VehicleId = 1, 
                    VehicleName = "Truck 101", 
                    VehicleRegistration = "ABC-1234",
                    Status = "active", 
                    Latitude = 34.0522, 
                    Longitude = -118.2437,
                    Speed = 65,
                    FuelLevel = 75,
                    EngineOn = true,
                    LastUpdated = DateTime.UtcNow,
                    IsOnline = true,
                    CurrentLocation = "Los Angeles, CA"
                },
                new VehicleStatus 
                { 
                    VehicleId = 2, 
                    VehicleName = "Van 202", 
                    VehicleRegistration = "XYZ-5678",
                    Status = "idle", 
                    Latitude = 37.7749, 
                    Longitude = -122.4194,
                    Speed = 0,
                    FuelLevel = 50,
                    EngineOn = true,
                    LastUpdated = DateTime.UtcNow,
                    IsOnline = true,
                    CurrentLocation = "San Francisco, CA"
                },
                new VehicleStatus 
                { 
                    VehicleId = 3, 
                    VehicleName = "Car 303", 
                    VehicleRegistration = "DEF-9012",
                    Status = "maintenance", 
                    Latitude = 32.7157, 
                    Longitude = -117.1611,
                    Speed = 0,
                    FuelLevel = 25,
                    EngineOn = false,
                    LastUpdated = DateTime.UtcNow,
                    IsOnline = false,
                    CurrentLocation = "San Diego, CA"
                }
            };

            foreach (var vehicle in mockVehicles)
            {
                _vehicleStatuses[vehicle.VehicleId] = vehicle;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Vehicle Status Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);
            _idleTimer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _idleTimer?.Dispose();
        }

        // Get the current vehicle statuses 
        public IEnumerable<VehicleStatus> GetAllVehicleStatuses()
        {
            return _vehicleStatuses.Values.ToList();
        }

        // Get a specific vehicle's status
        public VehicleStatus GetVehicleStatus(int vehicleId)
        {
            return _vehicleStatuses.TryGetValue(vehicleId, out var status) ? status : null;
        }

        // Update the status of a vehicle
        public async Task UpdateVehicleStatus(VehicleStatus status)
        {
            // Process status change
            if (_vehicleStatuses.TryGetValue(status.VehicleId, out var existingStatus))
            {
                string oldStatus = existingStatus.Status;
                if (oldStatus != status.Status)
                {
                    await NotifyStatusChange(status.VehicleId, status.VehicleName, oldStatus, status.Status);
                }
            }

            // Update or add the status
            _vehicleStatuses[status.VehicleId] = status;

            // Broadcast the status update
            await BroadcastStatusUpdate(status);
        }

        // Called by timer to fetch status updates from API
        private async void FetchVehicleStatusUpdates(object state)
        {
            try
            {
                _logger.LogInformation("Attempting to fetch vehicle status updates");
                
                // Commented out actual API call to prevent errors
                /*
                var response = await _httpClient.GetAsync("/api/vehicle/status");
                if (response.IsSuccessStatusCode)
                {
                    var statuses = await response.Content.ReadFromJsonAsync<List<VehicleStatus>>(_jsonOptions);
                    if (statuses != null)
                    {
                        foreach (var status in statuses)
                        {
                            await UpdateVehicleStatus(status);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to fetch vehicle status updates: {StatusCode}", response.StatusCode);
                }
                */
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vehicle status updates");
            }
        }

        // Process idle vehicles and update their idle time
        private async void ProcessIdleVehicles(object state)
        {
            try
            {
                var now = DateTime.UtcNow;
                var idleVehicles = _vehicleStatuses.Values
                    .Where(v => v.Status == "idle" || (v.Status == "active" && v.Speed < 2))
                    .ToList();

                foreach (var vehicle in idleVehicles)
                {
                    // If currently active but speed < 2, change to idle
                    if (vehicle.Status == "active" && vehicle.Speed < 2)
                    {
                        string oldStatus = vehicle.Status;
                        vehicle.Status = "idle";
                        vehicle.IdleTimeSeconds = 0;
                        
                        await NotifyStatusChange(vehicle.VehicleId, vehicle.VehicleName, oldStatus, "idle");
                    }
                    // If already idle, increment idle time
                    else if (vehicle.Status == "idle")
                    {
                        vehicle.IdleTimeSeconds += 30; // Add 30 seconds (our timer interval)
                        
                        // Check idle time thresholds for alerts
                        if (vehicle.IdleTimeSeconds == 300) // 5 minutes
                        {
                            await SendAlert("warning", $"Vehicle {vehicle.VehicleName} has been idle for 5 minutes", vehicle.VehicleId);
                        }
                        else if (vehicle.IdleTimeSeconds == 900) // 15 minutes
                        {
                            await SendAlert("warning", $"Vehicle {vehicle.VehicleName} has been idle for 15 minutes", vehicle.VehicleId);
                        }
                    }

                    // Broadcast the updated status
                    await BroadcastStatusUpdate(vehicle);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing idle vehicles");
            }
        }

        // Broadcast status updates through SignalR
        private async Task BroadcastStatusUpdate(VehicleStatus status)
        {
            using var scope = _serviceProvider.CreateScope();
            try
            {
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<VehicleStatusHub>>();
                await hubContext.Clients.All.SendAsync("ReceiveVehicleStatus", status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting vehicle status");
            }
        }

        // Notify status changes through SignalR
        private async Task NotifyStatusChange(int vehicleId, string vehicleName, string oldStatus, string newStatus)
        {
            using var scope = _serviceProvider.CreateScope();
            try
            {
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<VehicleStatusHub>>();
                await hubContext.Clients.All.SendAsync("VehicleStatusChange", vehicleId, vehicleName, oldStatus, newStatus, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending status change notification");
            }
        }

        // Send alerts through SignalR
        private async Task SendAlert(string alertType, string message, int? vehicleId = null, int? driverId = null)
        {
            using var scope = _serviceProvider.CreateScope();
            try
            {
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<VehicleStatusHub>>();
                await hubContext.Clients.All.SendAsync("ReceiveAlert", alertType, message, vehicleId, driverId, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending alert");
            }
        }
    }
} 