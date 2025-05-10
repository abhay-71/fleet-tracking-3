package router

import (
	"net/http"

	"fleetvigil.com/api/internal/handlers"
	"fleetvigil.com/api/internal/middleware"
	"github.com/go-chi/chi/v5"
	chimiddleware "github.com/go-chi/chi/v5/middleware"
)

// Router defines the main router for the API
type Router struct {
	UserHandler    *handlers.UserHandler
	VehicleHandler *handlers.VehicleHandler
	AuthConfig     middleware.AuthConfig
}

// New creates a new Router
func New(
	userHandler *handlers.UserHandler,
	vehicleHandler *handlers.VehicleHandler,
	authConfig middleware.AuthConfig,
) *Router {
	return &Router{
		UserHandler:    userHandler,
		VehicleHandler: vehicleHandler,
		AuthConfig:     authConfig,
	}
}

// SetupRoutes sets up the routes for the API
func (r *Router) SetupRoutes() http.Handler {
	router := chi.NewRouter()

	// Apply middleware
	router.Use(chimiddleware.Logger)
	router.Use(chimiddleware.Recoverer)
	router.Use(chimiddleware.RequestID)
	router.Use(chimiddleware.RealIP)
	router.Use(middleware.Cors)

	// Public routes
	router.Group(func(router chi.Router) {
		// Auth endpoints
		router.Post("/api/auth/register", r.UserHandler.Register)
		router.Post("/api/auth/login", r.UserHandler.Login)
		router.Post("/api/auth/refresh", r.UserHandler.RefreshToken)

		// Health check
		router.Get("/api/health", func(w http.ResponseWriter, r *http.Request) {
			w.WriteHeader(http.StatusOK)
			w.Write([]byte("OK"))
		})
	})

	// Private routes
	router.Group(func(router chi.Router) {
		// Apply auth middleware
		router.Use(middleware.JWTAuth(r.AuthConfig))

		// User endpoints
		router.Route("/api/users", func(router chi.Router) {
			router.Get("/", r.UserHandler.ListUsers)
			router.Post("/", r.UserHandler.Register)
			router.Get("/{id}", r.UserHandler.GetUser)
			router.Put("/{id}", r.UserHandler.UpdateUser)
			router.Delete("/{id}", r.UserHandler.DeleteUser)
			router.Get("/{id}/roles", r.UserHandler.GetUserRoles)
			router.Post("/{id}/roles/{roleId}", r.UserHandler.AssignRole)
			router.Delete("/{id}/roles/{roleId}", r.UserHandler.RemoveRole)
		})

		// Vehicle endpoints
		router.Route("/api/vehicles", func(router chi.Router) {
			router.Get("/", r.VehicleHandler.ListVehicles)
			router.Post("/", r.VehicleHandler.CreateVehicle)
			router.Get("/registration/{registration}", r.VehicleHandler.GetVehicleByRegistration)
			router.Get("/{id}", r.VehicleHandler.GetVehicle)
			router.Put("/{id}", r.VehicleHandler.UpdateVehicle)
			router.Delete("/{id}", r.VehicleHandler.DeleteVehicle)
			router.Patch("/{id}/status", r.VehicleHandler.UpdateVehicleStatus)
		})

		// Company vehicle endpoints
		router.Get("/api/companies/{companyId}/vehicles", r.VehicleHandler.ListVehiclesByCompany)
	})

	return router
}
