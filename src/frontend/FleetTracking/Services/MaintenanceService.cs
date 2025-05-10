using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using FleetTracking.Models;

namespace FleetTracking.Services
{
    public class MaintenanceService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiUrl;

        public MaintenanceService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _apiUrl = _configuration["ApiEndpoints:Backend"] ?? "http://localhost:5001/api";
        }

        /// <summary>
        /// Gets all maintenance records
        /// </summary>
        public async Task<List<MaintenanceRecord>> GetMaintenanceRecordsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiUrl}/maintenance");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<List<MaintenanceRecord>>(content, options);
                }
                else
                {
                    // Log error
                    Console.WriteLine($"Error retrieving maintenance records: {response.StatusCode}");
                    return new List<MaintenanceRecord>();
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Exception in GetMaintenanceRecordsAsync: {ex.Message}");
                return GenerateSampleMaintenanceRecords();
            }
        }

        /// <summary>
        /// Gets maintenance records for a specific vehicle
        /// </summary>
        public async Task<List<MaintenanceRecord>> GetVehicleMaintenanceRecordsAsync(int vehicleId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiUrl}/maintenance/vehicle/{vehicleId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<List<MaintenanceRecord>>(content, options);
                }
                else
                {
                    // Log error
                    Console.WriteLine($"Error retrieving vehicle maintenance records: {response.StatusCode}");
                    return new List<MaintenanceRecord>();
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Exception in GetVehicleMaintenanceRecordsAsync: {ex.Message}");
                return GenerateSampleMaintenanceRecords(vehicleId);
            }
        }

        /// <summary>
        /// Gets a specific maintenance record by ID
        /// </summary>
        public async Task<MaintenanceRecord> GetMaintenanceRecordAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiUrl}/maintenance/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<MaintenanceRecord>(content, options);
                }
                else
                {
                    // Log error
                    Console.WriteLine($"Error retrieving maintenance record: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Exception in GetMaintenanceRecordAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets all maintenance types
        /// </summary>
        public async Task<List<MaintenanceType>> GetMaintenanceTypesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiUrl}/maintenance/types");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<List<MaintenanceType>>(content, options);
                }
                else
                {
                    // Log error
                    Console.WriteLine($"Error retrieving maintenance types: {response.StatusCode}");
                    return new List<MaintenanceType>();
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Exception in GetMaintenanceTypesAsync: {ex.Message}");
                return GenerateSampleMaintenanceTypes();
            }
        }

        /// <summary>
        /// Creates a new maintenance record
        /// </summary>
        public async Task<bool> CreateMaintenanceRecordAsync(MaintenanceRecord record)
        {
            try
            {
                var json = JsonSerializer.Serialize(record);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_apiUrl}/maintenance", content);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Exception in CreateMaintenanceRecordAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates an existing maintenance record
        /// </summary>
        public async Task<bool> UpdateMaintenanceRecordAsync(MaintenanceRecord record)
        {
            try
            {
                var json = JsonSerializer.Serialize(record);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"{_apiUrl}/maintenance/{record.Id}", content);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Exception in UpdateMaintenanceRecordAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets maintenance forecasts
        /// </summary>
        public async Task<List<MaintenanceForecast>> GetMaintenanceForecastsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiUrl}/maintenance/forecasts");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<List<MaintenanceForecast>>(content, options);
                }
                else
                {
                    // Log error
                    Console.WriteLine($"Error retrieving maintenance forecasts: {response.StatusCode}");
                    return new List<MaintenanceForecast>();
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Exception in GetMaintenanceForecastsAsync: {ex.Message}");
                return GenerateSampleMaintenanceForecasts();
            }
        }

        /// <summary>
        /// Gets upcoming maintenance that is due soon
        /// </summary>
        public async Task<List<MaintenanceRecord>> GetUpcomingMaintenanceAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiUrl}/maintenance/upcoming");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<List<MaintenanceRecord>>(content, options);
                }
                else
                {
                    // Log error
                    Console.WriteLine($"Error retrieving upcoming maintenance: {response.StatusCode}");
                    return new List<MaintenanceRecord>();
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Exception in GetUpcomingMaintenanceAsync: {ex.Message}");
                return GenerateSampleMaintenanceRecords().Where(r => r.Status == "scheduled" && r.ScheduledDate >= DateTime.Now && r.ScheduledDate <= DateTime.Now.AddDays(30)).ToList();
            }
        }

        /// <summary>
        /// Gets maintenance statistics for a vehicle
        /// </summary>
        public async Task<dynamic> GetMaintenanceStatsAsync(int vehicleId)
        {
            try
            {
                var records = await GetVehicleMaintenanceRecordsAsync(vehicleId);
                
                if (records == null || !records.Any())
                {
                    return new 
                    {
                        TotalMaintenanceCount = 0,
                        CompletedMaintenanceCount = 0,
                        TotalMaintenanceCost = 0.0m,
                        UpcomingMaintenanceCount = 0,
                        AverageMaintenanceCost = 0.0m
                    };
                }
                
                var totalCount = records.Count;
                var completedCount = records.Count(r => r.Status == "completed");
                var upcomingCount = records.Count(r => r.Status == "scheduled" && r.ScheduledDate > DateTime.Now);
                var totalCost = records.Where(r => r.ActualCost.HasValue).Sum(r => r.ActualCost.Value);
                var averageCost = completedCount > 0 ? totalCost / completedCount : 0;
                
                return new 
                {
                    TotalMaintenanceCount = totalCount,
                    CompletedMaintenanceCount = completedCount,
                    TotalMaintenanceCost = totalCost,
                    UpcomingMaintenanceCount = upcomingCount,
                    AverageMaintenanceCost = averageCost
                };
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Exception in GetMaintenanceStatsAsync: {ex.Message}");
                return new 
                {
                    TotalMaintenanceCount = 0,
                    CompletedMaintenanceCount = 0,
                    TotalMaintenanceCost = 0.0m,
                    UpcomingMaintenanceCount = 0,
                    AverageMaintenanceCost = 0.0m,
                    Error = ex.Message
                };
            }
        }

        #region Sample Data Generation for Development

        /// <summary>
        /// Generates sample maintenance records for testing
        /// </summary>
        private List<MaintenanceRecord> GenerateSampleMaintenanceRecords(int? vehicleId = null)
        {
            var maintenanceTypes = GenerateSampleMaintenanceTypes();
            var random = new Random();
            var records = new List<MaintenanceRecord>();

            for (int i = 1; i <= 10; i++)
            {
                var vid = vehicleId ?? random.Next(1, 10);
                var typeId = random.Next(0, maintenanceTypes.Count);
                var type = maintenanceTypes[typeId];
                var now = DateTime.Now;
                var status = GetRandomStatus(random);
                var scheduledDate = now.AddDays(random.Next(-30, 30));
                var completedDate = status == "completed" ? scheduledDate.AddDays(random.Next(1, 5)) : (DateTime?)null;
                var estimatedCost = type.EstimatedCost ?? random.Next(50, 500);
                var actualCost = completedDate.HasValue ? estimatedCost * (decimal)(0.8 + (random.NextDouble() * 0.4)) : (decimal?)null;

                var record = new MaintenanceRecord
                {
                    Id = i,
                    VehicleId = vid,
                    MaintenanceTypeId = type.Id,
                    MaintenanceType = type,
                    Description = $"{type.Name} for Vehicle #{vid}",
                    ScheduledDate = scheduledDate,
                    CompletedDate = completedDate,
                    Status = status,
                    OdometerReading = random.Next(10000, 100000),
                    EstimatedCost = estimatedCost,
                    ActualCost = actualCost,
                    PerformedBy = status == "completed" ? GetRandomMechanic(random) : null,
                    Notes = status == "completed" ? "Maintenance completed successfully" : null,
                    CreatedAt = now.AddDays(-random.Next(5, 60)),
                    UpdatedAt = now.AddDays(-random.Next(0, 5))
                };

                records.Add(record);
            }

            return vehicleId.HasValue 
                ? records.Where(r => r.VehicleId == vehicleId.Value).ToList() 
                : records;
        }

        /// <summary>
        /// Generates sample maintenance types for testing
        /// </summary>
        private List<MaintenanceType> GenerateSampleMaintenanceTypes()
        {
            return new List<MaintenanceType>
            {
                new MaintenanceType
                {
                    Id = 1,
                    Name = "Oil Change",
                    Description = "Changing the engine oil and filter",
                    DefaultIntervalDays = 90,
                    DefaultIntervalDistance = 5000,
                    IsRequired = true,
                    EstimatedDurationHours = 1.0m,
                    EstimatedCost = 75.0m,
                    Category = "routine",
                    CreatedAt = DateTime.Now.AddDays(-365)
                },
                new MaintenanceType
                {
                    Id = 2,
                    Name = "Tire Rotation",
                    Description = "Rotating the tires for even wear",
                    DefaultIntervalDays = 180,
                    DefaultIntervalDistance = 10000,
                    IsRequired = true,
                    EstimatedDurationHours = 0.5m,
                    EstimatedCost = 50.0m,
                    Category = "routine",
                    CreatedAt = DateTime.Now.AddDays(-365)
                },
                new MaintenanceType
                {
                    Id = 3,
                    Name = "Brake Inspection",
                    Description = "Inspecting brake pads, rotors, and fluid",
                    DefaultIntervalDays = 180,
                    DefaultIntervalDistance = 20000,
                    IsRequired = true,
                    EstimatedDurationHours = 1.0m,
                    EstimatedCost = 100.0m,
                    Category = "inspection",
                    CreatedAt = DateTime.Now.AddDays(-365)
                },
                new MaintenanceType
                {
                    Id = 4,
                    Name = "Engine Tune-up",
                    Description = "Comprehensive engine tuning and maintenance",
                    DefaultIntervalDays = 365,
                    DefaultIntervalDistance = 30000,
                    IsRequired = false,
                    EstimatedDurationHours = 3.0m,
                    EstimatedCost = 300.0m,
                    Category = "routine",
                    CreatedAt = DateTime.Now.AddDays(-365)
                },
                new MaintenanceType
                {
                    Id = 5,
                    Name = "Air Filter Replacement",
                    Description = "Replacing the engine air filter",
                    DefaultIntervalDays = 365,
                    DefaultIntervalDistance = 15000,
                    IsRequired = true,
                    EstimatedDurationHours = 0.5m,
                    EstimatedCost = 30.0m,
                    Category = "routine",
                    CreatedAt = DateTime.Now.AddDays(-365)
                }
            };
        }

        /// <summary>
        /// Generates sample maintenance forecasts for testing
        /// </summary>
        private List<MaintenanceForecast> GenerateSampleMaintenanceForecasts()
        {
            var maintenanceTypes = GenerateSampleMaintenanceTypes();
            var random = new Random();
            var forecasts = new List<MaintenanceForecast>();

            for (int i = 1; i <= 10; i++)
            {
                var vehicleId = random.Next(1, 10);
                var typeIndex = random.Next(0, maintenanceTypes.Count);
                var type = maintenanceTypes[typeIndex];
                var now = DateTime.Now;
                var daysUntilDue = random.Next(-10, 120); // Some overdue, some upcoming
                var predictedDate = now.AddDays(daysUntilDue);
                var confidence = random.Next(50, 100);
                var basedOn = random.NextDouble() > 0.5 ? "mileage" : "time";
                var lastMaintenanceDate = now.AddDays(-random.Next(30, 365));

                var forecast = new MaintenanceForecast
                {
                    Id = i,
                    VehicleId = vehicleId,
                    MaintenanceTypeId = type.Id,
                    MaintenanceType = type,
                    PredictedDueDate = predictedDate,
                    ConfidenceLevel = confidence,
                    EstimatedCost = type.EstimatedCost ?? random.Next(50, 500),
                    BasedOn = basedOn,
                    LastMaintenanceDate = lastMaintenanceDate,
                    Priority = DeterminePriority(daysUntilDue),
                    Notes = $"Forecast based on {basedOn} patterns",
                    CreatedAt = now.AddDays(-random.Next(1, 30)),
                    UpdatedAt = now
                };

                forecasts.Add(forecast);
            }

            return forecasts;
        }

        private string GetRandomStatus(Random random)
        {
            var statuses = new[] { "scheduled", "in_progress", "completed", "cancelled" };
            var weights = new[] { 0.5, 0.1, 0.3, 0.1 };
            
            var value = random.NextDouble();
            var cumulativeWeight = 0.0;
            
            for (int i = 0; i < statuses.Length; i++)
            {
                cumulativeWeight += weights[i];
                if (value < cumulativeWeight)
                    return statuses[i];
            }
            
            return statuses[0];
        }

        private string GetRandomMechanic(Random random)
        {
            var mechanics = new[] 
            { 
                "John Smith", 
                "Maria Rodriguez", 
                "Akira Tanaka", 
                "Ahmed Hassan", 
                "Pavel Ivanov", 
                "Sarah Johnson"
            };
            
            return mechanics[random.Next(0, mechanics.Length)];
        }

        private string DeterminePriority(int daysUntilDue)
        {
            if (daysUntilDue < 0)
                return "high";
            else if (daysUntilDue < 14)
                return "high";
            else if (daysUntilDue < 30)
                return "medium";
            else
                return "low";
        }

        #endregion
    }
} 