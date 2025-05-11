using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FleetTracking.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        public IActionResult AccessDenied()
        {
            _logger.LogWarning("Access denied page accessed");
            return View();
        }
    }
} 