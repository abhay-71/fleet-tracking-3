using FleetTracking.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FleetTracking.Services
{
    public class GeofenceService
    {
        private readonly IApiService _apiService;
        private readonly IConfiguration _configuration;

        public GeofenceService(IApiService apiService, IConfiguration configuration)
        {
            _apiService = apiService;
            _configuration = configuration;
        }

        public async Task<List<Geofence>> GetAllGeofencesAsync()
        {
            try
            {
                return await _apiService.GetAsync<List<Geofence>>("/api/geofences");
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error getting geofences: {ex.Message}");
                return new List<Geofence>();
            }
        }

        public async Task<Geofence> GetGeofenceByIdAsync(int id)
        {
            try
            {
                return await _apiService.GetAsync<Geofence>($"/api/geofences/{id}");
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error getting geofence {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<Geofence> CreateGeofenceAsync(Geofence geofence)
        {
            try
            {
                return await _apiService.PostAsync<Geofence>("/api/geofences", geofence);
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error creating geofence: {ex.Message}");
                return null;
            }
        }

        public async Task<Geofence> UpdateGeofenceAsync(int id, Geofence geofence)
        {
            try
            {
                return await _apiService.PutAsync<Geofence>($"/api/geofences/{id}", geofence);
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error updating geofence {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteGeofenceAsync(int id)
        {
            try
            {
                return await _apiService.DeleteAsync($"/api/geofences/{id}");
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error deleting geofence {id}: {ex.Message}");
                return false;
            }
        }
    }
} 