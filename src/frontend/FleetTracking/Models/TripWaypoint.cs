using System;
using System.ComponentModel.DataAnnotations;

namespace FleetTracking.Models
{
    public class TripWaypoint
    {
        public int Id { get; set; }
        
        public int TripId { get; set; }
        
        [Display(Name = "Timestamp")]
        public DateTime Timestamp { get; set; }
        
        [Display(Name = "Latitude")]
        public double Latitude { get; set; }
        
        [Display(Name = "Longitude")]
        public double Longitude { get; set; }
        
        [Display(Name = "Speed (km/h)")]
        public double Speed { get; set; }
        
        [Display(Name = "Heading (degrees)")]
        public double Heading { get; set; }
        
        [Display(Name = "Fuel Level (%)")]
        public double FuelLevel { get; set; }
        
        [Display(Name = "Engine Status")]
        public string EngineStatus { get; set; }
        
        [Display(Name = "Location")]
        public string Location { get; set; }
        
        [Display(Name = "Event")]
        public string Event { get; set; }
        
        [Display(Name = "Odometer (km)")]
        public double OdometerReading { get; set; }
        
        [Display(Name = "Temperature (°C)")]
        public double? EngineTemperature { get; set; }
        
        [Display(Name = "RPM")]
        public int? EngineRpm { get; set; }
        
        // Navigation property
        public Trip Trip { get; set; }
        
        // Helper method to check if this is a stop point
        public bool IsStopPoint()
        {
            return Speed < 2;
        }
        
        // Helper method to check if this is a high-speed point
        public bool IsHighSpeedPoint(double threshold = 80)
        {
            return Speed > threshold;
        }
        
        // Helper method to get formatted coordinates
        public string GetFormattedCoordinates()
        {
            return $"{Latitude.ToString("F6")}, {Longitude.ToString("F6")}";
        }
    }
} 