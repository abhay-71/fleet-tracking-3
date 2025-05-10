package postgres

import (
	"context"
	"database/sql"
	"errors"
	"fmt"
	"strings"
	"time"

	"fleetvigil.com/api/internal/models"
	"github.com/jmoiron/sqlx"
)

// VehicleRepo implements the VehicleRepository interface for PostgreSQL
type VehicleRepo struct {
	db *sqlx.DB
}

// NewVehicleRepository creates a new VehicleRepo
func NewVehicleRepository(db *sqlx.DB) *VehicleRepo {
	return &VehicleRepo{
		db: db,
	}
}

// Create creates a new vehicle
func (r *VehicleRepo) Create(ctx context.Context, vehicle *models.Vehicle) error {
	query := `
		INSERT INTO vehicles (
			company_id, vehicle_type_id, registration, make, model, year, color, vin, 
			fuel_type, fuel_capacity, current_fuel_level, odometer_reading, status,
			last_service_date, next_service_date, created_at, updated_at
		) VALUES (
			$1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14, $15, NOW(), NOW()
		) RETURNING id, created_at, updated_at`

	err := r.db.QueryRowxContext(
		ctx,
		query,
		vehicle.CompanyID,
		vehicle.VehicleTypeID,
		vehicle.Registration,
		vehicle.Make,
		vehicle.Model,
		vehicle.Year,
		vehicle.Color,
		vehicle.VIN,
		vehicle.FuelType,
		vehicle.FuelCapacity,
		vehicle.CurrentFuelLevel,
		vehicle.OdometerReading,
		vehicle.Status,
		vehicle.LastServiceDate,
		vehicle.NextServiceDate,
	).StructScan(vehicle)

	if err != nil {
		if strings.Contains(err.Error(), "duplicate key") {
			if strings.Contains(err.Error(), "vehicles_registration_key") {
				return fmt.Errorf("registration number already exists: %v", err)
			}
			if strings.Contains(err.Error(), "vehicles_vin_key") {
				return fmt.Errorf("VIN already exists: %v", err)
			}
		}
		return fmt.Errorf("failed to create vehicle: %v", err)
	}

	return nil
}

// GetByID gets a vehicle by ID
func (r *VehicleRepo) GetByID(ctx context.Context, id int) (*models.Vehicle, error) {
	vehicle := &models.Vehicle{}
	query := `
		SELECT id, company_id, vehicle_type_id, registration, make, model, year, color, vin, 
			   fuel_type, fuel_capacity, current_fuel_level, odometer_reading, status,
			   last_service_date, next_service_date, created_at, updated_at
		FROM vehicles
		WHERE id = $1 AND status != 'deleted'`

	err := r.db.QueryRowxContext(ctx, query, id).StructScan(vehicle)
	if err != nil {
		if errors.Is(err, sql.ErrNoRows) {
			return nil, fmt.Errorf("vehicle not found: %v", err)
		}
		return nil, fmt.Errorf("failed to get vehicle: %v", err)
	}

	return vehicle, nil
}

// GetByRegistration gets a vehicle by registration number
func (r *VehicleRepo) GetByRegistration(ctx context.Context, registration string) (*models.Vehicle, error) {
	vehicle := &models.Vehicle{}
	query := `
		SELECT id, company_id, vehicle_type_id, registration, make, model, year, color, vin, 
			   fuel_type, fuel_capacity, current_fuel_level, odometer_reading, status,
			   last_service_date, next_service_date, created_at, updated_at
		FROM vehicles
		WHERE registration = $1 AND status != 'deleted'`

	err := r.db.QueryRowxContext(ctx, query, registration).StructScan(vehicle)
	if err != nil {
		if errors.Is(err, sql.ErrNoRows) {
			return nil, fmt.Errorf("vehicle not found: %v", err)
		}
		return nil, fmt.Errorf("failed to get vehicle by registration: %v", err)
	}

	return vehicle, nil
}

// Update updates a vehicle
func (r *VehicleRepo) Update(ctx context.Context, vehicle *models.Vehicle) error {
	query := `
		UPDATE vehicles
		SET company_id = $1, vehicle_type_id = $2, registration = $3, make = $4, model = $5,
			year = $6, color = $7, vin = $8, fuel_type = $9, fuel_capacity = $10,
			current_fuel_level = $11, odometer_reading = $12, status = $13,
			last_service_date = $14, next_service_date = $15, updated_at = NOW()
		WHERE id = $16
		RETURNING updated_at`

	err := r.db.QueryRowxContext(
		ctx,
		query,
		vehicle.CompanyID,
		vehicle.VehicleTypeID,
		vehicle.Registration,
		vehicle.Make,
		vehicle.Model,
		vehicle.Year,
		vehicle.Color,
		vehicle.VIN,
		vehicle.FuelType,
		vehicle.FuelCapacity,
		vehicle.CurrentFuelLevel,
		vehicle.OdometerReading,
		vehicle.Status,
		vehicle.LastServiceDate,
		vehicle.NextServiceDate,
		vehicle.ID,
	).Scan(&vehicle.UpdatedAt)

	if err != nil {
		if errors.Is(err, sql.ErrNoRows) {
			return fmt.Errorf("vehicle not found: %v", err)
		}
		if strings.Contains(err.Error(), "duplicate key") {
			if strings.Contains(err.Error(), "vehicles_registration_key") {
				return fmt.Errorf("registration number already exists: %v", err)
			}
			if strings.Contains(err.Error(), "vehicles_vin_key") {
				return fmt.Errorf("VIN already exists: %v", err)
			}
		}
		return fmt.Errorf("failed to update vehicle: %v", err)
	}

	return nil
}

// Delete soft deletes a vehicle
func (r *VehicleRepo) Delete(ctx context.Context, id int) error {
	query := `
		UPDATE vehicles
		SET status = 'deleted', updated_at = NOW()
		WHERE id = $1`

	result, err := r.db.ExecContext(ctx, query, id)
	if err != nil {
		return fmt.Errorf("failed to delete vehicle: %v", err)
	}

	rowsAffected, err := result.RowsAffected()
	if err != nil {
		return fmt.Errorf("failed to get rows affected: %v", err)
	}

	if rowsAffected == 0 {
		return fmt.Errorf("vehicle not found")
	}

	return nil
}

// List lists vehicles with pagination
func (r *VehicleRepo) List(ctx context.Context, page, limit int) ([]*models.Vehicle, int, error) {
	offset := (page - 1) * limit

	// Get total count
	var total int
	countQuery := `
		SELECT COUNT(*)
		FROM vehicles
		WHERE status != 'deleted'`

	err := r.db.QueryRowContext(ctx, countQuery).Scan(&total)
	if err != nil {
		return nil, 0, fmt.Errorf("failed to count vehicles: %v", err)
	}

	// Get vehicles
	query := `
		SELECT id, company_id, vehicle_type_id, registration, make, model, year, color, vin, 
			   fuel_type, fuel_capacity, current_fuel_level, odometer_reading, status,
			   last_service_date, next_service_date, created_at, updated_at
		FROM vehicles
		WHERE status != 'deleted'
		ORDER BY id
		LIMIT $1 OFFSET $2`

	rows, err := r.db.QueryxContext(ctx, query, limit, offset)
	if err != nil {
		return nil, 0, fmt.Errorf("failed to list vehicles: %v", err)
	}
	defer rows.Close()

	vehicles := make([]*models.Vehicle, 0)
	for rows.Next() {
		var vehicle models.Vehicle
		if err := rows.StructScan(&vehicle); err != nil {
			return nil, 0, fmt.Errorf("failed to scan vehicle: %v", err)
		}
		vehicles = append(vehicles, &vehicle)
	}

	if err := rows.Err(); err != nil {
		return nil, 0, fmt.Errorf("error iterating vehicle rows: %v", err)
	}

	return vehicles, total, nil
}

// ListByCompany lists vehicles for a company with pagination
func (r *VehicleRepo) ListByCompany(ctx context.Context, companyID, page, limit int) ([]*models.Vehicle, int, error) {
	offset := (page - 1) * limit

	// Get total count
	var total int
	countQuery := `
		SELECT COUNT(*)
		FROM vehicles
		WHERE company_id = $1 AND status != 'deleted'`

	err := r.db.QueryRowContext(ctx, countQuery, companyID).Scan(&total)
	if err != nil {
		return nil, 0, fmt.Errorf("failed to count vehicles: %v", err)
	}

	// Get vehicles
	query := `
		SELECT id, company_id, vehicle_type_id, registration, make, model, year, color, vin, 
			   fuel_type, fuel_capacity, current_fuel_level, odometer_reading, status,
			   last_service_date, next_service_date, created_at, updated_at
		FROM vehicles
		WHERE company_id = $1 AND status != 'deleted'
		ORDER BY id
		LIMIT $2 OFFSET $3`

	rows, err := r.db.QueryxContext(ctx, query, companyID, limit, offset)
	if err != nil {
		return nil, 0, fmt.Errorf("failed to list vehicles: %v", err)
	}
	defer rows.Close()

	vehicles := make([]*models.Vehicle, 0)
	for rows.Next() {
		var vehicle models.Vehicle
		if err := rows.StructScan(&vehicle); err != nil {
			return nil, 0, fmt.Errorf("failed to scan vehicle: %v", err)
		}
		vehicles = append(vehicles, &vehicle)
	}

	if err := rows.Err(); err != nil {
		return nil, 0, fmt.Errorf("error iterating vehicle rows: %v", err)
	}

	return vehicles, total, nil
}

// UpdateStatus updates the status of a vehicle
func (r *VehicleRepo) UpdateStatus(ctx context.Context, id int, status string) error {
	query := `
		UPDATE vehicles
		SET status = $1, updated_at = NOW()
		WHERE id = $2
		RETURNING updated_at`

	var updatedAt time.Time
	err := r.db.QueryRowContext(ctx, query, status, id).Scan(&updatedAt)
	if err != nil {
		if errors.Is(err, sql.ErrNoRows) {
			return fmt.Errorf("vehicle not found: %v", err)
		}
		return fmt.Errorf("failed to update vehicle status: %v", err)
	}

	return nil
}
