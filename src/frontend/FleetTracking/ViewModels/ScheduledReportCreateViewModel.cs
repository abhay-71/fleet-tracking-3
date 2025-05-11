using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FleetTracking.ViewModels
{
    public class ScheduledReportCreateViewModel
    {
        [Required]
        [Display(Name = "Report Name")]
        public string Name { get; set; }
        
        [Required]
        [Display(Name = "Report Type")]
        public string ReportType { get; set; }
        
        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;
        
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
        
        [Required]
        [Display(Name = "Frequency")]
        public string Frequency { get; set; }
        
        [Required]
        [Display(Name = "Time")]
        [DataType(DataType.Time)]
        public DateTime Time { get; set; } = DateTime.Today.AddHours(8); // Default 8:00 AM
        
        [Required]
        [Display(Name = "File Format")]
        public string FileFormat { get; set; }
        
        [Required]
        [Display(Name = "Delivery Method")]
        public string DeliveryMethod { get; set; }
        
        [Display(Name = "Email Recipients")]
        public string EmailRecipients { get; set; }
        
        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
        
        // Dropdown options
        public List<SelectListItem> ReportTypeOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "VehicleStatus", Text = "Vehicle Status Report" },
            new SelectListItem { Value = "TripSummary", Text = "Trip Summary Report" },
            new SelectListItem { Value = "FuelConsumption", Text = "Fuel Consumption Report" },
            new SelectListItem { Value = "MaintenanceCost", Text = "Maintenance Cost Report" },
            new SelectListItem { Value = "DriverPerformance", Text = "Driver Performance Report" },
            new SelectListItem { Value = "FinancialSummary", Text = "Financial Summary Report" }
        };
        
        public List<SelectListItem> FrequencyOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Daily", Text = "Daily" },
            new SelectListItem { Value = "Weekly", Text = "Weekly" },
            new SelectListItem { Value = "Monthly", Text = "Monthly" },
            new SelectListItem { Value = "Quarterly", Text = "Quarterly" }
        };
        
        public List<SelectListItem> FileFormatOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "PDF", Text = "PDF" },
            new SelectListItem { Value = "Excel", Text = "Excel" },
            new SelectListItem { Value = "CSV", Text = "CSV" }
        };
        
        public List<SelectListItem> DeliveryMethodOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Email", Text = "Email" },
            new SelectListItem { Value = "Download", Text = "Download" },
            new SelectListItem { Value = "Dashboard", Text = "Dashboard" }
        };
    }
} 