# Fleet Tracking Application - Setup and Testing Guide for Windows

This guide provides step-by-step instructions for setting up and testing the Fleet Tracking Application through Phase 4 of development on a Windows environment.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Environment Setup](#environment-setup)
   - [Database Setup](#database-setup)
   - [API Setup](#api-setup)
   - [Frontend Setup](#frontend-setup)
3. [Running the Application](#running-the-application)
4. [Testing Core Features](#testing-core-features)
   - [Phase 1: Setup & Infrastructure](#phase-1-setup--infrastructure)
   - [Phase 2: Core Backend](#phase-2-core-backend)
   - [Phase 3: Frontend Foundation](#phase-3-frontend-foundation)
   - [Phase 4: Feature Implementation](#phase-4-feature-implementation)
5. [Troubleshooting](#troubleshooting)

## Prerequisites

Before you begin, ensure you have the following installed on your Windows system:

- **PostgreSQL 14+** with PostGIS extension
- **pgAdmin 4** (usually comes with PostgreSQL installation)
- **Go 1.18+**
- **.NET 6.0 SDK**
- **Git**
- **Docker** (optional, for containerized setup)
- **Node.js and npm** (for frontend dependencies)

## Environment Setup

### Database Setup

1. **Install PostgreSQL and PostGIS on Windows:**

   - Download and install PostgreSQL from the [official website](https://www.postgresql.org/download/windows/)
   - During installation:
     - Remember the password you set for the 'postgres' user
     - Keep the default port (5432)
     - Make sure to check the option to install pgAdmin 4
     - Install all offered components including Stack Builder
   - After installation completes, launch Stack Builder:
     - Select your PostgreSQL installation
     - Navigate to Spatial Extensions > PostGIS bundle
     - Install the PostGIS extension

2. **Launch pgAdmin 4:**

   - Open the Windows Start menu and search for "pgAdmin 4"
   - Launch pgAdmin 4
   - When prompted, enter the master password you created during installation
   - In the Browser panel on the left, expand Servers > PostgreSQL > your server (usually named "PostgreSQL xx")
   - Enter the password for the postgres user when prompted

3. **Create the Database using pgAdmin:**

   - Right-click on "Databases" in the browser tree and select "Create" > "Database"
   - In the Create Database dialog:
     - Enter "fleetvigil" as the Database name
     - Leave the other settings at their defaults
     - Click "Save"

4. **Create a Database User using pgAdmin:**

   - Expand your PostgreSQL server in the browser tree
   - Right-click on "Login/Group Roles" and select "Create" > "Login/Group Role"
   - In the Create Login/Group Role dialog, configure the following tabs:
     - General: Enter "fleetvigil_user" as the Name
     - Definition: Enter a secure password and select "Can login?"
     - Privileges: Select "Can create database objects?" and "Can create roles?"
     - Click "Save"

5. **Enable PostGIS Extension:**

   - In pgAdmin, expand Databases > fleetvigil
   - Right-click on "Extensions" and select "Create" > "Extension"
   - In the Create Extension dialog:
     - Select "postgis" from the Name dropdown
     - Click "Save"

6. **Run Database Migrations:**

   - In pgAdmin, right-click on the "fleetvigil" database and select "Query Tool"
   - Click the folder icon in the Query Tool toolbar to open a file
   - Navigate to the `src\database\schema.sql` file in your project and open it
   - Click the Execute/Refresh icon (or press F5) to run the SQL script
   - Check the "Messages" tab at the bottom to verify successful execution

### API Setup

1. **Configure the API:**

   ```powershell
   # Navigate to the API directory
   cd src\api
   
   # Update config.json with your database credentials
   # Edit the file using Notepad or another text editor
   notepad config.json
   ```

   Update the database section with your PostgreSQL credentials:
   ```json
   "database": {
     "host": "localhost",
     "port": 5432,
     "user": "fleetvigil_user",
     "password": "your_secure_password",
     "dbname": "fleetvigil",
     "sslmode": "disable"
   }
   ```

2. **Install Go Dependencies:**

   ```powershell
   # In the API directory
   go mod download
   ```

### Frontend Setup

1. **Install .NET SDK:**
   - Download and install .NET 6.0 SDK from [Microsoft's .NET download page](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
   - After installation, restart your PowerShell terminal
   - Verify installation by running:
   ```powershell
   dotnet --version
   ```

2. **Configure the Frontend:**

   ```powershell
   # Navigate to the frontend directory
   cd src\frontend\FleetTracking
   
   # Restore .NET dependencies
   dotnet restore
   ```

3. **Update API Connection:**
   
   - Open `src\frontend\FleetTracking\appsettings.json` using Notepad or another text editor
   - Update the API URL to match your Go API endpoint (default: http://localhost:8080)

## Running the Application

1. **Start the API Server:**

   ```powershell
   # First navigate to the project root
   cd D:\CURSOR\FLEETVIGIL\FleerTracking_3\fleet-tracking-3
   
   # Navigate to the API directory
   cd src\api\cmd
   
   # Run the API server
   go run main.go
   ```

2. **Start the Frontend Server:**

   ```powershell
   # Open a new PowerShell window
   # Navigate to the project root
   cd D:\CURSOR\FLEETVIGIL\FleerTracking_3\fleet-tracking-3
   
   # Navigate to the frontend directory
   cd src\frontend\FleetTracking
   
   # Run the frontend application
   dotnet run
   ```

3. **Access the Application:**
   
   Open your web browser and navigate to: `https://localhost:5001`

## Testing Core Features

### Phase 1: Setup & Infrastructure

The infrastructure setup is already complete. You can verify it by:

1. **Checking Database Connection:**
   - Log in to the application (credentials below)
   - Navigate to Dashboard to confirm data is loading

2. **Verifying Project Structure:**
   - The codebase follows the architecture defined in the documentation
   - Separation of concerns between frontend, API, and database components

### Phase 2: Core Backend

Test the following API endpoints using tools like Postman or the built-in frontend:

1. **Authentication:**
   - Login with test credentials:
     - Username: `admin@fleetvigil.com`
     - Password: `admin123`

2. **User Management:**
   - Navigate to Users section
   - Create a new user
   - Edit user details
   - Assign roles to users

3. **Vehicle Management:**
   - Navigate to Vehicles section
   - Add a new vehicle
   - Edit vehicle details
   - Assign vehicle to a driver

4. **Trip Management:**
   - Navigate to Trips section
   - Create a new trip
   - View trip details
   - Update trip status

### Phase 3: Frontend Foundation

Test the foundational UI components:

1. **Dashboard:**
   - Verify widgets display correct data
   - Check responsive design on different screen sizes

2. **Navigation:**
   - Test main menu navigation
   - Verify breadcrumbs work correctly
   - Test role-based menu visibility

3. **User Interface:**
   - Test login/logout functionality
   - Verify forms validation
   - Test responsive design on mobile devices

4. **Driver Management:**
   - Navigate to Drivers section
   - Add a new driver
   - Edit driver details
   - View driver's assigned vehicles

### Phase 4: Feature Implementation

Test the advanced features implemented in Phase 4:

1. **Vehicle Tracking & Monitoring:**
   - View real-time vehicle status on dashboard
   - Test geofencing functionality
     - Create a new geofence area
     - Assign to vehicles
     - Test alerts when vehicles enter/exit
   - View vehicle history tracking
   - Test maintenance tracking
     - Create maintenance schedules
     - View maintenance history

2. **Trip Management:**
   - Create a trip with multiple waypoints
   - Test route optimization
   - View trip analytics
   - Test trip playback functionality

3. **Mapping & Geospatial Features:**
   - Test map visualization of vehicles
   - Toggle between map providers
   - Create and manage geofences
   - Test location-based alerts
   - Create and manage points of interest

4. **Analytics & Reporting:**
   - Generate vehicle performance reports
   - View driver performance metrics
   - Test vendor performance analytics
   - Create custom reports
   - Schedule automated reports
   - Export reports in different formats

5. **Inventory & Financial:**
   - Test inventory management
     - Add inventory items
     - Track inventory levels
     - Generate inventory reports
   - Test purchase management
     - Create purchase orders
     - Track order status
   - Test payment processing
     - Record payments
     - Generate invoices
   - View financial reports

## Troubleshooting

### Database Connection Issues

If you encounter database connection problems:

1. **Verify PostgreSQL is running:**
   - Check Windows Services (Win+R, type "services.msc")
   - Look for "postgresql-x64-14" (or your version) and ensure it's running

2. **Check database connection in pgAdmin:**
   - Open pgAdmin 4
   - Verify you can connect to your PostgreSQL server
   - Check if the fleetvigil database exists and is accessible
   - Verify the fleetvigil_user has proper permissions

3. **Check connection settings in `src\api\config.json`:**
   - Make sure the host, port, username, password, and database name match your pgAdmin settings

4. **Set proper permissions using pgAdmin:**
   - In pgAdmin, right-click on the fleetvigil database
   - Select "Properties"
   - Go to the "Security" tab
   - Ensure fleetvigil_user has appropriate privileges (CONNECT, USAGE, etc.)

5. **Check Windows Firewall settings for PostgreSQL**

### API Connection Issues

If the frontend cannot connect to the API:

1. Ensure the API server is running
2. Check the API URL in `src\frontend\FleetTracking\appsettings.json`
3. Verify Windows Firewall settings allow connections on port 8080
4. Run API with verbose logging to debug connection issues:
   ```powershell
   go run main.go -v
   ```

### Frontend Issues

If the frontend doesn't load correctly:

1. Check for errors in the browser console
2. Verify .NET SDK is properly installed:
   ```powershell
   dotnet --info
   ```
3. Restart the application with:
   ```powershell
   dotnet run --environment Development
   ```

### .NET SDK Not Found

If you see errors about .NET SDK not being found:

1. Download and install the .NET 6.0 SDK from [Microsoft's .NET download page](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
2. After installation, restart your PowerShell terminal
3. Verify the installation was successful:
   ```powershell
   dotnet --version
   ```
4. Make sure the installation directory is in your PATH environment variable

### Authentication Issues

If you cannot log in:

1. Verify the database has been properly seeded with the admin user
2. Reset the admin password using pgAdmin:
   - Open pgAdmin 4
   - Connect to your PostgreSQL server
   - Expand fleetvigil > Schemas > public > Tables
   - Right-click on the users table and select "View/Edit Data" > "All Rows"
   - Find the admin@fleetvigil.com user
   - Update the password_hash field with a new value
   - Click the "Save" button in the toolbar

For additional assistance, refer to the project documentation in the `docs\` directory. 