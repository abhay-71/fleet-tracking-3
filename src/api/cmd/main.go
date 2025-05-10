package main

import (
	"fmt"
	"log"
	"net/http"
	"os"
	"os/signal"
	"syscall"
	"time"

	"fleetvigil.com/api/internal/config"
	"fleetvigil.com/api/internal/handlers"
	"fleetvigil.com/api/internal/middleware"
	"fleetvigil.com/api/pkg/database"
)

func main() {
	// Load configuration
	cfg, err := config.Load()
	if err != nil {
		log.Fatalf("Failed to load configuration: %v", err)
	}

	// Initialize database connection
	db, err := database.Connect(cfg.Database)
	if err != nil {
		log.Fatalf("Failed to connect to database: %v", err)
	}
	defer db.Close()

	// Check PostGIS extension
	hasPostGIS, err := db.CheckPostGIS()
	if err != nil {
		log.Printf("Failed to check PostGIS extension: %v", err)
	} else if !hasPostGIS {
		log.Println("PostGIS extension not found. Attempting to install...")
		if err := db.InstallPostGIS(); err != nil {
			log.Printf("Warning: Failed to install PostGIS extension: %v", err)
		} else {
			log.Println("PostGIS extension successfully installed")
		}
	} else {
		log.Println("PostGIS extension found")
	}

	// Setup router
	router := http.NewServeMux()

	// Register routes
	handlers.RegisterRoutes(router, db)

	// Add middleware
	var handler http.Handler = router
	handler = middleware.Logging(handler)
	handler = middleware.Recovery(handler)

	// Create auth config for JWT middleware
	authConfig := middleware.AuthConfig{
		JWTKey:    []byte(cfg.Auth.JWTSecret),
		JWTIssuer: "fleetvigil-api",
	}
	handler = middleware.JWTAuth(authConfig)(handler)

	// Configure and start server
	server := &http.Server{
		Addr:         fmt.Sprintf(":%d", cfg.Server.Port),
		Handler:      handler,
		ReadTimeout:  15 * time.Second,
		WriteTimeout: 15 * time.Second,
		IdleTimeout:  60 * time.Second,
	}

	// Create logs directory if it doesn't exist
	if err := os.MkdirAll("logs", 0755); err != nil {
		log.Printf("Warning: Failed to create logs directory: %v", err)
	}

	// Start server in a goroutine
	go func() {
		log.Printf("Server starting on port %d", cfg.Server.Port)
		if err := server.ListenAndServe(); err != nil && err != http.ErrServerClosed {
			log.Fatalf("Failed to start server: %v", err)
		}
	}()

	// Set up graceful shutdown
	quit := make(chan os.Signal, 1)
	signal.Notify(quit, syscall.SIGINT, syscall.SIGTERM)
	<-quit

	log.Println("Server shutting down...")
}
