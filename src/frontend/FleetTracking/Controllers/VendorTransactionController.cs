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
    public class VendorTransactionController : Controller
    {
        private readonly ILogger<VendorTransactionController> _logger;
        private readonly ApiService _apiService;
        private readonly ApplicationDbContext _context;

        public VendorTransactionController(
            ILogger<VendorTransactionController> logger,
            ApiService apiService,
            ApplicationDbContext context)
        {
            _logger = logger;
            _apiService = apiService;
            _context = context;
        }

        // GET: VendorTransaction
        public async Task<IActionResult> Index()
        {
            try
            {
                // Try to get transactions from API first
                var transactions = await _apiService.GetAsync<List<VendorTransaction>>("vendortransactions");
                
                // Load related entities
                foreach (var transaction in transactions)
                {
                    transaction.Vendor = await _context.Vendors.FindAsync(transaction.VendorId);
                    if (transaction.VehicleId.HasValue)
                    {
                        transaction.Vehicle = await _context.Vehicles.FindAsync(transaction.VehicleId.Value);
                    }
                }
                
                return View(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve vendor transactions from API, falling back to database");
                
                // Fallback to database
                var transactions = await _context.VendorTransactions
                    .Include(vt => vt.Vendor)
                    .Include(vt => vt.Vehicle)
                    .OrderByDescending(vt => vt.TransactionDate)
                    .ToListAsync();
                
                return View(transactions);
            }
        }

        // GET: VendorTransaction/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var transaction = await _apiService.GetAsync<VendorTransaction>($"vendortransactions/{id}");
                if (transaction == null)
                {
                    return NotFound();
                }
                
                // Load related entities
                transaction.Vendor = await _context.Vendors.FindAsync(transaction.VendorId);
                if (transaction.VehicleId.HasValue)
                {
                    transaction.Vehicle = await _context.Vehicles.FindAsync(transaction.VehicleId.Value);
                }
                if (transaction.MaintenanceRecordId.HasValue)
                {
                    transaction.MaintenanceRecord = await _context.Set<MaintenanceRecord>().FindAsync(transaction.MaintenanceRecordId.Value);
                }
                
                return View(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to retrieve vendor transaction {id} from API, falling back to database");
                
                // Fallback to database
                var transaction = await _context.VendorTransactions
                    .Include(vt => vt.Vendor)
                    .Include(vt => vt.Vehicle)
                    .Include(vt => vt.MaintenanceRecord)
                    .FirstOrDefaultAsync(vt => vt.Id == id);
                
                if (transaction == null)
                {
                    return NotFound();
                }
                
                return View(transaction);
            }
        }

        // GET: VendorTransaction/Create
        public async Task<IActionResult> Create(int? vendorId)
        {
            var vendors = await _context.Vendors.OrderBy(v => v.Name).ToListAsync();
            ViewBag.Vendors = new SelectList(vendors, "Id", "Name", vendorId);
            
            var vehicles = await _context.Vehicles.OrderBy(v => v.RegistrationNumber).ToListAsync();
            ViewBag.Vehicles = new SelectList(vehicles, "Id", "RegistrationNumber");
            
            ViewBag.TransactionTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "purchase", Text = "Purchase" },
                new SelectListItem { Value = "service", Text = "Service" },
                new SelectListItem { Value = "maintenance", Text = "Maintenance" },
                new SelectListItem { Value = "fuel", Text = "Fuel" },
                new SelectListItem { Value = "parts", Text = "Parts" },
                new SelectListItem { Value = "repair", Text = "Repair" },
                new SelectListItem { Value = "other", Text = "Other" }
            };
            
            ViewBag.StatusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "pending", Text = "Pending" },
                new SelectListItem { Value = "paid", Text = "Paid" },
                new SelectListItem { Value = "cancelled", Text = "Cancelled" },
                new SelectListItem { Value = "disputed", Text = "Disputed" }
            };
            
            ViewBag.PaymentMethods = new List<SelectListItem>
            {
                new SelectListItem { Value = "credit_card", Text = "Credit Card" },
                new SelectListItem { Value = "bank_transfer", Text = "Bank Transfer" },
                new SelectListItem { Value = "cash", Text = "Cash" },
                new SelectListItem { Value = "check", Text = "Check" },
                new SelectListItem { Value = "other", Text = "Other" }
            };
            
            var transaction = new VendorTransaction
            {
                TransactionDate = DateTime.UtcNow,
                Status = "pending",
                VendorId = vendorId ?? 0
            };
            
            return View(transaction);
        }

        // POST: VendorTransaction/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VendorTransaction transaction)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    transaction.CreatedAt = DateTime.UtcNow;
                    transaction.UpdatedAt = DateTime.UtcNow;
                    
                    var response = await _apiService.PostAsync<VendorTransaction, VendorTransaction>("vendortransactions", transaction);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating vendor transaction");
                    ModelState.AddModelError("", "Failed to create vendor transaction.");
                    
                    // Fallback to database
                    try
                    {
                        _context.VendorTransactions.Add(transaction);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception dbEx)
                    {
                        _logger.LogError(dbEx, "Error creating vendor transaction in database");
                        ModelState.AddModelError("", "Failed to create vendor transaction in database.");
                    }
                }
            }
            
            // If we got this far, something failed, redisplay form
            var vendors = await _context.Vendors.OrderBy(v => v.Name).ToListAsync();
            ViewBag.Vendors = new SelectList(vendors, "Id", "Name", transaction.VendorId);
            
            var vehicles = await _context.Vehicles.OrderBy(v => v.RegistrationNumber).ToListAsync();
            ViewBag.Vehicles = new SelectList(vehicles, "Id", "RegistrationNumber", transaction.VehicleId);
            
            ViewBag.TransactionTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "purchase", Text = "Purchase" },
                new SelectListItem { Value = "service", Text = "Service" },
                new SelectListItem { Value = "maintenance", Text = "Maintenance" },
                new SelectListItem { Value = "fuel", Text = "Fuel" },
                new SelectListItem { Value = "parts", Text = "Parts" },
                new SelectListItem { Value = "repair", Text = "Repair" },
                new SelectListItem { Value = "other", Text = "Other" }
            };
            
            ViewBag.StatusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "pending", Text = "Pending" },
                new SelectListItem { Value = "paid", Text = "Paid" },
                new SelectListItem { Value = "cancelled", Text = "Cancelled" },
                new SelectListItem { Value = "disputed", Text = "Disputed" }
            };
            
            ViewBag.PaymentMethods = new List<SelectListItem>
            {
                new SelectListItem { Value = "credit_card", Text = "Credit Card" },
                new SelectListItem { Value = "bank_transfer", Text = "Bank Transfer" },
                new SelectListItem { Value = "cash", Text = "Cash" },
                new SelectListItem { Value = "check", Text = "Check" },
                new SelectListItem { Value = "other", Text = "Other" }
            };
            
            return View(transaction);
        }

        // GET: VendorTransaction/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            VendorTransaction transaction;
            
            try
            {
                transaction = await _apiService.GetAsync<VendorTransaction>($"vendortransactions/{id}");
                if (transaction == null)
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to retrieve vendor transaction {id} from API, falling back to database");
                
                // Fallback to database
                transaction = await _context.VendorTransactions.FindAsync(id);
                if (transaction == null)
                {
                    return NotFound();
                }
            }
            
            var vendors = await _context.Vendors.OrderBy(v => v.Name).ToListAsync();
            ViewBag.Vendors = new SelectList(vendors, "Id", "Name", transaction.VendorId);
            
            var vehicles = await _context.Vehicles.OrderBy(v => v.RegistrationNumber).ToListAsync();
            ViewBag.Vehicles = new SelectList(vehicles, "Id", "RegistrationNumber", transaction.VehicleId);
            
            ViewBag.TransactionTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "purchase", Text = "Purchase" },
                new SelectListItem { Value = "service", Text = "Service" },
                new SelectListItem { Value = "maintenance", Text = "Maintenance" },
                new SelectListItem { Value = "fuel", Text = "Fuel" },
                new SelectListItem { Value = "parts", Text = "Parts" },
                new SelectListItem { Value = "repair", Text = "Repair" },
                new SelectListItem { Value = "other", Text = "Other" }
            };
            
            ViewBag.StatusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "pending", Text = "Pending" },
                new SelectListItem { Value = "paid", Text = "Paid" },
                new SelectListItem { Value = "cancelled", Text = "Cancelled" },
                new SelectListItem { Value = "disputed", Text = "Disputed" }
            };
            
            ViewBag.PaymentMethods = new List<SelectListItem>
            {
                new SelectListItem { Value = "credit_card", Text = "Credit Card" },
                new SelectListItem { Value = "bank_transfer", Text = "Bank Transfer" },
                new SelectListItem { Value = "cash", Text = "Cash" },
                new SelectListItem { Value = "check", Text = "Check" },
                new SelectListItem { Value = "other", Text = "Other" }
            };
            
            return View(transaction);
        }

        // POST: VendorTransaction/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VendorTransaction transaction)
        {
            if (id != transaction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    transaction.UpdatedAt = DateTime.UtcNow;

                    await _apiService.PutAsync<VendorTransaction, object>($"vendortransactions/{id}", transaction);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating vendor transaction with ID {id}");
                    ModelState.AddModelError("", "Failed to update vendor transaction.");
                    
                    // Fallback to database
                    try
                    {
                        _context.Update(transaction);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!TransactionExists(transaction.Id))
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
                        _logger.LogError(dbEx, $"Error updating vendor transaction with ID {id} in database");
                        ModelState.AddModelError("", "Failed to update vendor transaction in database.");
                    }
                }
            }
            
            // If we got this far, something failed, redisplay form
            var vendors = await _context.Vendors.OrderBy(v => v.Name).ToListAsync();
            ViewBag.Vendors = new SelectList(vendors, "Id", "Name", transaction.VendorId);
            
            var vehicles = await _context.Vehicles.OrderBy(v => v.RegistrationNumber).ToListAsync();
            ViewBag.Vehicles = new SelectList(vehicles, "Id", "RegistrationNumber", transaction.VehicleId);
            
            ViewBag.TransactionTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "purchase", Text = "Purchase" },
                new SelectListItem { Value = "service", Text = "Service" },
                new SelectListItem { Value = "maintenance", Text = "Maintenance" },
                new SelectListItem { Value = "fuel", Text = "Fuel" },
                new SelectListItem { Value = "parts", Text = "Parts" },
                new SelectListItem { Value = "repair", Text = "Repair" },
                new SelectListItem { Value = "other", Text = "Other" }
            };
            
            ViewBag.StatusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "pending", Text = "Pending" },
                new SelectListItem { Value = "paid", Text = "Paid" },
                new SelectListItem { Value = "cancelled", Text = "Cancelled" },
                new SelectListItem { Value = "disputed", Text = "Disputed" }
            };
            
            ViewBag.PaymentMethods = new List<SelectListItem>
            {
                new SelectListItem { Value = "credit_card", Text = "Credit Card" },
                new SelectListItem { Value = "bank_transfer", Text = "Bank Transfer" },
                new SelectListItem { Value = "cash", Text = "Cash" },
                new SelectListItem { Value = "check", Text = "Check" },
                new SelectListItem { Value = "other", Text = "Other" }
            };
            
            return View(transaction);
        }

        // GET: VendorTransaction/RateTransaction/5
        public async Task<IActionResult> RateTransaction(int id)
        {
            try
            {
                var transaction = await _apiService.GetAsync<VendorTransaction>($"vendortransactions/{id}");
                if (transaction == null)
                {
                    return NotFound();
                }
                
                transaction.Vendor = await _context.Vendors.FindAsync(transaction.VendorId);
                return View(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to retrieve vendor transaction {id} from API for rating, falling back to database");
                
                // Fallback to database
                var transaction = await _context.VendorTransactions
                    .Include(vt => vt.Vendor)
                    .FirstOrDefaultAsync(vt => vt.Id == id);
                
                if (transaction == null)
                {
                    return NotFound();
                }
                
                return View(transaction);
            }
        }

        // POST: VendorTransaction/RateTransaction/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RateTransaction(int id, VendorTransaction transaction)
        {
            if (id != transaction.Id)
            {
                return NotFound();
            }

            // Only update the ratings fields
            try
            {
                var existingTransaction = await _context.VendorTransactions.FindAsync(id);
                if (existingTransaction == null)
                {
                    return NotFound();
                }
                
                existingTransaction.QualityRating = transaction.QualityRating;
                existingTransaction.ServiceRating = transaction.ServiceRating;
                existingTransaction.TimelinessRating = transaction.TimelinessRating;
                existingTransaction.PriceRating = transaction.PriceRating;
                existingTransaction.OverallRating = transaction.OverallRating;
                existingTransaction.IsOnTime = transaction.IsOnTime;
                existingTransaction.IsWithinBudget = transaction.IsWithinBudget;
                existingTransaction.IsCompliant = transaction.IsCompliant;
                existingTransaction.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Transaction rated successfully!";
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error rating vendor transaction with ID {id}");
                ModelState.AddModelError("", "Failed to rate transaction.");
                
                transaction.Vendor = await _context.Vendors.FindAsync(transaction.VendorId);
                return View(transaction);
            }
        }

        // GET: VendorTransaction/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var transaction = await _apiService.GetAsync<VendorTransaction>($"vendortransactions/{id}");
                if (transaction == null)
                {
                    return NotFound();
                }
                
                // Load related entities
                transaction.Vendor = await _context.Vendors.FindAsync(transaction.VendorId);
                
                return View(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to retrieve vendor transaction {id} from API for delete, falling back to database");
                
                // Fallback to database
                var transaction = await _context.VendorTransactions
                    .Include(vt => vt.Vendor)
                    .FirstOrDefaultAsync(vt => vt.Id == id);
                
                if (transaction == null)
                {
                    return NotFound();
                }
                
                return View(transaction);
            }
        }

        // POST: VendorTransaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _apiService.DeleteAsync($"vendortransactions/{id}");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting vendor transaction with ID {id}");
                
                // Fallback to database
                try
                {
                    var transaction = await _context.VendorTransactions.FindAsync(id);
                    if (transaction != null)
                    {
                        _context.VendorTransactions.Remove(transaction);
                        await _context.SaveChangesAsync();
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, $"Error deleting vendor transaction with ID {id} from database");
                    return RedirectToAction(nameof(Index));
                }
            }
        }
        
        // GET: VendorTransaction/CostAnalysis
        public async Task<IActionResult> CostAnalysis()
        {
            try
            {
                var transactions = await _context.VendorTransactions
                    .Include(vt => vt.Vendor)
                    .Include(vt => vt.Vehicle)
                    .ToListAsync();
                
                // Group transactions by vendor, vehicle, and type for analysis
                var vendorAnalysis = transactions
                    .GroupBy(t => t.VendorId)
                    .Select(g => new VendorCostAnalysis 
                    {
                        VendorId = g.Key,
                        VendorName = g.First().Vendor?.Name ?? "Unknown",
                        TotalTransactions = g.Count(),
                        TotalSpent = g.Sum(t => t.TotalAmount),
                        AverageAmount = g.Average(t => t.TotalAmount),
                        TransactionsByType = g.GroupBy(t => t.TransactionType)
                            .Select(tg => new TransactionTypeBreakdown
                            {
                                TransactionType = tg.Key,
                                TransactionTypeDisplayName = tg.First().TransactionTypeDisplayName,
                                Count = tg.Count(),
                                TotalAmount = tg.Sum(t => t.TotalAmount)
                            }).ToList()
                    })
                    .OrderByDescending(v => v.TotalSpent)
                    .ToList();
                
                var vehicleAnalysis = transactions
                    .Where(t => t.VehicleId.HasValue)
                    .GroupBy(t => t.VehicleId)
                    .Select(g => new VehicleCostAnalysis
                    {
                        VehicleId = g.Key,
                        VehicleName = g.First().Vehicle?.RegistrationNumber ?? "Unknown",
                        TotalTransactions = g.Count(),
                        TotalSpent = g.Sum(t => t.TotalAmount),
                        AverageAmount = g.Average(t => t.TotalAmount),
                        TransactionsByType = g.GroupBy(t => t.TransactionType)
                            .Select(tg => new TransactionTypeBreakdown
                            {
                                TransactionType = tg.Key,
                                TransactionTypeDisplayName = tg.First().TransactionTypeDisplayName,
                                Count = tg.Count(),
                                TotalAmount = tg.Sum(t => t.TotalAmount)
                            }).ToList()
                    })
                    .OrderByDescending(v => v.TotalSpent)
                    .ToList();
                
                // Calculate total spend by transaction type
                var spendByType = transactions
                    .GroupBy(t => t.TransactionType)
                    .Select(g => new TransactionTypeBreakdown
                    {
                        TransactionType = g.Key,
                        TransactionTypeDisplayName = g.First().TransactionTypeDisplayName,
                        Count = g.Count(),
                        TotalAmount = g.Sum(t => t.TotalAmount)
                    })
                    .OrderByDescending(t => t.TotalAmount)
                    .ToList();
                
                // Calculate monthly spend for trend analysis
                var monthlySpend = transactions
                    .GroupBy(t => new { Year = t.TransactionDate.Year, Month = t.TransactionDate.Month })
                    .Select(g => new MonthlySpend
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        MonthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                        TotalAmount = g.Sum(t => t.TotalAmount)
                    })
                    .OrderBy(m => m.Year)
                    .ThenBy(m => m.Month)
                    .ToList();
                
                ViewBag.VendorAnalysis = vendorAnalysis;
                ViewBag.VehicleAnalysis = vehicleAnalysis;
                ViewBag.SpendByType = spendByType;
                ViewBag.MonthlySpend = monthlySpend;
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating cost analysis");
                TempData["ErrorMessage"] = "Failed to generate cost analysis.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool TransactionExists(int id)
        {
            return _context.VendorTransactions.Any(e => e.Id == id);
        }
    }
    
    // Helper classes for analysis
    public class VendorCostAnalysis
    {
        public int? VendorId { get; set; }
        public string VendorName { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AverageAmount { get; set; }
        public List<TransactionTypeBreakdown> TransactionsByType { get; set; }
    }
    
    public class VehicleCostAnalysis
    {
        public int? VehicleId { get; set; }
        public string VehicleName { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AverageAmount { get; set; }
        public List<TransactionTypeBreakdown> TransactionsByType { get; set; }
    }
    
    public class TransactionTypeBreakdown
    {
        public string TransactionType { get; set; }
        public string TransactionTypeDisplayName { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }
    
    public class MonthlySpend
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public decimal TotalAmount { get; set; }
        public string MonthYear => $"{MonthName} {Year}";
    }
} 