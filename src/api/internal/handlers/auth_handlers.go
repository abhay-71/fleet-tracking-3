package handlers

import (
	"encoding/json"
	"net/http"
	"time"

	"fleetvigil.com/api/internal/config"
	"fleetvigil.com/api/pkg/auth"
	"fleetvigil.com/api/pkg/database"
)

// LoginRequest represents a login request
type LoginRequest struct {
	Username string `json:"username"`
	Password string `json:"password"`
}

// LoginResponse represents a login response
type LoginResponse struct {
	AccessToken  string    `json:"access_token"`
	RefreshToken string    `json:"refresh_token"`
	ExpiresAt    time.Time `json:"expires_at"`
	UserID       int       `json:"user_id"`
	Username     string    `json:"username"`
	Email        string    `json:"email"`
	Role         string    `json:"role"`
}

// RegisterRequest represents a registration request
type RegisterRequest struct {
	Username  string `json:"username"`
	Email     string `json:"email"`
	Password  string `json:"password"`
	FirstName string `json:"first_name"`
	LastName  string `json:"last_name"`
}

// RefreshRequest represents a token refresh request
type RefreshRequest struct {
	RefreshToken string `json:"refresh_token"`
}

// handleAuth handles authentication-related requests
func handleAuth(w http.ResponseWriter, r *http.Request, db *database.DB) {
	// In Phase 1, we're just setting up the infrastructure
	// This will be expanded in Phase 2 with actual database integration
	
	// For now, simulate a login process
	if r.Method == "POST" && r.URL.Path == "/api/v1/auth/login" {
		handleLogin(w, r, db)
		return
	} else if r.Method == "POST" && r.URL.Path == "/api/v1/auth/register" {
		handleRegister(w, r, db)
		return
	} else if r.Method == "POST" && r.URL.Path == "/api/v1/auth/refresh" {
		handleRefresh(w, r, db)
		return
	}

	// Return a 404 for any other paths
	http.NotFound(w, r)
}

// handleLogin handles user login
func handleLogin(w http.ResponseWriter, r *http.Request, db *database.DB) {
	// Parse request body
	var req LoginRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status: "error",
			Error:  "Invalid request format",
		})
		return
	}

	// In Phase 1, we're just simulating a successful login
	// This will be replaced with actual database authentication in Phase 2
	
	// For the admin user created in the migration, we'll allow login
	if req.Username == "admin" && req.Password == "admin" {
		// Load configuration to get JWT settings
		cfg, err := config.Load()
		if err != nil {
			sendJSONResponse(w, http.StatusInternalServerError, Response{
				Status: "error",
				Error:  "Failed to load configuration",
			})
			return
		}

		// Generate tokens
		accessToken, err := auth.GenerateToken(1, "admin", "admin@fleetvigil.com", "admin", cfg.Auth.JWTSecret, cfg.Auth.TokenExpirationHour)
		if err != nil {
			sendJSONResponse(w, http.StatusInternalServerError, Response{
				Status: "error",
				Error:  "Failed to generate access token",
			})
			return
		}

		refreshToken, err := auth.GenerateRefreshToken(1, cfg.Auth.JWTSecret)
		if err != nil {
			sendJSONResponse(w, http.StatusInternalServerError, Response{
				Status: "error",
				Error:  "Failed to generate refresh token",
			})
			return
		}

		// Create response
		expiresAt := time.Now().Add(time.Duration(cfg.Auth.TokenExpirationHour) * time.Hour)
		response := LoginResponse{
			AccessToken:  accessToken,
			RefreshToken: refreshToken,
			ExpiresAt:    expiresAt,
			UserID:       1,
			Username:     "admin",
			Email:        "admin@fleetvigil.com",
			Role:         "admin",
		}

		sendJSONResponse(w, http.StatusOK, Response{
			Status: "success",
			Data:   response,
		})
		return
	}

	// Return unauthorized for any other credentials
	sendJSONResponse(w, http.StatusUnauthorized, Response{
		Status: "error",
		Error:  "Invalid credentials",
	})
}

// handleRegister handles user registration
func handleRegister(w http.ResponseWriter, r *http.Request, db *database.DB) {
	// Parse request body
	var req RegisterRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status: "error",
			Error:  "Invalid request format",
		})
		return
	}

	// In Phase 1, we're just setting up the infrastructure
	// This will be expanded in Phase 2 with actual database integration

	sendJSONResponse(w, http.StatusOK, Response{
		Status:  "success",
		Message: "Registration endpoint ready. User registration will be implemented in Phase 2.",
		Data: map[string]interface{}{
			"username": req.Username,
			"email":    req.Email,
		},
	})
}

// handleRefresh handles token refresh
func handleRefresh(w http.ResponseWriter, r *http.Request, db *database.DB) {
	// Parse request body
	var req RefreshRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		sendJSONResponse(w, http.StatusBadRequest, Response{
			Status: "error",
			Error:  "Invalid request format",
		})
		return
	}

	// In Phase 1, we're just setting up the infrastructure
	// This will be expanded in Phase 2 with actual token verification and refresh

	sendJSONResponse(w, http.StatusOK, Response{
		Status:  "success",
		Message: "Token refresh endpoint ready. Token refresh will be implemented in Phase 2.",
	})
} 