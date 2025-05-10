package service

import (
	"context"

	"fleetvigil.com/api/internal/models"
)

// UserService defines the interface for user operations
type UserService interface {
	// Create creates a new user
	Create(ctx context.Context, user *models.User) error

	// Get gets a user by ID
	Get(ctx context.Context, id int) (*models.User, error)

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

	// GetRoles gets the roles for a user
	GetRoles(ctx context.Context, userID int) ([]string, error)

	// AssignRole assigns a role to a user
	AssignRole(ctx context.Context, userID, roleID int) error

	// RemoveRole removes a role from a user
	RemoveRole(ctx context.Context, userID, roleID int) error

	// Register registers a new user
	Register(ctx context.Context, registration *models.UserRegistration) (*models.User, error)

	// Login logs in a user
	Login(ctx context.Context, credentials *models.LoginCredentials) (string, string, error)

	// RefreshToken refreshes an authentication token
	RefreshToken(ctx context.Context, refreshToken string) (string, string, error)
}

// VehicleService defines the interface for vehicle operations
type VehicleService interface {
	// Create creates a new vehicle
	Create(ctx context.Context, vehicle *models.Vehicle) error

	// Get gets a vehicle by ID
	Get(ctx context.Context, id int) (*models.Vehicle, error)

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

// DriverService defines the interface for driver operations
type DriverService interface {
	// Create creates a new driver
	Create(ctx context.Context, driver *models.Driver) error

	// Get gets a driver by ID
	Get(ctx context.Context, id int) (*models.Driver, error)

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
}

// TripService defines the interface for trip operations
type TripService interface {
	// Create creates a new trip
	Create(ctx context.Context, trip *models.Trip) error

	// Get gets a trip by ID
	Get(ctx context.Context, id int) (*models.Trip, error)

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

	// StartTrip starts a trip
	StartTrip(ctx context.Context, id int) error

	// EndTrip ends a trip
	EndTrip(ctx context.Context, id int, distanceKm float64) error

	// AddWaypoint adds a waypoint to a trip
	AddWaypoint(ctx context.Context, tripID int, waypoint *models.Waypoint) error

	// GetWaypoints gets the waypoints for a trip
	GetWaypoints(ctx context.Context, tripID int) ([]*models.Waypoint, error)
}

// VehicleStatusService defines the interface for vehicle status operations
type VehicleStatusService interface {
	// Create creates a new vehicle status update
	Create(ctx context.Context, status *models.VehicleStatus) error

	// GetLatestByVehicle gets the latest status for a vehicle
	GetLatestByVehicle(ctx context.Context, vehicleID int) (*models.VehicleStatus, error)

	// GetHistoryByVehicle gets the status history for a vehicle
	GetHistoryByVehicle(ctx context.Context, vehicleID int, limit int) ([]*models.VehicleStatus, error)

	// GetAll gets the latest status for all vehicles
	GetAll(ctx context.Context) ([]*models.VehicleStatus, error)
}
