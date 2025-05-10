package handlers

import (
	"encoding/json"
	"net/http"
	"strconv"

	"fleetvigil.com/api/internal/models"
	"fleetvigil.com/api/internal/service"
	"github.com/go-chi/chi/v5"
)

// VehicleHandler handles HTTP requests for vehicles
type VehicleHandler struct {
	vehicleService service.VehicleService
}

// NewVehicleHandler creates a new VehicleHandler
func NewVehicleHandler(vehicleService service.VehicleService) *VehicleHandler {
	return &VehicleHandler{
		vehicleService: vehicleService,
	}
}

// CreateVehicle creates a new vehicle
func (h *VehicleHandler) CreateVehicle(w http.ResponseWriter, r *http.Request) {
	var vehicle models.Vehicle
	if err := json.NewDecoder(r.Body).Decode(&vehicle); err != nil {
		respondWithError(w, http.StatusBadRequest, "Invalid request payload")
		return
	}

	if err := h.vehicleService.Create(r.Context(), &vehicle); err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}

	respondWithJSON(w, http.StatusCreated, vehicle)
}

// GetVehicle gets a vehicle by ID
func (h *VehicleHandler) GetVehicle(w http.ResponseWriter, r *http.Request) {
	idStr := chi.URLParam(r, "id")
	id, err := strconv.Atoi(idStr)
	if err != nil {
		respondWithError(w, http.StatusBadRequest, "Invalid vehicle ID")
		return
	}

	vehicle, err := h.vehicleService.Get(r.Context(), id)
	if err != nil {
		respondWithError(w, http.StatusNotFound, "Vehicle not found")
		return
	}

	respondWithJSON(w, http.StatusOK, vehicle)
}

// GetVehicleByRegistration gets a vehicle by registration number
func (h *VehicleHandler) GetVehicleByRegistration(w http.ResponseWriter, r *http.Request) {
	registration := chi.URLParam(r, "registration")
	if registration == "" {
		respondWithError(w, http.StatusBadRequest, "Registration number is required")
		return
	}

	vehicle, err := h.vehicleService.GetByRegistration(r.Context(), registration)
	if err != nil {
		respondWithError(w, http.StatusNotFound, "Vehicle not found")
		return
	}

	respondWithJSON(w, http.StatusOK, vehicle)
}

// UpdateVehicle updates a vehicle
func (h *VehicleHandler) UpdateVehicle(w http.ResponseWriter, r *http.Request) {
	idStr := chi.URLParam(r, "id")
	id, err := strconv.Atoi(idStr)
	if err != nil {
		respondWithError(w, http.StatusBadRequest, "Invalid vehicle ID")
		return
	}

	var vehicle models.Vehicle
	if err := json.NewDecoder(r.Body).Decode(&vehicle); err != nil {
		respondWithError(w, http.StatusBadRequest, "Invalid request payload")
		return
	}

	vehicle.ID = id

	if err := h.vehicleService.Update(r.Context(), &vehicle); err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}

	respondWithJSON(w, http.StatusOK, map[string]string{"message": "Vehicle updated successfully"})
}

// DeleteVehicle deletes a vehicle
func (h *VehicleHandler) DeleteVehicle(w http.ResponseWriter, r *http.Request) {
	idStr := chi.URLParam(r, "id")
	id, err := strconv.Atoi(idStr)
	if err != nil {
		respondWithError(w, http.StatusBadRequest, "Invalid vehicle ID")
		return
	}

	if err := h.vehicleService.Delete(r.Context(), id); err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}

	respondWithJSON(w, http.StatusOK, map[string]string{"message": "Vehicle deleted successfully"})
}

// ListVehicles lists vehicles with pagination
func (h *VehicleHandler) ListVehicles(w http.ResponseWriter, r *http.Request) {
	pageStr := r.URL.Query().Get("page")
	limitStr := r.URL.Query().Get("limit")

	page := 1
	limit := 10

	if pageStr != "" {
		pageInt, err := strconv.Atoi(pageStr)
		if err == nil && pageInt > 0 {
			page = pageInt
		}
	}

	if limitStr != "" {
		limitInt, err := strconv.Atoi(limitStr)
		if err == nil && limitInt > 0 {
			limit = limitInt
		}
	}

	vehicles, total, err := h.vehicleService.List(r.Context(), page, limit)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}

	response := models.ListResponse{
		Data: vehicles,
		Pagination: models.Pagination{
			Page:  page,
			Limit: limit,
			Total: total,
		},
	}

	respondWithJSON(w, http.StatusOK, response)
}

// ListVehiclesByCompany lists vehicles for a company with pagination
func (h *VehicleHandler) ListVehiclesByCompany(w http.ResponseWriter, r *http.Request) {
	companyIDStr := chi.URLParam(r, "companyId")
	companyID, err := strconv.Atoi(companyIDStr)
	if err != nil {
		respondWithError(w, http.StatusBadRequest, "Invalid company ID")
		return
	}

	pageStr := r.URL.Query().Get("page")
	limitStr := r.URL.Query().Get("limit")

	page := 1
	limit := 10

	if pageStr != "" {
		pageInt, err := strconv.Atoi(pageStr)
		if err == nil && pageInt > 0 {
			page = pageInt
		}
	}

	if limitStr != "" {
		limitInt, err := strconv.Atoi(limitStr)
		if err == nil && limitInt > 0 {
			limit = limitInt
		}
	}

	vehicles, total, err := h.vehicleService.ListByCompany(r.Context(), companyID, page, limit)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}

	response := models.ListResponse{
		Data: vehicles,
		Pagination: models.Pagination{
			Page:  page,
			Limit: limit,
			Total: total,
		},
	}

	respondWithJSON(w, http.StatusOK, response)
}

// UpdateVehicleStatus updates the status of a vehicle
func (h *VehicleHandler) UpdateVehicleStatus(w http.ResponseWriter, r *http.Request) {
	idStr := chi.URLParam(r, "id")
	id, err := strconv.Atoi(idStr)
	if err != nil {
		respondWithError(w, http.StatusBadRequest, "Invalid vehicle ID")
		return
	}

	var statusUpdate struct {
		Status string `json:"status"`
	}

	if err := json.NewDecoder(r.Body).Decode(&statusUpdate); err != nil {
		respondWithError(w, http.StatusBadRequest, "Invalid request payload")
		return
	}

	if statusUpdate.Status == "" {
		respondWithError(w, http.StatusBadRequest, "Status is required")
		return
	}

	if err := h.vehicleService.UpdateStatus(r.Context(), id, statusUpdate.Status); err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}

	respondWithJSON(w, http.StatusOK, map[string]string{"message": "Vehicle status updated successfully"})
}
