using Microsoft.Extensions.DependencyInjection;

public void ConfigureServices(IServiceCollection services)
{
    services.AddControllersWithViews();

    // Add HttpClient
    services.AddHttpClient();

    // Add services
    services.AddTransient<VehicleService>();
    services.AddTransient<DriverService>();
    services.AddTransient<MaintenanceService>();
    services.AddTransient<GeofenceService>();
    services.AddTransient<VehicleHistoryService>();
    
    // Add signalR for real-time updates
    services.AddSignalR();
} 