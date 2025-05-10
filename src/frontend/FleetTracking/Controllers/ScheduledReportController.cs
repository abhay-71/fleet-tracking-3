using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FleetTracking.Models;
using FleetTracking.Data;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace FleetTracking.Controllers
{
    [Authorize]
    public class ScheduledReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ScheduledReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ScheduledReport
        public async Task<IActionResult> Index()
        {
            var scheduledReports = await _context.ScheduledReports
                .Include(s => s.CreatedByUser)
                .OrderByDescending(s => s.NextRunDate)
                .ToListAsync();
                
            return View(scheduledReports);
        }

        // GET: ScheduledReport/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scheduledReport = await _context.ScheduledReports
                .Include(s => s.CreatedByUser)
                .Include(s => s.ScheduledReportRecipients)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (scheduledReport == null)
            {
                return NotFound();
            }

            return View(scheduledReport);
        }

        // GET: ScheduledReport/Create
        public IActionResult Create()
        {
            ViewBag.ReportTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "vehicle_status", Text = "Vehicle Status Report" },
                new SelectListItem { Value = "trip_summary", Text = "Trip Summary Report" },
                new SelectListItem { Value = "fuel_consumption", Text = "Fuel Consumption Report" },
                new SelectListItem { Value = "maintenance_cost", Text = "Maintenance Cost Report" },
                new SelectListItem { Value = "driver_performance", Text = "Driver Performance Report" },
                new SelectListItem { Value = "financial_summary", Text = "Financial Summary Report" }
            };
            
            ViewBag.RecurrenceOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "daily", Text = "Daily" },
                new SelectListItem { Value = "weekly", Text = "Weekly" },
                new SelectListItem { Value = "monthly", Text = "Monthly" },
                new SelectListItem { Value = "quarterly", Text = "Quarterly" }
            };
            
            ViewBag.FormatOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "pdf", Text = "PDF" },
                new SelectListItem { Value = "excel", Text = "Excel" },
                new SelectListItem { Value = "csv", Text = "CSV" }
            };
            
            ViewBag.Users = new SelectList(_context.Users, "Id", "Email");
            
            return View();
        }

        // POST: ScheduledReport/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ScheduledReport scheduledReport, List<string> recipientEmails)
        {
            if (ModelState.IsValid)
            {
                // Set created by user ID and dates
                scheduledReport.CreatedByUserId = User.Identity.Name;
                scheduledReport.CreatedDate = DateTime.UtcNow;
                
                // Calculate next run date based on recurrence pattern
                scheduledReport.NextRunDate = CalculateNextRunDate(scheduledReport.RecurrencePattern, scheduledReport.StartDate);
                
                // Add report to database
                _context.Add(scheduledReport);
                await _context.SaveChangesAsync();
                
                // Add recipients if provided
                if (recipientEmails != null && recipientEmails.Any())
                {
                    foreach (var email in recipientEmails)
                    {
                        if (!string.IsNullOrWhiteSpace(email))
                        {
                            var recipient = new ScheduledReportRecipient
                            {
                                ScheduledReportId = scheduledReport.Id,
                                RecipientEmail = email.Trim()
                            };
                            _context.ScheduledReportRecipients.Add(recipient);
                        }
                    }
                    await _context.SaveChangesAsync();
                }
                
                TempData["SuccessMessage"] = "Scheduled report created successfully.";
                return RedirectToAction(nameof(Index));
            }
            
            // If we got this far, something went wrong, redisplay form
            ViewBag.ReportTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "vehicle_status", Text = "Vehicle Status Report" },
                new SelectListItem { Value = "trip_summary", Text = "Trip Summary Report" },
                new SelectListItem { Value = "fuel_consumption", Text = "Fuel Consumption Report" },
                new SelectListItem { Value = "maintenance_cost", Text = "Maintenance Cost Report" },
                new SelectListItem { Value = "driver_performance", Text = "Driver Performance Report" },
                new SelectListItem { Value = "financial_summary", Text = "Financial Summary Report" }
            };
            
            ViewBag.RecurrenceOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "daily", Text = "Daily" },
                new SelectListItem { Value = "weekly", Text = "Weekly" },
                new SelectListItem { Value = "monthly", Text = "Monthly" },
                new SelectListItem { Value = "quarterly", Text = "Quarterly" }
            };
            
            ViewBag.FormatOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "pdf", Text = "PDF" },
                new SelectListItem { Value = "excel", Text = "Excel" },
                new SelectListItem { Value = "csv", Text = "CSV" }
            };
            
            ViewBag.Users = new SelectList(_context.Users, "Id", "Email");
            
            return View(scheduledReport);
        }

        // GET: ScheduledReport/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scheduledReport = await _context.ScheduledReports
                .Include(s => s.ScheduledReportRecipients)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (scheduledReport == null)
            {
                return NotFound();
            }
            
            ViewBag.ReportTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "vehicle_status", Text = "Vehicle Status Report" },
                new SelectListItem { Value = "trip_summary", Text = "Trip Summary Report" },
                new SelectListItem { Value = "fuel_consumption", Text = "Fuel Consumption Report" },
                new SelectListItem { Value = "maintenance_cost", Text = "Maintenance Cost Report" },
                new SelectListItem { Value = "driver_performance", Text = "Driver Performance Report" },
                new SelectListItem { Value = "financial_summary", Text = "Financial Summary Report" }
            };
            
            ViewBag.RecurrenceOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "daily", Text = "Daily" },
                new SelectListItem { Value = "weekly", Text = "Weekly" },
                new SelectListItem { Value = "monthly", Text = "Monthly" },
                new SelectListItem { Value = "quarterly", Text = "Quarterly" }
            };
            
            ViewBag.FormatOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "pdf", Text = "PDF" },
                new SelectListItem { Value = "excel", Text = "Excel" },
                new SelectListItem { Value = "csv", Text = "CSV" }
            };
            
            ViewBag.Users = new SelectList(_context.Users, "Id", "Email");
            
            // Get recipient emails
            ViewBag.RecipientEmails = string.Join(",", scheduledReport.ScheduledReportRecipients.Select(r => r.RecipientEmail));
            
            return View(scheduledReport);
        }

        // POST: ScheduledReport/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ScheduledReport scheduledReport, List<string> recipientEmails)
        {
            if (id != scheduledReport.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get original record to copy unchanged fields
                    var originalReport = await _context.ScheduledReports
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Id == id);
                    
                    if (originalReport == null)
                    {
                        return NotFound();
                    }
                    
                    // Keep original values for these fields
                    scheduledReport.CreatedByUserId = originalReport.CreatedByUserId;
                    scheduledReport.CreatedDate = originalReport.CreatedDate;
                    
                    // Recalculate next run date based on potentially changed recurrence pattern
                    scheduledReport.NextRunDate = CalculateNextRunDate(scheduledReport.RecurrencePattern, scheduledReport.StartDate);
                    
                    // Update the report
                    _context.Update(scheduledReport);
                    
                    // Delete existing recipients
                    var existingRecipients = await _context.ScheduledReportRecipients
                        .Where(r => r.ScheduledReportId == id)
                        .ToListAsync();
                    
                    _context.ScheduledReportRecipients.RemoveRange(existingRecipients);
                    
                    // Add new recipients
                    if (recipientEmails != null && recipientEmails.Any())
                    {
                        foreach (var email in recipientEmails)
                        {
                            if (!string.IsNullOrWhiteSpace(email))
                            {
                                var recipient = new ScheduledReportRecipient
                                {
                                    ScheduledReportId = scheduledReport.Id,
                                    RecipientEmail = email.Trim()
                                };
                                _context.ScheduledReportRecipients.Add(recipient);
                            }
                        }
                    }
                    
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Scheduled report updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScheduledReportExists(scheduledReport.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                
                return RedirectToAction(nameof(Index));
            }
            
            // If we got this far, something went wrong, redisplay form
            ViewBag.ReportTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "vehicle_status", Text = "Vehicle Status Report" },
                new SelectListItem { Value = "trip_summary", Text = "Trip Summary Report" },
                new SelectListItem { Value = "fuel_consumption", Text = "Fuel Consumption Report" },
                new SelectListItem { Value = "maintenance_cost", Text = "Maintenance Cost Report" },
                new SelectListItem { Value = "driver_performance", Text = "Driver Performance Report" },
                new SelectListItem { Value = "financial_summary", Text = "Financial Summary Report" }
            };
            
            ViewBag.RecurrenceOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "daily", Text = "Daily" },
                new SelectListItem { Value = "weekly", Text = "Weekly" },
                new SelectListItem { Value = "monthly", Text = "Monthly" },
                new SelectListItem { Value = "quarterly", Text = "Quarterly" }
            };
            
            ViewBag.FormatOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "pdf", Text = "PDF" },
                new SelectListItem { Value = "excel", Text = "Excel" },
                new SelectListItem { Value = "csv", Text = "CSV" }
            };
            
            ViewBag.Users = new SelectList(_context.Users, "Id", "Email");
            
            return View(scheduledReport);
        }

        // GET: ScheduledReport/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scheduledReport = await _context.ScheduledReports
                .Include(s => s.CreatedByUser)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (scheduledReport == null)
            {
                return NotFound();
            }

            return View(scheduledReport);
        }

        // POST: ScheduledReport/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Delete recipients first (due to foreign key constraints)
            var recipients = await _context.ScheduledReportRecipients
                .Where(r => r.ScheduledReportId == id)
                .ToListAsync();
                
            _context.ScheduledReportRecipients.RemoveRange(recipients);
            
            // Delete the report
            var scheduledReport = await _context.ScheduledReports.FindAsync(id);
            if (scheduledReport != null)
            {
                _context.ScheduledReports.Remove(scheduledReport);
            }
            
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Scheduled report deleted successfully.";
            
            return RedirectToAction(nameof(Index));
        }
        
        // GET: ScheduledReport/History/5
        public async Task<IActionResult> History(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scheduledReport = await _context.ScheduledReports
                .Include(s => s.CreatedByUser)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (scheduledReport == null)
            {
                return NotFound();
            }
            
            var reportVersions = await _context.ScheduledReportVersions
                .Where(v => v.ScheduledReportId == id)
                .OrderByDescending(v => v.GeneratedDate)
                .ToListAsync();
                
            ViewBag.Report = scheduledReport;
            
            return View(reportVersions);
        }
        
        // Helper method to calculate next run date based on recurrence pattern
        private DateTime CalculateNextRunDate(string recurrencePattern, DateTime startDate)
        {
            DateTime nextRun;
            
            // If start date is in the future, use that as next run date
            if (startDate > DateTime.UtcNow)
            {
                return startDate;
            }
            
            // Otherwise, calculate next run based on pattern
            switch (recurrencePattern.ToLower())
            {
                case "daily":
                    // Next day
                    nextRun = DateTime.UtcNow.Date.AddDays(1);
                    break;
                    
                case "weekly":
                    // Next week, same day of week
                    nextRun = DateTime.UtcNow.Date.AddDays(7 - (int)DateTime.UtcNow.DayOfWeek + (int)startDate.DayOfWeek);
                    if (nextRun <= DateTime.UtcNow)
                    {
                        nextRun = nextRun.AddDays(7);
                    }
                    break;
                    
                case "monthly":
                    // Next month, same day of month
                    var nextMonth = DateTime.UtcNow.AddMonths(1);
                    var day = Math.Min(startDate.Day, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
                    nextRun = new DateTime(nextMonth.Year, nextMonth.Month, day);
                    break;
                    
                case "quarterly":
                    // Next quarter, same day of month
                    var monthsToAdd = 3 - ((DateTime.UtcNow.Month - 1) % 3);
                    var nextQuarter = DateTime.UtcNow.AddMonths(monthsToAdd);
                    day = Math.Min(startDate.Day, DateTime.DaysInMonth(nextQuarter.Year, nextQuarter.Month));
                    nextRun = new DateTime(nextQuarter.Year, nextQuarter.Month, day);
                    break;
                    
                default:
                    // Default to daily
                    nextRun = DateTime.UtcNow.Date.AddDays(1);
                    break;
            }
            
            return nextRun;
        }
        
        private bool ScheduledReportExists(int id)
        {
            return _context.ScheduledReports.Any(e => e.Id == id);
        }
    }
} 