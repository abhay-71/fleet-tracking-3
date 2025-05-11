using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using FleetTracking.Models;
using FleetTracking.Services;
using Microsoft.EntityFrameworkCore;
using FleetTracking.Data;

namespace FleetTracking.Controllers
{
    public class VendorController : Controller
    {
        private readonly ILogger<VendorController> _logger;
        private readonly ApiService _apiService;
        private readonly ApplicationDbContext _context;

        public VendorController(
            ILogger<VendorController> logger,
            ApiService apiService,
            ApplicationDbContext context)
        {
            _logger = logger;
            _apiService = apiService;
            _context = context;
        }

        // GET: Vendor
        public async Task<IActionResult> Index()
        {
            try
            {
                // Try to get vendors from API first
                var vendors = await _apiService.GetAsync<List<Vendor>>("vendors");
                return View(vendors);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve vendors from API, falling back to database");
                
                // Fallback to database
                var vendors = await _context.Vendors.ToListAsync();
                return View(vendors);
            }
        }

        // GET: Vendor/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var vendor = await _apiService.GetAsync<Vendor>($"vendors/{id}");
                if (vendor == null)
                {
                    return NotFound();
                }
                
                // Get vendor transactions for this vendor
                var transactions = await _context.VendorTransactions
                    .Where(vt => vt.VendorId == id)
                    .OrderByDescending(vt => vt.TransactionDate)
                    .ToListAsync();
                
                ViewBag.Transactions = transactions;
                
                return View(vendor);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to retrieve vendor {id} from API, falling back to database");
                
                // Fallback to database
                var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == id);
                if (vendor == null)
                {
                    return NotFound();
                }
                
                // Get vendor transactions for this vendor
                var transactions = await _context.VendorTransactions
                    .Where(vt => vt.VendorId == id)
                    .OrderByDescending(vt => vt.TransactionDate)
                    .ToListAsync();
                
                ViewBag.Transactions = transactions;
                
                return View(vendor);
            }
        }

        // GET: Vendor/Create
        public IActionResult Create()
        {
            ViewBag.VendorTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "maintenance", Text = "Maintenance" },
                new SelectListItem { Value = "fuel", Text = "Fuel" },
                new SelectListItem { Value = "parts", Text = "Parts" },
                new SelectListItem { Value = "service", Text = "Service" },
                new SelectListItem { Value = "tires", Text = "Tires" },
                new SelectListItem { Value = "insurance", Text = "Insurance" },
                new SelectListItem { Value = "other", Text = "Other" }
            };
            
            return View(new Vendor { IsActive = true });
        }

        // POST: Vendor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vendor vendor)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    vendor.CreatedAt = DateTime.UtcNow;
                    vendor.UpdatedAt = DateTime.UtcNow;

                    var response = await _apiService.PostAsync<Vendor>("vendors", vendor);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating vendor");
                    ModelState.AddModelError("", "Failed to create vendor.");
                    
                    // Fallback to database
                    try
                    {
                        _context.Vendors.Add(vendor);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception dbEx)
                    {
                        _logger.LogError(dbEx, "Error creating vendor in database");
                        ModelState.AddModelError("", "Failed to create vendor in database.");
                    }
                }
            }

            ViewBag.VendorTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "maintenance", Text = "Maintenance" },
                new SelectListItem { Value = "fuel", Text = "Fuel" },
                new SelectListItem { Value = "parts", Text = "Parts" },
                new SelectListItem { Value = "service", Text = "Service" },
                new SelectListItem { Value = "tires", Text = "Tires" },
                new SelectListItem { Value = "insurance", Text = "Insurance" },
                new SelectListItem { Value = "other", Text = "Other" }
            };

            return View(vendor);
        }

        // GET: Vendor/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var vendor = await _apiService.GetAsync<Vendor>($"vendors/{id}");
                if (vendor == null)
                {
                    return NotFound();
                }

                ViewBag.VendorTypes = new List<SelectListItem>
                {
                    new SelectListItem { Value = "maintenance", Text = "Maintenance" },
                    new SelectListItem { Value = "fuel", Text = "Fuel" },
                    new SelectListItem { Value = "parts", Text = "Parts" },
                    new SelectListItem { Value = "service", Text = "Service" },
                    new SelectListItem { Value = "tires", Text = "Tires" },
                    new SelectListItem { Value = "insurance", Text = "Insurance" },
                    new SelectListItem { Value = "other", Text = "Other" }
                };

                return View(vendor);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to retrieve vendor {id} from API for edit, falling back to database");
                
                // Fallback to database
                var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == id);
                if (vendor == null)
                {
                    return NotFound();
                }
                
                ViewBag.VendorTypes = new List<SelectListItem>
                {
                    new SelectListItem { Value = "maintenance", Text = "Maintenance" },
                    new SelectListItem { Value = "fuel", Text = "Fuel" },
                    new SelectListItem { Value = "parts", Text = "Parts" },
                    new SelectListItem { Value = "service", Text = "Service" },
                    new SelectListItem { Value = "tires", Text = "Tires" },
                    new SelectListItem { Value = "insurance", Text = "Insurance" },
                    new SelectListItem { Value = "other", Text = "Other" }
                };

                return View(vendor);
            }
        }

        // POST: Vendor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Vendor vendor)
        {
            if (id != vendor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    vendor.UpdatedAt = DateTime.UtcNow;

                    await _apiService.PutAsync<Vendor>($"vendors/{id}", vendor);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating vendor with ID {id}");
                    ModelState.AddModelError("", "Failed to update vendor.");
                    
                    // Fallback to database
                    try
                    {
                        _context.Update(vendor);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!VendorExists(vendor.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    catch (Exception dbEx)
                    {
                        _logger.LogError(dbEx, $"Error updating vendor with ID {id} in database");
                        ModelState.AddModelError("", "Failed to update vendor in database.");
                    }
                }
            }

            ViewBag.VendorTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "maintenance", Text = "Maintenance" },
                new SelectListItem { Value = "fuel", Text = "Fuel" },
                new SelectListItem { Value = "parts", Text = "Parts" },
                new SelectListItem { Value = "service", Text = "Service" },
                new SelectListItem { Value = "tires", Text = "Tires" },
                new SelectListItem { Value = "insurance", Text = "Insurance" },
                new SelectListItem { Value = "other", Text = "Other" }
            };

            return View(vendor);
        }

        // GET: Vendor/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var vendor = await _apiService.GetAsync<Vendor>($"vendors/{id}");
                if (vendor == null)
                {
                    return NotFound();
                }
                return View(vendor);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to retrieve vendor {id} from API for delete, falling back to database");
                
                // Fallback to database
                var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == id);
                if (vendor == null)
                {
                    return NotFound();
                }
                return View(vendor);
            }
        }

        // POST: Vendor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _apiService.DeleteAsync($"vendors/{id}");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting vendor with ID {id}");
                
                // Fallback to database
                try
                {
                    var vendor = await _context.Vendors.FindAsync(id);
                    if (vendor != null)
                    {
                        _context.Vendors.Remove(vendor);
                        await _context.SaveChangesAsync();
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, $"Error deleting vendor with ID {id} from database");
                    return RedirectToAction(nameof(Index));
                }
            }
        }
        
        // GET: Vendor/Analytics
        public async Task<IActionResult> Analytics()
        {
            try
            {
                var vendors = await _context.Vendors.ToListAsync();
                var transactions = await _context.VendorTransactions.ToListAsync();
                
                // Calculate metrics for each vendor
                var vendorMetrics = new List<VendorMetrics>();
                
                foreach (var vendor in vendors)
                {
                    var vendorTransactions = transactions.Where(t => t.VendorId == vendor.Id).ToList();
                    
                    var metrics = new VendorMetrics
                    {
                        Vendor = vendor,
                        TotalTransactions = vendorTransactions.Count,
                        TotalSpent = vendorTransactions.Sum(t => t.TotalAmount),
                        AverageQualityRating = vendorTransactions.Where(t => t.QualityRating.HasValue).Select(t => t.QualityRating.Value).DefaultIfEmpty().Average(),
                        AverageServiceRating = vendorTransactions.Where(t => t.ServiceRating.HasValue).Select(t => t.ServiceRating.Value).DefaultIfEmpty().Average(),
                        AverageTimelinessRating = vendorTransactions.Where(t => t.TimelinessRating.HasValue).Select(t => t.TimelinessRating.Value).DefaultIfEmpty().Average(),
                        AveragePriceRating = vendorTransactions.Where(t => t.PriceRating.HasValue).Select(t => t.PriceRating.Value).DefaultIfEmpty().Average(),
                        OnTimeDeliveryRate = vendorTransactions.Any(t => t.IsOnTime.HasValue) 
                            ? (double)vendorTransactions.Count(t => t.IsOnTime == true) / vendorTransactions.Count(t => t.IsOnTime.HasValue) * 100 
                            : 0,
                        WithinBudgetRate = vendorTransactions.Any(t => t.IsWithinBudget.HasValue) 
                            ? (double)vendorTransactions.Count(t => t.IsWithinBudget == true) / vendorTransactions.Count(t => t.IsWithinBudget.HasValue) * 100 
                            : 0,
                        ComplianceRate = vendorTransactions.Any(t => t.IsCompliant.HasValue) 
                            ? (double)vendorTransactions.Count(t => t.IsCompliant == true) / vendorTransactions.Count(t => t.IsCompliant.HasValue) * 100 
                            : 0
                    };
                    
                    vendorMetrics.Add(metrics);
                }
                
                // Calculate comparative rankings
                var rankedMetrics = vendorMetrics.OrderByDescending(m => 
                    m.AverageQualityRating + 
                    m.AverageServiceRating + 
                    m.AverageTimelinessRating + 
                    m.AveragePriceRating + 
                    m.OnTimeDeliveryRate / 100 + 
                    m.WithinBudgetRate / 100 + 
                    m.ComplianceRate / 100
                ).ToList();
                
                // Add rank to each vendor
                for (int i = 0; i < rankedMetrics.Count; i++)
                {
                    rankedMetrics[i].OverallRank = i + 1;
                }
                
                return View(rankedMetrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating vendor analytics");
                TempData["ErrorMessage"] = "Failed to generate vendor analytics.";
                return RedirectToAction(nameof(Index));
            }
        }
        
        // GET: Vendor/ComparisonReport
        public async Task<IActionResult> ComparisonReport(int? vendor1Id, int? vendor2Id)
        {
            try
            {
                ViewBag.Vendors = new SelectList(await _context.Vendors.ToListAsync(), "Id", "Name");
                
                if (vendor1Id.HasValue && vendor2Id.HasValue)
                {
                    var vendor1 = await _context.Vendors.FindAsync(vendor1Id.Value);
                    var vendor2 = await _context.Vendors.FindAsync(vendor2Id.Value);
                    
                    if (vendor1 == null || vendor2 == null)
                    {
                        TempData["ErrorMessage"] = "One or both vendors not found.";
                        return View();
                    }
                    
                    var vendor1Transactions = await _context.VendorTransactions
                        .Where(t => t.VendorId == vendor1Id.Value)
                        .ToListAsync();
                        
                    var vendor2Transactions = await _context.VendorTransactions
                        .Where(t => t.VendorId == vendor2Id.Value)
                        .ToListAsync();
                    
                    var comparison = new VendorComparison
                    {
                        Vendor1 = vendor1,
                        Vendor2 = vendor2,
                        Vendor1Metrics = CalculateVendorMetrics(vendor1, vendor1Transactions),
                        Vendor2Metrics = CalculateVendorMetrics(vendor2, vendor2Transactions)
                    };
                    
                    ViewBag.Comparison = comparison;
                }
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating vendor comparison report");
                TempData["ErrorMessage"] = "Failed to generate vendor comparison report.";
                return RedirectToAction(nameof(Index));
            }
        }
        
        // Helper method to calculate metrics for a vendor
        private VendorMetrics CalculateVendorMetrics(Vendor vendor, List<VendorTransaction> transactions)
        {
            return new VendorMetrics
            {
                Vendor = vendor,
                TotalTransactions = transactions.Count,
                TotalSpent = transactions.Sum(t => t.TotalAmount),
                AverageQualityRating = transactions.Where(t => t.QualityRating.HasValue).Select(t => t.QualityRating.Value).DefaultIfEmpty().Average(),
                AverageServiceRating = transactions.Where(t => t.ServiceRating.HasValue).Select(t => t.ServiceRating.Value).DefaultIfEmpty().Average(),
                AverageTimelinessRating = transactions.Where(t => t.TimelinessRating.HasValue).Select(t => t.TimelinessRating.Value).DefaultIfEmpty().Average(),
                AveragePriceRating = transactions.Where(t => t.PriceRating.HasValue).Select(t => t.PriceRating.Value).DefaultIfEmpty().Average(),
                OnTimeDeliveryRate = transactions.Any(t => t.IsOnTime.HasValue) 
                    ? (double)transactions.Count(t => t.IsOnTime == true) / transactions.Count(t => t.IsOnTime.HasValue) * 100 
                    : 0,
                WithinBudgetRate = transactions.Any(t => t.IsWithinBudget.HasValue) 
                    ? (double)transactions.Count(t => t.IsWithinBudget == true) / transactions.Count(t => t.IsWithinBudget.HasValue) * 100 
                    : 0,
                ComplianceRate = transactions.Any(t => t.IsCompliant.HasValue) 
                    ? (double)transactions.Count(t => t.IsCompliant == true) / transactions.Count(t => t.IsCompliant.HasValue) * 100 
                    : 0
            };
        }

        private bool VendorExists(int id)
        {
            return _context.Vendors.Any(e => e.Id == id);
        }
    }
    
    // Helper class for vendor metrics calculations
    public class VendorMetrics
    {
        public Vendor Vendor { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalSpent { get; set; }
        public double AverageQualityRating { get; set; }
        public double AverageServiceRating { get; set; }
        public double AverageTimelinessRating { get; set; }
        public double AveragePriceRating { get; set; }
        public double OverallAverageRating => (AverageQualityRating + AverageServiceRating + AverageTimelinessRating + AveragePriceRating) / 4;
        public double OnTimeDeliveryRate { get; set; } // Percentage
        public double WithinBudgetRate { get; set; } // Percentage
        public double ComplianceRate { get; set; } // Percentage
        public int OverallRank { get; set; }
    }
    
    // Helper class for vendor comparison
    public class VendorComparison
    {
        public Vendor Vendor1 { get; set; }
        public Vendor Vendor2 { get; set; }
        public VendorMetrics Vendor1Metrics { get; set; }
        public VendorMetrics Vendor2Metrics { get; set; }
    }
} 