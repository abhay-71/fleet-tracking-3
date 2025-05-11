using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FleetTracking.Data
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            string[] roleNames = { "Administrator", "Manager", "Dispatcher", "Driver", "Viewer" };

            foreach (var roleName in roleNames)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    try
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                        logger.LogInformation("Created role {RoleName}", roleName);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error creating role {RoleName}", roleName);
                    }
                }
            }
        }

        public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            // Check if admin user exists
            const string adminEmail = "admin@fleettracking.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Administrator",
                    JobTitle = "Administrator",
                    Department = "Administration",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                try
                {
                    var result = await userManager.CreateAsync(adminUser, "Admin123!");
                    
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Created admin user {Email}", adminEmail);

                        // Add admin to the Administrator role
                        await userManager.AddToRoleAsync(adminUser, "Administrator");
                        logger.LogInformation("Added admin user to Administrator role");
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors);
                        logger.LogError("Failed to create admin user: {Errors}", errors);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error creating admin user");
                }
            }
        }
    }
} 