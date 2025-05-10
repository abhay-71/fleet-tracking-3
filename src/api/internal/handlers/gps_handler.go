package handlers

import (
	"context"
	"encoding/json"
	"fmt"
	"math"
	"net/http"
	"time"

	"fleetvigil.com/api/internal/models"
	"fleetvigil.com/api/pkg/database"
)

// GPSDataHandler handles GPS data related requests
type GPSDataHandler struct {
	db *database.DB
}

// NewGPSDataHandler creates a new GPSDataHandler
func NewGPSDataHandler(db *database.DB) *GPSDataHandler {
	return &GPSDataHandler{db: db}
}

// GPSDataPoint represents a single GPS data point
type GPSDataPoint struct {
	VehicleID      int             `json:"vehicle_id"`
	Latitude       float64         `json:"latitude"`
	Longitude      float64         `json:"longitude"`
	Altitude       float64         `json:"altitude"`
	Speed          float64         `json:"speed"`
	Heading        float64         `json:"heading"`
	Timestamp      time.Time       `json:"timestamp"`
	TripID         *int            `json:"trip_id,omitempty"`
	FuelLevel      *float64        `json:"fuel_level,omitempty"`
	EngineStatus   *string         `json:"engine_status,omitempty"`
	IgnitionStatus *string         `json:"ignition_status,omitempty"`
	AdditionalData json.RawMessage `json:"additional_data,omitempty"`
}

// IngestGPSData handles POST /gps/data for individual GPS data points
func (h *GPSDataHandler) IngestGPSData(w http.ResponseWriter, r *http.Request) {
	// Parse request body
	var dataPoint GPSDataPoint
	if err := json.NewDecoder(r.Body).Decode(&dataPoint); err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid request body",
			Data:    nil,
		})
		return
	}

	// Validate GPS data
	if err := validateGPSData(dataPoint); err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: err.Error(),
			Data:    nil,
		})
		return
	}

	// Process the GPS data
	result, err := h.processGPSData(r.Context(), dataPoint)
	if err != nil {
		sendJSONResponse(w, http.StatusInternalServerError, Response{
			Status:  "error",
			Message: "Failed to process GPS data: " + err.Error(),
			Data:    nil,
		})
		return
	}

	// Prepare success response
	sendJSONResponse(w, http.StatusOK, Response{
		Status:  "success",
		Message: "GPS data processed successfully",
		Data:    result,
	})
}

// IngestBatchGPSData handles POST /gps/batch for batch GPS data points
func (h *GPSDataHandler) IngestBatchGPSData(w http.ResponseWriter, r *http.Request) {
	// Parse request body
	var dataPoints []GPSDataPoint
	if err := json.NewDecoder(r.Body).Decode(&dataPoints); err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid request body",
			Data:    nil,
		})
		return
	}

	// Validate each GPS data point
	for i, point := range dataPoints {
		if err := validateGPSData(point); err != nil {
			sendJSONResponse(w, http.StatusBadRequest, Response{
				Status:  "error",
				Message: fmt.Sprintf("Invalid data at index %d: %s", i, err.Error()),
				Data:    nil,
			})
			return
		}
	}

	results := make([]interface{}, 0, len(dataPoints))
	for _, point := range dataPoints {
		// Process the GPS data
		result, err := h.processGPSData(r.Context(), point)
		if err != nil {
			// Log error but continue processing other points
			// In a real implementation, you might want to handle errors differently
			fmt.Printf("Error processing GPS data for vehicle %d: %s\n", point.VehicleID, err.Error())
			continue
		}
		results = append(results, result)
	}

	// Prepare success response
	sendJSONResponse(w, http.StatusOK, Response{
		Status:  "success",
		Message: fmt.Sprintf("%d GPS data points processed successfully", len(results)),
		Data:    results,
	})
}

// validateGPSData validates GPS data points
func validateGPSData(data GPSDataPoint) error {
	// Check required fields
	if data.VehicleID <= 0 {
		return fmt.Errorf("vehicle_id is required and must be positive")
	}

	// Validate latitude (-90 to 90)
	if data.Latitude < -90 || data.Latitude > 90 {
		return fmt.Errorf("latitude must be between -90 and 90")
	}

	// Validate longitude (-180 to 180)
	if data.Longitude < -180 || data.Longitude > 180 {
		return fmt.Errorf("longitude must be between -180 and 180")
	}

	// Validate speed (non-negative)
	if data.Speed < 0 {
		return fmt.Errorf("speed must be non-negative")
	}

	// Validate heading (0 to 360)
	if data.Heading < 0 || data.Heading > 360 {
		return fmt.Errorf("heading must be between 0 and 360")
	}

	// Validate timestamp (not future)
	if data.Timestamp.After(time.Now().Add(5 * time.Minute)) {
		return fmt.Errorf("timestamp cannot be in the future")
	}

	// Validate optional fields if present
	if data.FuelLevel != nil && (*data.FuelLevel < 0 || *data.FuelLevel > 100) {
		return fmt.Errorf("fuel_level must be between 0 and 100")
	}

	return nil
}

// processGPSData processes a GPS data point
func (h *GPSDataHandler) processGPSData(ctx context.Context, data GPSDataPoint) (interface{}, error) {
	// 1. Transform data to vehicle status model
	vehicleStatus := &models.VehicleStatus{
		VehicleID: data.VehicleID,
		TripID:    data.TripID,
		Latitude:  data.Latitude,
		Longitude: data.Longitude,
		Speed:     data.Speed,
		Heading:   data.Heading,
		Timestamp: data.Timestamp,
		CreatedAt: time.Now(),
	}

	// Add optional fields if present
	if data.FuelLevel != nil {
		vehicleStatus.FuelLevel = *data.FuelLevel
	}
	if data.EngineStatus != nil {
		vehicleStatus.EngineStatus = *data.EngineStatus
	}
	if data.IgnitionStatus != nil {
		vehicleStatus.IgnitionStatus = *data.IgnitionStatus
	}

	// 2. Store the vehicle status
	// In a real implementation, this would use a service to store the data
	// For now, we'll simulate storage

	// 3. Process geofence checks
	// This will be implemented in later tasks

	// 4. Calculate derived data (e.g., distance)
	// For this implementation, we'll return the processed data
	// In a real implementation, this would update trip statistics, etc.

	return vehicleStatus, nil
}

// calculateDistance calculates the distance between two points using the Haversine formula
func calculateDistance(lat1, lon1, lat2, lon2 float64) float64 {
	// Convert latitude and longitude from degrees to radians
	lat1Rad := lat1 * math.Pi / 180
	lon1Rad := lon1 * math.Pi / 180
	lat2Rad := lat2 * math.Pi / 180
	lon2Rad := lon2 * math.Pi / 180

	// Haversine formula
	dLat := lat2Rad - lat1Rad
	dLon := lon2Rad - lon1Rad
	a := math.Sin(dLat/2)*math.Sin(dLat/2) + math.Cos(lat1Rad)*math.Cos(lat2Rad)*math.Sin(dLon/2)*math.Sin(dLon/2)
	c := 2 * math.Atan2(math.Sqrt(a), math.Sqrt(1-a))
	distance := 6371 * c // Earth radius in kilometers

	return distance
}

// calculateHeading calculates the compass heading from point 1 to point 2
func calculateHeading(lat1, lon1, lat2, lon2 float64) float64 {
	// Convert latitude and longitude from degrees to radians
	lat1Rad := lat1 * math.Pi / 180
	lon1Rad := lon1 * math.Pi / 180
	lat2Rad := lat2 * math.Pi / 180
	lon2Rad := lon2 * math.Pi / 180

	// Calculate heading
	dLon := lon2Rad - lon1Rad
	y := math.Sin(dLon) * math.Cos(lat2Rad)
	x := math.Cos(lat1Rad)*math.Sin(lat2Rad) - math.Sin(lat1Rad)*math.Cos(lat2Rad)*math.Cos(dLon)
	heading := math.Atan2(y, x) * 180 / math.Pi

	// Normalize to 0-360
	if heading < 0 {
		heading += 360
	}

	return heading
}
