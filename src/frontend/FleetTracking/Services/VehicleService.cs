using FleetTracking.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FleetTracking.Services
{
    public class VehicleService
    {
        private readonly IApiService _apiService;
        private readonly IConfiguration _configuration;

        public VehicleService(IApiService apiService, IConfiguration configuration)
        {
            _apiService = apiService;
            _configuration = configuration;
        }

        public async Task<List<Vehicle>> GetAllVehiclesAsync()
        {
            try
            {
                return await _apiService.GetAsync<List<Vehicle>>("/api/vehicles");
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error getting vehicles: {ex.Message}");
                return new List<Vehicle>();
            }
        }

        public async Task<Vehicle> GetVehicleByIdAsync(int id)
        {
            try
            {
                return await _apiService.GetAsync<Vehicle>($"/api/vehicles/{id}");
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error getting vehicle {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<Vehicle> CreateVehicleAsync(Vehicle vehicle)
        {
            try
            {
                return await _apiService.PostAsync<Vehicle>("/api/vehicles", vehicle);
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error creating vehicle: {ex.Message}");
                return null;
            }
        }

        public async Task<Vehicle> UpdateVehicleAsync(int id, Vehicle vehicle)
        {
            try
            {
                return await _apiService.PutAsync<Vehicle>($"/api/vehicles/{id}", vehicle);
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error updating vehicle {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteVehicleAsync(int id)
        {
            try
            {
                return await _apiService.DeleteAsync($"/api/vehicles/{id}");
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error deleting vehicle {id}: {ex.Message}");
                return false;
            }
        }
    }
} 