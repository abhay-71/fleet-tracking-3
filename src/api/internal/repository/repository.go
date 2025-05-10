package repository

import (
	"context"

	"fleetvigil.com/api/internal/models"
)

// UserRepository defines the interface for user data access
type UserRepository interface {
	// Create creates a new user
	Create(ctx context.Context, user *models.User) error

	// GetByID gets a user by ID
	GetByID(ctx context.Context, id int) (*models.User, error)

	// GetByUsername gets a user by username
	GetByUsername(ctx context.Context, username string) (*models.User, error)

	// GetByEmail gets a user by email
	GetByEmail(ctx context.Context, email string) (*models.User, error)

	// Update updates a user
	Update(ctx context.Context, user *models.User) error

	// Delete deletes a user
	Delete(ctx context.Context, id int) error

	// List lists users with pagination
	List(ctx context.Context, page, limit int) ([]*models.User, int, error)

	// GetUserRoles gets the roles for a user
	GetUserRoles(ctx context.Context, userID int) ([]string, error)

	// AssignRole assigns a role to a user
	AssignRole(ctx context.Context, userID, roleID int) error

	// RemoveRole removes a role from a user
	RemoveRole(ctx context.Context, userID, roleID int) error
}

// RoleRepository defines the interface for role data access
type RoleRepository interface {
	// Create creates a new role
	Create(ctx context.Context, role *models.Role) error

	// GetByID gets a role by ID
	GetByID(ctx context.Context, id int) (*models.Role, error)

	// GetByName gets a role by name
	GetByName(ctx context.Context, name string) (*models.Role, error)

	// Update updates a role
	Update(ctx context.Context, role *models.Role) error

	// Delete deletes a role
	Delete(ctx context.Context, id int) error

	// List lists roles with pagination
	List(ctx context.Context, page, limit int) ([]*models.Role, int, error)

	// GetRolePermissions gets the permissions for a role
	GetRolePermissions(ctx context.Context, roleID int) ([]string, error)

	// AssignPermission assigns a permission to a role
	AssignPermission(ctx context.Context, roleID, permissionID int) error

	// RemovePermission removes a permission from a role
	RemovePermission(ctx context.Context, roleID, permissionID int) error
}

// VehicleRepository defines the interface for vehicle data access
type VehicleRepository interface {
	// Create creates a new vehicle
	Create(ctx context.Context, vehicle *models.Vehicle) error

	// GetByID gets a vehicle by ID
	GetByID(ctx context.Context, id int) (*models.Vehicle, error)

	// GetByRegistration gets a vehicle by registration number
	GetByRegistration(ctx context.Context, registration string) (*models.Vehicle, error)

	// Update updates a vehicle
	Update(ctx context.Context, vehicle *models.Vehicle) error

	// Delete deletes a vehicle
	Delete(ctx context.Context, id int) error

	// List lists vehicles with pagination
	List(ctx context.Context, page, limit int) ([]*models.Vehicle, int, error)

	// ListByCompany lists vehicles for a company with pagination
	ListByCompany(ctx context.Context, companyID, page, limit int) ([]*models.Vehicle, int, error)

	// UpdateStatus updates the status of a vehicle
	UpdateStatus(ctx context.Context, id int, status string) error
}

// DriverRepository defines the interface for driver data access
type DriverRepository interface {
	// Create creates a new driver
	Create(ctx context.Context, driver *models.Driver) error

	// GetByID gets a driver by ID
	GetByID(ctx context.Context, id int) (*models.Driver, error)

	// GetByUserID gets a driver by user ID
	GetByUserID(ctx context.Context, userID int) (*models.Driver, error)

	// Update updates a driver
	Update(ctx context.Context, driver *models.Driver) error

	// Delete deletes a driver
	Delete(ctx context.Context, id int) error

	// List lists drivers with pagination
	List(ctx context.Context, page, limit int) ([]*models.Driver, int, error)

	// ListByCompany lists drivers for a company with pagination
	ListByCompany(ctx context.Context, companyID, page, limit int) ([]*models.Driver, int, error)

	// UpdateStatus updates the status of a driver
	UpdateStatus(ctx context.Context, id int, status string) error
}

// TripRepository defines the interface for trip data access
type TripRepository interface {
	// Create creates a new trip
	Create(ctx context.Context, trip *models.Trip) error

	// GetByID gets a trip by ID
	GetByID(ctx context.Context, id int) (*models.Trip, error)

	// Update updates a trip
	Update(ctx context.Context, trip *models.Trip) error

	// Delete deletes a trip
	Delete(ctx context.Context, id int) error

	// List lists trips with pagination
	List(ctx context.Context, page, limit int) ([]*models.Trip, int, error)

	// ListByVehicle lists trips for a vehicle with pagination
	ListByVehicle(ctx context.Context, vehicleID, page, limit int) ([]*models.Trip, int, error)

	// ListByDriver lists trips for a driver with pagination
	ListByDriver(ctx context.Context, driverID, page, limit int) ([]*models.Trip, int, error)

	// ListActive lists active trips with pagination
	ListActive(ctx context.Context, page, limit int) ([]*models.Trip, int, error)

	// UpdateStatus updates the status of a trip
	UpdateStatus(ctx context.Context, id int, status string) error

	// StartTrip starts a trip
	StartTrip(ctx context.Context, id int, startTime string) error

	// EndTrip ends a trip
	EndTrip(ctx context.Context, id int, endTime string, distanceKm float64) error

	// AddWaypoint adds a waypoint to a trip
	AddWaypoint(ctx context.Context, waypoint *models.Waypoint) error

	// GetWaypoints gets the waypoints for a trip
	GetWaypoints(ctx context.Context, tripID int) ([]*models.Waypoint, error)
}

// CompanyRepository defines the interface for company data access
type CompanyRepository interface {
	// Create creates a new company
	Create(ctx context.Context, company *models.Company) error

	// GetByID gets a company by ID
	GetByID(ctx context.Context, id int) (*models.Company, error)

	// Update updates a company
	Update(ctx context.Context, company *models.Company) error

	// Delete deletes a company
	Delete(ctx context.Context, id int) error

	// List lists companies with pagination
	List(ctx context.Context, page, limit int) ([]*models.Company, int, error)
}

// VehicleStatusRepository defines the interface for vehicle status data access
type VehicleStatusRepository interface {
	// Create creates a new vehicle status
	Create(ctx context.Context, status *models.VehicleStatus) error

	// GetLatestByVehicle gets the latest status for a vehicle
	GetLatestByVehicle(ctx context.Context, vehicleID int) (*models.VehicleStatus, error)

	// GetHistoryByVehicle gets the status history for a vehicle
	GetHistoryByVehicle(ctx context.Context, vehicleID int, limit int) ([]*models.VehicleStatus, error)

	// GetByTrip gets the status updates for a trip
	GetByTrip(ctx context.Context, tripID int) ([]*models.VehicleStatus, error)

	// GetAll gets the latest status for all vehicles
	GetAll(ctx context.Context) ([]*models.VehicleStatus, error)
}
