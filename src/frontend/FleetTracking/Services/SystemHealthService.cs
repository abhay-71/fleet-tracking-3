using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FleetTracking.Models;
using Microsoft.Extensions.Logging;

namespace FleetTracking.Services
{
    /// <summary>
    /// Interface for system health service
    /// </summary>
    public interface ISystemHealthService
    {
        /// <summary>
        /// Gets the current system health
        /// </summary>
        Task<SystemHealth> GetSystemHealthAsync();
        
        /// <summary>
        /// Gets the health of a specific component
        /// </summary>
        Task<ComponentHealth> GetComponentHealthAsync(string componentName);
        
        /// <summary>
        /// Gets recent system errors
        /// </summary>
        Task<List<SystemError>> GetRecentErrorsAsync(int count = 20);
        
        /// <summary>
        /// Gets system performance metrics
        /// </summary>
        Task<PerformanceMetrics> GetPerformanceMetricsAsync();
        
        /// <summary>
        /// Gets system resource usage
        /// </summary>
        Task<SystemResources> GetSystemResourcesAsync();
        
        /// <summary>
        /// Checks if a specific endpoint is healthy
        /// </summary>
        Task<bool> CheckEndpointHealthAsync(string endpoint);
    }
    
    /// <summary>
    /// Service for monitoring system health
    /// </summary>
    public class SystemHealthService : ISystemHealthService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<SystemHealthService> _logger;
        
        /// <summary>
        /// Initializes a new instance of the SystemHealthService class
        /// </summary>
        public SystemHealthService(IApiService apiService, ILogger<SystemHealthService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }
        
        /// <inheritdoc/>
        public async Task<SystemHealth> GetSystemHealthAsync()
        {
            try
            {
                var systemHealth = await _apiService.GetAsync<SystemHealth>("health/system");
                return systemHealth ?? CreateDefaultSystemHealth();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get system health");
                return CreateDefaultSystemHealth(SystemStatus.Unhealthy, "Failed to retrieve system health information");
            }
        }
        
        /// <inheritdoc/>
        public async Task<ComponentHealth> GetComponentHealthAsync(string componentName)
        {
            try
            {
                var componentHealth = await _apiService.GetAsync<ComponentHealth>($"health/component/{Uri.EscapeDataString(componentName)}");
                return componentHealth ?? CreateDefaultComponentHealth(componentName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get component health for {Component}", componentName);
                return CreateDefaultComponentHealth(componentName, SystemStatus.Unknown, "Failed to retrieve component health information");
            }
        }
        
        /// <inheritdoc/>
        public async Task<List<SystemError>> GetRecentErrorsAsync(int count = 20)
        {
            try
            {
                var errors = await _apiService.GetAsync<List<SystemError>>($"health/errors?count={count}");
                return errors ?? new List<SystemError>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get recent system errors");
                return new List<SystemError>
                {
                    new SystemError
                    {
                        Timestamp = DateTime.UtcNow,
                        Source = "SystemHealthService",
                        Message = "Failed to retrieve system errors",
                        ExceptionType = ex.GetType().Name,
                        StackTrace = ex.StackTrace
                    }
                };
            }
        }
        
        /// <inheritdoc/>
        public async Task<PerformanceMetrics> GetPerformanceMetricsAsync()
        {
            try
            {
                var metrics = await _apiService.GetAsync<PerformanceMetrics>("health/performance");
                return metrics ?? new PerformanceMetrics();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get performance metrics");
                return new PerformanceMetrics();
            }
        }
        
        /// <inheritdoc/>
        public async Task<SystemResources> GetSystemResourcesAsync()
        {
            try
            {
                var resources = await _apiService.GetAsync<SystemResources>("health/resources");
                return resources ?? new SystemResources();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get system resources");
                return new SystemResources();
            }
        }
        
        /// <inheritdoc/>
        public async Task<bool> CheckEndpointHealthAsync(string endpoint)
        {
            try
            {
                var result = await _apiService.GetAsync<dynamic>($"health/endpoint?url={Uri.EscapeDataString(endpoint)}");
                return result != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check endpoint health for {Endpoint}", endpoint);
                return false;
            }
        }
        
        private SystemHealth CreateDefaultSystemHealth(SystemStatus status = SystemStatus.Unknown, string message = null)
        {
            return new SystemHealth
            {
                Status = status,
                Timestamp = DateTime.UtcNow,
                Components = new List<ComponentHealth>
                {
                    CreateDefaultComponentHealth("API", status, message),
                    CreateDefaultComponentHealth("Database", status, message),
                    CreateDefaultComponentHealth("Cache", status, message),
                    CreateDefaultComponentHealth("File Storage", status, message)
                },
                Resources = new SystemResources(),
                Performance = new PerformanceMetrics(),
                RecentErrors = message != null 
                    ? new List<SystemError> { new SystemError { Message = message, Timestamp = DateTime.UtcNow, Source = "SystemHealthService" } }
                    : new List<SystemError>()
            };
        }
        
        private ComponentHealth CreateDefaultComponentHealth(string name, SystemStatus status = SystemStatus.Unknown, string description = null)
        {
            return new ComponentHealth
            {
                Name = name,
                Status = status,
                Description = description ?? "No health information available",
                LastChecked = DateTime.UtcNow
            };
        }
    }
} 