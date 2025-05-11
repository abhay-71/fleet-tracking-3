using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FleetTracking.Data;
using FleetTracking.Models.ViewModels;

namespace FleetTracking.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UserRolesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserRolesController> _logger;

        public UserRolesController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UserRolesController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET: UserRoles
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Retrieving all users with roles");
                
                var users = await _userManager.Users.ToListAsync();
                var userRolesViewModel = new List<UserRolesViewModel>();
                
                foreach (var user in users)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    var viewModel = new UserRolesViewModel
                    {
                        UserId = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FullName = user.FullName,
                        Roles = userRoles.ToList()
                    };
                    
                    userRolesViewModel.Add(viewModel);
                }
                
                _logger.LogInformation("Successfully retrieved {Count} users with their roles", userRolesViewModel.Count);
                return View(userRolesViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving users with roles");
                TempData["ErrorMessage"] = "An error occurred while retrieving users and their roles. Please try again.";
                return View(new List<UserRolesViewModel>());
            }
        }

        // GET: UserRoles/Manage/userId
        public async Task<IActionResult> Manage(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UserRoles Manage accessed with null or empty userId");
                return NotFound();
            }

            try
            {
                _logger.LogInformation("Managing roles for user with ID: {UserId}", userId);
                
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID: {UserId} not found", userId);
                    return NotFound();
                }

                var model = new ManageUserRolesViewModel
                {
                    UserId = userId,
                    UserName = user.UserName,
                    FullName = user.FullName
                };

                // Get all available roles and check if user is in them
                var roles = await _roleManager.Roles.ToListAsync();
                var userRoles = await _userManager.GetRolesAsync(user);
                
                var roleItems = new List<RoleViewModel>();
                foreach (var role in roles)
                {
                    var roleItem = new RoleViewModel
                    {
                        RoleId = role.Id,
                        RoleName = role.Name,
                        IsSelected = userRoles.Contains(role.Name)
                    };
                    roleItems.Add(roleItem);
                }
                
                model.Roles = roleItems;
                
                _logger.LogInformation("Successfully retrieved roles for user with ID: {UserId}", userId);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while managing roles for user with ID: {UserId}", userId);
                TempData["ErrorMessage"] = "An error occurred while loading the role management page. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: UserRoles/Manage/userId
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(ManageUserRolesViewModel model)
        {
            try
            {
                _logger.LogInformation("Updating roles for user with ID: {UserId}", model.UserId);
                
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID: {UserId} not found", model.UserId);
                    return NotFound();
                }

                // Get current roles the user is in
                var userRoles = await _userManager.GetRolesAsync(user);

                // Selected role names from the form
                var selectedRoles = model.Roles.Where(r => r.IsSelected).Select(r => r.RoleName).ToList();

                // Roles to add (selected but not in current roles)
                var rolesToAdd = selectedRoles.Except(userRoles).ToList();

                // Roles to remove (in current roles but not selected)
                var rolesToRemove = userRoles.Except(selectedRoles).ToList();

                // Update roles
                if (rolesToAdd.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                    if (!addResult.Succeeded)
                    {
                        _logger.LogError("Failed to add roles for user with ID: {UserId}. Errors: {Errors}", 
                            model.UserId, string.Join(", ", addResult.Errors.Select(e => e.Description)));
                        
                        // Adding to ModelState manually since we're not using a traditional model binding
                        foreach (var error in addResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(model);
                    }
                }

                if (rolesToRemove.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    if (!removeResult.Succeeded)
                    {
                        _logger.LogError("Failed to remove roles for user with ID: {UserId}. Errors: {Errors}", 
                            model.UserId, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                        
                        foreach (var error in removeResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(model);
                    }
                }

                _logger.LogInformation("Successfully updated roles for user with ID: {UserId}", model.UserId);
                TempData["SuccessMessage"] = "User roles have been updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating roles for user with ID: {UserId}", model.UserId);
                TempData["ErrorMessage"] = "An error occurred while updating user roles. Please try again.";
                return RedirectToAction(nameof(Manage), new { userId = model.UserId });
            }
        }
    }
} 