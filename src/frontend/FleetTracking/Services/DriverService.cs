using FleetTracking.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FleetTracking.Services
{
    public class DriverService
    {
        private readonly IApiService _apiService;
        private readonly IConfiguration _configuration;

        public DriverService(IApiService apiService, IConfiguration configuration)
        {
            _apiService = apiService;
            _configuration = configuration;
        }

        public async Task<List<Driver>> GetAllDriversAsync()
        {
            try
            {
                return await _apiService.GetAsync<List<Driver>>("/api/drivers");
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error getting drivers: {ex.Message}");
                return new List<Driver>();
            }
        }

        public async Task<Driver> GetDriverByIdAsync(int id)
        {
            try
            {
                return await _apiService.GetAsync<Driver>($"/api/drivers/{id}");
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error getting driver {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<Driver> CreateDriverAsync(Driver driver)
        {
            try
            {
                return await _apiService.PostAsync<Driver>("/api/drivers", driver);
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error creating driver: {ex.Message}");
                return null;
            }
        }

        public async Task<Driver> UpdateDriverAsync(int id, Driver driver)
        {
            try
            {
                return await _apiService.PutAsync<Driver>($"/api/drivers/{id}", driver);
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error updating driver {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteDriverAsync(int id)
        {
            try
            {
                return await _apiService.DeleteAsync($"/api/drivers/{id}");
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error deleting driver {id}: {ex.Message}");
                return false;
            }
        }
    }
} 