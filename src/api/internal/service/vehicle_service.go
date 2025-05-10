package service

import (
	"context"
	"errors"
	"fmt"

	"fleetvigil.com/api/internal/models"
	"fleetvigil.com/api/internal/repository"
)

// VehicleSvc implements the VehicleService interface
type VehicleSvc struct {
	vehicleRepo repository.VehicleRepository
	companyRepo repository.CompanyRepository
}

// NewVehicleService creates a new VehicleSvc
func NewVehicleService(
	vehicleRepo repository.VehicleRepository,
	companyRepo repository.CompanyRepository,
) *VehicleSvc {
	return &VehicleSvc{
		vehicleRepo: vehicleRepo,
		companyRepo: companyRepo,
	}
}

// Create creates a new vehicle
func (s *VehicleSvc) Create(ctx context.Context, vehicle *models.Vehicle) error {
	// Check if company exists
	_, err := s.companyRepo.GetByID(ctx, vehicle.CompanyID)
	if err != nil {
		return fmt.Errorf("company not found: %v", err)
	}

	// Validate vehicle status
	if vehicle.Status == "" {
		vehicle.Status = "active"
	}

	return s.vehicleRepo.Create(ctx, vehicle)
}

// Get gets a vehicle by ID
func (s *VehicleSvc) Get(ctx context.Context, id int) (*models.Vehicle, error) {
	vehicle, err := s.vehicleRepo.GetByID(ctx, id)
	if err != nil {
		return nil, err
	}

	return vehicle, nil
}

// GetByRegistration gets a vehicle by registration number
func (s *VehicleSvc) GetByRegistration(ctx context.Context, registration string) (*models.Vehicle, error) {
	if registration == "" {
		return nil, errors.New("registration number cannot be empty")
	}

	vehicle, err := s.vehicleRepo.GetByRegistration(ctx, registration)
	if err != nil {
		return nil, err
	}

	return vehicle, nil
}

// Update updates a vehicle
func (s *VehicleSvc) Update(ctx context.Context, vehicle *models.Vehicle) error {
	// Check if vehicle exists
	existingVehicle, err := s.vehicleRepo.GetByID(ctx, vehicle.ID)
	if err != nil {
		return fmt.Errorf("vehicle not found: %v", err)
	}

	// Check if company exists
	_, err = s.companyRepo.GetByID(ctx, vehicle.CompanyID)
	if err != nil {
		return fmt.Errorf("company not found: %v", err)
	}

	// Validate vehicle status
	if vehicle.Status == "" {
		vehicle.Status = existingVehicle.Status
	}

	return s.vehicleRepo.Update(ctx, vehicle)
}

// Delete deletes a vehicle
func (s *VehicleSvc) Delete(ctx context.Context, id int) error {
	// Check if vehicle exists
	_, err := s.vehicleRepo.GetByID(ctx, id)
	if err != nil {
		return fmt.Errorf("vehicle not found: %v", err)
	}

	return s.vehicleRepo.Delete(ctx, id)
}

// List lists vehicles with pagination
func (s *VehicleSvc) List(ctx context.Context, page, limit int) ([]*models.Vehicle, int, error) {
	if page < 1 {
		page = 1
	}
	if limit < 1 {
		limit = 10
	}

	return s.vehicleRepo.List(ctx, page, limit)
}

// ListByCompany lists vehicles for a company with pagination
func (s *VehicleSvc) ListByCompany(ctx context.Context, companyID, page, limit int) ([]*models.Vehicle, int, error) {
	if page < 1 {
		page = 1
	}
	if limit < 1 {
		limit = 10
	}

	// Check if company exists
	_, err := s.companyRepo.GetByID(ctx, companyID)
	if err != nil {
		return nil, 0, fmt.Errorf("company not found: %v", err)
	}

	return s.vehicleRepo.ListByCompany(ctx, companyID, page, limit)
}

// UpdateStatus updates the status of a vehicle
func (s *VehicleSvc) UpdateStatus(ctx context.Context, id int, status string) error {
	// Check if vehicle exists
	_, err := s.vehicleRepo.GetByID(ctx, id)
	if err != nil {
		return fmt.Errorf("vehicle not found: %v", err)
	}

	// Validate status
	if status == "" {
		return errors.New("status cannot be empty")
	}

	// Only allow certain statuses
	validStatuses := map[string]bool{
		"active":         true,
		"inactive":       true,
		"maintenance":    true,
		"out_of_service": true,
		"deleted":        true,
	}

	if !validStatuses[status] {
		return errors.New("invalid status")
	}

	return s.vehicleRepo.UpdateStatus(ctx, id, status)
}
