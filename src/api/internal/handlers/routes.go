package handlers

import (
	"net/http"

	"fleetvigil.com/api/pkg/database"
)

// RegisterRoutes registers all API routes
func RegisterRoutes(mux *http.ServeMux, db *database.DB) {
	// Health check endpoint
	mux.HandleFunc("GET /health", healthCheckHandler)

	// API version prefix
	apiV1 := "/api/v1"

	// Authentication endpoints
	mux.HandleFunc("POST "+apiV1+"/auth/login", func(w http.ResponseWriter, r *http.Request) {
		handleLogin(w, r, db)
	})
	mux.HandleFunc("POST "+apiV1+"/auth/register", func(w http.ResponseWriter, r *http.Request) {
		handleRegister(w, r, db)
	})
	mux.HandleFunc("POST "+apiV1+"/auth/refresh", func(w http.ResponseWriter, r *http.Request) {
		handleRefresh(w, r, db)
	})

	// User endpoints
	mux.HandleFunc("GET "+apiV1+"/users", func(w http.ResponseWriter, r *http.Request) {
		handleUsers(w, r, db)
	})
	mux.HandleFunc("GET "+apiV1+"/users/{id}", func(w http.ResponseWriter, r *http.Request) {
		handleUserByID(w, r, db)
	})

	// Vehicle endpoints
	mux.HandleFunc("GET "+apiV1+"/vehicles", func(w http.ResponseWriter, r *http.Request) {
		handleVehicles(w, r, db)
	})
	mux.HandleFunc("GET "+apiV1+"/vehicles/{id}", func(w http.ResponseWriter, r *http.Request) {
		handleVehicleByID(w, r, db)
	})

	// GPS data ingestion endpoints
	gpsHandler := NewGPSDataHandler(db)
	mux.HandleFunc("POST "+apiV1+"/gps/data", gpsHandler.IngestGPSData)
	mux.HandleFunc("POST "+apiV1+"/gps/batch", gpsHandler.IngestBatchGPSData)

	// Geofencing endpoints
	geofenceHandler := NewGeofenceHandler(db)
	mux.HandleFunc("GET "+apiV1+"/geofences", geofenceHandler.ListGeofences)
	mux.HandleFunc("POST "+apiV1+"/geofences", geofenceHandler.CreateGeofence)
	mux.HandleFunc("GET "+apiV1+"/geofences/{id}", geofenceHandler.GetGeofence)
	mux.HandleFunc("PUT "+apiV1+"/geofences/{id}", geofenceHandler.UpdateGeofence)
	mux.HandleFunc("DELETE "+apiV1+"/geofences/{id}", geofenceHandler.DeleteGeofence)
	mux.HandleFunc("POST "+apiV1+"/geofences/check", geofenceHandler.CheckPoint)
	mux.HandleFunc("POST "+apiV1+"/geofences/batch-check", geofenceHandler.BatchCheck)

	// Trip endpoints
	tripHandler := NewTripHandler(db)

	mux.HandleFunc("GET "+apiV1+"/trips", tripHandler.ListTrips)
	mux.HandleFunc("POST "+apiV1+"/trips", tripHandler.CreateTrip)
	mux.HandleFunc("GET "+apiV1+"/trips/{id}", tripHandler.GetTrip)
	mux.HandleFunc("PUT "+apiV1+"/trips/{id}", tripHandler.UpdateTrip)
	mux.HandleFunc("DELETE "+apiV1+"/trips/{id}", tripHandler.DeleteTrip)
	mux.HandleFunc("POST "+apiV1+"/trips/{id}/start", tripHandler.StartTrip)
	mux.HandleFunc("POST "+apiV1+"/trips/{id}/end", tripHandler.EndTrip)
	mux.HandleFunc("GET "+apiV1+"/trips/{id}/waypoints", tripHandler.GetWaypoints)
	mux.HandleFunc("POST "+apiV1+"/trips/{id}/waypoints", tripHandler.AddWaypoint)
}
