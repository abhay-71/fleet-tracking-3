package handlers

import (
	"encoding/json"
	"fmt"
	"net/http"
	"strconv"
	"strings"
	"time"

	"fleetvigil.com/api/internal/models"
	"fleetvigil.com/api/pkg/database"
)

// GeofenceHandler handles geofence-related requests
type GeofenceHandler struct {
	db *database.DB
}

// NewGeofenceHandler creates a new GeofenceHandler
func NewGeofenceHandler(db *database.DB) *GeofenceHandler {
	return &GeofenceHandler{db: db}
}

// ListGeofences handles GET /geofences
func (h *GeofenceHandler) ListGeofences(w http.ResponseWriter, r *http.Request) {
	// Extract pagination parameters
	page, limit := getPaginationParams(r)

	// Extract filter parameters
	companyID := r.URL.Query().Get("company_id")
	category := r.URL.Query().Get("category")
	active := r.URL.Query().Get("active")

	// In a real implementation, fetch geofences based on filters
	// For now, return a placeholder list
	geofences := []models.Geofence{
		{
			ID:          1,
			CompanyID:   1,
			Name:        "Warehouse Zone",
			Description: "Main warehouse perimeter",
			Type:        models.GeofenceTypePolygon,
			Category:    models.GeofenceCategoryWarehouse,
			Color:       "#FF5733",
			Active:      true,
			CreatedAt:   time.Now().Add(-24 * time.Hour),
			UpdatedAt:   time.Now(),
			CreatedBy:   1,
		},
		{
			ID:          2,
			CompanyID:   1,
			Name:        "Customer Site Alpha",
			Description: "Alpha customer delivery area",
			Type:        models.GeofenceTypeCircle,
			Category:    models.GeofenceCategoryCustomer,
			Color:       "#33FF57",
			Active:      true,
			CreatedAt:   time.Now().Add(-48 * time.Hour),
			UpdatedAt:   time.Now(),
			CreatedBy:   1,
		},
	}

	// Create pagination info
	pagination := models.Pagination{
		Page:  page,
		Limit: limit,
		Total: len(geofences),
	}

	// Create response
	response := Response{
		Status:  "success",
		Message: "Geofences retrieved successfully",
		Data: models.ListResponse{
			Data:       geofences,
			Pagination: pagination,
		},
	}

	sendJSONResponse(w, http.StatusOK, response)
}

// GetGeofence handles GET /geofences/{id}
func (h *GeofenceHandler) GetGeofence(w http.ResponseWriter, r *http.Request) {
	// Extract geofence ID from path
	path := r.URL.Path
	parts := strings.Split(path, "/")
	if len(parts) < 3 {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid geofence ID",
			Data:    nil,
		})
		return
	}
	idStr := parts[len(parts)-1]
	id, err := strconv.Atoi(idStr)
	if err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid geofence ID",
			Data:    nil,
		})
		return
	}

	// In a real implementation, fetch the geofence from database
	// For now, return a placeholder
	geofence := models.Geofence{
		ID:          id,
		CompanyID:   1,
		Name:        "Warehouse Zone",
		Description: "Main warehouse perimeter",
		Type:        models.GeofenceTypePolygon,
		Category:    models.GeofenceCategoryWarehouse,
		Color:       "#FF5733",
		Coordinates: "[[34.0522, -118.2437], [34.0508, -118.2437], [34.0508, -118.2423], [34.0522, -118.2423]]",
		Active:      true,
		CreatedAt:   time.Now().Add(-24 * time.Hour),
		UpdatedAt:   time.Now(),
		CreatedBy:   1,
	}

	// Create response
	response := Response{
		Status:  "success",
		Message: "Geofence retrieved successfully",
		Data:    geofence,
	}

	sendJSONResponse(w, http.StatusOK, response)
}

// CreateGeofence handles POST /geofences
func (h *GeofenceHandler) CreateGeofence(w http.ResponseWriter, r *http.Request) {
	// Parse request body
	var geofence models.Geofence
	if err := json.NewDecoder(r.Body).Decode(&geofence); err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid request body",
			Data:    nil,
		})
		return
	}

	// Validate geofence data
	if err := validateGeofence(geofence); err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: err.Error(),
			Data:    nil,
		})
		return
	}

	// In a real implementation, save to database
	// For now, simulate database operation
	geofence.ID = 1 // Simulate ID assignment
	geofence.CreatedAt = time.Now()
	geofence.UpdatedAt = time.Now()

	// Create response
	response := Response{
		Status:  "success",
		Message: "Geofence created successfully",
		Data:    geofence,
	}

	sendJSONResponse(w, http.StatusCreated, response)
}

// UpdateGeofence handles PUT /geofences/{id}
func (h *GeofenceHandler) UpdateGeofence(w http.ResponseWriter, r *http.Request) {
	// Extract geofence ID from path
	path := r.URL.Path
	parts := strings.Split(path, "/")
	if len(parts) < 3 {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid geofence ID",
			Data:    nil,
		})
		return
	}
	idStr := parts[len(parts)-1]
	id, err := strconv.Atoi(idStr)
	if err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid geofence ID",
			Data:    nil,
		})
		return
	}

	// Parse request body
	var geofence models.Geofence
	if err := json.NewDecoder(r.Body).Decode(&geofence); err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid request body",
			Data:    nil,
		})
		return
	}

	// Set ID from path
	geofence.ID = id

	// Validate geofence data
	if err := validateGeofence(geofence); err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: err.Error(),
			Data:    nil,
		})
		return
	}

	// In a real implementation, update in database
	// For now, simulate database operation
	geofence.UpdatedAt = time.Now()

	// Create response
	response := Response{
		Status:  "success",
		Message: "Geofence updated successfully",
		Data:    geofence,
	}

	sendJSONResponse(w, http.StatusOK, response)
}

// DeleteGeofence handles DELETE /geofences/{id}
func (h *GeofenceHandler) DeleteGeofence(w http.ResponseWriter, r *http.Request) {
	// Extract geofence ID from path
	path := r.URL.Path
	parts := strings.Split(path, "/")
	if len(parts) < 3 {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid geofence ID",
			Data:    nil,
		})
		return
	}
	idStr := parts[len(parts)-1]
	id, err := strconv.Atoi(idStr)
	if err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid geofence ID",
			Data:    nil,
		})
		return
	}

	// In a real implementation, delete from database
	// For now, just return success

	// Create response
	response := Response{
		Status:  "success",
		Message: fmt.Sprintf("Geofence with ID %d deleted successfully", id),
		Data:    nil,
	}

	sendJSONResponse(w, http.StatusOK, response)
}

// CheckPoint handles POST /geofences/check to check if a point is within any geofences
func (h *GeofenceHandler) CheckPoint(w http.ResponseWriter, r *http.Request) {
	// Parse request body
	var request struct {
		Latitude  float64 `json:"latitude"`
		Longitude float64 `json:"longitude"`
		VehicleID int     `json:"vehicle_id"`
		TripID    *int    `json:"trip_id,omitempty"`
		Timestamp string  `json:"timestamp,omitempty"`
	}

	if err := json.NewDecoder(r.Body).Decode(&request); err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid request body",
			Data:    nil,
		})
		return
	}

	// Validate request data
	if request.Latitude < -90 || request.Latitude > 90 {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Latitude must be between -90 and 90",
			Data:    nil,
		})
		return
	}

	if request.Longitude < -180 || request.Longitude > 180 {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Longitude must be between -180 and 180",
			Data:    nil,
		})
		return
	}

	// In a real implementation, check against all geofences in database
	// For this example, we'll check against some hardcoded geofences

	// For example, a circular geofence
	circleCenter := struct {
		Lat float64
		Lng float64
	}{34.0522, -118.2437}
	circleRadius := 0.5 // kilometers

	// Check if point is in circle using Haversine formula
	distance := calculateDistance(
		request.Latitude, request.Longitude,
		circleCenter.Lat, circleCenter.Lng,
	)

	inCircle := distance <= circleRadius

	// Return the results
	result := struct {
		InGeofences     bool    `json:"in_geofences"`
		GeofenceMatches []int   `json:"geofence_matches,omitempty"`
		VehicleID       int     `json:"vehicle_id"`
		Latitude        float64 `json:"latitude"`
		Longitude       float64 `json:"longitude"`
	}{
		InGeofences: inCircle,
		VehicleID:   request.VehicleID,
		Latitude:    request.Latitude,
		Longitude:   request.Longitude,
	}

	if inCircle {
		result.GeofenceMatches = []int{2} // Hardcoded ID of our example circle
	}

	// Create response
	response := Response{
		Status:  "success",
		Message: "Point checked against geofences",
		Data:    result,
	}

	sendJSONResponse(w, http.StatusOK, response)
}

// BatchCheck handles POST /geofences/batch-check to check multiple points against geofences
func (h *GeofenceHandler) BatchCheck(w http.ResponseWriter, r *http.Request) {
	// Parse request body
	var request []struct {
		Latitude  float64 `json:"latitude"`
		Longitude float64 `json:"longitude"`
		VehicleID int     `json:"vehicle_id"`
		TripID    *int    `json:"trip_id,omitempty"`
		Timestamp string  `json:"timestamp,omitempty"`
	}

	if err := json.NewDecoder(r.Body).Decode(&request); err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid request body",
			Data:    nil,
		})
		return
	}

	// Process each point
	results := make([]interface{}, 0, len(request))
	for _, point := range request {
		// In a real implementation, check against geofences in database
		// For this example, let's just check against our hardcoded circle
		circleCenter := struct {
			Lat float64
			Lng float64
		}{34.0522, -118.2437}
		circleRadius := 0.5 // kilometers

		distance := calculateDistance(
			point.Latitude, point.Longitude,
			circleCenter.Lat, circleCenter.Lng,
		)

		inCircle := distance <= circleRadius

		result := struct {
			InGeofences     bool    `json:"in_geofences"`
			GeofenceMatches []int   `json:"geofence_matches,omitempty"`
			VehicleID       int     `json:"vehicle_id"`
			Latitude        float64 `json:"latitude"`
			Longitude       float64 `json:"longitude"`
		}{
			InGeofences: inCircle,
			VehicleID:   point.VehicleID,
			Latitude:    point.Latitude,
			Longitude:   point.Longitude,
		}

		if inCircle {
			result.GeofenceMatches = []int{2} // Hardcoded ID of our example circle
		}

		results = append(results, result)
	}

	// Create response
	response := Response{
		Status:  "success",
		Message: fmt.Sprintf("%d points checked against geofences", len(results)),
		Data:    results,
	}

	sendJSONResponse(w, http.StatusOK, response)
}

// validateGeofence validates a geofence
func validateGeofence(geofence models.Geofence) error {
	// Validate required fields
	if geofence.CompanyID <= 0 {
		return fmt.Errorf("company_id is required and must be positive")
	}

	if geofence.Name == "" {
		return fmt.Errorf("name is required")
	}

	// Validate type
	validTypes := map[models.GeofenceType]bool{
		models.GeofenceTypeCircle:    true,
		models.GeofenceTypePolygon:   true,
		models.GeofenceTypeRectangle: true,
	}
	if !validTypes[geofence.Type] {
		return fmt.Errorf("invalid geofence type: %s", geofence.Type)
	}

	// Validate category
	validCategories := map[models.GeofenceCategory]bool{
		models.GeofenceCategoryRestricted: true,
		models.GeofenceCategoryWarehouse:  true,
		models.GeofenceCategoryCustomer:   true,
		models.GeofenceCategoryPOI:        true,
		models.GeofenceCategoryCustom:     true,
	}
	if !validCategories[geofence.Category] {
		return fmt.Errorf("invalid geofence category: %s", geofence.Category)
	}

	// Validate coordinates based on type
	switch geofence.Type {
	case models.GeofenceTypeCircle:
		// For circle, we need center coordinates and radius
		if geofence.Radius == nil {
			return fmt.Errorf("radius is required for circle type")
		}
		if *geofence.Radius <= 0 {
			return fmt.Errorf("radius must be positive")
		}

		// Parse coordinates to ensure valid center point
		var circle models.Circle
		if err := json.Unmarshal([]byte(geofence.Coordinates), &circle); err != nil {
			return fmt.Errorf("invalid circle coordinates format")
		}
	case models.GeofenceTypePolygon:
		// For polygon, we need at least 3 vertices
		var polygon models.Polygon
		if err := json.Unmarshal([]byte(geofence.Coordinates), &polygon); err != nil {
			return fmt.Errorf("invalid polygon coordinates format")
		}
		if len(polygon.Vertices) < 3 {
			return fmt.Errorf("polygon must have at least 3 vertices")
		}
	case models.GeofenceTypeRectangle:
		// For rectangle, we need northeast and southwest corners
		var rectangle models.Rectangle
		if err := json.Unmarshal([]byte(geofence.Coordinates), &rectangle); err != nil {
			return fmt.Errorf("invalid rectangle coordinates format")
		}
	}

	return nil
}

// isPointInPolygon checks if a point is within a polygon using the ray casting algorithm
func isPointInPolygon(lat, lng float64, polygon [][]float64) bool {
	inside := false
	j := len(polygon) - 1

	for i := 0; i < len(polygon); i++ {
		// Check if point is on polygon edge
		if (polygon[i][0] == lat && polygon[i][1] == lng) ||
			(polygon[j][0] == lat && polygon[j][1] == lng) {
			return true
		}

		// Ray casting algorithm
		if ((polygon[i][0] > lat) != (polygon[j][0] > lat)) &&
			(lng < (polygon[j][1]-polygon[i][1])*(lat-polygon[i][0])/(polygon[j][0]-polygon[i][0])+polygon[i][1]) {
			inside = !inside
		}

		j = i
	}

	return inside
}

// isPointInCircle checks if a point is within a circular geofence
func isPointInCircle(lat, lng, centerLat, centerLng, radiusKm float64) bool {
	distance := calculateDistance(lat, lng, centerLat, centerLng)
	return distance <= radiusKm
}

// isPointInRectangle checks if a point is within a rectangular geofence
func isPointInRectangle(lat, lng, neLat, neLng, swLat, swLng float64) bool {
	return lat <= neLat && lat >= swLat && lng <= neLng && lng >= swLng
}
