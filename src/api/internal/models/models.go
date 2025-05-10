package models

import (
	"time"
)

// User represents a user in the system
type User struct {
	ID           int       `json:"id" db:"id"`
	Username     string    `json:"username" db:"username"`
	Email        string    `json:"email" db:"email"`
	PasswordHash string    `json:"-" db:"password_hash"`
	FirstName    string    `json:"first_name" db:"first_name"`
	LastName     string    `json:"last_name" db:"last_name"`
	Phone        string    `json:"phone" db:"phone"`
	Status       string    `json:"status" db:"status"`
	CreatedAt    time.Time `json:"created_at" db:"created_at"`
	UpdatedAt    time.Time `json:"updated_at" db:"updated_at"`
}

// Role represents a role in the system
type Role struct {
	ID          int       `json:"id" db:"id"`
	Name        string    `json:"name" db:"name"`
	Description string    `json:"description" db:"description"`
	CreatedAt   time.Time `json:"created_at" db:"created_at"`
	UpdatedAt   time.Time `json:"updated_at" db:"updated_at"`
}

// Permission represents a permission in the system
type Permission struct {
	ID          int       `json:"id" db:"id"`
	Name        string    `json:"name" db:"name"`
	Description string    `json:"description" db:"description"`
	CreatedAt   time.Time `json:"created_at" db:"created_at"`
	UpdatedAt   time.Time `json:"updated_at" db:"updated_at"`
}

// Company represents a company in the system
type Company struct {
	ID         int       `json:"id" db:"id"`
	Name       string    `json:"name" db:"name"`
	Address    string    `json:"address" db:"address"`
	City       string    `json:"city" db:"city"`
	State      string    `json:"state" db:"state"`
	Country    string    `json:"country" db:"country"`
	PostalCode string    `json:"postal_code" db:"postal_code"`
	Phone      string    `json:"phone" db:"phone"`
	Email      string    `json:"email" db:"email"`
	Website    string    `json:"website" db:"website"`
	LogoURL    string    `json:"logo_url" db:"logo_url"`
	Status     string    `json:"status" db:"status"`
	CreatedAt  time.Time `json:"created_at" db:"created_at"`
	UpdatedAt  time.Time `json:"updated_at" db:"updated_at"`
}

// VehicleType represents a type of vehicle
type VehicleType struct {
	ID          int       `json:"id" db:"id"`
	Name        string    `json:"name" db:"name"`
	Description string    `json:"description" db:"description"`
	CreatedAt   time.Time `json:"created_at" db:"created_at"`
	UpdatedAt   time.Time `json:"updated_at" db:"updated_at"`
}

// Vehicle represents a vehicle in the system
type Vehicle struct {
	ID               int       `json:"id" db:"id"`
	CompanyID        int       `json:"company_id" db:"company_id"`
	VehicleTypeID    int       `json:"vehicle_type_id" db:"vehicle_type_id"`
	Registration     string    `json:"registration" db:"registration"`
	Make             string    `json:"make" db:"make"`
	Model            string    `json:"model" db:"model"`
	Year             int       `json:"year" db:"year"`
	Color            string    `json:"color" db:"color"`
	VIN              string    `json:"vin" db:"vin"`
	FuelType         string    `json:"fuel_type" db:"fuel_type"`
	FuelCapacity     float64   `json:"fuel_capacity" db:"fuel_capacity"`
	CurrentFuelLevel float64   `json:"current_fuel_level" db:"current_fuel_level"`
	OdometerReading  float64   `json:"odometer_reading" db:"odometer_reading"`
	Status           string    `json:"status" db:"status"`
	LastServiceDate  time.Time `json:"last_service_date" db:"last_service_date"`
	NextServiceDate  time.Time `json:"next_service_date" db:"next_service_date"`
	CreatedAt        time.Time `json:"created_at" db:"created_at"`
	UpdatedAt        time.Time `json:"updated_at" db:"updated_at"`
}

// Driver represents a driver in the system
type Driver struct {
	ID                int       `json:"id" db:"id"`
	UserID            int       `json:"user_id" db:"user_id"`
	CompanyID         int       `json:"company_id" db:"company_id"`
	LicenseNumber     string    `json:"license_number" db:"license_number"`
	LicenseClass      string    `json:"license_class" db:"license_class"`
	LicenseExpiryDate time.Time `json:"license_expiry_date" db:"license_expiry_date"`
	Status            string    `json:"status" db:"status"`
	CreatedAt         time.Time `json:"created_at" db:"created_at"`
	UpdatedAt         time.Time `json:"updated_at" db:"updated_at"`
}

// Trip represents a trip in the system
type Trip struct {
	ID            int       `json:"id" db:"id"`
	VehicleID     int       `json:"vehicle_id" db:"vehicle_id"`
	DriverID      int       `json:"driver_id" db:"driver_id"`
	StartLocation string    `json:"start_location" db:"start_location"`
	EndLocation   string    `json:"end_location" db:"end_location"`
	StartTime     time.Time `json:"start_time" db:"start_time"`
	EndTime       time.Time `json:"end_time" db:"end_time"`
	Status        string    `json:"status" db:"status"`
	Distance      float64   `json:"distance" db:"distance"`
	FuelUsed      float64   `json:"fuel_used" db:"fuel_used"`
	AverageSpeed  float64   `json:"average_speed" db:"average_speed"`
	Notes         string    `json:"notes" db:"notes"`
	CreatedAt     time.Time `json:"created_at" db:"created_at"`
	UpdatedAt     time.Time `json:"updated_at" db:"updated_at"`
}

// Waypoint represents a waypoint in a trip
type Waypoint struct {
	ID        int       `json:"id" db:"id"`
	TripID    int       `json:"trip_id" db:"trip_id"`
	Latitude  float64   `json:"latitude" db:"latitude"`
	Longitude float64   `json:"longitude" db:"longitude"`
	Altitude  float64   `json:"altitude" db:"altitude"`
	Speed     float64   `json:"speed" db:"speed"`
	Heading   float64   `json:"heading" db:"heading"`
	Timestamp time.Time `json:"timestamp" db:"timestamp"`
	CreatedAt time.Time `json:"created_at" db:"created_at"`
}

// VehicleStatus represents the status of a vehicle at a point in time
type VehicleStatus struct {
	ID                int       `json:"id" db:"id"`
	VehicleID         int       `json:"vehicle_id" db:"vehicle_id"`
	TripID            *int      `json:"trip_id" db:"trip_id"`
	Latitude          float64   `json:"latitude" db:"latitude"`
	Longitude         float64   `json:"longitude" db:"longitude"`
	Speed             float64   `json:"speed" db:"speed"`
	Heading           float64   `json:"heading" db:"heading"`
	FuelLevel         float64   `json:"fuel_level" db:"fuel_level"`
	EngineStatus      string    `json:"engine_status" db:"engine_status"`
	IgnitionStatus    string    `json:"ignition_status" db:"ignition_status"`
	BatteryVoltage    float64   `json:"battery_voltage" db:"battery_voltage"`
	EngineTemperature float64   `json:"engine_temperature" db:"engine_temperature"`
	OilPressure       float64   `json:"oil_pressure" db:"oil_pressure"`
	OdometerReading   float64   `json:"odometer_reading" db:"odometer_reading"`
	Timestamp         time.Time `json:"timestamp" db:"timestamp"`
	CreatedAt         time.Time `json:"created_at" db:"created_at"`
}

// MaintenanceRecord represents a maintenance record for a vehicle
type MaintenanceRecord struct {
	ID              int       `json:"id" db:"id"`
	VehicleID       int       `json:"vehicle_id" db:"vehicle_id"`
	Type            string    `json:"type" db:"type"`
	Description     string    `json:"description" db:"description"`
	Cost            float64   `json:"cost" db:"cost"`
	OdometerReading float64   `json:"odometer_reading" db:"odometer_reading"`
	ServiceDate     time.Time `json:"service_date" db:"service_date"`
	NextServiceDate time.Time `json:"next_service_date" db:"next_service_date"`
	Notes           string    `json:"notes" db:"notes"`
	CreatedAt       time.Time `json:"created_at" db:"created_at"`
	UpdatedAt       time.Time `json:"updated_at" db:"updated_at"`
}

// Alert represents an alert in the system
type Alert struct {
	ID         int        `json:"id" db:"id"`
	VehicleID  int        `json:"vehicle_id" db:"vehicle_id"`
	TripID     *int       `json:"trip_id" db:"trip_id"`
	Type       string     `json:"type" db:"type"`
	Severity   string     `json:"severity" db:"severity"`
	Message    string     `json:"message" db:"message"`
	Latitude   float64    `json:"latitude" db:"latitude"`
	Longitude  float64    `json:"longitude" db:"longitude"`
	Status     string     `json:"status" db:"status"`
	Timestamp  time.Time  `json:"timestamp" db:"timestamp"`
	CreatedAt  time.Time  `json:"created_at" db:"created_at"`
	ResolvedAt *time.Time `json:"resolved_at" db:"resolved_at"`
}

// GeoZone represents a geographic zone for geofencing
type GeoZone struct {
	ID          int       `json:"id" db:"id"`
	CompanyID   int       `json:"company_id" db:"company_id"`
	Name        string    `json:"name" db:"name"`
	Description string    `json:"description" db:"description"`
	Type        string    `json:"type" db:"type"`
	Coordinates string    `json:"coordinates" db:"coordinates"`
	CreatedAt   time.Time `json:"created_at" db:"created_at"`
	UpdatedAt   time.Time `json:"updated_at" db:"updated_at"`
}

// Pagination represents pagination parameters
type Pagination struct {
	Page  int `json:"page"`
	Limit int `json:"limit"`
	Total int `json:"total"`
}

// ListResponse represents a paginated list response
type ListResponse struct {
	Data       interface{} `json:"data"`
	Pagination Pagination  `json:"pagination"`
}

// LoginCredentials represents login credentials
type LoginCredentials struct {
	Username string `json:"username"`
	Password string `json:"password"`
}

// UserRegistration represents user registration input
type UserRegistration struct {
	Username  string `json:"username" validate:"required,min=3,max=50"`
	Email     string `json:"email" validate:"required,email"`
	Password  string `json:"password" validate:"required,min=8"`
	FirstName string `json:"first_name" validate:"required"`
	LastName  string `json:"last_name" validate:"required"`
	Phone     string `json:"phone" validate:"required"`
}

// RefreshTokenRequest represents a refresh token request
type RefreshTokenRequest struct {
	RefreshToken string `json:"refresh_token" validate:"required"`
}
