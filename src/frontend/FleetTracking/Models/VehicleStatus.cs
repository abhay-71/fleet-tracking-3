using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FleetTracking.Models
{
    public class VehicleStatus
    {
        // Basic identification
        public int VehicleId { get; set; }
        public string VehicleRegistration { get; set; }
        public string VehicleName { get; set; }

        // Driver information (if assigned)
        public int? DriverId { get; set; }
        public string DriverName { get; set; }

        // Location information
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double Heading { get; set; }   // Direction in degrees (0-360)
        public double Speed { get; set; }     // Speed in km/h
        
        // Status information
        public string Status { get; set; }    // "active", "idle", "stopped", "offline", "maintenance"
        public int IdleTimeSeconds { get; set; }  // How long vehicle has been idle
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public bool IsOnline { get; set; }
        
        // Vehicle condition
        public decimal FuelLevel { get; set; }
        public decimal FuelLevelPercentage { get; set; }
        public decimal OdometerReading { get; set; }
        public decimal EngineTemperature { get; set; }
        public bool EngineOn { get; set; }
        public bool DoorsLocked { get; set; }
        
        // Trip information (if on a trip)
        public int? CurrentTripId { get; set; }
        public string CurrentLocation { get; set; }
        public string NextWaypoint { get; set; }
        public DateTime? EstimatedArrival { get; set; }
        
        // Alerts and warnings
        public List<string> ActiveAlerts { get; set; } = new List<string>();
        public bool HasCriticalAlert { get; set; }
        public bool MaintenanceDue { get; set; }
        
        // Geofencing
        public List<int> CurrentGeofenceIds { get; set; } = new List<int>();
        public bool IsInAuthorizedZone { get; set; } = true;
        
        // Calculated and helper properties
        public int SecondsSinceLastUpdate => (int)(DateTime.UtcNow - LastUpdated).TotalSeconds;
        public string StatusDisplay 
        { 
            get
            {
                return Status switch
                {
                    "active" => "Active",
                    "idle" => "Idle",
                    "stopped" => "Stopped",
                    "offline" => "Offline",
                    "maintenance" => "Maintenance",
                    _ => Status
                };
            }
        }
        
        public string FormattedSpeed => $"{Math.Round(Speed, 1)} km/h";
        public string FormattedIdleTime
        {
            get
            {
                if (IdleTimeSeconds < 60) return $"{IdleTimeSeconds} sec";
                if (IdleTimeSeconds < 3600) return $"{Math.Floor(IdleTimeSeconds / 60.0)} min";
                return $"{Math.Floor(IdleTimeSeconds / 3600.0)}h {Math.Floor((IdleTimeSeconds % 3600) / 60.0)}m";
            }
        }
    }
} 