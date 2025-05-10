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

// TripHandler handles trip-related requests
type TripHandler struct {
	DB *database.DB
}

// NewTripHandler creates a new trip handler
func NewTripHandler(db *database.DB) *TripHandler {
	return &TripHandler{DB: db}
}

// ListTrips handles GET /trips
func (h *TripHandler) ListTrips(w http.ResponseWriter, r *http.Request) {
	// Extract pagination parameters from the query string
	page, limit := getPaginationParams(r)

	// In a real implementation, this would interact with a TripService
	// This is a placeholder for Phase 2
	trips := []models.Trip{}

	// Prepare response
	response := Response{
		Status:  "success",
		Message: "Trips retrieved successfully",
		Data: map[string]interface{}{
			"trips": trips,
			"page":  page,
			"limit": limit,
			"total": 0,
			"pages": 0,
		},
	}

	sendJSONResponse(w, http.StatusOK, response)
}

// GetTrip handles GET /trips/{id}
func (h *TripHandler) GetTrip(w http.ResponseWriter, r *http.Request) {
	// Extract trip ID from path
	path := r.URL.Path
	parts := strings.Split(path, "/")
	if len(parts) < 3 {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid trip ID",
			Data:    nil,
		})
		return
	}
	idStr := parts[len(parts)-1]
	id, err := strconv.Atoi(idStr)
	if err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid trip ID",
			Data:    nil,
		})
		return
	}

	// In a real implementation, this would fetch the trip from a TripService
	// This is a placeholder for Phase 2
	trip := models.Trip{
		ID:            id,
		VehicleID:     1,
		DriverID:      1,
		StartLocation: "Start Location",
		EndLocation:   "End Location",
		StartTime:     time.Now(),
		EndTime:       time.Now().Add(time.Hour),
		Status:        "scheduled",
		Distance:      15.5,
		Notes:         "Trip notes",
		CreatedAt:     time.Now(),
		UpdatedAt:     time.Now(),
	}

	// Prepare response
	response := Response{
		Status:  "success",
		Message: "Trip retrieved successfully",
		Data:    trip,
	}

	sendJSONResponse(w, http.StatusOK, response)
}

// CreateTrip handles POST /trips
func (h *TripHandler) CreateTrip(w http.ResponseWriter, r *http.Request) {
	// Parse request body
	var trip models.Trip
	if err := json.NewDecoder(r.Body).Decode(&trip); err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid request body",
			Data:    nil,
		})
		return
	}

	// Validate trip data
	if trip.VehicleID == 0 || trip.DriverID == 0 || trip.StartLocation == "" || trip.EndLocation == "" {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Missing required fields",
			Data:    nil,
		})
		return
	}

	// In a real implementation, this would create the trip using a TripService
	// This is a placeholder for Phase 2
	trip.ID = 1 // Simulate ID assignment
	trip.Status = "scheduled"
	trip.CreatedAt = time.Now()
	trip.UpdatedAt = time.Now()

	// Prepare response
	response := Response{
		Status:  "success",
		Message: "Trip created successfully",
		Data:    trip,
	}

	sendJSONResponse(w, http.StatusCreated, response)
}

// UpdateTrip handles PUT /trips/{id}
func (h *TripHandler) UpdateTrip(w http.ResponseWriter, r *http.Request) {
	// Extract trip ID from path
	path := r.URL.Path
	parts := strings.Split(path, "/")
	if len(parts) < 3 {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid trip ID",
			Data:    nil,
		})
		return
	}
	idStr := parts[len(parts)-1]
	id, err := strconv.Atoi(idStr)
	if err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid trip ID",
			Data:    nil,
		})
		return
	}

	// Parse request body
	var trip models.Trip
	if err := json.NewDecoder(r.Body).Decode(&trip); err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid request body",
			Data:    nil,
		})
		return
	}

	// Ensure trip ID matches path parameter
	if trip.ID != 0 && trip.ID != id {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Trip ID mismatch",
			Data:    nil,
		})
		return
	}
	trip.ID = id

	// In a real implementation, this would update the trip using a TripService
	// This is a placeholder for Phase 2
	trip.UpdatedAt = time.Now()

	// Prepare response
	response := Response{
		Status:  "success",
		Message: "Trip updated successfully",
		Data:    trip,
	}

	sendJSONResponse(w, http.StatusOK, response)
}

// DeleteTrip handles DELETE /trips/{id}
func (h *TripHandler) DeleteTrip(w http.ResponseWriter, r *http.Request) {
	// Extract trip ID from path
	path := r.URL.Path
	parts := strings.Split(path, "/")
	if len(parts) < 3 {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid trip ID",
			Data:    nil,
		})
		return
	}
	idStr := parts[len(parts)-1]
	id, err := strconv.Atoi(idStr)
	if err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid trip ID",
			Data:    nil,
		})
		return
	}

	// In a real implementation, this would delete the trip using a TripService
	// This is a placeholder for Phase 2

	// Prepare response
	response := Response{
		Status:  "success",
		Message: fmt.Sprintf("Trip with ID %d deleted successfully", id),
		Data:    nil,
	}

	sendJSONResponse(w, http.StatusOK, response)
}

// StartTrip handles POST /trips/{id}/start
func (h *TripHandler) StartTrip(w http.ResponseWriter, r *http.Request) {
	// Extract trip ID from path
	path := r.URL.Path
	parts := strings.Split(path, "/")
	if len(parts) < 4 {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid trip ID",
			Data:    nil,
		})
		return
	}
	idStr := parts[len(parts)-2] // ID is second-to-last part in /trips/{id}/start
	id, err := strconv.Atoi(idStr)
	if err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid trip ID",
			Data:    nil,
		})
		return
	}

	// In a real implementation, this would start the trip using a TripService
	// This is a placeholder for Phase 2

	// Prepare response
	response := Response{
		Status:  "success",
		Message: fmt.Sprintf("Trip with ID %d started successfully", id),
		Data:    nil,
	}

	sendJSONResponse(w, http.StatusOK, response)
}

// EndTrip handles POST /trips/{id}/end
func (h *TripHandler) EndTrip(w http.ResponseWriter, r *http.Request) {
	// Extract trip ID from path
	path := r.URL.Path
	parts := strings.Split(path, "/")
	if len(parts) < 4 {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid trip ID",
			Data:    nil,
		})
		return
	}
	idStr := parts[len(parts)-2] // ID is second-to-last part in /trips/{id}/end
	id, err := strconv.Atoi(idStr)
	if err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid trip ID",
			Data:    nil,
		})
		return
	}

	// In a real implementation, this would end the trip using a TripService
	// This is a placeholder for Phase 2

	// Prepare response
	response := Response{
		Status:  "success",
		Message: fmt.Sprintf("Trip with ID %d ended successfully", id),
		Data:    nil,
	}

	sendJSONResponse(w, http.StatusOK, response)
}

// AddWaypoint handles POST /trips/{id}/waypoints
func (h *TripHandler) AddWaypoint(w http.ResponseWriter, r *http.Request) {
	// Extract trip ID from path
	path := r.URL.Path
	parts := strings.Split(path, "/")
	if len(parts) < 4 {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid trip ID",
			Data:    nil,
		})
		return
	}
	idStr := parts[len(parts)-2] // ID is second-to-last part in /trips/{id}/waypoints
	id, err := strconv.Atoi(idStr)
	if err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid trip ID",
			Data:    nil,
		})
		return
	}

	// Parse request body
	var waypoint models.Waypoint
	if err := json.NewDecoder(r.Body).Decode(&waypoint); err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid request body",
			Data:    nil,
		})
		return
	}

	// Set trip ID on waypoint
	waypoint.TripID = id

	// In a real implementation, this would add the waypoint using a TripService
	// This is a placeholder for Phase 2
	waypoint.ID = 1 // Simulate ID assignment
	waypoint.CreatedAt = time.Now()

	// Prepare response
	response := Response{
		Status:  "success",
		Message: "Waypoint added successfully",
		Data:    waypoint,
	}

	sendJSONResponse(w, http.StatusCreated, response)
}

// GetWaypoints handles GET /trips/{id}/waypoints
func (h *TripHandler) GetWaypoints(w http.ResponseWriter, r *http.Request) {
	// Extract trip ID from path
	path := r.URL.Path
	parts := strings.Split(path, "/")
	if len(parts) < 4 {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid trip ID",
			Data:    nil,
		})
		return
	}
	idStr := parts[len(parts)-2] // ID is second-to-last part in /trips/{id}/waypoints
	id, err := strconv.Atoi(idStr)
	if err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status:  "error",
			Message: "Invalid trip ID",
			Data:    nil,
		})
		return
	}

	// In a real implementation, this would fetch the waypoints from a TripService
	// This is a placeholder for Phase 2
	waypoints := []models.Waypoint{}
	
	// Add trip ID to response message for clarity
	message := fmt.Sprintf("Waypoints for trip %d retrieved successfully", id)

	// Prepare response
	response := Response{
		Status:  "success",
		Message: message,
		Data:    waypoints,
	}

	sendJSONResponse(w, http.StatusOK, response)
}

// Helper function to extract pagination parameters from request
func getPaginationParams(r *http.Request) (int, int) {
	pageStr := r.URL.Query().Get("page")
	limitStr := r.URL.Query().Get("limit")

	page := 1
	if pageVal, err := strconv.Atoi(pageStr); err == nil && pageVal > 0 {
		page = pageVal
	}

	limit := 10
	if limitVal, err := strconv.Atoi(limitStr); err == nil && limitVal > 0 && limitVal <= 100 {
		limit = limitVal
	}

	return page, limit
}
