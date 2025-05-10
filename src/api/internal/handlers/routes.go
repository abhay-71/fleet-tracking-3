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

	// Trip endpoints
	mux.HandleFunc("GET "+apiV1+"/trips", func(w http.ResponseWriter, r *http.Request) {
		handleTrips(w, r, db)
	})
	mux.HandleFunc("GET "+apiV1+"/trips/{id}", func(w http.ResponseWriter, r *http.Request) {
		handleTripByID(w, r, db)
	})
}
