using System;
using System.Collections.Generic;

namespace FleetTracking.Models
{
    /// <summary>
    /// Overall system health model
    /// </summary>
    public class SystemHealth
    {
        /// <summary>
        /// Gets or sets the status of the system
        /// </summary>
        public SystemStatus Status { get; set; }
        
        /// <summary>
        /// Gets or sets the timestamp of the health check
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Gets or sets individual component health statuses
        /// </summary>
        public List<ComponentHealth> Components { get; set; } = new List<ComponentHealth>();
        
        /// <summary>
        /// Gets or sets system resource metrics
        /// </summary>
        public SystemResources Resources { get; set; }
        
        /// <summary>
        /// Gets or sets recent errors
        /// </summary>
        public List<SystemError> RecentErrors { get; set; } = new List<SystemError>();
        
        /// <summary>
        /// Gets or sets performance metrics
        /// </summary>
        public PerformanceMetrics Performance { get; set; }
    }
    
    /// <summary>
    /// Health status of an individual system component
    /// </summary>
    public class ComponentHealth
    {
        /// <summary>
        /// Gets or sets the component name
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the component status
        /// </summary>
        public SystemStatus Status { get; set; }
        
        /// <summary>
        /// Gets or sets additional status description
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the last check time
        /// </summary>
        public DateTime LastChecked { get; set; }
        
        /// <summary>
        /// Gets or sets response time (ms) if applicable
        /// </summary>
        public double? ResponseTime { get; set; }
        
        /// <summary>
        /// Gets or sets uptime percentage if applicable
        /// </summary>
        public double? UptimePercentage { get; set; }
    }
    
    /// <summary>
    /// System resource metrics
    /// </summary>
    public class SystemResources
    {
        /// <summary>
        /// Gets or sets CPU usage percentage
        /// </summary>
        public double CpuUsage { get; set; }
        
        /// <summary>
        /// Gets or sets memory usage in MB
        /// </summary>
        public double MemoryUsage { get; set; }
        
        /// <summary>
        /// Gets or sets total memory in MB
        /// </summary>
        public double TotalMemory { get; set; }
        
        /// <summary>
        /// Gets or sets disk usage in GB
        /// </summary>
        public double DiskUsage { get; set; }
        
        /// <summary>
        /// Gets or sets total disk space in GB
        /// </summary>
        public double TotalDiskSpace { get; set; }
        
        /// <summary>
        /// Gets or sets database size in MB
        /// </summary>
        public double DatabaseSize { get; set; }
        
        /// <summary>
        /// Gets or sets active database connections
        /// </summary>
        public int ActiveDatabaseConnections { get; set; }
    }
    
    /// <summary>
    /// Performance metrics
    /// </summary>
    public class PerformanceMetrics
    {
        /// <summary>
        /// Gets or sets average API response time in ms
        /// </summary>
        public double AverageApiResponseTime { get; set; }
        
        /// <summary>
        /// Gets or sets requests per minute
        /// </summary>
        public double RequestsPerMinute { get; set; }
        
        /// <summary>
        /// Gets or sets database query average time in ms
        /// </summary>
        public double DatabaseQueryTime { get; set; }
        
        /// <summary>
        /// Gets or sets active user sessions
        /// </summary>
        public int ActiveSessions { get; set; }
        
        /// <summary>
        /// Gets or sets active vehicles being tracked
        /// </summary>
        public int ActiveVehicles { get; set; }
    }
    
    /// <summary>
    /// System error information
    /// </summary>
    public class SystemError
    {
        /// <summary>
        /// Gets or sets the error timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Gets or sets the error source
        /// </summary>
        public string Source { get; set; }
        
        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Gets or sets the exception type
        /// </summary>
        public string ExceptionType { get; set; }
        
        /// <summary>
        /// Gets or sets the stack trace
        /// </summary>
        public string StackTrace { get; set; }
    }
    
    /// <summary>
    /// System status enum
    /// </summary>
    public enum SystemStatus
    {
        /// <summary>
        /// Healthy status
        /// </summary>
        Healthy = 0,
        
        /// <summary>
        /// Degraded performance or partial functionality
        /// </summary>
        Degraded = 1,
        
        /// <summary>
        /// Unhealthy, system not functioning properly
        /// </summary>
        Unhealthy = 2,
        
        /// <summary>
        /// System offline
        /// </summary>
        Offline = 3,
        
        /// <summary>
        /// Status unknown or not checked
        /// </summary>
        Unknown = 4
    }
} 