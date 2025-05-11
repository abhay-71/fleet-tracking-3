using FleetTracking.Data;
using FleetTracking.Hubs;
using FleetTracking.Middleware;
using FleetTracking.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using System.Net.Mime;

var builder = WebApplication.CreateBuilder(args);

// Configure specific port to avoid conflicts - using different ports (5004, 5005) and binding to all interfaces
builder.WebHost.UseUrls("http://0.0.0.0:5004", "https://0.0.0.0:5005");

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

// Add ApiService
builder.Services.AddHttpClient();
builder.Services.AddTransient<IApiService, ApiService>();

// Add Email Sender service
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, FleetTracking.Services.EmailSender>();

// Add Services from Startup.cs
builder.Services.AddTransient<VehicleService>();
builder.Services.AddTransient<DriverService>();
builder.Services.AddTransient<MaintenanceService>();
builder.Services.AddTransient<GeofenceService>();
builder.Services.AddTransient<VehicleHistoryService>();

// Add SignalR support
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 102400; // 100 KB
});

// Register vehicle status service
builder.Services.AddHostedService<VehicleStatusService>();
builder.Services.AddSingleton<VehicleStatusService>();

// Configure Identity options
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// Configure API client services
builder.Services.AddHttpClient("FleetTrackingApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:8080");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Configure session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Seed roles and users
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
        
        // Seed roles and admin user
        RoleSeeder.SeedRolesAsync(app.Services).Wait();
        RoleSeeder.SeedAdminUserAsync(app.Services).Wait();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    // Use our custom error handling middleware instead of the default exception handler
    // app.UseExceptionHandler("/Home/Error");
    
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}

// Add global error handling middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

// Comment out HTTPS redirection to allow HTTP connections
// app.UseHttpsRedirection();

// Add default files configuration - will look for index.html
app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "index.html" }
});

// Enhanced static files configuration (only one UseStaticFiles call)
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static files for 24 hours
        ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=86400";
        
        // Set appropriate MIME types for common file extensions
        var path = ctx.File.PhysicalPath;
        if (path != null)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();
            switch (extension)
            {
                case ".css":
                    ctx.Context.Response.ContentType = "text/css";
                    break;
                case ".js":
                    ctx.Context.Response.ContentType = "text/javascript";
                    break;
                case ".woff":
                case ".woff2":
                    ctx.Context.Response.ContentType = "application/font-woff";
                    break;
                case ".ttf":
                    ctx.Context.Response.ContentType = "application/octet-stream";
                    break;
                case ".svg":
                    ctx.Context.Response.ContentType = "image/svg+xml";
                    break;
                case ".eot":
                    ctx.Context.Response.ContentType = "application/vnd.ms-fontobject";
                    break;
            }
        }
    }
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Make sure we have appropriate endpoint routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Ensure SignalR hubs are mapped
app.MapHub<VehicleStatusHub>("/hubs/vehicleStatus");

// Map Razor pages
app.MapRazorPages();

app.Run(); 