using FleetTracking.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FleetTracking.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSet properties for application models
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<Trip> Trips { get; set; }
    public DbSet<Waypoint> Waypoints { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<VehicleType> VehicleTypes { get; set; }
    public DbSet<PointOfInterest> PointsOfInterest { get; set; }
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<VendorTransaction> VendorTransactions { get; set; }
    public DbSet<ScheduledReport> ScheduledReports { get; set; }
    public DbSet<ScheduledReportRecipient> ScheduledReportRecipients { get; set; }
    public DbSet<ScheduledReportVersion> ScheduledReportVersions { get; set; }
    
    // Inventory Management
    public DbSet<InventoryCategory> InventoryCategories { get; set; }
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
    
    // Purchase Management
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }

    public DbSet<Geofence> Geofences { get; set; }
    public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
    public DbSet<Maintenance> Maintenances { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure entity relationships and constraints
        builder.Entity<Vehicle>()
            .HasOne(v => v.Company)
            .WithMany(c => c.Vehicles)
            .HasForeignKey(v => v.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Vehicle>()
            .HasOne(v => v.VehicleType)
            .WithMany()
            .HasForeignKey(v => v.VehicleTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Driver>()
            .HasOne(d => d.User)
            .WithOne()
            .HasForeignKey<Driver>(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Driver>()
            .HasOne(d => d.Company)
            .WithMany(c => c.Drivers)
            .HasForeignKey(d => d.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Trip>()
            .HasOne(t => t.Vehicle)
            .WithMany(v => v.Trips)
            .HasForeignKey(t => t.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Trip>()
            .HasOne(t => t.Driver)
            .WithMany(d => d.Trips)
            .HasForeignKey(t => t.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Waypoint>()
            .HasOne(w => w.Trip)
            .WithMany(t => t.Waypoints)
            .HasForeignKey(w => w.TripId)
            .OnDelete(DeleteBehavior.Cascade);

        // POI Configurations
        builder.Entity<PointOfInterest>()
            .HasIndex(p => new { p.Latitude, p.Longitude });
            
        // Vendor Configurations
        builder.Entity<Vendor>()
            .HasOne(v => v.Company)
            .WithMany()
            .HasForeignKey(v => v.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Entity<VendorTransaction>()
            .HasOne(vt => vt.Vendor)
            .WithMany()
            .HasForeignKey(vt => vt.VendorId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Entity<VendorTransaction>()
            .HasOne(vt => vt.Vehicle)
            .WithMany()
            .HasForeignKey(vt => vt.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Entity<VendorTransaction>()
            .HasOne(vt => vt.MaintenanceRecord)
            .WithMany()
            .HasForeignKey(vt => vt.MaintenanceRecordId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Entity<VendorTransaction>()
            .HasOne(vt => vt.Company)
            .WithMany()
            .HasForeignKey(vt => vt.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Scheduled Report Configurations
        builder.Entity<ScheduledReport>()
            .HasOne(sr => sr.CreatedByUser)
            .WithMany()
            .HasForeignKey(sr => sr.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Entity<ScheduledReportRecipient>()
            .HasOne(srr => srr.ScheduledReport)
            .WithMany(sr => sr.ScheduledReportRecipients)
            .HasForeignKey(srr => srr.ScheduledReportId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Entity<ScheduledReportVersion>()
            .HasOne(srv => srv.ScheduledReport)
            .WithMany(sr => sr.ScheduledReportVersions)
            .HasForeignKey(srv => srv.ScheduledReportId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Inventory Configurations
        builder.Entity<InventoryCategory>()
            .HasOne(ic => ic.ParentCategory)
            .WithMany(ic => ic.ChildCategories)
            .HasForeignKey(ic => ic.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Entity<InventoryItem>()
            .HasOne(ii => ii.Category)
            .WithMany(ic => ic.Items)
            .HasForeignKey(ii => ii.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Entity<InventoryTransaction>()
            .HasOne(it => it.Item)
            .WithMany()
            .HasForeignKey(it => it.ItemId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Entity<InventoryTransaction>()
            .HasOne(it => it.Vehicle)
            .WithMany()
            .HasForeignKey(it => it.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Entity<InventoryTransaction>()
            .HasOne(it => it.MaintenanceRecord)
            .WithMany()
            .HasForeignKey(it => it.MaintenanceRecordId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Entity<InventoryTransaction>()
            .HasOne(it => it.PerformedBy)
            .WithMany()
            .HasForeignKey(it => it.PerformedById)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Purchase Order Configurations
        builder.Entity<PurchaseOrder>()
            .HasOne(po => po.Vendor)
            .WithMany()
            .HasForeignKey(po => po.VendorId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Entity<PurchaseOrder>()
            .HasOne(po => po.CreatedBy)
            .WithMany()
            .HasForeignKey(po => po.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Entity<PurchaseOrder>()
            .HasOne(po => po.ApprovedBy)
            .WithMany()
            .HasForeignKey(po => po.ApprovedById)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Entity<PurchaseOrderItem>()
            .HasOne(poi => poi.PurchaseOrder)
            .WithMany(po => po.Items)
            .HasForeignKey(poi => poi.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Entity<PurchaseOrderItem>()
            .HasOne(poi => poi.InventoryItem)
            .WithMany()
            .HasForeignKey(poi => poi.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Customize the ASP.NET Identity model and override the defaults
        builder.Entity<ApplicationUser>()
            .HasMany(e => e.Claims)
            .WithOne()
            .HasForeignKey(uc => uc.UserId)
            .IsRequired();

        builder.Entity<ApplicationUser>()
            .HasMany(e => e.Logins)
            .WithOne()
            .HasForeignKey(ul => ul.UserId)
            .IsRequired();

        builder.Entity<ApplicationUser>()
            .HasMany(e => e.Tokens)
            .WithOne()
            .HasForeignKey(ut => ut.UserId)
            .IsRequired();

        builder.Entity<ApplicationUser>()
            .HasMany(e => e.UserRoles)
            .WithOne()
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();
    }
} 