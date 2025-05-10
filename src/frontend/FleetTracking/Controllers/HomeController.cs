using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FleetTracking.Models;

namespace FleetTracking.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // If the user is authenticated, redirect to dashboard
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Dashboard");
        }

        return View();
    }

    [Authorize]
    public IActionResult Dashboard()
    {
        // Dashboard will be the main entry point for logged-in users
        // In a real implementation, we would load summary data for the dashboard widgets
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
} 